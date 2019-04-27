using UnityEngine;
using System.Collections;

public class FlameBullet3 : _BulkedBullet3
{


	public float	maxDamage;

	public float	minDamageRate;	// 最終的に何割のダメージに減衰するか

	public float	moveStoppingDamage;

	public float	minSize;

	public float	maxSize;

	public float	lifeTime;

	public float	linearWeight;	// 線型に近づく割合。1.0f なら均等の速度で飛ぶ。

	public float	wide;	// 拡散性。最終的にその距離広がる。


	

	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var bullet = emitForBulk( pos, rot, minSize, act );

		bullet.barrelFactor = barrelFactor;


		bullet.upAngle = Random.Range( 0.0f, 360.0f );//


		var rollAngle = Random.value * 360.0f - 180.0f;//

		bullet.rotFaceDirection = Quaternion.Euler( 0.0f, 180.0f, rollAngle );//

	}
	


	public override bool update( BulkedBulletUnit3 bullet )
	{

		bullet.time += GM.t.delta;


		var prepos = bullet.position;


		var adtt = ( bullet.time * bullet.time ) / ( lifeTime * lifeTime );
		// 終盤に加速して増える感じのレート

		var dtt = 1.0f - adtt;
		// 終盤に加速して減る感じのレート


		bullet.size = maxSize - ( maxSize - minSize ) * dtt;


		var speed = ( dtt * bullet.barrelFactor ) * ( 1.0f - linearWeight ) + ( bullet.time * bullet.barrelFactor ) * linearWeight;

		var dir = bullet.rotation * Vector3.forward;

		var d = speed * GM.t.delta;


		var upAngle = bullet.upAngle;// Random.Range( 0.0f, 360.0f );

		//var up = bullet.rotation * Quaternion.AngleAxis( upAngle, Vector3.forward ) * Vector3.up;
		var up = bullet.rotation * bullet.rotFaceDirection * Vector3.up;

		var diflect = up * ( wide * adtt * GM.t.delta );

		
		var move = dir * d + diflect;

		bullet.position += move;



		var res = new RaycastHit();

		var isBgHit = Physics.Linecast( prepos, bullet.position, out res, UserLayer.fireHittableBg );

		if( isBgHit )
		{

			var w = move - Vector3.Dot( move, res.normal ) * res.normal;

			bullet.position = prepos + w;

			bullet.rotation = Quaternion.LookRotation( w );

		}
		else
		{

			var cs = Physics.OverlapSphere( bullet.position, bullet.size * 0.5f, UserLayer.fireHittableCh );

			foreach( var c in cs )
			{

				var	hitter = getHitter( c );

				if( hitter )
				{

					if( hitter.isThroughOwnHit( bullet.owner, bullet.barrelFactor * bullet.time ) )
					{

						var damage = maxDamage * minDamageRate + maxDamage * dtt * ( 1.0f - minDamageRate );

						var ds = new DamageSourceUnit( damage * GM.t.delta, 0.0f, 2.0f, moveStoppingDamage * GM.t.delta, 0.0f );

						var force = dir * ( moveStoppingDamage * 1000.0f * dtt * GM.t.delta );

						hitter.fired( ref ds, force, bullet.position, c, bullet.owner );

					}
				}
				else
				{
					//if( hit.rigidbody ) hit.rigidbody.AddForce( dir * damage, ForceMode.Impulse );
				}

			}

		}


		return bullet.time > lifeTime;

	}



}
