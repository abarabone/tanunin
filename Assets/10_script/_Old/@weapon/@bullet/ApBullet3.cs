using UnityEngine;
using System.Collections;
/*
public class ApBullet3 : _PooledBullet3
{
	
	
	
	public float	damage;
	
	public float	fragmentationRate;	// 軟体へのダメージ補正（硬弾の場合は 0 ～ 1）

	
	public float	bulletSize;

	public float	bulletSpeed;

	public float	bulletRange;

	
	public float	damping;			// 秒間減衰加速度

	public float	ricochetAngleLimit;	// 跳弾する角度


	
	public override bool update( ref PooledBulletUnit3 bullet )
	{

		
		var prepos = bullet.position;
		
		var dir = bullet.rotation * Vector3.forward;

		var dampingSpeed = bullet.time * damping;

		var d = ( bulletSpeed - dampingSpeed ) * GM.t.delta;

		
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
				hitter.shot( damage, bulletSpeed, dir, hit.point, hit.triangleIndex, hit.collider, -1 );
			}
			else
			{
				if( hit.rigidbody ) hit.rigidbody.AddForce( dir * bulletSpeed, ForceMode.Impulse );
			}
			
			
			return true;
			
		}

		
		return bullet.time * bulletSpeed >= bulletRange | d <= 0.0f;

	}

	
	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float radius, int numCount, _Action3 act )
	{
		
		var bullet = emitWithPoolingBulletUnit( pos, rot, bulletSize, act );
		
		bullet.barrelFactor = barrelFactor;
		
	}

}
*/