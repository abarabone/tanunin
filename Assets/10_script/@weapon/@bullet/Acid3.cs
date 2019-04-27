using UnityEngine;
using System.Collections;

public class Acid3 : _BulkedBullet3
{
	
	
	public float	damage;
	
	
	public float	moveStoppingDamage;
	
	public float	moveStoppingRate;	// 目標が移動していた場合に、威力から停止力へ変換される率（0 ～ 1）

	
	public float	bulletSize;

	public float	bulletSpeed;

	public float	lifeTime;


	public float	speedRate = 1.0f;



	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var bullet = this.emitForBulk( pos, rot, bulletSize, act );

		bullet.barrelFactor = barrelFactor;


		var rollAngle = Random.value * 360.0f - 180.0f;

		bullet.rotFaceDirection = Quaternion.Euler( 0.0f, 180.0f, rollAngle );

	}




	public override bool update( BulkedBulletUnit3 bullet )
	{
		
		bullet.time += GM.t.delta;

		bullet.size = bulletSize + bullet.time * 3.0f;


		//var prepos = bullet.position;
		
		var dir = bullet.rotation * Vector3.forward;

		var speed = ( bulletSpeed * effectFactor( bullet.barrelFactor, speedRate ) ) - bullet.size * bullet.time;// * 0.5f;
		
		var d = speed * GM.t.delta;
		

		var force = dir * d + Physics.gravity * ( GM.t.delta * bullet.time );

		bullet.position += force;

		
		if( Physics.CheckSphere( bullet.position, bullet.size * 0.5f * 0.3f, UserLayer.acidHittableBg ) )
		{
			// 背景に触ったらとにかく消える

			var landing = GM.defaultLandings.erode;
			
			landing.emit( bullet.position, Quaternion.identity );

			return true;

		}
		else
		{
			// もし毎フレームやる負荷が高かったら、接触してから一定時間は接触処理スキップ、としてみるといい
			var cols = Physics.OverlapSphere( bullet.position, bullet.size * 0.5f * 0.8f, UserLayer.acidHittableCh );

			foreach( var c in cols )
			{
				
				var	hitter = getHitter( c );
				
				if( hitter )
				{

					if( hitter.isThroughOwnHit( bullet.owner, speed * bullet.time ) )
					{
						
						var hitpos = bullet.position + ( c.transform.position - bullet.position ) * 0.8f;


						if( Time.time > bullet.barrelFactor + 0.75f )
						{
							var landing = hitter.landings.erode;
							
							landing.emit( hitpos, Quaternion.identity );

							bullet.barrelFactor = Time.time;
						}
						
						var ds = new DamageSourceUnit( damage * GM.t.delta, 0.5f, 1.0f, moveStoppingDamage * GM.t.delta, moveStoppingRate ); 
						
						hitter.eroded( ref ds, force, hitpos, c, bullet.owner );
						
					}

				}
				else
				{
					//if( hit.rigidbody ) hit.rigidbody.AddForce( dir * bulletSpeed, ForceMode.Impulse );
				}

			}

		}


		return speed <= 0.0f | bullet.time >= lifeTime;
		
	}

}
