using UnityEngine;
using System.Collections;
using System.Text;
using System;

public abstract class _PlayerShoot3 : _Shoot3
{


	protected _WaponInfomation	waponInfo;


	[SerializeField]
	public ShootSoundUnit	sounds;

	[SerializeField]
	public SiteReloadBar	eyeMarker;


	public Transform	tfHand;

	public Transform	tfMuzzle;



	// 初期化 --------------------------------------------------------

	public abstract _WaponInfomation createWaponInfo();

	public override void deepInit( _Action3 action )
	{

		base.deepInit( action );

		
		sounds.init( GetComponent<AudioSource>() );


		eyeMarker.init();


		waponInfo = createWaponInfo();

		waponInfo.buildGuiLocations( weapons );

	}


	protected override void initWapons()
	{

		weapons.acquireWaponsInChildren( this );
		//weapons.acquireWaponsInPrefabArrays( this );

	}


	public virtual void init()
	{


		var currentWapon = weapons.current;
		
		currentWapon.equip( this );

		waponInfo.renewalText( currentWapon, true );

	}
	



	
	// 運用 --------------------------------------------------------

	public void checkWapons()
	// 武器の使用・切り替えを行うために action から呼ばれる
	{

		var isExcuting = checkExcuting();

		var isChanged = checkChangeWapon();

		
		var currentWapon = weapons.current;
		
		var isToggleReload = eyeMarker.show( currentWapon, currentWapon.isReloading & isReloadable );

		waponInfo.renewalText( currentWapon, isExcuting | isChanged | isToggleReload );

		for( var i = 0; i < weapons.length; i++ )
		{
			waponInfo.showVisual( weapons[ i ], i, i == weapons.nowWapon );
		}

	}

	protected abstract bool checkExcuting();

	protected abstract bool checkChangeWapon();



	




	
	// ＧＵＩ --------------------------------------------------------

	public abstract class _WaponInfomation
	// 武器ＧＵＩ表示を行う
	{
		
		public GUIText	infomationText;
		
		StringBuilder	infoString;	// 名前１６文字 ＋ "\n" ＋ 情報１０文字（弾４桁 ＋ " / " ＋ 弾４桁 等）× ２ ＋ "\n" ： ４４？、余裕もって ６４


		protected Quaternion			visualRotation;

		protected WaponPositionUnit[]	visualPositions;

		protected struct WaponPositionUnit
		{
			public Vector3	now;
			public Vector3	hide;
			public Vector3	show;
			public Vector3	scale;
		}

		
		//public void init()
		public _WaponInfomation()
		{

			infoString = new StringBuilder( 64 );
			
			infomationText = GameObject.Find( "system/IFCamera/wapon info v3" ).GetComponent<GUIText>();
			
		}

		public void buildGuiLocations( WaponHolder wapons )
		{

			visualPositions = new WaponPositionUnit[ wapons.length ];

			for( var i = 0; i < wapons.length; i++ )
			{
				setVisualPosition( wapons, i );
			}

			visualRotation = Quaternion.Euler( new Vector3( 320.0f, 90.0f, 0.0f ) );

		}



		public void renewalText( _Weapon3 wapon, bool isRenewal )
		{

			if( isRenewal )
			{

				infoString.Length = 0;


				foreach( var unit in wapon.units )
				{

					unit.renewalInfoText( infoString );
					
					infoString.AppendLine();

				}

				infoString.Append( wapon.waponName );


				infomationText.text = infoString.ToString();

			}

		}

		public void showText( bool visibility )
		{

			infomationText.enabled = visibility;

		}



		
		public abstract void setVisualPosition( WaponHolder wapons, int id );
		// 武器それぞれの「表示・表示の位置」をあらかじめ計算して配列に保存しておく。
		

		public void showVisual( _Weapon3 wapon, int id, bool isToShow )
		// 指定の武器を、指定の視覚状態で表示する。の位置は各自配列に持っているので、そこに移動するだけ。
		{

			var goalpos = isToShow ? visualPositions[id].show : visualPositions[id].hide;

			var nowpos = visualPositions[id].now;


			var isGoaled = ( goalpos - nowpos ).sqrMagnitude < 0.05f * 0.05f;

			if( isToShow || !isGoaled )
			{

				if( !isGoaled )
				{

					visualPositions[id].now += ( goalpos - nowpos ) * 0.5f * GM.t.delta * 30.0f;

				}

				var mt = new Matrix4x4();

				mt.SetTRS( visualPositions[id].now, visualRotation, visualPositions[id].scale );


				Graphics.DrawMesh( wapon.mf.sharedMesh, mt, wapon.mr.material, UserLayer._userInterface, GM.cameras.iface );

			}

		}

		public void showVisual( _Weapon3 wapon )
		// 使用していないが、参考として残しておく。シンプルに表示するだけ。
		{

			var mt = new Matrix4x4();


			var scale = new Vector3( 1.0f, 1.0f, 0.55f );
			scale = Vector3.Scale( scale, wapon.tf.localScale );

			var x = ( GM.cameras.iface.aspect - 0.05f ) - wapon.mf.sharedMesh.bounds.max.z * scale.z * 0.7f;// 左端が -1.0f * aspect 右端が 1.0f * acpect
			var y = ( -1.0f + 0.05f ) - wapon.mf.sharedMesh.bounds.min.y * scale.y * 2.0f;// 上端が 1.0f 下端が -1.0f　最後の定数値は斜め表示の係数(0.7f と 2.0f)。
			var pos = new Vector3( x, y, 2.0f );

			var rot = Quaternion.Euler( new Vector3( 320.0f, 90.0f, 0.0f ) );

			mt.SetTRS( pos, rot, scale );


			Graphics.DrawMesh( wapon.mf.sharedMesh, mt, wapon.mr.material, UserLayer._userInterface, GM.cameras.iface );

		}

	}



	
	[ System.Serializable ]
	public struct SiteReloadBar
	{

		public Transform	tfSite;


		MeshRenderer	mr;

		public Material	matProgress;

		public Material	matSite;


		bool	isShowReloadingBar;


		public void init()
		{

			mr = tfSite.GetComponent<MeshRenderer>();

		}


		public bool show( _Weapon3 wapon, bool isReloading )
		{

			var toggle = isShowReloadingBar ^ isReloading;


			if( isReloading )
			{

				showReloadBar( wapon );

			}
			else
			{
				
				showSite();
				
			}

			return toggle;

		}

		void showReloadBar( _Weapon3 wapon )
		{

			//if( !isShowReloadingBar )
			{

				var	progress = wapon.reloadRemainingTime * wapon.maxReloadTimeR;
					
				tfSite.localScale = new Vector3( progress, 0.1f, 1.0f );
					
				mr.sharedMaterial = matProgress;


				isShowReloadingBar = true;

			}

		}

		void showSite()
		{

			if( isShowReloadingBar )
			{

				tfSite.localScale = new Vector3( 0.2f, 0.2f, 1.0f );
				
				mr.sharedMaterial = matSite;


				isShowReloadingBar = false;

			}

		}

		public void hide()
		{

			tfSite.localScale = Vector3.zero;

		}

	}

	


	
	// 音 --------------------------------------------------------
	
	[ System.Serializable ]
	public struct ShootSoundUnit
	{
		
		AudioSource	sound;
		
		
		public AudioClip	reload;
		
		public AudioClip	changeWapon;
		
		
		
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
		
		public void playChangeWapon()
		{
			
			sound.PlayOneShot( changeWapon );
			
		}
		
	}
	
	


}
