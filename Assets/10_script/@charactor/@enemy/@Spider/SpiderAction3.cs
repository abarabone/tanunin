using UnityEngine;
using System.Collections;

public class SpiderAction3 : _Action3Enemy
{
#if true

	

	//MotionCrossFader	motion;
	MotionFader		motion;

	protected MotionStateHolder ms;



	public float attackTiming	= 0.06f;	// 0.00f ～ 0.07f の範囲で遅延可能、回避のしやすいタイミングで！
	
	protected float	ratedAttackTiming;


	
	// 初期化 ------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();



		var anim = GetComponentInChildren<Animation>();

		ms.deepInit( anim );
		

	}


	public override void init()
	{

		base.init();


		state.init( stay );

		motion.init( ms.stand );



		posture.fittingRate = 3.0f;


		ratedAttackTiming = def.attackTiming / def.quickness * figure.scaleRateR;


		GetComponent<AudioSource>().pitch = 0.5f * figure.scaleRateR;

		shoot.weapons[ 0 ].GetComponent<AudioSource>().pitch = 1.5f * figure.scaleRateR;


		move.init( -100.0f, 200.0f, 200.0f );

		//rb.inertiaTensor = rb.inertiaTensor;
		//rb.inertiaTensorRotation = rb.inertiaTensorRotation;
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

		finder.target.clear();

		mode = finder.toMode();

		migration.setTimer( Random.value * 3.0f );

	}

	public override void changeToAttackMode( _Action3 attacker )
	{

		if( attacker == null ) return;

		if( attacker == this )
		{
			changeToAlertMode();// 自分自身が攻撃者の時はアラートなの？なんか特殊なことしてるんだっけ？？？特殊なのやめような…

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

	public override void changeToDamageMode( _Action3 attacker )
	{

		changeToAttackMode( attacker );


		if( figure.scaleRate >= 3.0f )
		{
			// スーパーアーマー
		}
		else
		{
			
			state.changeTo( damage );

		}

	}

	public override void changeToBlowingMode( _Action3 attacker, int level )
	{

		changeToAttackMode( attacker );


		if( figure.scaleRate >= 3.0f )
		{
			// スーパーアーマー
		}
		else
		{

			state.setPhysics( physFree );

			shiftToPhysicalMode();//

			
			state.changeTo( blow );

		}

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

			state.setPhysics( physStayOnWall );
			posture.setCollision( collisionWallInStay );

			
			migration.setTimer( mode.isBattling ? 0.0f : Random.Range( 0.0f, 3.0f ) );



			move.setRepairTime( 1.5f );

			stance.setActionSpeed( 1.0f );

			motion.fadeIn( ms.stand, 0.3f );

			ms.stand.speed = stance.getMotionSpeed( this );


			posture.fittingRate = 30.0f;

		};

		action.last = () =>
		{

			posture.fittingRate = 3.0f;

		};

	}

	void staying()
	{


		var preMode = mode;

		mode = finder.toMode( finder.search( 0 ) );


		switch( mode.state )
		{

			case BattleState.EnMode.doubt:
			
				migration.setTargetPoint( this, ref finder.target );
				
				var isFront = lookAtTarget( 1.0f, 20.0f );

				if( !isFront ) state.changeTo( walk );

				break;
			

			case BattleState.EnMode.lost:
			case BattleState.EnMode.alert:
			case BattleState.EnMode.weary:
			case BattleState.EnMode.wait:
			
				if( migration.isLimitOver )
				{

					if( isGround() )
					{
						state.changeTo( walk );// jump );
					}
					else
					{
					//	motion.fadeIn( ms.walk, 0.5f );
					}

				}

				break;
			

			case BattleState.EnMode.decision:

				state.changeTo( walk );
				
				//if( !preMode.isChaseTrigger( ref finder.target ) )
				if( !preMode.isChaseEnemy )
				{
					//Debug.Log( "decision" );
					connection.notifyAttack( this, finder.target.act );
					
					migration.resetTimer();// すぐに追いかけ始めるように
				}

				break;
			
		}


		var speedRate01 = move.speedRate01;

		stance.setSpeed( motion.preWeight, speedRate01 );

		output.loudness = speedRate01;//0.3f;

	}



	// walk ----------------------------------------------------

	void walk( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{

			state.shiftTo( walking );


			state.setPhysics( physMoveOnWall );
			posture.setCollision( collisionWallInMove );

			move.setRepairTime( 1.0f );


			if( mode.isWait )
			{
				migration.setTargetPoint( this );
			}

			
			motion.fadeIn( mode.isDecisionEnemy ? ms.walkAnger : ms.walk, 0.3f );

		};

		action.last = () =>
		{

		};

	}

	void walking()
	{

		var preMode = mode;

		mode = finder.toMode( finder.search( 0 ) );

		switch( mode.state )
		{

			case BattleState.EnMode.doubt:

				if( preMode.isChaseEnemy )
				{

					state.changeTo( stay );

				}
				else
				{
					migration.setTargetPoint( this, ref finder.target );

					var isFront = lookAtTarget( 1.0f, 5.0f );

					if( isFront ) state.changeTo( stay );
				}

				break;


			case BattleState.EnMode.lost:
			case BattleState.EnMode.alert:

				migration.routine( this, ref finder.target );

				lookAtTarget();
				
				{
					var isFront = lookAtTarget( 5.0f, 10.0f );

					if( isFront )
					{
						state.changeTo( jump );// stay );
					}
				}

				break;


			case BattleState.EnMode.wait:

				var isGoal = migration.isGoalOrLimit( ref figure, rb.position );

				if( isGoal )
				{
					state.changeTo( stay );

					break;
				}

				{
					var isFront = lookAtTarget();

					if( isFront )
					{
						state.changeTo( jump );// stay );
					}
				}

				break;


			case BattleState.EnMode.weary:
			case BattleState.EnMode.decision:

				//if( !preMode.isChaseTrigger( ref finder.target ) )
				if( !preMode.isChaseEnemy )
				{
					//Debug.Log( "decision" );
					connection.notifyAttack( this, finder.target.act );

					migration.resetTimer();// すぐに追いかけ始めるように
				}


				migration.routine( this, ref finder.target );

				{
					var isFront = lookAtTarget( 3.0f, 10.0f );


					if( isFront && finder.target.isReach( def.reach, ref finder ) )
					{

						if( shoot.isReady( 0, 0 ) )
						{

							state.changeTo( attack );

						}
						else
						{
							if( figure.scaleRate < 3.0f )//&& finder.target.isReach( figure.bodyRadius * 3.0f, this ) )//moves.isGoal( figure.moveCastRadius * 3.0f, rb.position ) )
							{

								// 近くに寄りすぎた場合はジャンプする。
								state.changeTo( jump );

							}
						}

					}
				}

				break;

		}



		var speedRate01 = move.speedRate01;

		var msWalk = ms.walkAnger.enabled ? ms.walkAnger : ms.walk;//mode.isDecisionEnemy ? ms.walkAnger : ms.walk;

		var modeSpeed = finder.target.isRelease ? 1.0f : 3.0f;

		msWalk.speed = stance.getMotionSpeed( this, modeSpeed, speedRate01 );
		//Debug.Log( move.velocity );

		stance.setSpeed( modeSpeed, motion.currentWeight, speedRate01 );
		//move.lerp( modeSpeed * def.quickness, 3.0f );

		/*
		var msWalk = ms.walkAnger.enabled ? ms.walkAnger : ms.walk;//mode.isDecisionEnemy ? ms.walkAnger : ms.walk;
		msWalk.speed = move.speed * def.quickness * figure.scaleRateR;
		//Debug.Log( move.velocity );

		var modeSpeed = finder.target.isRelease ? 1.0f : 3.0f;

		move.lerp( modeSpeed * def.quickness, 3.0f );
		*/

		output.loudness = speedRate01;

	}




	// jump ------------------------------------------

	void jump( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			if( isGround() )
			{
				state.shiftTo( preJumping );

				ms.jumpStart.speed = def.quickness * figure.scaleRateR * ( mode.isBattling ? 1.5f : 1.0f );
			}
			else
			{
				//state.changeTo( stay );
				state.shiftTo( jumpingDown );
			}


			//state.setPhysics( physStayOnWall );

			posture.setCollision( collisionWallInStay );

			move.setRepairTime( 1.8f );

			stance.setActionSpeed( 1.0f );


			motion.fadeIn( ms.jumpStart, 0.1f );

			

			posture.fittingRate = 30.0f;

		};
		
		action.last = () =>
		{
			
			posture.fittingRate = 3.0f;

		};
		
	}


	void preJumping()
	{


		stance.setSpeed( motion.preWeight, move.speedRate01 );


		if( ms.jumpStart.time > ms.jumpStart.length * 0.5f )
		{
			// ジャンプアップへ変化


			motion.fadeIn( ms.jumpUp, 0.2f );

			ms.jumpUp.speed = stance.getMotionSpeed( this );

			stance.setSpeedZero();


			state.setPhysics( physFree );
			posture.setCollision( null );
			shiftToPhysicalMode();
			

			state.shiftTo( jumpingUp );



			var forward = posture.rot * Vector3.forward;

			var up = posture.rot * Vector3.up;

			var slope = Vector3.Dot( up, Vector3.up );

			if( slope > 0.1f )//|| Random.value > 0.7f & slope > -0.1f )
			{
				// 垂直か平地

				up = Vector3.up;

				forward *= 0.5f;

			}
			else
			{
				// 垂直か天井

				up *= 0.5f;

			}


			var d = ( migration.targetPoint - rb.position ).magnitude;


			var fwdForce = ( d > 70.0f * figure.scaleRate ? 70.0f * figure.scaleRate : d * figure.scaleRate ) * 150.0f * forward;//90.0f * forward;

			var upForce = 8000.0f * ( 0.7f + figure.scaleRate * figure.scaleRate * 0.3f ) * up;


			rb.AddForce( fwdForce + upForce, ForceMode.Impulse );



			migration.setTimer( 1.0f * figure.scaleRate * figure.scaleRate );	// ジャンプ出始めに地面に吸着しちゃうのを防止
			//jlimit = Time.time + 1.0f * figure.scaleRate * figure.scaleRate;
		}

		
		var isFound = searchTargetInJumping();

		if( isFound ) ms.jumpStart.speed = stance.getMotionSpeed( this ) * 1.5f;

	}

	//float	jlimit;


	void jumpingUp()
	{

		if( migration.isLimitOver )
		{
			
			posture.setCollision( collisionWallInStay );


			if( isGround() )
			{

				rb.velocity *= 0.1f;// 無理やり臭いが

				state.changeTo( stay );
				
			}
			
		}

		if( rb.velocity.y < 0.05f )
		{
			
			motion.fadeIn( ms.jumpDown, 3.0f );

			ms.jumpDown.speed = stance.getMotionSpeed( this );
			
			
			state.shiftTo( jumpingDown );

			posture.setCollision( collisionWallInStay );

		}


		searchTargetInJumping();

	}
	
	void jumpingDown()
	{

		if( isGround() )
		{

			rb.velocity *= 0.1f;// 無理やり臭いが

			state.changeTo( stay );
			
		}


		searchTargetInJumping();

	}
	
	
	bool searchTargetInJumping()
	{

		var preMode = mode;

		mode = finder.toMode( finder.search(0) );

		if( mode.isDecisionEnemy )
		{
			
			//if( !preMode.isChaseTrigger( ref finder.target ) )
			if( !preMode.isChaseEnemy )
			{
				//Debug.Log( "decision" );
				connection.notifyAttack( this, finder.target.act );

				migration.resetTimer();// すぐに追いかけ始めるように
			}


			if( figure.scaleRate >= 3.0f & finder.target.isReach( def.reach, ref finder ) )
			{
				state.changeTo( attack );
			}

		}
		

		return finder.target.isExists;
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


			move.setRepairPowerRate( 0.0f );
			


			motion.fadeIn( ms.attack, 0.3f );
			//anim.CrossFade( ms.attack.name, 0.3f );

			ms.attack.speed = stance.getMotionSpeed( this );

		};

		action.last = () =>
		{

			move.setRepairTime( 2.0f );

		};

	}


	void attacking()
	{
		

		if( ms.attack.time < ms.attack.length * 0.5f )
		{

			if( finder.search( 0 ) )
			{

				migration.setTargetPoint( this, ref finder.target );

				lookAtTarget( mode.isBattling ? 3.0f : 1.0f, 10.0f, true );

			}

		}

		if( ms.attack.time >= ratedAttackTiming )
		{

			shootTarget( 0, 0, 30.0f );

		}


		if( ms.attack.time >= ms.attack.length )
		{

			state.changeTo( stay );// walk );

			if( character.aggressive >= CharacterInfo.EnAggressive.low )
			{
				finder.target.toLost( character );
			}

		}


		stance.setSpeed( motion.preWeight, move.speedRate01 );

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
			//shiftToPhysicalMode();

			posture.setCollision( collisionWallInStay );


			// 復帰スピードは move による。回復値は元のモーションのものとなる。


			motion.fadeIn( ms.damage, 0.2f );

			ms.damage.speed = stance.getMotionSpeed( this );

		};

		action.last = () =>
		{

			//shiftToPhysicalMode();

		};

	}

	void damaging()
	{

		//move.lerp( 0.0f, 5.0f );
		stance.setSpeed( motion.preWeight, move.speedRate01 );



		if( move.isMovable )
		{

			state.changeTo( walk );

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
			shiftToPhysicalMode();
			posture.setCollision( null );


			// 復帰スピードは move による。回復値は元のモーションのものとなる。


			motion.fadeIn( ms.walk, 0.2f );//ms.damage, 0.2f );

			ms.damage.speed = stance.getMotionSpeed( this );


			rb.constraints = RigidbodyConstraints.None;

		};

		action.last = () =>
		{

			rb.constraints = figure.rbDefaultConstraints;

			//shiftToPhysicalMode();

		};

	}

	void browing()
	{

		//move.lerp( 0.0f, 5.0f );
		stance.setSpeed( motion.preWeight, move.speedRate01 );


		if( move.isMovable )
		{

			state.changeTo( walk );

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
			shiftToPhysicalMode();
			posture.setCollision( null );


			move.setRepairPowerRate( 0.0f );
			stance.setSpeedZero();


			motion.fadeIn( ms.dead, 0.3f );
			//anim.CrossFade( ms.dead.name, 0.3f );


			migration.setTimer( 30.0f );


			rb.constraints = RigidbodyConstraints.None;



			figure.moveCollider.enabled = false;

			rb.isKinematic = false;


			switchEnvelopeDeadOrArive( UserLayer._enemyEnvelope, UserLayer._enemyEnvelopeDead );


			deadize();

		};

		action.last = () =>
		{

			//shiftToPhysicalMode();


			rb.detectCollisions = true;

			switchEnvelopeDeadOrArive( UserLayer._enemyEnvelopeDead, UserLayer._enemyEnvelope );

			rb.constraints = figure.rbDefaultConstraints;

			//shiftToPhysicalMode();


			final();

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


	protected void switchEnvelopeDeadOrArive( int oldLayer, int newLayer )
	{

		var tfEnv = tfBody.findWithLayerInDirectChildren( oldLayer );
		//var tfEnv = tf.findWithLayerInDirectChildren( oldLayer );

		if( tfEnv != null )
		{

			var cs = tfEnv.GetComponents<Collider>();

			cs[ 1 ].enabled = !cs[ 1 ].enabled;

			cs[ 0 ].enabled = !cs[ 0 ].enabled;

			tfEnv.gameObject.layer = newLayer;

		}

	}



	// ------------------------------------------

	protected struct MotionStateHolder
	{

		public AnimationState	stand;

		public AnimationState	walk;

		public AnimationState	walkAnger;


		public AnimationState	jumpStart;

		public AnimationState	jumpUp;

		public AnimationState	jumpDown;


		public AnimationState	attack;

		public AnimationState	damage;

		public AnimationState	dead;


		public void deepInit( Animation anim )
		{


			stand = anim[ "stand" ];

			walk = anim[ "walk" ];

			walkAnger = anim[ "angwalk" ];


			jumpStart = anim[ "jumpstart" ];

			jumpUp = anim[ "jumpup" ];

			jumpDown = anim[ "jumpdown" ];


			attack = anim[ "attack" ];

			damage = anim[ "ang" ];

			dead = anim[ "dead" ];
			//msDead.speed = 0.0f;


			//anim.animatePhysics = true;

		}

	}

#endif
}
