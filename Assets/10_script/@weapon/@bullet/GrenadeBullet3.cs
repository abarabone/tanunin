using UnityEngine;
using System.Collections;

public class GrenadeBullet3 : _RigidBullet3
{


	public _Emitter3	subEmitObject;



	public float	bulletSpeed;

	public float	lifeTime = 5.0f;


	public float	speedRate = 1.0f;

	public float	lifeTimeRate = 1.0f;	// インスタンスではスタートタイムとしても使う（ややこしい）




	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var bullet = (GrenadeBullet3)this.instantiateWithPoolingGameObject( pos, rot, true );

		bullet.init();

		bullet.owner = act;

		bullet.lifeTime = Time.time + lifeTime * effectFactor( barrelFactor, lifeTimeRate );


		bullet.bulletSpeed *= effectFactor( barrelFactor, speedRate );

		bullet.lifeTimeRate = Time.time;

		var force = rot * Vector3.forward * bullet.bulletSpeed;// + _owner.rb.GetPointVelocity( pos ) * 10.0f;

		bullet.rb.AddForce( force, ForceMode.VelocityChange );

	}



	
	void FixedUpdate()
	{

		//var swing = Random.insideUnitCircle * bulletSwing * GM.t.delta;

		//var dir = rb.rotation * Vector3.forward;// + new Vector3( swing.x, swing.y, 0.0f );

		var	nowpos = rb.position;



		RaycastHit	hit;

		var res = Physics.Linecast( prePosition, nowpos, out hit, UserLayer.bulletHittable );

		if( res )
		{

			var	hitter = getHitter( hit.collider );
			
			if( hitter )
			{

				if( hitter.isThroughOwnHit( owner, bulletSpeed * (Time.time - lifeTimeRate) ) )
				{
					
					subEmitObject.emit( hit.point, hit.normal, owner );
					

					final();

				}

			}

		}
		else
		{

			prePosition = nowpos;


			if( Time.time > lifeTime ) final();

		}

	}


}
