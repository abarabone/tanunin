using UnityEngine;
using System.Collections;
using System;

public class EnemyShoot3 : _Shoot3
{


	//public ShootSoundUnitEnemy	sounds;


	// èâä˙âª --------------------------------------------------------
	
	protected override void initWapons()
	{

		weapons.getWaponsInChildren( this );
		//weapons.getWaponsInPrefabArrays( this );

	}





	// â^óp --------------------------------------------------------

	public bool isReady( int id, int subId )
	{

		var wapon = weapons[ id ];

		return !wapon.isReloading & wapon.units[ subId ].isReady;

	}

	public bool isReloadingNow( int id )
	{
		return weapons[ id ].isReloading;
	}



	public bool reload( int id )
	{
		return weapons[ id ].reload( this );
	}

	public bool shoot( int id, int subId )
	{

		var mainTrigger	= new _Weapon3.TriggerUnit( false, true, false );
		
		
		var isExcuting = weapons[ id ].excute( this, subId, mainTrigger );

		
		return isExcuting;

	}






	// âπ --------------------------------------------------------

	[System.Serializable ]
	public struct ShootSoundUnitEnemy
	{
		
		AudioSource	sound;
		
		
		public AudioClip	reload;
		

		
		
		public void init( AudioSource a )
		{
			
			sound = a;
			
			sound.clip = reload;
			
		}
		
		
		public void playReload( bool isReloading )
		{
			
			if( isReloading )
			{
				if( !sound.isPlaying )
				{
					sound.Play();
				}
			}
			else
			{
				if( sound.isPlaying )
				{
					sound.Stop();
				}
			}
			
		}
		
	}

}
