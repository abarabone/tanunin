using UnityEngine;
using System.Collections;

public abstract class _FireUnit3 : _FunctionUnit3
{


	public _Emitter3		bullet;


	public ParticleSystem	flash;	

	public AudioClip	fireSound;

	

	public int		maxBullets; 		// 最大弾数

	public float	reloadTime;			// リロード時間（チャージタイム・クールタイム）


	public float	emitAngle;			// 射出角度

	public float	lowAccuracy;		// 誤差角度

	public float	barralFactor;		// 射程距離にかかわる要素

	
	public float	nextFireTime;		// 次弾装填間隔
	
	public float	triggerRagTime;		// トリガを引いてからのラグタイム
	

	
	protected float	fireReadyTime;

	protected int	bulletRemaining;



	protected AudioSource	sound;




	void Awake()
	{

		deepInit();

	}


	public override void deepInit()
	{

		base.deepInit();


		sound = GetComponent<AudioSource>();

		sound.clip = fireSound;


		bulletRemaining = maxBullets;


		//bullet.init( this );

	}


	public override bool isReady
	{
		get { return Time.time > fireReadyTime; }
	}



	
	
	public override void ready( _Shoot3 shoot, _Weapon3 wapon )
	{

		var readyTime = Time.time + wapon.readyTime;

		fireReadyTime = readyTime > fireReadyTime ? readyTime : fireReadyTime;

	}


	public override _Weapon3.enWaponState excute( _Shoot3 shoot, _Weapon3 wapon, _Weapon3.TriggerUnit trigger )
	{

		if( !shoot.isShootable ) trigger = new _Weapon3.TriggerUnit();



		if( Time.time > fireReadyTime )
		{


			/*if( wapon.reloadRemainingTime > 0.0f )
			{

				return _Wapon3.enWaponState.reloading;
			
			}
			else */if( triggerRagTime > 0.0f && trigger.press )
			{

				fireReadyTime = Time.time + triggerRagTime;

			}
			else if( trigger.push )
			{


				fire( shoot, wapon );


				if( --bulletRemaining > 0 )
				{
					
					fireReadyTime = Time.time + nextFireTime;// - ( Time.time - fireReadyTime );


					return _Weapon3.enWaponState.excuting;

				}
				else
				{

					fireReadyTime = 0.0f;


					return _Weapon3.enWaponState.enterReload | _Weapon3.enWaponState.excuting;

				}

			}
			else
			{

				return _Weapon3.enWaponState.ready;

			}


		}


		return _Weapon3.enWaponState.busy;

	}


	protected virtual void fire( _Shoot3 shoot, _Weapon3 wapon )
	{
		
		var noiseAngle = Random.insideUnitCircle * lowAccuracy;
		
		var pos = wapon.tfMuzzle.position;
		
		var rot = wapon.tfMuzzle.rotation * Quaternion.Euler( -emitAngle + noiseAngle.x, noiseAngle.y, 0.0f );
		
		bullet.emit( pos, rot, barralFactor, 1.0f, 0, shoot.act, wapon.tf.childCount > 0 ? wapon.tf.GetChild(0) : wapon.tfMuzzle );
		
		sound.Play();

		if( flash != null ) flash.Emit( 1 );

	}








	public override void refresh()
	{
		base.refresh();

		bulletRemaining = maxBullets;
	}

	public override void discard()
	{
		base.discard();

		bulletRemaining = 0;
	}






	
	
	public override void renewalInfoText( System.Text.StringBuilder infoString )
	{
		
		infoString.Append( bulletRemaining.ToString() ).Append( " / " ).Append( maxBullets.ToString() );
		
	}



}
