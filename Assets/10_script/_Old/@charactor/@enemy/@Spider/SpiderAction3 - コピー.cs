using UnityEngine;
using System.Collections;

public class cSpiderAction3 : _Action3Enemy
{



	protected AnimationState	msStand;

	protected AnimationState	msWalk;

	protected AnimationState	msWalkAnger;
	
	protected AnimationState	msJumpStart;
	
	protected AnimationState	msJumpUp;

	protected AnimationState	msJumpDown;

	protected AnimationState	msAttack;

	protected AnimationState	msDamage;

	protected AnimationState	msDead;



	//MotionCrossFader	motion;
	MotionFader		motion;

	
	
	public float attackTiming	= 0.06f;	// 0.00f ～ 0.07f の範囲で遅延可能、回避のしやすいタイミングで！
	
	protected float	ratedAttackTiming;



	#if Dummy
	// 初期化 ------------------------------------------

	protected override void deepInit()
	{

		base.deepInit();



		var anim = GetComponentInChildren<Animation>();


		
		msStand = anim["stand"];
		
		msWalk = anim["walk"];

		msWalkAnger = anim["angwalk"];

		msJumpStart = anim["jumpstart"];

		msJumpUp = anim["jumpup"];

		msJumpDown = anim["jumpdown"];
		
		msAttack = anim["attack"];
		
		msDamage = anim["ang"];

		msDead = anim["dead"];
		//msDead.speed = 0.0f;



	}


	public override void init()
	{

		base.init();
		

		GetComponent<AudioSource>().pitch = 0.5f * figure.scaleRateR;

		shoot.weapons[0].GetComponent<AudioSource>().pitch = 1.5f * figure.scaleRateR;


		state.init( stay );


		posture.fittingRate = 3.0f;


		ratedAttackTiming = attackTiming / def.quickness * figure.scaleRateR;

	}





	// 更新処理 ----------------------------------

	new protected void Update()
	{

		shoot.reload( 0 );//暫定

		motion.update();

		base.Update();


	}







	// モード変更 ----------------------------------------
	
	public override void changeToWaitMode()
	{
		
		battleModeOff();
		
		finder.forceUntarget();
		
	}
	
	public override void changeToAttackMode( _Action3 attacker )
	{
		
		battleModeOn();
		
		finder.forceTarget( attacker );
		
	}
	
	public override void changeToAlertMode()
	{
		
		battleModeOn();
		
	}
	
	public override void changeToDamageMode( _Action3 attacker, float time )
	{

		battleModeOn();
		
		
		if( figure.scaleRate >= 3.0f )
		{

		}
		else
		{
			
			state.changeTo( damage );
			
			releaseTime = Time.time + time;

		}
		
	}
	
	public override void changeToBlowingMode( _Action3 attacker, float time, int level )
	{
		
		battleModeOn();
		
		
		if( figure.scaleRate >= 3.0f )
		{
			
		}
		else
		{
			
			state.setPhysics( physFree );
			
			state.changeTo( blow );
			
			releaseTime = Time.time + time;
			
		}

	}
	
	public override void changeToDeadMode()
	{
		
		battleModeOff();
		
		finder.forceUntarget();
		
		state.changeTo( dead );
		
	}




	
	// アクション ================================================================================

	// stay ----------------------------------------------------

	void stay( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{

			state.shiftTo( staying );


			//state.setPhysics( physStayOnWall );

			posture.setCollision( collisionWallInStay );


			motion.fadeIn( msStand, 0.3f );

			msStand.speed = def.quickness * figure.scaleRateR;


			releaseTime = 0.0f;


			posture.fittingRate = 30.0f;

		};

		action.last = () =>
		{
			
			posture.fittingRate = 3.0f;

		};

	}

	void staying()
	{
		
		var	isFoundTarget = finder.search(0); 
		

		var speed = setMovePoint( isFoundTarget );
		
		
		var isFront = lookAtTarget( isBattling ? 3.0f : 1.0f, 5.0f, true );


		moveSpeed.lerp( def.quickness * speed, 3.0f );




		if( isFoundTarget )
		{

			motion.fadeIn( msWalkAnger, 0.1f );
			
			msWalkAnger.speed = moveSpeed.get();// * figure.scaleRateR;


			if( isFront )
			{
				
				state.changeTo( walk );

			}

		}
		else
		{

			if( isFront )
			{
				state.changeTo( jump );
			}
			else
			{
				motion.fadeIn( msWalk, 0.3f );//msStand, 0.3f );

				msWalk.speed = moveSpeed.get();// * figure.scaleRateR;
			}

		}



		output.loudness = msWalk.speed;//0.3f;

	}

	

	// walk ----------------------------------------------------
	
	void walk( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{
			
			state.shiftTo( walking );


			//state.setPhysics( physMoveOnWall );

			posture.setCollision( collisionWallInMove );


			releaseTime = 0.0f;

		};
		
		action.last = () =>
		{

		};

	}


	void walking()
	{

		var	isFoundTarget = finder.search(0); 

		
		var speed = setMovePoint( isFoundTarget );

		
		var isFront = lookAtTarget( isBattling ? 3.0f : 1.0f, 10.0f, true );


		moveSpeed.lerp( def.quickness * speed, 3.0f );


		if( isFoundTarget )
		{
			
			motion.fadeIn( msWalkAnger, 0.3f );
			
			msWalkAnger.speed = moveSpeed.get();// * figure.scaleRateR;


			if( isFront & isReachOnAttack() )
			{
				
				//moveSpeed.lerp( 0.0f, 6.0f );
				
				if( /*moveSpeed.get() < 0.5f && */shoot.isReady( 0, 0 ) )
				{
					state.changeTo( attack );
				}
				
			}

		}
		else
		{

			if( isGoaled( 30.0f * figure.scaleRate ) )
			{
				motion.fadeIn( msWalk, 0.3f );
				
				msWalk.speed = moveSpeed.get();// * figure.scaleRateR;
			}
			else
			{
				state.changeTo( jump );
			}

		}



		output.loudness = moveSpeed.get();

	}
	



	// jump ------------------------------------------

	void jump( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			if( isGround() )
			{
				state.shiftTo( preJumping );

				msJumpStart.speed = def.quickness * figure.scaleRateR * ( isBattling ? 1.5f : 1.0f );
			}
			else
			{
				//state.changeTo( stay );
				state.shiftTo( jumpingDown );
			}


			//state.setPhysics( physStayOnWall );

			posture.setCollision( collisionWallInStay );
			
			
			motion.fadeIn( msJumpStart, 0.1f );
			

			moveSpeed.set( 0.0f );
			
			posture.fittingRate = 30.0f;

		};
		
		action.last = () =>
		{
			
			posture.fittingRate = 3.0f;

		};
		
	}


	void preJumping()
	{

		if( msJumpStart.time > msJumpStart.length * 0.5f )
		{
			// ジャンプアップへ変化


			motion.fadeIn( msJumpUp, 0.2f );

			msJumpUp.speed = def.quickness * figure.scaleRateR;
			
			
			state.setPhysics( physFree );
			posture.setCollision( null );
			shiftToPhysicalMode();
			

			state.shiftTo( jumpingUp );



			var up = posture.rot * Vector3.up;

			var slope = Vector3.Dot( up, Vector3.up );

			if( slope > 0.1f || Random.value > 0.7f & slope > -0.1f )
			{
				// 垂直か平地

				up = Vector3.up;

			}
			else
			{
				// 垂直か天井

				up *= 0.5f;

			}

			var forward = posture.rot * Vector3.forward;

			var d = ( target.point - rb.position ).magnitude;


			var fwdForce = ( d > 70.0f * figure.scaleRate ? 70.0f * figure.scaleRate : d * figure.scaleRate ) * 90.0f * forward;

			var upForce = 8000.0f * ( 0.7f + figure.scaleRate * figure.scaleRate * 0.3f ) * up;


			rb.AddForce( fwdForce + upForce, ForceMode.Impulse );


			
			releaseTime = Time.time + 1.0f * figure.scaleRate * figure.scaleRate;	// ジャンプ出始めに地面に吸着しちゃうのを防止

		}

		
		var isFound = searchTargetInJumping();

		if( isFound ) msJumpStart.speed = def.quickness * figure.scaleRateR * 1.5f;

	}

	void jumpingUp()
	{

		if( Time.time > releaseTime )
		{
			
			posture.setCollision( collisionWallInStay );


			if( isGround() )
			{
				
				state.changeTo( stay );
				
			}
			
		}

		if( rb.velocity.y < 0.05f )
		{
			
			motion.fadeIn( msJumpDown, 3.0f );

			msJumpDown.speed = def.quickness * figure.scaleRateR;
			
			
			state.shiftTo( jumpingDown );

			posture.setCollision( collisionWallInStay );

		}


		searchTargetInJumping();

	}
	
	void jumpingDown()
	{

		if( isGround() )
		{
			
			state.changeTo( stay );
			
		}


		searchTargetInJumping();

	}
	
	
	bool searchTargetInJumping()
	{
		var	isFoundTarget = finder.search(0); 
		
		if( isFoundTarget )
		{
			battleModeOn();
			
			if( figure.scaleRate >= 3.0f & isReachOnAttack() )
			{
				state.changeTo( attack );
			}
		}
		
		return isFoundTarget;
	}



	// attack ----------------------------------------------------

	void attack( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{
			
			state.shiftTo( attacking );

			
			//state.setPhysics( physStayOnWall );
			//setPhysicsStayMode();
			
			posture.setCollision( collisionWallInStay );


			motion.fadeIn( msAttack, 0.3f );

			msAttack.speed = def.quickness * figure.scaleRateR;

		};
		
		action.last = () =>
		{

		};

	}


	void attacking()
	{


		if( msAttack.time < msAttack.length * 0.5f )
		{

			if( finder.search(0) )
			{
				
				target.point = finder.target.position;//.act.rb.position;
				
				lookAtTarget( isBattling ? 3.0f : 1.0f, 10.0f, true );


				moveSpeed.lerp( def.quickness * 1.0f, 3.0f );

			}

		}

		if( msAttack.time >= ratedAttackTiming & !shoot.weapons[0].isReloading )
		{

			shootAngled( 0, 0, 30.0f );

		}


		if( msAttack.time >= msAttack.length )
		{

			state.changeTo( stay );//walk );

		}


		output.loudness = 1.5f;

	}



	// damaging ----------------------------------------------------
	
	void damage( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( damaging );

			//state.setPhysics( physStayOnWall );
			//setPhysicsStayMode();
			
			posture.setCollision( collisionWallInStay );


			motion.fadeIn( msDamage, 0.2f );

			msDamage.speed = def.quickness * figure.scaleRateR;
			
		};
		
		action.last = () =>
		{

		};
		
	}
	
	void damaging()
	{

		moveSpeed.lerp( 0.0f, 5.0f );


		if( Time.time > releaseTime )
		{

			state.changeTo( stay );

		}


		output.loudness = 2.0f;

	}


	
	// blowing ----------------------------------------------------
	
	void blow( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( browing );
			

			state.setPhysics( physFree );
			posture.setCollision( null );
			shiftToPhysicalMode();


			motion.fadeIn( msWalk, 0.2f );//msDamage, 0.2f );

			msDamage.speed = def.quickness * figure.scaleRateR;

			
			rb.constraints = RigidbodyConstraints.None;

		};
		
		action.last = () =>
		{

			rb.constraints = figure.rbDefaultConstraints;

		};
		
	}
	
	void browing()
	{
		
		moveSpeed.lerp( 0.0f, 5.0f );
		
		
		if( Time.time > releaseTime )
		{
			
			state.changeTo( stay );
			
		}
		
		
		output.loudness = 2.0f;
		
	}



	// dead -------------------------------------------------------

	void dead( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			state.shiftTo( deadingStay );


			state.setPhysics( physFree );
			posture.setCollision( null );
			shiftToPhysicalMode();


			moveSpeed.set( 0.0f );


			motion.fadeIn( msDead, 0.3f );


			releaseTime = Time.time + 30.0f;


			rb.constraints = RigidbodyConstraints.None;

			

			figure.moveCollider.enabled = false;

			rb.isKinematic = false;

			switchEnvelopeDeadOrArive( UserLayer._enemyEnvelope, UserLayer._enemyEnvelopeDead );


			deadize();

		};

		action.last = () =>
		{

			shiftToPhysicalMode();
			

			rb.detectCollisions = true;

			switchEnvelopeDeadOrArive( UserLayer._enemyEnvelopeDead, UserLayer._enemyEnvelope );

			rb.constraints = figure.rbDefaultConstraints;


			final();

		};

	}

	void deadingStay()
	{
		
		//releaseTime -= GM.t.delta;

		if( Time.time > releaseTime )
		{

			state.shiftTo( deadingFall );

			releaseTime = Time.time + 3.0f;

			rb.detectCollisions = false;

		}

	}

	void deadingFall()
	{
		
		//releaseTime -= GM.t.delta;

		if( Time.time > releaseTime )
		{
			
			state.changeToIdle();

		}

	}


	protected void switchEnvelopeDeadOrArive( int oldLayer, int newLayer )
	{

		var tfEnv = tfBody.findWithLayerInDirectChildren( oldLayer );
		//var tfEnv = tf.findWithLayerInDirectChildren( oldLayer );

		if( tfEnv != null )
		{

			var cs = tfEnv.GetComponents<Collider>();
			
			cs[1].enabled = !cs[1].enabled;

			cs[0].enabled = !cs[0].enabled;

			tfEnv.gameObject.layer = newLayer;

		}

	}
#endif

}
