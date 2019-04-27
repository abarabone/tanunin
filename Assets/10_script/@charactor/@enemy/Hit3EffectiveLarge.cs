using UnityEngine;
using System.Collections;

public class Hit3EffectiveLarge : Hit3Effective
// 怪獣などの巨大な敵のパーツ処理
{



	const float	hitVelocityLimit = 5.0f;


	float	hitSoundLimitTime;


	// 衝突 -------------------------------------------
	
	protected void OnCollisionEnter( Collision c )
	{

		if( c.contacts.Length == 0 ) return;


		var lr = 1 << c.collider.gameObject.layer;

		if( ( lr & UserLayer.groundForEnemyLarge ) == 0 ) return;
		

		if( c.relativeVelocity.sqrMagnitude < hitVelocityLimit * hitVelocityLimit ) return;


		var pos = c.contacts[ 0 ].point;
		

		var radius = getRadius();

		var hitmask = UserLayer.solasHittable;//UserLayer.explosionHittable

		var hitForce = c.relativeVelocity.magnitude * 3.0f;


		var act = (SolasAction3)mainHitter.act;

		if( act.isDead )
		{

			if( Time.time > hitSoundLimitTime & c.collider is TerrainCollider )
			{
				// 死んでたら地形との衝突で煙もわもわ出す

				act.dustCloud.emit( pos, Vector3.up, 1.0f, Mathf.Max( radius * 0.2f, 0.8f ) );

			}


			hitmask = UserLayer.explosionHittable;


			hitSoundLimitTime = Time.time + 5.0f;

		}
		else
		{
			// 生きてる場合は足音だけ

			if( Time.time > hitSoundLimitTime ) act.dustCloud.playSound( pos );


			if( ( lr & ( UserLayer.bgEnvelope | UserLayer.bgSleepEnvelope ) ) != 0 )
			{
				// 建物に対しての破壊を行う

				hitTo( c.collider, pos, hitForce, 0.0f, radius * 3.0f );

			}


			hitSoundLimitTime = Time.time + 1.0f;

		}



		// 地面等のえぐれ

		hitAround( pos, hitForce, 0.1f, radius, hitmask );
		//hitAround( pos, mainHitter.act.rb.mass * 0.3f, rb.mass * ( 1.0f / 1000.0f ), radius, hitmask );
		//hitAround( pos, c.relativeVelocity.magnitude, rb.mass * ( 1.0f / 1000.0f ), radius, hitmask );

	}




	float getRadius()
	{

		var c = GetComponentInChildren<Collider>();

		if( c is SphereCollider )
		{

			var sp = (SphereCollider)c;

			return sp.radius;

		}
		else if( c is CapsuleCollider )
		{

			var cp = (CapsuleCollider)c;

			return cp.radius;

		}
		else
		{

			return c.bounds.size.magnitude;// *2.0f;// *0.4f;

		}

	}




	/// <summary>
	/// 特定コライダの被ヒット処理（爆発）を呼び出す。
	/// </summary>
	/// <param name="c"></param>
	/// <param name="pos"></param>
	/// <param name="impulsiveForce"></param>
	/// <param name="boringFactor"></param>
	/// <param name="radius"></param>
	void hitTo( Collider c, Vector3 pos, float impulsiveForce, float boringFactor, float radius )
	{

		var	otherHitter = _Bullet3.getHitter( c );


		if( otherHitter != null )
		{

			if( otherHitter.getAct() != mainHitter.act )
			{

				var ds = new _Bullet3.DamageSourceUnit( mainHitter.act.rb.mass, 1.0f, 1.0f, 1.0f, 0.0f );

				otherHitter.blasted( ref ds, impulsiveForce, boringFactor, pos, radius, c, null );

			}
		}
		else
		{



		}

	}


	/// <summary>
	/// 指定した範囲内にあるコライダの被ヒット処理（爆発）を呼び出す。
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="impulsiveForce"></param>
	/// <param name="boringFactor"></param>
	/// <param name="radius"></param>
	/// <param name="hitmask"></param>
	void hitAround( Vector3 pos, float impulsiveForce, float boringFactor, float radius, int hitmask )
	{

		var colliders = Physics.OverlapSphere( pos, radius, hitmask );

		foreach( var c in colliders )
		{

			hitTo( c, pos, impulsiveForce, boringFactor, radius );

		}

	}


}
