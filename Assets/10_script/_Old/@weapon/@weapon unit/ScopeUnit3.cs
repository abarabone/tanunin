using UnityEngine;
using System.Collections;

public class ScopeUnit3 : _FunctionUnit3
{
	
	public AudioClip	zoomSound;


	public bool		isScopeMode	{ get; protected set; }

	public float	zoomRatio;

	float	zoomRatioR;



	public override void deepInit()
	{

		base.deepInit ();


		zoomRatioR = 1.0f / zoomRatio;

	}


	
	public override _Weapon3.enWaponState excute( _Shoot3 shoot, _Weapon3 wapon, _Weapon3.TriggerUnit trigger )
	{

		if( shoot.isScopeable & trigger.press )
		{

			wapon.sound.PlayOneShot( zoomSound );


			if( isScopeMode )
			{

				zoomOff( shoot );

			}
			else
			{

				zoomOn( shoot );

			}


			return _Weapon3.enWaponState.excuting;

		}
		else
		{

			var	progress = wapon.reloadRemainingTime * wapon.maxReloadTimeR;

			if( progress == 1.0f )
			{

				zoomOff( shoot );

			}


			return _Weapon3.enWaponState.ready;

		}

	}

	

	public override void leave( _Shoot3 shoot, _Weapon3 wapon )
	{

		zoomOff( shoot );

	}



	void zoomOn( _Shoot3 shoot )
	{
		if( !isScopeMode )
		{

			shoot.act.zooming( zoomRatioR, true );


			isScopeMode	= true;

			shoot.available -= ShootOpenFlag.reload;

		}
	}

	void zoomOff( _Shoot3 shoot )
	{
		if( isScopeMode )
		{

			shoot.act.zooming( 1.0f, false );


			isScopeMode	= false;

			shoot.available += ShootOpenFlag.reload;//　別の要因でリロード不可にできる場合はコード修正いるな、その要因フラグもみるとか

		}
	}





	public override void renewalInfoText( System.Text.StringBuilder infoString )
	{
		
		infoString.Append( "x" ).Append( isScopeMode ? zoomRatio.ToString() : "1" );
		
	}




}
