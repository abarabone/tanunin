using UnityEngine;
using System.Collections;

public abstract class _Weapon3 : MonoBehaviour
{
	

	public Transform	tf	{ get; private set; }

	public MeshRenderer	mr	{ get; private set; }
	
	public MeshFilter	mf	{ get; private set; }
	
	public AudioSource	sound	{ get; private set; }
	

	public Transform	tfMuzzle;


	
	public string	waponName;



	public _FunctionUnit3[]	units	{ get; protected set; }
	
	
	public float	weight;		// 武器重量
	
	public float	readyTime;	// 武器装備してから発射できるように構えるまでの時間


	public float	reloadRemainingTime		{ set; get; }

	public float	maxReloadTimeR			{ set; get; }

	public bool		isReloading	{ get{ return reloadRemainingTime > 0.0f; } }
	




	
	// 初期化 --------------------------------------------------------

	void Awake()
	{

		deepInit();

	}
	
	public virtual void deepInit()
	{
		if( tf != null ) return;//ほんとうはいらないが、プレイヤーの場合 Awake() の中で aquire() を呼んでしまっている…

		tf = transform;

		mr = GetComponent<MeshRenderer>();

		mf = GetComponent<MeshFilter>();

		sound = GetComponent<AudioSource>();


		build();

	}

	public virtual void build()
	// 武器を再構築するなど、武器の機能の更新が必要な時に呼ぶ。
	{
 
		units = GetComponents<_FunctionUnit3>();


		reloadRemainingTime = 0.0f;

	}



	
	public virtual void acquire( _PlayerShoot3 shoot )
	// 「プレイヤーが」武器を取得した時に呼ばれる
	{
		if( tf == null ) deepInit();//ほんとうはいらないが、プレイヤーの場合 Awake() の中で aquire() を呼んでしまっている…

		tf.parent = shoot.tfHand;
			
		tf.localPosition = Vector3.zero;
			
		tf.localRotation = Quaternion.identity;


		tfMuzzle = shoot.tfMuzzle;


		if( mr != null ) mr.enabled = false;

	}

	public virtual void discard()
	// 武器を放棄した時に呼ばれる（誰でも）
	{

		tf.parent = null;



		if( mr != null ) mr.enabled = true;

	}



	
	// 切り替え --------------------------------------------------------
	
	public void equip( _PlayerShoot3 shoot )
	// 「プレイヤーが」武器チェンジして手に持った時に呼ばれる
	{
		
		foreach( var unit in units )
		{
			unit.ready( shoot, this );
		}
		
		mr.enabled = true;
		
	}
	
	public void unequip( _PlayerShoot3 shoot )
	// 「プレイヤーが」武器チェンジで裏手にまわる時に呼ばれる
	{
		
		foreach( var unit in units )
		{
			unit.leave( shoot, this );
		}
		
		mr.enabled = false;
		
	}
	

	// リロード --------------------------------------------------------

	public bool reload( _Shoot3 shoot )
	{

		if( shoot.isReloadable & isReloading )
		{
			
			reloadRemainingTime -= GM.t.delta;
			
			if( reloadRemainingTime < 0.0f ) reloadRemainingTime = 0.0f;
			
		}

		return isReloading;

	}

	public void forceReload()
	{
 
		// かなりてきとうなので後でちゃんと調べて書いて

		reloadRemainingTime = 0.0f;

		foreach( var unit in units )
		{
			
			var fu = unit as _FireUnit3;

			if( fu == null ) continue;

			reloadRemainingTime += fu.reloadTime;


			unit.discard();

		}

	}


	// 実行 --------------------------------------------------------

	public virtual bool excute( _Shoot3 shoot, TriggerUnit mainTrigger, TriggerUnit subTrigger )
	{

		reload( shoot );


		// リロードの必要ないユニットの場合もあるので、リロード中でも一応通す。

		var statesMain	= units.Length > 0 ? units[ 0 ].excute( shoot, this, mainTrigger ): enWaponState.ready;

		var statesSub	= units.Length > 1 ? units[ 1 ].excute( shoot, this, subTrigger ) : enWaponState.ready;


		var isExcuting = ( ( (int)statesMain | (int)statesSub ) & (int)enWaponState.excuting ) != 0;

		return isExcuting;

	}

	public virtual bool excute( _Shoot3 shoot, int subId, TriggerUnit trigger )
	{

		reload( shoot );
		
		
		// リロードの必要ないユニットの場合もあるので、リロード中でも一応通す。

		var statesMain	= units.Length > subId ? units[ subId ].excute( shoot, this, trigger ): enWaponState.ready;

		
		var isExcuting = ( (int)statesMain & (int)enWaponState.excuting ) != 0;
		
		return isExcuting;
		
	}




	
	
	// 定義 --------------------------------------------------------

	public struct TriggerUnit
	{
		public bool	press;
		
		public bool	push;
		
		public bool	pull;
		
		public TriggerUnit( bool _t, bool t, bool t_ )
		{
			press	= _t;
			
			push	= t;
			
			pull	= t_;
		}
	}

	
	
	
	public enum enWaponState
	{
		
		ready		= 0,
		
		busy		= 1 << 1,
		
		excuting	= 1 << 2,
		
		enterReload	= 1 << 3,
		
		reloading	= 1 << 4
		
	}
	

	

	// ユーティリティ --------------------------------

	abstract public void muzzleLookAt( Vector3 line );
	// 敵との距離等から、放物線等の射出角度を計算し、tfMuzzle をその方向に向かせる。


}
