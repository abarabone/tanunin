using UnityEngine;
using System.Collections;

public class PlayerHit3OnRagdoll : _Hit3
{



	protected PlayerHit3	mainHitter;
	
	public Rigidbody rbBase { get; private set; }

	

	public override _Action3 getAct()
	{
		return mainHitter.act;
	}

	public override bool isThroughOwnHit( _Action3 emissionOwnerAct, float emissionDistance )
	{
		return mainHitter.isThroughOwnHit( emissionOwnerAct, emissionDistance );
	}



	public override void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	{
		
		mainHitter.shot( ref ds, force, ref hit, attacker );
		
	}

	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider_, _Action3 attacker )
	{
		
		mainHitter.blasted( ref ds, pressure, boringFactor, center, radius, collider_, attacker );
		
	}

	public override void fired( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{

		mainHitter.fired( ref ds, force, hitpos, collider, attacker );

	}

	public override void eroded( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{

		mainHitter.eroded( ref ds, force, hitpos, collider, attacker );

	}






	// 初期化 -------------------------------

	protected void Start()
	{

		rbBase = GetComponentInParent<RagdollSwitcher>().rbs[ 0 ];

		mainHitter = rbBase.GetComponentInParent<PlayerHit3>();

		//Debug.Log( rbBase +" "+ mainHitter );
	}



}
