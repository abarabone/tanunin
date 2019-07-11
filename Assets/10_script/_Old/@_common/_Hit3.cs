using UnityEngine;
using System.Collections;

public class _Hit3 : MonoBehaviour
{

	public enMaterial	armorClass;

	public LandingList	landings;




	// 初期化・更新 ----------------------------

	protected void Awake() 
	{}

	public virtual void init()
	{}

	public virtual void update()
	{}

	// --------------------



	public virtual _Hit3 getParentHitter()
	{// 爆発用の試験実装（本体とパーツに分かれているもの以外は null を返す）。
		return null;
	}
	
	public virtual bool isThroughOwnHit( _Action3 emissionOwnerAct, float emissionDistance )
	{
		return true;
	}

	public virtual _Action3 getAct()
	{
		return null;
	}

	public virtual Transform getRandomBone()
	{
		return transform;
	}

	/*public void rejectHook()
	{
		gameObject.layer = 0;// フック解除のためにレイヤーをゼロにしてるけど、これは混乱をまねくかもしれない…
	}
*/



	public virtual void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	{}

	public virtual void eroded( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{}

	public virtual void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, _Action3 attacker )
	{}

	public virtual void fired( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{}
	

	/*
	public virtual void shot( float damage, float pressure, Vector3 direction, Vector3 point, int hitIndex, Collider collider, int teamFlag )
	{}

	public virtual void blasted( float damage, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, int teamFlag )
	{}

	public virtual void fired(	float damage, Collider collider, int teamFlag )
	{}

	public virtual void eroded( float damage, float pressure, Vector3 direction, Collider collider, int teamFlag )
	{}
	*/



	public enum enMaterial : byte
	{

		liquid,
		glass,

		clay,
		sand,
		wood,
		
		concrete,
		exoskull,
		metal,
		heavyMetal,

		unknownMatter,
		unknonwTeqSkin,
		unknownTeqSkull,


		// 以下は比較用の閾表現

		soft = clay,
		hard = concrete,
		solidity = unknownMatter

	}



}
