using UnityEngine;
using System.Collections;

public class Hit3BodyPart : _Hit3
{

	protected Hit3EnemyCenter	mainHitter;
	
	public Transform	tf	{ get; private set; }

	public Rigidbody	rb	{ get; private set; }

	
	public float	damageRate = 1.0f;



	public override _Hit3 getParentHitter()
	{// 爆発用の試験実装
		return mainHitter;
	}

	public override _Action3 getAct()
	{
		return mainHitter.act;
	}

	/// <summary>
	/// 適当なボーン位置を返す。自分か、直の子のみ対象となる。
	/// </summary>
	public override Transform getRandomBone()
	{
		var i = UnityEngine.Random.Range( (int)0, tf.childCount );
		
		return i == 0 ? tf : tf.GetChild( i - 1 );
	}


	public override bool isThroughOwnHit( _Action3 emissionOwnerAct, float emissionDistance )
	{
		return mainHitter.isThroughOwnHit( emissionOwnerAct, emissionDistance );
	}
	
	
	
	public override void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	{

		ds.damage *= damageRate;

		mainHitter.shot( ref ds, force, ref hit, attacker );


		rb.AddForceAtPosition( force * damageRate, hit.point, ForceMode.Impulse );
		
	}
	
	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider_, _Action3 attacker )
	{

	//	ds.damage *= damageRate;

	//	mainHitter.blasted( ref ds, pressure, boringFactor, center, radius, GetComponent<Collider>(), attacker );


		rb.AddForceAtPosition( pressure * damageRate * 20.0f * 0.75f * ( ( rb.position - center ).normalized + Vector3.up * 0.25f ), center, ForceMode.Impulse );
		
	}










	// 初期化 -------------------------------

	new protected void Awake()
	{

		mainHitter = GetComponentInParent<Hit3EnemyCenter>();

		rb = GetComponent<Rigidbody>();

		tf = transform;

	}



}
