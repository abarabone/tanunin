using UnityEngine;
using System.Collections;
using System;


public abstract class _StructurePart3 : MonoBehaviour
{


	public enType	type;

	public byte     subType;
	
	public byte     siblingIndex;


	public enBreakFlag	breakFlag = enBreakFlag.shootAndBlast;




	// 

	public enum enType : byte
	{
		
		common,		// 通常の触れて見える物体。
		
		occlusion,	// 見えるけど触れない物体。

		invisible,	// 見えないもの。ガラスなど。
		
		fence,		// 見えて触れるけど、弾丸がヒットしない。
		
		length,
		
		inhittable,	// ヒットメッシュは必要ない、ドローメッシュだけでいい物体。
		
		lengthInBuild

	}

	public enum enBreakFlag : byte
	{
		none	= 0,
		shoot	= 1 << 0,
		blast	= 1 << 1,
		acid	= 1 << 2,
		fire	= 1 << 3,
		shootAndBlast	= shoot | blast
	}



	public bool isHittable
	{
		get { return type != enType.inhittable; }
	}


	public bool isShootBreakable	{ get { return ( breakFlag & enBreakFlag.shoot ) != 0; } }
	public bool isBlastBreakable	{ get { return ( breakFlag & enBreakFlag.blast ) != 0; } }
	public bool isAcidBreakable		{ get { return ( breakFlag & enBreakFlag.acid ) != 0; } }
	public bool isfireBreakable		{ get { return ( breakFlag & enBreakFlag.fire ) != 0; } }




	
	//[ HideInInspector ]
	//public Transform	tf	{ get; protected set; }

	
	//[ HideInInspector ]
	public int	partId	{ get; protected set; }


	//[ HideInInspector ]
	public _StructurePart3[] children { get; protected set; }
	//protected _StructurePart3[]	children;
	//protected int[]	idsOfChildren;



	public hitIndexInfoUnit		hitIndexInfo;


	public HitLocalBoundsUnit	hitLocalBounds;





	

	// 初期化 ----------------------------------

	public virtual void init( int id )
	{
		
		partId		= id;


		setChildrenLinks();


		buildFromPieces();

	}



	// これはいずれ、直下の子のみ所持するように変更したい。さらに親を複数持てるようにしたい。
	// シーンセットアップでは親をセットし、初期化で子リストを作るとか。子は親がひとつでも残っていれば壊れない、といった感じに。
	protected void setChildrenLinks()
	{

		var childs = GetComponentsInChildren<_StructurePart3>();


		if( childs.Length > 1 )
		{

			children = new _StructurePart3[ childs.Length - 1 ];

			Array.Copy( childs, 1, children, 0, childs.Length - 1 );

		}

	}
	



	
	// 瓦解 =============================================
	// ちょっとごちゃごちゃしてなくもない。後から加えた仕組みも多いし。場合分けを再検討してすっきりさせたい。


	// 瓦解（継承用：子孫ごとのふるまい）--------------------------------------
	// 　fallDownAllChildren() からパーツ個別に呼ばれる
	// 　パーツごとに異なるふるまいが必要な場合は、これを継承して記述する

	protected abstract bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point );

	

	// 瓦解（パーツ本体用：メッシュ破壊など） ---------------------------
	// 　主に _Hit3 関連から呼ばれる　破壊が必要な際の窓口
	//　子孫の差異に関係なく定型なので、仮想関数ではない

	public void fallDownAllChildren( _StructureHit3 hitter, Vector3 force, Vector3 point )
	// 所持する子パーツを含めて破壊する（単純に fallDownEntity() を子の分まわしている）
	{
		
		
		fallDownEntity( hitter, force, point );

		
		if( children != null )
		{
			foreach( var child in children )
			{

				if( child != null && !hitter.breakManager.isPartBroken( child.partId ) )
				{
					child.fallDownEntity( hitter, force, point );
				}

			}
		}

	}

	protected void fallDownEntity( _StructureHit3 hitter, Vector3 force, Vector3 point )
	// パーツ個体のみの破壊（子パーツ破壊不要ならこれの直呼びでＯＫ）
	{


		var isBreak = fallDown( hitter, force, point );


		if( isBreak )
		{
			hitter.breakAndPartVisibilityOff( this );
		}

	}



	// 瓦解（表示用：破片メッシュのインスタンス化など）-----------------------
	//　子孫の fallDown() から必要に応じて呼ばれる

	public _StructurePart3 fallDownDestructionSurface( _StructureHit3 hitter )
	// ただ消える物体用
	{

		var instance = getDestructionSurfaceInstance( hitter.structure.tf );

		Destroy( instance.gameObject );

		return instance;
		
	}

	public _StructurePart3 fallDownDestructionSurface( _StructureHit3 hitter, Vector3 force, Vector3 point )
	// 落下する物体用
	{

		var instance = getDestructionSurfaceInstance( hitter.structure.tf );

		instance.fallDownDestructionSurface( force, point );
		
		return instance;

	}

	public void fallDownDestructionSurface( Vector3 force, Vector3 point )
	{
		
		transform.parent = null;

		var c = GetComponent<MeshCollider>();//
		if( c ) c.enabled = false;// まだメッシュコライダが残っているので必要になってしまう


		var rb = GetComponent<Rigidbody>();// ?? gameObject.AddComponent<Rigidbody>();
		
		rb = rb? rb: gameObject.AddComponent<Rigidbody>();
		
		rb.detectCollisions = false;//
		
		rb.AddForceAtPosition( force, point, ForceMode.Impulse );
		
		
		Destroy( gameObject, 5.0f );
		
	}

	
	// 瓦解パーツのインスタンス取得 --------------------------------
	
	/// <summary>
	/// 瓦解時に表示すべきパーツインスタンスを取得する。パーツのコンテンツ形態にあわせ、複製か直接の取得を返す。
	/// （再配置や修復可能の場合は複製、直子持ちは自身を直接）
	/// </summary>
	protected virtual _StructurePart3 getDestructionSurfaceInstance( Transform tfParent )
	{
		
		return this;
		
	}


	// =============================================



	

	// ダメージ計算 ---------------------------------

	/*
	public virtual bool isDestroy( ref _Bullet3.DamageSourceUnit ds )
	{

		// 基底では、breakFlag によってのみ壊せるか壊せないかが決まる。

		return true;

	}
	*/
	/// <summary>
	/// このパーツを攻撃した時に、アーマーダメージへ及ぼす影響のレート。
	/// </summary>
	public virtual float getDamageRate()
	{

		// 基底では、パーツはアーマーダメージへ影響を及ぼさない。

		return	1.0f;

	}

	/// <summary>
	/// このパーツを破壊した時に、全体の耐久力から減じる値を返す。
	/// </summary>
	public virtual float getDurabilityWeight()
	{

		// 基底では、１パーツが１ダメージをになう。
		// パーツの耐久力は、パーツが壊れた時に全体の耐久力から差し引かれる。

		return 1.0f;

	}







	// 構築 --------------------------------------
	//linqにしたらシンプルになるか？
	/// <summary>
	/// 自身のメッシュと、パーツ以外の子孫のメッシュ（_StructurePart を持たないもの）をすべて統合する。
	/// 途中でパーツを発見した場合、そのパーツとその子孫は統合対象から外れる。
	/// パーツ以外の子孫は、ゲームオブジェクトレベルですべて破棄される。
	/// 子孫がレンダラを一つも持っていない場合は、さっさと処理を抜ける。
	/// </summary>
	public virtual void buildFromPieces()
	{

		var tf = transform;

		var mf = tf.GetComponent<MeshFilter>();

		if( mf == null )
		{
			var mr = GetComponentInChildren<MeshRenderer>();

			if( mr == null ) return;

			gameObject.AddComponent<MeshRenderer>().sharedMaterial = mr.sharedMaterial;
		}



		var counter	= new MeshElementsCounter();


		if( mf != null ) counter.count( mf.sharedMesh );

		countChildrenMeshes( ref counter, tf );



		var creator	= new ColoredNormalMesthCreator();//SimpleMeshCreator();
		
		creator.alloc( ref counter );


		if( mf != null )
		{
			var mt = Matrix4x4.identity;

			creator.addGeometory( mf.sharedMesh, ref mt );
		}

		var mtInv = tf.worldToLocalMatrix;

		buildChildrenMeshes( creator, tf, ref mtInv );


	
		if( mf == null ) mf = gameObject.AddComponent<MeshFilter>();

		mf.sharedMesh = creator.create();

	}

	void countChildrenMeshes( ref MeshElementsCounter counter, Transform tf )
	{
		for( var i = 0; i < tf.childCount; i++ )
		{
			
			var tfChild = tf.GetChild( i );
			
			if( tfChild.GetComponent<_StructurePart3>() == null )
			{
				
				if( tfChild.childCount > 0 )
				{
					countChildrenMeshes( ref counter, tfChild );
				}


				var mf = tfChild.GetComponent<MeshFilter>();

				if( mf != null ) counter.count( mf.sharedMesh );

			}
			
		}
	}

	int buildChildrenMeshes( SimpleMeshCreator creator, Transform tf, ref Matrix4x4 mtInvRoot )
	{

		var destroyCount = 0;


		for( var i = 0; i < tf.childCount; i++ )
		{

			var tfChild = tf.GetChild( i );

			if( tfChild.GetComponent<_StructurePart3>() == null )
			{

				var subChildrenLength = tfChild.childCount;

				if( subChildrenLength > 0 )
				{

					var subChildrenDestroyedCount = buildChildrenMeshes( creator, tfChild, ref mtInvRoot );

					subChildrenLength -= subChildrenDestroyedCount;

				}


				var mf = tfChild.GetComponent<MeshFilter>();
				
				if( mf != null )
				{
					
					var mt	= mtInvRoot * tfChild.localToWorldMatrix;
					
					creator.addGeometory( mf.sharedMesh, ref mt );

				}


				if( subChildrenLength == 0 )
				{

					Destroy( tfChild.gameObject );

					destroyCount++;

				}

			}

		}


		return destroyCount;

	}


/*	public virtual void buildFromPieces()
	// とりあえず今のところ、自身がＭＦをもっていなければ、直下のみ（かつ Part でないもの）のＭＦをまとめる、という仕様でいきます
	{

		if( gameObject.GetComponent<MeshFilter>() == null )
		{

			var mfs = this.getComponentsInDirectChildren<MeshFilter>();

			if( mfs.Length > 0 )
			{

				var mat = mfs[0].renderer.sharedMaterial;
				
				var mesh = combineMesh( mfs );
				
				
				var mf	= gameObject.AddComponent<MeshFilter>();
				var mr	= gameObject.AddComponent<MeshRenderer>();
				
				
				mf.sharedMesh		= mesh;
				
				mr.sharedMaterial	= mat;
				
			}

		}

	}

	Mesh combineMesh( MeshFilter[] mfs )
	{
		
		var counter	= new MeshElementsCounter( mfs );
		
		
		var creator	= new ColoredNormalMesthCreator();//SimpleMeshCreator();
		
		creator.alloc( ref counter );
		
		var mtInvParent	= transform.worldToLocalMatrix;


		foreach( var mf in mfs )
		{
			if( mf.GetComponent<_StructurePart3>() == null )
			{

				var tfp = mf.transform;

				var mt	= mtInvParent * tfp.localToWorldMatrix;
				
				creator.addGeometory( mf.sharedMesh, ref mt );
				
				if( tfp.childCount == 0 ) Destroy( mf.gameObject );

			}
		}
		
		return creator.create();
		
	}
*/







	// 破壊判定 ------------------------------------------------------------------

	/// <summary>
	/// パーツの軸沿いボックスを管理する。
	/// </summary>
	public struct HitLocalBoundsUnit
	{
		
		/// <summary>ワールド座標上のメッシュ境界ボックス中心</summary>
		public Vector3	structureViewCenter	{ get; private set; }

		/// <summary>メッシュ境界ボックスの外接球半径</summary>
		public float	radius				{ get; private set; }
		
		/// <summary>メッシュ境界ボックスのコピー</summary>
		public Bounds	localBounds			{ get; private set; }
		
		
		public void set( Bounds bounds, ref Matrix4x4 mtInvParent, Transform tf )
		{
			
			localBounds = bounds;
			
			radius		= bounds.extents.magnitude;
			
			var mt		= mtInvParent * tf.localToWorldMatrix;
			
			structureViewCenter = mt.MultiplyPoint3x4( bounds.center );
			
		}

		/// <summary>
		/// 球と境界ボックスの接触を判定する。
		/// 判定はローカル座標で行う。そのため雛形式でも実体所持式でも同じ処理で可能。
		/// </summary>
		/// <param name="otherCenter">球の中心（ワールド座標）</param>
		/// <param name="otherRadius">球の半径（ワールド座標）</param>
		public bool check( Vector3 otherCenter, float otherRadius )
		{
			

			// 相手の球をローカル座標に変換

			var localOtherCenter = otherCenter - structureViewCenter;
			

			var hitLimit = otherRadius + radius;
			
			if( localOtherCenter.sqrMagnitude < hitLimit * hitLimit )	// 球 vs 球
			{
				
				return localBounds.SqrDistance( localOtherCenter ) < otherRadius * otherRadius;
				// 球とＡＡＢＢの接触判定。ここまでやらなくても（球vs球でやめても）いい気はする。
				
			}
			
			return false;
			
		}

	}
	
	
	/// <summary>
	/// 全体メッシュインデックス内における位置を三角形単位に直して保管する。
	/// </summary>
	public struct hitIndexInfoUnit
	{

		/// <summary>開始三角形位置</summary>
		public int	startTriangle	{ get; private set; }

		/// <summary>三角形数</summary>
		public int	triangleLength	{ get; private set; }
		
		
		/// <summary>
		/// インデックス位置を三角形位置として保管する。
		/// 先に三角形位置に直すのは、後に使用する場合のため（除算/乗算のコスト）。
		/// </summary>
		/// <param name="st">三角形開始位置</param>
		/// <param name="len">三角形数</param>
		public void set( int st, int len )
		{
			
			startTriangle	= st;
			
			triangleLength	= len;
			
		}

		public bool isEmpty
		{
			get { return triangleLength == 0; }
		}

	}



}






