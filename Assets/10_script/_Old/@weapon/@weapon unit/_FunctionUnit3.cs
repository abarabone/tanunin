using UnityEngine;
using System.Collections;


public abstract class _FunctionUnit3 : MonoBehaviour
{



	void Awake()
	{

		deepInit();

	}



	public abstract _Weapon3.enWaponState excute( _Shoot3 shoot, _Weapon3 wapon, _Weapon3.TriggerUnit trigger );



	public virtual void refresh()	// �t���[�d��Ԃɂ���i�b��j
	{}

	public virtual void discard()	// �e�Ȃǂ����Ԃɂ���i�b��j
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
