using UnityEngine;
using System.Collections;

public class BulletFireUnit3 : _FireUnit3
{
	




	public override _Weapon3.enWaponState excute( _Shoot3 shoot, _Weapon3 wapon, _Weapon3.TriggerUnit trigger )
	{

		if( wapon.reloadRemainingTime > 0.0f )
		{

			return _Weapon3.enWaponState.reloading;

		}
		else
		{

			if( bulletRemaining == 0 )
			{

				bulletRemaining = maxBullets;

			}


			var state = base.excute( shoot, wapon, trigger );


			if( ( state & _Weapon3.enWaponState.enterReload ) != 0 )
			{

				if( wapon.reloadRemainingTime == 0.0f )
				{
					wapon.maxReloadTimeR = 1.0f / reloadTime;
				}

				wapon.reloadRemainingTime += reloadTime;

			}


			return state;

		}

	}



}
