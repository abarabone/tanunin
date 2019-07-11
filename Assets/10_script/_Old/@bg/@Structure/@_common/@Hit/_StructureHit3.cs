using UnityEngine;
using System.Collections;


public abstract class _StructureHit3 : _Hit3
{


	public _Structure3		structure	{ get; private set; }
	


	public float	durability;


	public bool isDestructed	{ get { return durability <= 0.0f; } }





	public BreakUnit	breakManager;





	// ----------------------------


	/// <summary>
	/// 適当なボーン位置を返す。
	/// </summary>
	public override Transform getRandomBone()
	{
		return structure.tf;
	}



	/// <summary>
	/// 破壊フラグを立て、レンダラに非表示を指示する。
	/// </summary>
	/// <param name="part">破壊したいパーツ</param>
	public void breakAndPartVisibilityOff( _StructurePart3 part )
	{

		breakManager.brokenFlags[ part.partId ] = true;

		structure.nearRenderer.setPartVisibilityOff( ref breakManager.brokenFlags, part.partId );


		if( part.isHittable & !isDestructed )
		{
			breakManager.breakers[ (int)part.type ].breakPartInHitMesh( part );
		}

	}



	// 初期化・更新 ----------------------------

	new protected void Awake()
	{

		base.Awake();


		structure = GetComponent<_Structure3>();

	}


	public virtual void init( StructureNearObjectBuilder builder )
	{

		base.init();
		

		breakManager.init( builder, structure );

	}





	// ダメージ系 --------------------


	public virtual bool isPartDestructed( int partId, ref _Bullet3.DamageSourceUnit ds )//これパーツにもたせられん？
	{

		// 基底のパーツは、breakFlag によってのみ壊せるか壊せないかが決まる（一撃で壊れる）。

		return true;

	}



	public virtual void applyDamage( ref _Bullet3.DamageSourceUnit ds, float sumDamageRate, float hitCount )
	{

		//ds.damage *= sumDamageRate / hitCount;

		checkDestruction();

	}

	
	public void breakDurability( float partDurabilityWeight )
	{

		durability -= partDurabilityWeight;

	}


	public void checkDestruction()
	{
		
		if( isDestructed )
		{
			structure.destruct();

			breakManager.downAllMeshColliders();

			//rejectHook();
		}

	}






	// ヒット処理 -----------------------

	public override void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	{

		if( ds.damage > 0.0f )
		{

			var breaker = breakManager.getBreaker( hit.collider );


			force *= 0.005f;


			var partId  = breaker.triIdToPartId( hit.triangleIndex );

			var part = structure.parts[ partId ];


			if( part.isShootBreakable && !breakManager.isPartBroken( partId ) )
			{

				if( isPartDestructed( partId, ref ds ) )//part.isDestroy( ref ds ) )
				{

					breakDurability( part.getDurabilityWeight() );


					part.fallDownAllChildren( this, force, hit.point );

				}


				applyDamage( ref ds, part.getDamageRate(), 1 );

			}


		}

	}

	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, _Action3 attacker )
	{

		if( ds.damage > 0.0f )
		{

			var tf = structure.tf;

			var mt = tf.localToWorldMatrix;

			var structureViewCenter = tf.InverseTransformPoint( center );//mt.transpose.MultiplyPoint3x4( center );

			var spos = tf.position;


			var force = pressure * 0.005f;
			


			var hitCount = 0.0f;

			var sumDamageRate = 0.0f;


			foreach( var part in structure.parts )
			{

				if( !part.isBlastBreakable || breakManager.isPartBroken( part.partId ) ) continue;


				if( part.hitLocalBounds.check( structureViewCenter, radius ) )
				{

					if( isPartDestructed( part.partId, ref ds ) )//part.isDestroy( ref ds ) )
					{

						breakDurability( part.getDurabilityWeight() );


						var v = mt.MultiplyVector( part.hitLocalBounds.structureViewCenter - structureViewCenter );

						part.fallDownAllChildren( this, v * force, spos );

					}


					hitCount++;

					sumDamageRate += part.getDamageRate();

				}
			}


			applyDamage( ref ds, sumDamageRate, hitCount );

		}


	}





	// 破壊関係 --------------------

	public struct BreakUnit
	{


		public StructureMeshBreakController3[] breakers { get; private set; }


		public BitBoolArray         brokenFlags;//	{ get; private set; }
												// パーツがこわれてたら 1 になるビットフラグ配列



		// --------------------

		
		static public _StructurePart3.enType getBrekerIndex( Collider collider )
		{
			return (_StructurePart3.enType)( collider.gameObject.layer - UserLayer._bgDetail );
		}



		/// <summary>
		/// 破壊コントローラーを取得する。
		/// </summary>
		/// <param name="partType">取得したい破壊コントローラーを示すタイプ</param>
		/// <returns>破壊コントローラー</returns>
		public StructureMeshBreakController3 getBreaker( _StructurePart3.enType partType )
		{
			return breakers[ (int)partType ];
		}


		/// <summary>
		/// 破壊コントローラーを取得する。
		/// </summary>
		/// <param name="collider">取得したい破壊コントローラーのメッシュコライダー</param>
		/// <returns>破壊コントローラー</returns>
		public StructureMeshBreakController3 getBreaker( Collider collider )
		{
			return breakers[ (int)getBrekerIndex( collider ) ];
		}


		/// <summary>
		/// 破壊されているかどうかを返す。
		/// </summary>
		public bool isPartBroken( int partId )
		{
			return brokenFlags[ partId ];
		}


		

		/// <summary>
		/// すべてのメッシュコライダを破壊状態（disable）にする。
		/// </summary>
		public void downAllMeshColliders()
		{

			foreach( var breaker in breakers )
			{
				if( breaker != null ) breaker.downMeshCollider();
			}

		}




		// 初期化・更新 ----------------------------

		public void init( StructureNearObjectBuilder builder, _Structure3 structure )
		{
			
			breakers = new StructureMeshBreakController3[ (int)_StructurePart3.enType.length ];

			brokenFlags.init( builder.parts.Length );


			var cs = builder.near.GetComponentsInChildren<MeshCollider>( true );

			foreach( var c in cs )
			{

				var i = getBrekerIndex( c );

				var breaker = new StructureMeshBreakController3( structure );

				breaker.init( c, builder.meshBuilder.hits[ (int)i ].triIndexToPartIdList, i );

				breakers[ (int)i ] = breaker;

			}

		}



	}


}
