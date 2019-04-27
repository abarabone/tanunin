using UnityEngine;
using System.Collections;

public abstract class _RigidBullet3 : _Bullet3
{


	protected Rigidbody	rb;

	protected Vector3	prePosition;




	//protected override void deepInit()
	protected void Awake()
	{

		base.Awake();


		rb = GetComponent<Rigidbody>();

	}

	public override void init()
	{

		base.init();


		rb.velocity = Vector3.zero;
		
		rb.angularVelocity = Vector3.zero;

		prePosition = rb.position;

	}


	

	

	




	
	
}
