using UnityEngine;
using System.Collections;

public class BulletPool3 : MonoBehaviour
{

	
	_BulkedBullet3.BulkedBulletUnit3[]	bullets;
	
	public int	entityLength;

	MaterialPropertyBlock	mpb;

	int	colorId;


	

	public int	maxBullets = 200;
	
	public int	incBullets = 50;


	void Awake()
	{

		bullets = new _BulkedBullet3.BulkedBulletUnit3[ maxBullets ];
		
		entityLength = 0;

		mpb = new MaterialPropertyBlock();

		colorId = Shader.PropertyToID( "_TintColor" );

	}


	void Update()
	{

		//var campos = Camera.main.transform.position;
		var cam = GM.cameras.player;
		var camrot = cam.transform.rotation;


		for( var i = 0; i < entityLength; i++ )
		{

			var bs = bullets[ i ].updater;


			var isDestroyed = bs.update( bullets[ i ] );


			if( isDestroyed )
			{
				remove( i );
			}
			else
			{

				var pos = bullets[ i ].position;

				//var rot = bs.isBillborad ? Quaternion.LookRotation( bullets[ i ].position - campos ) : bullets[ i ].rotation;
				var rot = bs.isBillborad ? camrot * bullets[ i ].rotFaceDirection : bullets[ i ].rotFaceDirection;

				var mt = new Matrix4x4();

				mt.SetTRS( pos, rot, Vector3.one * bullets[ i ].size );

				mpb.Clear();
				mpb.SetColor( colorId, bullets[i].updater.bulletColor );

				Graphics.DrawMesh( bs.mesh, mt, bs.material, 0, cam, 0, mpb, false, false );
				//Graphics.DrawMesh( bs.mesh, pos, rot, bs.material, 0);//, cam );

			}

		}

	}






	
	public _BulkedBullet3.BulkedBulletUnit3 emit( _BulkedBullet3 bulletUpdater, Vector3 pos, Quaternion rot, float size, _Action3 act )
	{
		
		if( entityLength >= bullets.Length )
		{
			
			maxBullets = bullets.Length + incBullets;

			Debug.Log( "resize bullet pool : " + bullets.Length.ToString() + " => " + maxBullets.ToString() );

			System.Array.Resize( ref bullets, maxBullets );

		}
		
		
		var i = entityLength++;

		if( bullets[ i ] == null ) bullets[ i ] = new _BulkedBullet3.BulkedBulletUnit3();

		bullets[ i ].init( bulletUpdater, pos, rot, size, act );

		return bullets[ i ];

	}
	
	public void remove( int i )
	{
		
		var blank = bullets[ i ];


		var ilast = --entityLength;

		bullets[ i ] = bullets[ ilast ];


		bullets[ ilast ] = blank;	// 一度確保したヒープはとっておく
		
	}






}
