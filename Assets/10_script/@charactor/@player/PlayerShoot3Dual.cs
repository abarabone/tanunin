using UnityEngine;
using System.Collections;
using System;

public class PlayerShoot3Dual : _PlayerShoot3
{


	// 初期化 --------------------------------------------------------

	public override _WaponInfomation createWaponInfo()
	{
		return new WaponInfomationDual();
	}
	




	// 運用 --------------------------------------------------------

	protected override bool checkExcuting()
	{
		
		var mainTrigger	= new _Weapon3.TriggerUnit( GamePad._r1, GamePad.r1, GamePad.r1_ );

		var subTrigger	= new _Weapon3.TriggerUnit( GamePad._l2, GamePad.l2, GamePad.l2_ );
			
			
		var isExcuting = weapons.current.excute( this, mainTrigger, subTrigger );

		sounds.playReload( weapons.current.isReloading & isReloadable );


		return isExcuting;

	}
	
	
	protected override bool checkChangeWapon()
	{
		
		if( GamePad._r2 )
		{
			
			weapons.current.unequip( this );

			weapons.toggle();
			
			weapons.current.equip( this );

			sounds.playChangeWapon();


			return true;
		}


		return false;

	}





	// ＧＵＩ --------------------------------------------------------

	public class WaponInfomationDual : _WaponInfomation
	{

		public override void setVisualPosition( WaponHolder wapons, int id )
		// 武器それぞれの「表示・表示の位置」をあらかじめ計算して配列に保存しておく。
		{

			if( id > 2 ) return; 


			var wapon = wapons[ id ];
			
			
			var aspect = GM.cameras.iface.aspect;


			var scale = new Vector3( 1.0f, 1.0f, 0.55f );

			scale = Vector3.Scale( scale, wapon.tf.localScale );
			
			
			var showx = ( aspect - 0.05f ) - wapon.mf.sharedMesh.bounds.max.z * scale.z * 0.7f;
			
			var showy = ( -1.0f + 0.05f ) - wapon.mf.sharedMesh.bounds.min.y * scale.y * 2.0f;
			
			var show = new Vector3( showx, showy, 1.5f );
			
			
			var hide = new Vector3( 0.0f, 0.0f, 2.0f );
			
			
			switch( id )
			{
					
				case 0:
				{
					
					hide.x = ( aspect + 0.05f ) - wapon.mf.sharedMesh.bounds.min.z * scale.z * 0.7f;
					
					hide.y = showy;
					
					break;
				}
					
				case 1:
				{
					
					hide.x = showx;
					
					hide.y = ( -1.0f - 0.05f ) - wapon.mf.sharedMesh.bounds.max.y * scale.y * 2.0f;
					
					break;
				}
					
			}
			
			
			visualPositions[ id ].show = show;
			
			visualPositions[ id ].hide = hide;
			
			visualPositions[ id ].now = hide;

			visualPositions[ id ].scale = scale;

		}

	}





}
