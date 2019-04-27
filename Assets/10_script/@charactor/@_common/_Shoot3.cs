using UnityEngine;
using System.Collections;

public abstract class _Shoot3 : MonoBehaviour
{
	
	public _Action3	act	{ get; private set; }


	public WaponHolder	weapons;



	public BitFlags	available;


	public bool	isShootable		// 弾丸発射可能状態か
	{ get { return available.isOn( ShootOpenFlag.shoot ); } }
	
	public bool	isReloadable	// リロード可能か
	{ get { return available.isOn( ShootOpenFlag.reload ); } }

	public bool	isScopeable		// スコープ系使用可能か
	{ get { return available.isOn( ShootOpenFlag.scope ); } }







	// 初期化 --------------------------------------------------------

	public virtual void deepInit( _Action3 action )
	{

		act = action;//GetComponentInParent<_Action3>();

		initWapons();

	}

	protected abstract void initWapons();
	


	



	// 武器所持配列 --------------------------------------------------------
	
	[ System.Serializable ]
	public struct WaponHolder
	{
		
		public _Weapon3[]	weapons;
		
		public int	nowWapon	{ get; private set; }


		public void initMaxWaponLength( int num )
		{
			weapons = new _Weapon3[ num ];
		}


		public void change( int i )
		{
			nowWapon = i;
		}


		public void toggle()
		{
			nowWapon ^= 1;
		}

		public void next()
		{
			nowWapon = ++nowWapon % weapons.Length;
		}
		public void prev()
		{
			nowWapon = --nowWapon % weapons.Length;
		}



		public _Weapon3 this[ int i ]
		{
			get { return weapons[ i ]; }

			set { weapons[ i ] = value; }
		}

		public int length
		{
			get { return weapons.Length; }
		}

		public _Weapon3[] array
		{
			get { return weapons; }
		}


		public _Weapon3 current
		{
			get { return weapons[ nowWapon ]; }
		}



		// weapons 配列にプレハブを指定しておく方式 --------------------
		// これだと、まずるを指定できない…（プレイヤーはそれでもＯＫ）

		public void acquireWaponsInPrefabArrays( _PlayerShoot3 s )
		{
			
			for( var i = 0; i < weapons.Length; i++ )
			{
				var w = (_Weapon3)Instantiate( weapons[i] );

				w.acquire( s );
			}
			
		}
		
		public void getWaponsInPrefabArrays( EnemyShoot3 s )
		{

			for( var i = 0; i < weapons.Length; i++ )
			{
				var w = (_Weapon3)Instantiate( weapons[i] );

				w.transform.parent = s.transform;

				w.GetComponent<Renderer>().enabled = false;
			}
			
		}


		// weapon 階層以下に実体を所持する方式 -------------------------
		// 　これだと、プレハブが反映されないのが困る

		public void acquireWaponsInChildren( _PlayerShoot3 s )
		{
			
			weapons = s.GetComponentsInChildren<_Weapon3>();

			foreach( var w in weapons )
			{
				w.acquire( s );
			}
			
		}

		public void getWaponsInChildren( EnemyShoot3 s )
		{
			
			weapons = s.GetComponentsInChildren<_Weapon3>();

			foreach( var w in weapons )
			{
				w.GetComponent<Renderer>().enabled = false;
			}

		}

		public void releaseBackToChildren( _Shoot3 s )
		{

			var tf = s.transform;
			
			var pshoot = s as _PlayerShoot3;

			foreach( var w in weapons )
			{
				if( pshoot ) w.unequip( pshoot );

				w.GetComponent<Renderer>().enabled = false;

				w.tf.parent = tf;
			}

			weapons = null;

		}

	}



}



// 定義 --------------------------------------------------------

public class ShootOpenFlag
{
	public const int	notable		= 0;
	public const int	shoot		= 1 << 0;
	public const int	reload		= 1 << 1;
	public const int	scope		= 1 << 2;

	public const int	enemyReady	= shoot | reload;

	public const int	playerReady		= shoot | reload | scope;
	public const int	playerNoShoot	= reload | scope;
	
	public const int	shootAndReload	= shoot | reload;
}




