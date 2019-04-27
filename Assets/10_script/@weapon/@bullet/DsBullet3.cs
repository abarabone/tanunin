using UnityEngine;
using System.Collections;
/*
public class DsBullet3 : _PooledBullet3
{


	public float	damage;
	
	public float	fragmentationRate;	// 軟体へのダメージ補正（硬弾の場合は 0 ～ 1）

	
	public float	bulletSwing;

	public float	bulletSize;
	
	public float	bulletSpeed;

	public float	bulletRange;
	
	
	public float	ricochetAngleLimit;	// 跳弾する角度

	public float	rangeErosionLimit;	// 流体化可能な距離（より近ければ発動）


	
	public override bool update( ref PooledBulletUnit3 bullet )
	{

		var prepos = bullet.position;
		
		var swing = Random.insideUnitCircle * bulletSwing * GM.t.delta;

		var dir = bullet.rotation * Vector3.forward + new Vector3( swing.x, swing.y, 0.0f );
		
		var d = bulletSpeed * GM.t.delta;


		bullet.position += dir * d;
		
		bullet.time += GM.t.delta;



		var ray = new Ray( prepos, dir );
		
		RaycastHit	hit;

		var res = Physics.Raycast( ray, out hit, d, UserLayer.bulletHittable );

		if( res )
		{
			
			var	hitter = getHitter( hit.collider );

			if( hitter )
			{
				
				if( hitter.isThroughOwnHit( bullet.owner, bulletSpeed * bullet.time ) )
				{

					var landing = hitter.landings ? hitter.landings.erosion : SystemManager.defaultLandings.erosion;
					//var landing = hitter.landings.erosion;
					
					landing.emit( hit.point, hit.normal );
					
					
					var ds = new DamageSourceUnit( damage, 0.5f, fragmentationRate, 0.0f, 0.0f ); 

					hitter.shot( ref ds, dir * damage, ref hit, owner.targetTeam );
				}
				
			}
			else
			{
				if( hit.rigidbody ) hit.rigidbody.AddForce( dir * bulletSpeed, ForceMode.Impulse );
			}

			
			return true;

		}

		
		return bullet.time * bulletSpeed >= bulletRange;

	}

	
	protected override void _emit( Vector3 pos, Quaternion rot, float barrelFactor, _Action3 act )
	{
		
		var bullet = emitWithPoolingBulletUnit( pos, rot, bulletSize, act );
		
		bullet.barrelFactor = barrelFactor;
		
	}

}
*/