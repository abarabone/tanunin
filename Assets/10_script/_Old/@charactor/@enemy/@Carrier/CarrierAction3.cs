using UnityEngine;
using System.Collections;

public class CarrierAction3 : _Action3Enemy
{

	_Structure3	structure;





	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();


		structure = GetComponent<_Structure3>();

	}




	public override void init()
	{

		base.init();


		state.init( stay );

		posture.fittingRate = 3.0f;

		
		rb.isKinematic = true;

		//rb.inertiaTensor = rb.inertiaTensor;
		//rb.inertiaTensorRotation = rb.inertiaTensorRotation;
	}




	new protected void Start()
	{

		structure.build();

		var contents = structure.GetComponentInChildren<_StructurePartContents>();

		contents.clean();

		structure.switchToNear();


		base.Start();

	}



	// 更新処理 ----------------------------------

	new protected void Update()
	{

		base.Update();

		rb.rotation *= Quaternion.AngleAxis( -10.0f * GM.t.delta, Vector3.up );
		tfBody.GetChild( 0 ).transform.rotation *= Quaternion.AngleAxis( 20.0f * GM.t.delta, Vector3.up );

	}

	new protected void FixedUpdate()
	{

		if( isDead ) return;


		//posture.fitting( rb );

		var moveRatio = classBasedSpeed * figure.scaleRate;

		var movedist = moveRatio * move.speedRate01 * GM.t.delta;

		rb.MovePosition( rb.position + posture.rot * Vector3.forward * movedist );

	}




	// モード変更 --------------------------------------
	
	public override void changeToWaitMode()
	{

		finder.target.clear();

		mode = finder.toMode();
		
		migration.setTimer( Random.value * 3.0f );

	}

	public override void changeToAttackMode( _Action3 attacker )
	{

		if( attacker == null ) return;

		if( attacker == this )
		{
			changeToAlertMode();

			return;
		}


		finder.target.toDecision( character, attacker );

		mode = finder.toMode(); // ここで追跡モードにしておかないと、被通知側も通知を出して収集がつかない
		
		migration.setTimer( Random.value * 3.0f );	// ちょっとの間だけ方向転換しない、遊び

	}

	public override void changeToAlertMode()
	{

		finder.target.toAlert( character );

		mode = finder.toMode(); // ここで追跡モードにしておかないと、被通知側も通知を出して収集がつかない

		migration.setTimer( Random.value * 3.0f );	// ちょっとの間だけ方向転換しない、遊び

	}


	public override void changeToDeadMode()
	{

		finder.target.clear();

		state.changeTo( dead );

	}





	// アクション ================================================================================

	// stay ----------------------------------------------------

	void stay( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{
	
			state.shiftTo( staying );

			state.setPhysics( physFree );
			posture.setCollision( null );



		};

		action.last = () =>
		{

		};

	}

	void staying()
	{
		
		mode = finder.toMode( finder.search( 0 ) );
		
		if( mode.isChaseEnemy )
		{
			state.changeTo( locomote );
		}


		//move.inc( 0.0f, 3.0f );

	}


	// locomote -----------------------------

	void locomote( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{

			state.shiftTo( locomoting );

			state.setPhysics( physFree );
			posture.setCollision( null );

		};

		action.last = () =>
		{

		};

	}

	void locomoting()
	{

		var preMode = mode;


		mode = finder.toMode( finder.search( 0 ) );

		
		migration.setTargetPoint( this, ref finder.target );


		lookAtTarget();


		//move.inc( 1.0f, 3.0f );

	}




	// dead -------------------------------------------------------

	void dead( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{

			state.shiftTo( deadingStay );


			state.setPhysics( physFree );
			shiftToPhysicalMode();
			posture.setCollision( null );


			//move.velocity = 0.0f;



			migration.setTimer( 30.0f );


			rb.constraints = RigidbodyConstraints.None;



			figure.moveCollider.enabled = false;

			rb.isKinematic = false;


			//switchEnvelopeDeadOrArive( UserLayer._enemyEnvelope, UserLayer._enemyEnvelopeDead );


			deadize();

		};

		action.last = () =>
		{


			rb.detectCollisions = true;

			//switchEnvelopeDeadOrArive( UserLayer._enemyEnvelopeDead, UserLayer._enemyEnvelope );

			rb.constraints = figure.rbDefaultConstraints;


			final();
			structure.destruct();

		};

	}

	void deadingStay()
	{

		if( migration.isLimitOver )
		{

			state.shiftTo( deadingFall );

			migration.setTimer( 3.0f );

			rb.detectCollisions = false;

		}

	}

	void deadingFall()
	{

		if( migration.isLimitOver )
		{

			state.changeToIdle();

		}

	}

}
