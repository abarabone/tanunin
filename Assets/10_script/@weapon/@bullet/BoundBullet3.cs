using UnityEngine;
using System.Collections;
/*
public class BoundBullet3 : _PooledBullet3
{
	
	
	public float	damage;
	
	public float	fragmentationRate;	// 軟体へのダメージ補正（硬弾の場合は 0 ～ 1）

	
	public float	bulletSize;

	public float	bulletSpeed;

	public float	bulletRange;
	
	




	
	public override bool update( ref PooledBulletUnit3 bullet )
	{

		var prepos = bullet.position;
		
		var dir = bullet.rotation * Vector3.forward;
		
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
				hitter.shot( damage, bulletSpeed, dir, hit.point, hit.triangleIndex, hit.collider, -1 );
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
		
		var bullet = emitWithPoolingBulletUnit( pos, rot, bulletSize, hitter );
		
		bullet.barrelFactor = barrelFactor;
		
	}

}
*/