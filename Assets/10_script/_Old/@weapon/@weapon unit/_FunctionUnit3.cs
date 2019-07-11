using UnityEngine;
using System.Collections;


public abstract class _FunctionUnit3 : MonoBehaviour
{



	void Awake()
	{

		deepInit();

	}



	public abstract _Weapon3.enWaponState excute( _Shoot3 shoot, _Weapon3 wapon, _Weapon3.TriggerUnit trigger );



	public virtual void refresh()	// フル充電状態にする（暫定）
	{}

	public virtual void discard()	// 弾などを空状態にする（暫定）
	{}



	public virtual void deepInit()
	{

	}

	
	public virtual void ready( _Shoot3 shoot, _Weapon3 wapon )
	{

	}

	public virtual void leave( _Shoot3 shoot, _Weapon3 wapon )
	{

	}


	public virtual bool isReady
	{
		get { return true; }
	}



	public abstract void renewalInfoText( System.Text.StringBuilder infoString );



}
