using UnityEngine;
using System.Collections;

public class DarogaAction : _Action3Enemy
{
	/*
	public MotionPlayer	motion;



	enum enMotion
	{
 		wait,
		walk
	}



	// 初期化 ------------------------------------------

	protected override void deepInit()
	{

		base.deepInit();
		base.combineMesh( true );

		motion.buildLite( this, typeof( DarogaAction ), typeof( enMotion ) );

	}

	public override void init()
	{

		base.init();


		state.init( stay );

		motion.init( 1 );


		posture.fittingRate = 3.0f;

		rb.isKinematic = true;
		foreach( var r in tfBody.GetComponentsInChildren<Rigidbody>() )
		{
			Destroy( r.GetComponent<Collider>() );
			Destroy( r.GetComponent<Joint>() );
			Destroy( r );//.isKinematic = true;
		}
	}



	// 更新処理 ----------------------------------

	new protected void Update()
	{

		base.Update();

		motion.update();

	}

	new protected void FixedUpdate()
	{

		rb.MoveRotation( Quaternion.Lerp( rb.rotation, posture.rot, Time.fixedDeltaTime * moveSpeed.get() * posture.fittingRate ) );

	}



	// アクション ================================================================================

	// stay ----------------------------------------------------
	
	void stay( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{

			state.shiftTo( staying );

			moveSpeed.set( 0.0f );


			releaseTime = 0.0f;


		};

		action.last = () =>
		{

		};

	}


	void staying()
	{




	}
	*/
}
