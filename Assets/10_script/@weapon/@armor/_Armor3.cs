using UnityEngine;
using System.Collections;


public abstract class _Armor3 : ScriptableObject
{

	public abstract _ArmorUnit instantiate();

}




// Recover	… 回復可能ダメージを修復
// Repair	… 永続ダメージを回復
// Refresh	… Repair が完了すること


public abstract class _ArmorUnit
{

	public abstract void init();

	public abstract bool update( _Action3 act );


	public abstract void applyDamage( ref _Bullet3.DamageSourceUnit ds, float takeTime, float recoveryTime = 0.0f );

	//public abstract void applyPenetratingDamage( ref _Bullet3.DamageSourceUnit ds, float takeTime, float recoveryTime = 0.0f );


	public abstract void showGui();



	protected struct DurabilityUnit
	{
		
		public float	durability;

		public float	normalizedDurability { get { return durability * maxDurabilityR; } }

		public float	recoverable;

		public float	normalizedRecoverable { get { return recoverable * maxDurabilityR; } }

		float	maxDurabilityR;


		public bool	isDestroyed	{ get; private set; }

		public bool isNowDamaging() { return damage.remainingTime > 0.0f; }


		DamageProgressUnit	damage;

		DamageProgressUnit	recovery;


		public DurabilityUnit( float max )
		{

			maxDurabilityR = 1.0f / max;


			durability = max;
			
			recoverable = max;
			
			damage = new DamageProgressUnit();
			
			recovery = new DamageProgressUnit();


			isDestroyed = false;

		}


		public void reset( float max )
		{

			isDestroyed = false;


			durability = max;
			
			recoverable = max;
			
			damage = new DamageProgressUnit();
			
			recovery = new DamageProgressUnit();

		}

		public void repair( float value, float takeTime )
		{

			isDestroyed = false;


			recoverable = value;

			recovery.applyLatest( value - durability, takeTime );

		}


		
		public void applyDamage( float newDamage, float takeTime, float recoverableRate, float recoveryTime )
		{
			
			_applyDamage( newDamage, takeTime, newDamage * recoverableRate, recoveryTime );
			
		}
		
		public void applyOverDamage( ref OverDamageUnit od )
		{
			
			_applyDamage( od.damage, od.damageRemaningTime, od.recoverable, od.recoverableRemaningTime );
			
		}

		void _applyDamage( float newDamage, float takeTime, float recoverableDamage, float recoveryTime )
		{
			
			if( takeTime > 0.0f )
			{
				damage.applyLatest( newDamage, takeTime );
			}
			else
			{
				durability -= newDamage;
			}
			
			
			if( recoveryTime > 0.0f )
			{
				recovery.applyOverall( recoverableDamage, recoveryTime );
			}
			else
			{
				durability += recoverableDamage;
			}

			recoverable -= newDamage - recoverableDamage;
			
		}

		public void applyDamage( float newDamage, float takeTime )
		{

			if( takeTime > 0.0f )
			{
				damage.applyLatest( newDamage, takeTime );
			}
			else
			{
				durability -= newDamage;
			}

			recoverable -= newDamage;

		}



		public void terminateRecovery()
		{

			recoverable = durability;

			recovery = new DamageProgressUnit();

		}

		public void terminateDamage()
		{

			damage = new DamageProgressUnit();

		}

		public OverDamageUnit desroy()
		{

			isDestroyed = true;


			var overDamage = new OverDamageUnit( ref this );

			durability = 0.0f;

			terminateDamage();

			terminateRecovery();

			return overDamage;

		}

		public bool update()
		{

			if( isDestroyed ) return false;

			
			var dt = GM.t.delta;
			
			if( damage.remainingTime > 0.0f )
			{
				
				var cdt = dt < damage.remainingTime ? dt : damage.remainingTime;
				
				durability -= damage.perSecound * cdt;
				
				damage.remainingTime -= cdt;
				
				if( damage.remainingTime <= 0.0f )
				{
					durability = Mathf.Round( durability );

					terminateDamage();
				}
				
			}
			else if( recovery.remainingTime > 0.0f )
			{
				
				var cdt = dt < recovery.remainingTime ? dt : recovery.remainingTime;
				
				durability += recovery.perSecound * cdt;
				
				recovery.remainingTime -= cdt;
				
				if( recovery.remainingTime <= 0.0f )
				{
					durability = recoverable;

					terminateRecovery();
				}
				
			}

			return durability < 0.0f;//durability <= 0.0f;

		}


		struct DamageProgressUnit
		{

			public float	perSecound;
			
			public float	remainingTime;


			public void applyOverall( float value, float takeTime )
			{
				var totalTimeR = 1.0f / ( remainingTime + takeTime );

				var prevRate = remainingTime * totalTimeR;
				var nextRate = takeTime * totalTimeR;

				var prevVps = perSecound * prevRate;
				var nextVps = ( value / takeTime ) * nextRate;

				perSecound = prevVps + nextVps;
				remainingTime = remainingTime + takeTime;
			}

			public void applyLatest( float value, float takeTime )
			{
				var prevValue = perSecound * remainingTime;
				var nextValue = value;

				perSecound = ( prevValue + nextValue ) / takeTime;
				remainingTime = takeTime;
			}

		}

		public struct OverDamageUnit
		{
			
			public float	damage;
			public float	damageRemaningTime;
			
			public float	recoverable;
			public float	recoverableRemaningTime;
			
			
			public OverDamageUnit( ref DurabilityUnit du )
			{

				damage = -du.durability + du.damage.perSecound * du.damage.remainingTime;
				
				damageRemaningTime = du.damage.remainingTime;
				
				recoverable = -du.recoverable < 0.0f ? 0.0f : -du.recoverable;
				
				recoverableRemaningTime = du.recovery.remainingTime;
				
			}
			
		}

	}

	
	protected struct DamageSourceInfo
	{
		public float	fragmentationRate;
		
		public DamageSourceInfo( ref _Bullet3.DamageSourceUnit ds )
		{
			fragmentationRate = ds.fragmentationRate;
		}
	}


	
	
	static protected class Gui
	{
		static public MaterialPropertyBlock	mpb = new MaterialPropertyBlock();
		
		static public int	idColor = Shader.PropertyToID( "_Color" );
		static public int	idTintColor = Shader.PropertyToID( "_TintColor" );
		
		static public Mesh	meshFullRound = createMesh( -0.5f, 0.5f );
		
		static public Mesh	meshHarfRound = createMesh( 0.0f, 0.5f );

		
		static Matrix4x4	mt = new Matrix4x4();


		static public Color	foreSafeColor	= new Color32( 214, 244, 213, 255 );
		static public Color	backSafeColor	= new Color32( 92, 207, 88, 255 );
		static public Color	backDamageColor	= new Color32( 84, 112, 197, 255 );
		static public Color	dangerColorOn	= new Color32( 255, 184, 107, 255 );
		static public Color	dangerColorOff	= new Color32( 150, 0, 0, 255 );
		static public Color	backDangerColor	= new Color32( 44, 44, 44, 255 );//95, 30, 10, 255 );

		static Mesh createMesh( float left, float right )
		{
			var vtxs = new Vector3[ 4 ];
			var uvs = new Vector2[ 4 ];
			
			vtxs[ 0 ]	= new Vector3( left,	0.5f, 1.0f );	// 左上
			vtxs[ 1 ]	= new Vector3( right, 0.5f, 1.0f );	// 右上
			vtxs[ 2 ]	= new Vector3( right,-0.5f, 1.0f );	// 右下
			vtxs[ 3 ]	= new Vector3( left, -0.5f, 1.0f );	// 左下
			
			uvs[ 0 ]	= Vector2.one * 0.5f + new Vector2( left, -0.5f );	// 左上
			uvs[ 1 ]	= Vector2.one * 0.5f + new Vector2( right,-0.5f );	// 右上
			uvs[ 2 ]	= Vector2.one * 0.5f + new Vector2( right, 0.5f );	// 右下
			uvs[ 3 ]	= Vector2.one * 0.5f + new Vector2( left,	0.5f );	// 左下
			
			var idxs = new int[ 3 * 2 ];
			var iidx = 0;
			
			idxs[ iidx++ ]	= 0;
			idxs[ iidx++ ]	= 1;
			idxs[ iidx++ ]	= 3;
			idxs[ iidx++ ]	= 3;
			idxs[ iidx++ ]	= 1;
			idxs[ iidx++ ]	= 2;
			
			var mesh = new Mesh();
			mesh.vertices = vtxs;
			mesh.uv = uvs;
			mesh.triangles = idxs;
			mesh.UploadMeshData( true );
			
			return mesh;
		}

		static public void draw( Vector3 pos, float size, Mesh mesh, Material mat, int id, Color color, float z )
		{
			mpb.Clear();

			mpb.SetColor( id, color );
			
			Gui.mt.SetTRS( pos, Quaternion.identity, new Vector3( size, size, z ) );

			Graphics.DrawMesh( mesh, mt, mat, UserLayer._userInterface, GM.cameras.iface, 0, mpb, false, false );
		}
	}
	


	
}












/*







public class SkinnedArmor3 : _Armor3
{
	
	public float	outerToughness;
	
	public float	innerToughness;

	

	public override void applyDamage( ref _Bullet3.DamageSourceUnit ds )
	{
		
		if( outerToughness > 0.0f )
		{
			
			outerToughness -= ds.damage;
			
			
			if( outerToughness < 0.0f )
			{
				innerToughness += outerToughness * ds.fragmentationRate;
				
				outerToughness = 0.0f;
			}
			
		}
		else
		{
			
			innerToughness -= ds.damage * ds.fragmentationRate;
			
		}
		
		
		
	}
	
	public override bool updateDamage()
	{
		
		
		
		return innerToughness < 0.0f;
	}

}
*/
