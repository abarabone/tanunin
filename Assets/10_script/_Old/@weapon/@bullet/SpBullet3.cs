using UnityEngine;
using System.Collections;

public class SpBullet3 : _BulkedBullet3
{


	public float	damage;
	
	public float	fragmentationRate;	// 軟体へのダメージ補正（軟弾の場合は 1 以上）

	
	public float	bulletSize;

	public float	bulletSpeed;

	public float	bulletRange;


	public float	rangeRate = 1.0f;	// rangeFactor が飛距離に関わる度合

	public float	speedRate = 1.0f;	// rangeFactor がスピードに関わる度合



	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var bullet = emitForBulk( pos, rot, bulletSize * sizeFactor, act );

		bullet.barrelFactor = barrelFactor;

	}
	




	public override bool update( BulkedBulletUnit3 bullet )
	{
		
		bullet.time += GM.t.delta;


		var prepos = bullet.position;


		var speed = bulletSpeed * effectFactor( bullet.barrelFactor, speedRate );

		var dir = bullet.rotation * Vector3.forward;
		
		var d = speed * GM.t.delta;


		bullet.position += dir * d;



		var ray = new Ray( prepos, dir );
		
		RaycastHit	hit;

		var res = Physics.Raycast( ray, out hit, d, UserLayer.bulletHittable );

		if( res )
		{

			var	hitter = getHitter( hit.collider );
			
			if( hitter )
			{

				if( hitter.isThroughOwnHit( bullet.owner, speed * bullet.time ) )
				{
					
					var landing = hitter.landings ? hitter.landings.fragmentation : GM.defaultLandings.fragmentation;
					//var landing = hitter.landings.fragmentation;
					
					landing.emit( hit.point, hit.normal, 0.0f );
					
					
					var ds = new DamageSourceUnit( damage, 0.8f, fragmentationRate, 0.0f, 0.0f ); 

					hitter.shot( ref ds, dir * damage, ref hit, bullet.owner );
					
				}
			}
			else
			{
				//if( hit.rigidbody ) hit.rigidbody.AddForce( dir * damage, ForceMode.Impulse );
			}

			return true;

		}

		
		return bullet.time * speed >= bulletRange * effectFactor( bullet.barrelFactor, rangeRate );

	}




}
