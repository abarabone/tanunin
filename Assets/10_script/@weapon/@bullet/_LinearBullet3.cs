using UnityEngine;
using System.Collections;

public abstract class _LinearBullet3 : _Bullet3
{
	
	
	public float	bulletSpeed;
	
	public float	bulletRange;
	
	public float	bulletSwing;


	public float	rangeRate = 1.0f;	// rangeFactor が飛距離に関わる度合

	public float	speedRate = 1.0f;	// rangeFactor がスピードに関わる度合
	
	
	protected float	movedDistance;





	public override void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var bullet = (_LinearBullet3)this.instantiateWithPoolingGameObject( pos, rot, true );

		bullet.init();

		bullet.owner = act;


		bullet.bulletSpeed *= effectFactor( rangeFactor, rangeRate );

		bullet.bulletRange *= effectFactor( rangeFactor, speedRate );

		if( sizeFactor != 1.0f ) bullet.tf.localScale = Vector3.one * sizeFactor;

	}





	protected abstract void onHit( _Hit3 hitter, ref RaycastHit hit, Vector3 dir );
	// 継承先でヒットした後の処理をここに書く。



	public override void init()
	{

		base.init();


		movedDistance = 0.0f;

	}



	
	void Update()
	{
		
		var prepos = tf.position;
		
		
		var dir = tf.forward;
		
		var d = bulletSpeed * GM.t.delta;
		
		Vector3 swing = Random.insideUnitCircle * bulletSwing * GM.t.delta;
		
		
		var	nowpos = prepos + dir * d + swing;
		
		
		
		RaycastHit	hit;
		
		var res = Physics.Linecast( prepos, nowpos, out hit, UserLayer.bulletHittable );
		
		if( res )
		{
			
			var	hitter = getHitter( hit.collider );
			
			if( hitter )
			{
				
				if( hitter.isThroughOwnHit( owner, movedDistance ) )
				{

					onHit( hitter, ref hit, dir );

					
					final();

					return;

				}
				
			}
			
		}


		movedDistance += d;
		
		if( movedDistance > bulletRange )
		{

			final();
			
			return;

		}


		tf.position = nowpos;

	}


}

