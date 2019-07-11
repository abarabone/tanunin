using UnityEngine;
using System.Collections;

public class AntAction3 : _Action3Enemy
{


	
	//public float attackTiming	= 0.06f;	// 0.00f ～ 0.07f の範囲で遅延可能、回避のしやすいタイミングで！
	
	protected float	ratedAttackTiming;



	protected MotionStateHolder	ms;

	public WriggleState	wriggler;	// 体幹の曲げを管理する



	//MotionCrossFader	motion;
	MotionFader		motion;



	// 初期化 ------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();



		var anim = GetComponentInChildren<Animation>();

		ms.deepInit( anim );

		wriggler.deepInit( anim );

	}


	public override void init()
	{

		base.init();


		state.init( stay );

		motion.init( ms.stand );


		wriggler.init( this );


		posture.fittingRate = 3.0f;


		move.init( -100.0f, 300.0f, 300.0f );


		ratedAttackTiming = def.attackTiming / def.quickness * figure.scaleRateR;

		//rb.inertiaTensor = rb.inertiaTensor;
		//rb.inertiaTensorRotation = rb.inertiaTensorRotation;
	}




	// 更新処理 ----------------------------------------
	
	new protected void Update()
	{
		
		shoot.reload( 0 );//暫定
		
		motion.update();
		
		base.Update();
		
		
		if( !isDead )
		{
			
			wriggler.update( this );
			
		}
		
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
		{Debug.Log("attacker is self");
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


			migration.setTimer( Random.Range( 1.0f, 6.0f ) );
			
			move.setRepairTime( 1.5f );

			stance.setActionSpeed( 1.0f );


			wriggler.bendAndTurn();

			motion.fadeIn( ms.stand, 0.3f );

			ms.stand.speed = stance.getMotionSpeed( this );

		};

		action.last = () =>
		{

		};

	}

	void staying()
	{
		

		var preMode = mode;
		
		mode = ( mode.isBattling | mode.isDoubtEnemy ) ?
			finder.toMode( finder.search( 0 ) || finder.search( 1 ) ) :
			finder.toMode( finder.search( 1 ) );
		

		switch( mode.state )
		{

			case BattleState.EnMode.doubt:

				migration.setTargetPoint( this, ref finder.target );

				var isFront = lookAtTarget( 1.0f, 40.0f );

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
						state.changeTo( walk );
					}
					else
					{
						motion.fadeIn( ms.walk, 0.5f );// 空中ジタバタ
					}

				}

				break;


			case BattleState.EnMode.decision:

				state.changeTo( walk );
				
				if( !preMode.isChaseEnemy )
				{
					//Debug.Log( "decision" );
					connection.notifyAttack( this, finder.target.act );

					migration.resetTimer();// すぐに追いかけ始めるように
				}

				break;

		}


		
		stance.setSpeed( motion.preWeight, move.speedRate01 );
		

		output.loudness = 0.3f;

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


			wriggler.bendAndTurn();

			motion.fadeIn( ms.walk, 0.3f );

		};
		
		action.last = () =>
		{

		};

	}

	void walking()
	{

		var preMode = mode;

		mode = ( mode.isBattling | mode.isDoubtEnemy ) ?
			finder.toMode( finder.search( 0 ) || finder.search( 1 ) ) :
			finder.toMode( finder.search( 1 ) );


		switch( mode.state )
		{

			case BattleState.EnMode.doubt:
			{
				
				if( preMode.isChaseEnemy )
				{
					
					state.changeTo( stay );
					
				}
				else
				{
					migration.setTargetPoint( this, ref finder.target );

					var isFront = lookAtTarget( 2.0f, 30.0f );

					if( isFront ) state.changeTo( stay );
				}

			}
			break;


			case BattleState.EnMode.lost:
			case BattleState.EnMode.alert:
			{
				
				migration.routine( this, ref finder.target );
				
				lookAtTarget();

			}
			break;


			case BattleState.EnMode.wait:
			{
				
				var isGoal = migration.isGoalOrLimit( ref figure, rb.position );

				if( isGoal )
				{
					state.changeTo( stay );

					break;
				}

				lookAtTarget();
				
			}
			break;

			
			case BattleState.EnMode.weary:
			case BattleState.EnMode.decision:
			{
				
				if( !preMode.isChaseEnemy )
				{
					//Debug.Log( "decision" );
					connection.notifyAttack( this, finder.target.act );
					
					migration.resetTimer();// すぐに追いかけ始めるように
				}


				migration.routine( this, ref finder.target );

				var isFront = lookAtTarget( 3.0f, 120.0f );


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

							// 近くに寄りすぎた場合はターンする。
							turnDirection( 10.0f, Random.Range( 2.0f, 5.0f ) );
							
							finder.target.toLost( character );

						}
					}

				}

			}
			break;

		}





		var modeSpeed = finder.target.isRelease ? 1.0f : 2.0f;

		var speedRate01 = move.speedRate01;

		stance.setSpeed( modeSpeed, motion.currentWeight, speedRate01 );
		//move.lerp( modeSpeed * def.quickness, 3.0f );

		ms.walk.speed = stance.getMotionSpeed( this, modeSpeed, speedRate01 );
		//Debug.Log( move.velocity );

		output.loudness = speedRate01;

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


			//moveSpeed.set( 0.0f );

			move.setRepairPowerRate( 0.0f );


			wriggler.turnOnly();

			motion.fadeIn( ms.attack, 0.3f );
			//anim.CrossFade( ms.attack.name, 0.3f );

			ms.attack.speed = stance.getMotionSpeed( this );

		};
		
		action.last = () =>
		{

			move.setRepairTime( 2.0f );	// ダメージ・吹き飛び回復用

		};

	}


	void attacking()
	{


		if( ms.attack.time < ms.attack.length * 0.5f )
		{

			if( finder.search(0) )
			{
				
				migration.setTargetPoint( this, ref finder.target );

				lookAtTarget( 1.0f, 30.0f, true );

			}

		}

		if( ms.attack.time >= ratedAttackTiming )
		{

			shootTarget( 0, 0, 30.0f );

		}


		if( ms.attack.time >= ms.attack.length )
		{

			state.changeTo( walk );

			if( character.aggressive >= CharacterInfo.EnAggressive.low )
			{
				finder.target.toLost( character );
			}

		}



		stance.setSpeed( motion.preWeight, move.speedRate01 );


		output.loudness = 2.0f;

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


			wriggler.off();
			
			motion.fadeIn( ms.damage, 0.2f );

			ms.damage.speed = 2.0f * def.quickness * figure.scaleRateR;
			
		};
		
		action.last = () =>
		{

			//shiftToPhysicalMode();

		};
		
	}
	
	void damaging()
	{
		
		
		if( move.isMovable )
		{

			state.changeTo( walk );

		}


		stance.setSpeed( motion.preWeight, move.speedRate01 );


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


			wriggler.off();
			
			motion.fadeIn( ms.walk, 0.2f );//ms.damage, 0.2f );

			ms.damage.speed = def.quickness * figure.scaleRateR;

			
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
		
		if( move.isMovable )
		{

			state.changeTo( walk );

		}


		stance.setSpeed( motion.preWeight, move.speedRate01 );


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


			wriggler.turnOnly();

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
			
			cs[1].enabled = !cs[1].enabled;

			cs[0].enabled = !cs[0].enabled;

			tfEnv.gameObject.layer = newLayer;

		}

	}




	// ------------------------------------------

	protected struct MotionStateHolder
	{

		public AnimationState	stand;

		public AnimationState	walk;

		public AnimationState	attack;

		public AnimationState	damage;

		public AnimationState	dead;


		public void deepInit( Animation anim )
		{

			stand = anim[ "standing" ];

			walk = anim[ "walking" ];

			attack = anim[ "attack02" ];

			damage = anim[ "damage" ];

			dead = anim[ "dead" ];
			//dead.speed = 0.0f;


			//anim.animatePhysics = true;

		}

	}


	[System.Serializable]
	public struct WriggleState
	{

		public AnimationState	bend;

		public AnimationState	turn;


		public Transform	tfHead;

		public Transform	tfWeist;

		Quaternion	preRotBody;// ターン表現のために必要


		public void deepInit( Animation anim )
		{

			turn = anim[ "turn02LR" ];
			turn.AddMixingTransform( tfHead );
			turn.AddMixingTransform( tfWeist );
			turn.layer = 1;
			turn.blendMode = AnimationBlendMode.Additive;
			turn.speed = 0.0f;
			turn.weight = 1.0f;

			bend = anim[ "turn02UD+" ];
			bend.AddMixingTransform( tfHead );
			bend.AddMixingTransform( tfWeist );
			bend.layer = 2;
			bend.blendMode = AnimationBlendMode.Additive;
			bend.speed = 0.0f;
			bend.weight = 1.0f;

		}

		public void init( _Action3Enemy act )
		{

			preRotBody = act.rb.rotation;

		}


		public void bendAndTurn()
		{
			turn.enabled = true;

			bend.enabled = true;
		}

		public void off()
		{
			turn.enabled = false;

			bend.enabled = false;
		}

		public void bendOnly()
		{
			turn.enabled = false;

			bend.enabled = true;
		}

		public void turnOnly()
		{
			turn.enabled = true;

			bend.enabled = false;
		}


		public void update( _Action3Enemy act )
		{

			var rotBody = act.rb.rotation;


			if( turn.enabled || bend.enabled )
			{

				var rotWriggle = Quaternion.Inverse( preRotBody ) * rotBody;

				var angles = rotWriggle.eulerAngles;


				if( turn.enabled )
				{
					var turnAngle = ( angles.y > 180.0f ? angles.y - 360.0f : angles.y ) * GM.t.deltaR;

					turnAngle = Mathf.Clamp( turnAngle, -60.0f, 60.0f );

					var turnTime = turnAngle * ( -1.0f / 60.0f ) + 1.0f + 1.0f;

					turn.time = Mathf.Lerp( turn.time, turnTime, GM.t.delta * act.move.speedRate01 * 3.0f );
				}


				if( bend.enabled )
				{
					var bendAngle = ( angles.x > 180.0f ? angles.x - 360.0f : angles.x ) * GM.t.deltaR;

					//	bendAngle = Mathf.Clamp( bendAngle, -90.0f, 90.0f );

					var bendTime = bendAngle * ( -1.0f / 90.0f ) + 2.0f + 1.0f;

					bend.time = Mathf.Lerp( bend.time, bendTime, GM.t.delta * act.move.speedRate01 * 3.0f );
				}

			}


			preRotBody = rotBody;

		}
	}


}
