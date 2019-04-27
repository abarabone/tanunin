using UnityEngine;
using System.Collections;

public abstract class _BulkedBullet3 : _Bullet3
{

	//public BulletPool3	pool;
	
	
	public Color32	bulletColor;

	public Material	material;
	
	public Mesh		mesh;

	public bool		isBillborad;







	public abstract bool update( BulkedBulletUnit3 bullet );





	protected BulkedBulletUnit3 emitForBulk( Vector3 pos, Quaternion rot, float size, _Action3 act )
	{

		return GM.bulletPool.emit( this, pos, rot, size, act );

	}




	//public struct BulkedBulletUnit3
	public class BulkedBulletUnit3
	{
		
		public _Action3	owner;

		public _BulkedBullet3	updater;
		
		
		public Vector3		position;
		
		public Quaternion	rotation;
		
		public float		size;


		public float		time;

		public Quaternion	rotFaceDirection;// いらなくね？ビルボードに絞っていいんじゃない？

		public float		barrelFactor;

		public float		upAngle;	// 上方向を現す角度。

		public float		rotAngle;	// Ｚ軸回転を現す角度。


		//public PooledBulletUnit3( _BulkedBullet3 _bullet, Vector3 _pos, Quaternion _rot, float _size, _Action3 act )
		public void init( _BulkedBullet3 _bulletUpdater, Vector3 _pos, Quaternion _rot, float _size, _Action3 act )
		{
			
			owner = act;
			
			updater = _bulletUpdater;

			position = _pos;
			
			rotation = _rot;
			
			size = _size;

			rotFaceDirection = Quaternion.identity;

			time = 0.0f;

			barrelFactor = 1.0f;

			upAngle = 0.0f;

			rotAngle = 0.0f;

		}
		
	}




}
