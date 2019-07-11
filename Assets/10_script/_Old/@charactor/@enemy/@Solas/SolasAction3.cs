using UnityEngine;
using System.Collections;

public class SolasAction3 : _Action3Enemy
{

	protected MotionStateHolder	ms;

	protected RagdollState		ragdoll;



	protected Transform	tfHip;

	protected Rigidbody	rbHip;



	protected Rigidbody[]	rbParts;




	protected Animation		anim;

	protected MotionFader	motion;



	public SoundUnit	se;

	public ParticlePoolingEmitter3WithSound	dustCloud;



	protected Transform	tfToeL;

	protected Transform	tfToeR;

	protected Transform	tfTail;

	protected Transform tfLegL;
	protected Transform tfLegR;
	protected Transform tfAbdomen;


	AnimationState	msWalkNow;

	AnimationState	msFireNow;

#if false

	
	// 初期化 ------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();
		//base.combineMesh( true );



		tfHip = tfBody.FindChild( "hip" );

		rbHip = tfHip.GetComponent<Rigidbody>();


		tfToeL = tfHip.FindChild( "leg1_L_/leg2_L_/foot_L_/toe_L_/toe hit_L_" );
		tfToeR = tfHip.FindChild( "leg1_R_/leg2_R_/foot_R_/toe_R_/toe hit_R_" );


		tfTail = tfHip.FindChild( "tail1" );
		tfLegL = tfHip.FindChild( "leg1_L_" );
		tfLegR = tfHip.FindChild( "leg1_R_" );
		tfAbdomen = tfHip.FindChild( "abdom" );


		rbParts = tfBody.GetComponentsInChildren<Rigidbody>();


		anim = GetComponentInChildren<Animation>();



		ms.deepInit( anim );

		ragdoll.deepInit( anim );

		se.deepInit( tfBody );


		setMaxMoveForDepenetration();
		
	}

	public override void init()
	{
		
		base.init();
		
		
		//audio.pitch = 0.5f * figure.scaleRateR;
		
		//shoot.wapons[0].audio.pitch = 0.5f * figure.scaleRateR;
		
		
		state.init( stay );



		posture.fittingRate = 3.0f;



		//rotMoveDir = rb.rotation;


		//shiftToPhysicalMode();
		//shiftToKinematicMode();
		//rb.isKinematic = false;
		
		
		setKinematicFullBody( true );
		
	}
	

	
	// 更新処理 ----------------------------------
	
	new protected void Update()
	{
		
		base.Update();


		shoot.reload( 0 );


		motion.update();

	}

	new protected void FixedUpdate()
	{
		
		if( isDead ) return;


		posture.fitting( rb );

		var moveRatio = classBasedSpeed * figure.scaleRate;

		var movedist = moveRatio * move.speed * GM.t.delta;

		rb.MovePosition( rb.position + posture.rot * Vector3.forward * movedist );

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
			changeToAlertMode();

			return;
		}


		finder.target.toDecision( character, attacker );

		mode = finder.toMode(); // ここで追跡モードにしておかないと、被通知側も通知を出して収集がつかない

		migration.setTimer( Random.value * 3.0f );

	}

	public override void changeToAlertMode()
	{

		finder.target.toAlert( character );

		mode = finder.toMode(); // ここで追跡モードにしておかないと、被通知側も通知を出して収集がつかない

		migration.setTimer( Random.value * 3.0f );

	}

	public override void changeToDamageMode( _Action3 attacker, float time )
	{

		changeToAttackMode( attacker );

		return;


		if( figure.scaleRate >= 3.0f )
		{
			// スーパーアーマー
		}
		else
		{

			setKinematicFullBody( false );


			migration.setTimer( time );

			state.changeTo( damage );

		}

	}

	public override void changeToBlowingMode( _Action3 attacker, float time, int level )
	{

		changeToAttackMode( attacker );


		if( true || figure.scaleRate >= 3.0f )
		{
			// スーパーアーマー
		}
		else
		{

			setKinematicFullBody( false );


			migration.setTimer( time );

			state.changeTo( blow );

		}

	}

	public override void changeToDeadMode()
	{

		finder.target.clear();

		state.changeTo( dead );

	}
	


	// ユーティリティ ------------------------------

	public void setKinematicFullBody( bool iskinematic )
	{

		if( rbHip.isKinematic != iskinematic )
		{
			/*
			foreach( var rbPart in rbParts )
			{
				rbPart.isKinematic = iskinematic;
			}
			*/

			rbHip.isKinematic = iskinematic;

			anim.enabled = iskinematic;
			
			if( !iskinematic ) velocityOff();

		}
		
	}


	protected void velocityOff()
	{
		foreach( var rbPart in rbParts )
		{

			rbPart.velocity = Vector3.zero;

			rbPart.angularVelocity = Vector3.zero;

		}
	}

	protected void setMaxMoveForDepenetration()
	{

		foreach( var rbPart in rbParts )
		{

			rbPart.maxDepenetrationVelocity = 0.1f;
			// めりこみを解消しようとする反発力の限界を決めてしまう。

		}

		var js = tfBody.GetComponentsInChildren<Joint>();
		foreach( var j in js )
		{

			j.enablePreprocessing = false;
			// よくわからんけど安定するらしい

		}

	}

	protected void setLayerUnderToes( int layer )
	{
		tfToeL.setChildrenLayer( layer );
		tfToeR.setChildrenLayer( layer );
	}

	protected void setTailAnimation( AnimationState ms, bool isEnable )
	{
		if( isEnable )
		{
			ms.AddMixingTransform( tfHip );
		}
		else
		{
			ms.AddMixingTransform( tfHip, false );
			ms.AddMixingTransform( tfLegL );
			ms.AddMixingTransform( tfLegR );
			ms.AddMixingTransform( tfAbdomen );
		}
	}



	// アクション ================================================================================

	// stay ----------------------------------------------------

	void stay( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			se.garuruOut();
			
			state.shiftTo( staying );


			//collisionProc = collisionWallInStay;
			
			
			motion.fadeIn( ms.stand, 1.5f );
			
			//msStand.speed = quickness * figure.scaleRateR;
			

		};
		
		action.last = () =>
		{

		};
		
	}

	
	void staying()
	{

		
		mode = finder.toMode( finder.search(0) );

		var isForward = lookAtTarget( 5.0f );

		
		move.lerp( 0.0f, 3.0f );

		//rotMoveDir = Quaternion.LookRotation( finder.target.position - rb.position );


		if( mode.isChaseEnemy )
		{

			state.changeTo( walk );

		}
		else if( finder.target.isExists )
		{

			if( !isForward )
			{

				state.changeTo( walk );

			}
			else
			{



			}

		}

	}


	// walk --------------------------------------------------

	void walk( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			se.garuruOut();

			state.shiftTo( walking );
			
						
			motion.fadeIn( ms.walk, 1.0f );

			msWalkNow = ms.walk;

			msWalkNow.speed = 0.3f * def.quickness * figure.scaleRateR;


			//setTailAnimation( msWalkNow, Random.value > 0.5f );

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
				{
					/*
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
					*/
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
					//	state.changeTo( stay );

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

						connection.notifyAttack( this, finder.target.act );

					}

					lookAtTarget();

					if( shoot.isReady( 0, 0 ) ) state.changeTo( attack );

				}
				break;

		}



		//ms.walk.speed = 0.3f * move.speed * figure.scaleRateR;
		//Debug.Log( move.velocity );


		/*
		if( msWalkNow.time > msWalkNow.length * 0.5f )
		{

			if( msWalkNow.time > msWalkNow.length )
			{

				msWalkNow = msWalkNow == ms.walkL ? ms.walkR : ms.walkL;

				motion.fadeIn( msWalkNow, 0.1f );

				msWalkNow.speed = 0.3f * def.quickness * figure.scaleRateR;

			}
			else
			{

				move.lerp( 0.0f, 3.0f );

			}

		}
		else
		{

			move.lerp( def.quickness, 3.0f );

		}
		*/

		var walkTime = msWalkNow.time % ( msWalkNow.length * 0.5f );

		var m = ( walkTime < 0.2f | walkTime > 0.5f ) ? def.quickness : 0.0f;
		//Debug.Log( walkTime + " / " + msWalkNow.length );

		move.lerp( m, 3.0f );


		output.loudness = move.speed;

	}



	// attack ------------------------------------------------

	void attack( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			se.cryOut();


			msFireNow = Random.value > 0.5f ? ms.fireH : ms.fireV;

			state.shiftTo( attacking );

			motion.fadeIn( msFireNow, 1.0f );
			

			//msStand.speed = quickness * figure.scaleRateR;

		};
		
		action.last = () =>
		{

		};
		
	}
	
	
	void attacking()
	{
		
		mode = finder.toMode( finder.search( 0 ) );
		

		//migration.pollTargetPoint( this, ref finder.target );

		//if( ms.fireV.time < ms.fireV.length * 0.1f ) lookAtTarget( 30.0f );


		var startTime = msFireNow.length * ( msFireNow == ms.fireV ? 0.4f : 0.33f );


		if( msFireNow.time > startTime ) 
		if( finder.target.isReach( def.reach, ref finder ) )
		{


			if( shoot.isReady( 0, 0 ) ) shootForward( 0, 0 );


		}


		if( msFireNow.time > msFireNow.length )
		{

			state.changeTo( stay );

			shoot.weapons[ 0 ].forceReload();// 暫定

		}


		move.lerp( 0.0f, 3.0f );


		output.loudness = move.speed;


	}


	// damage ----------------------------------------------------

	void damage( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( damaging );

			
			//motion.fadeIn( ms.cray, 0.2f );
			//motion.crossFade( msStand, 0.2f );
			
			//msDamage.speed = quickness * figure.scaleRateR;
			
			//msWalk.AddMixingTransform( tfBody.GetChild(0).GetChild(0), true );
			//tfBody.animation.enabled = false;
			setKinematicFullBody( false );

		};
		
		action.last = () =>
		{

			//msWalk.RemoveMixingTransform( tfBody.GetChild(0).GetChild(0).transform );
			//tfBody.animation.enabled = true;
			setKinematicFullBody( true );

		};
		
	}
	
	void damaging()
	{
		
		move.lerp( 0.0f, 2.0f );
		
		
		if( migration.isLimitOver )
		{
			
			state.changeTo( walk );//stay );
			
			RagdollCliper.clipPose( tfHip, ragdoll.ms.clip );

			motion.play( ragdoll.ms );

		}
		
		
		output.loudness = 2.0f;
		
	}
	
	
	
	// blowing ----------------------------------------------------
	
	void blow( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( browing );

			
			//motion.crossFade( msWalk, 0.2f );//msDamage, 0.2f );
			
			//msDamage.speed = quickness * figure.scaleRateR;
			
			
			//rb.constraints = RigidbodyConstraints.None;
			
		};
		
		action.last = () =>
		{

			setKinematicFullBody( true );

			//rb.constraints = rbDefaultConstraints;

		};
		
	}
	
	void browing()
	{

		move.lerp( 0.0f, 5.0f );

		if( migration.isLimitOver && isGround() )
		{

			state.changeTo( walk );//stay );

			RagdollCliper.clipPose( tfHip, ragdoll.ms.clip );

			motion.play( ragdoll.ms );

		}
		
		
		output.loudness = 2.0f;
		
	}
	
	
	
	// dead -------------------------------------------------------
	
	void dead( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			se.cryOut();

			state.shiftTo( deadingStay );

			
			move.velocity = 0.0f;
			

			
			migration.setTimer( float.PositiveInfinity );//30.0f );
			
			
			//rb.constraints = RigidbodyConstraints.None;
			
			
			
		//	figure.moveCollider.enabled = false;
			
			//rb.isKinematic = false;

		//	rbBones[0].isKinematic = false;

			//switchEnvelopeDeadOrArive( UserLayer._enemyEnvelope, UserLayer._enemyEnvelopeDead );

			//tfBody.animation.enabled = false;
			setKinematicFullBody( false );

			setLayerUnderToes( UserLayer._bgDetail );

			deadize();

		};
		
		action.last = () =>
		{
			
			figure.moveCollider.enabled = true;
			
			//rb.isKinematic = false;
			
			rbParts[1].isKinematic = false;


			rb.detectCollisions = true;
			foreach( var irb in rbParts ) irb.detectCollisions = true;
			
			//switchEnvelopeDeadOrArive( UserLayer._enemyEnvelopeDead, UserLayer._enemyEnvelope );
			
			rb.constraints = figure.rbDefaultConstraints;


			setLayerUnderToes( UserLayer._enemyRagdollLarge );
			

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

			foreach( var irb in rbParts )
			{
				irb.detectCollisions = false;
			}

		}
		
	}
	
	void deadingFall()
	{

		if( migration.isLimitOver )
		{
			
			state.changeToIdle();
			
		}
		
	}

#endif

	// ------------------------------------------

	protected struct MotionStateHolder
	{

		public AnimationState	stand;

		public AnimationState	wait;

		public AnimationState	sleep;


		public AnimationState	walk;

		public AnimationState	walkL;

		public AnimationState	walkR;



		public AnimationState	preMotion;

		public AnimationState	fireV;

		public AnimationState	fireH;

		public AnimationState	punch;

		public AnimationState	tailAttack;


		public AnimationState	cry;

		public AnimationState	buruburu;





		public void deepInit( Animation anim )
		{

			stand = anim[ "stand" ];
			stand.blendMode = AnimationBlendMode.Blend;
			stand.speed = 0.3f;

			wait = anim[ "wait" ];
			wait.blendMode = AnimationBlendMode.Blend;
			wait.speed = 0.3f;

		//	sleep = anim[ "sleep" ];
		//	sleep.blendMode = AnimationBlendMode.Blend;
		//	sleep.speed = 0.3f;


			walk = anim[ "walk" ];
			walk.blendMode = AnimationBlendMode.Blend;
			walk.speed = 0.3f;

			walkL = anim[ "walkL" ];
			walkL.blendMode = AnimationBlendMode.Blend;
			walkL.speed = 0.3f;

			walkR = anim[ "walkR" ];
			walkR.blendMode = AnimationBlendMode.Blend;
			walkR.speed = 0.3f;


			preMotion = anim[ "pre action" ];
			preMotion.blendMode = AnimationBlendMode.Blend;
			preMotion.speed = 0.3f;

			fireV = anim[ "fireV" ];
			fireV.blendMode = AnimationBlendMode.Blend;
			fireV.speed = 0.3f;

			fireH = anim[ "fireH" ];
			fireH.blendMode = AnimationBlendMode.Blend;
			fireH.speed = 0.3f;

			punch = anim[ "punch" ];
			punch.blendMode = AnimationBlendMode.Blend;
			punch.speed = 0.3f;

			tailAttack = anim[ "tail attack" ];
			tailAttack.blendMode = AnimationBlendMode.Blend;
			tailAttack.speed = 0.3f;

			cry = anim[ "cray" ];
			cry.blendMode = AnimationBlendMode.Blend;
			cry.speed = 0.3f;

			buruburu = anim[ "buruburu" ];
			buruburu.blendMode = AnimationBlendMode.Blend;
			buruburu.speed = 0.3f;


			anim.animatePhysics = false;// true;

		}


		public void setMixing( AnimationState ms, Transform tfApply, Transform tfOmit )
		{

			ms.AddMixingTransform( tfApply, false );


			for( var i = 0 ; i < tfApply.childCount ; i++ )
			{

				var tfThis = tfApply.GetChild( i );

				if( tfThis != tfOmit )
				{
					setMixing( ms, tfThis, tfOmit );
				}

			}

		}

	}




	protected struct RagdollState
	{

		public AnimationState	ms;


		public void deepInit( Animation anim )
		{

			ms = RagdollCliper.createState( anim );

			ms.blendMode = AnimationBlendMode.Blend;

			ms.speed = 1.0f;

		}


	}


	/*
	protected struct TailState
	{
		
		Transform	tfTail;

		Transform	tfLegL;
		Transform	tfLegR;

		Transform	tfHip;
		Transform	tfAbdomen;


		public deepInit( SolasAction3 act )
		{

			tfTail = act.tfBody.FindChild( "tail1" );



		}



		public void tailMotion( bool isMixing )
		{


			ms.AddMixingTransform( tfApply, false );


		}
		

	}
	*/


	[System.Serializable]
	public struct SoundUnit
	{

		AudioSource	sound;

		
		public AudioClip	cry;

		public AudioClip	garuru;


		public void deepInit( Transform tfBody )
		{

			sound = tfBody.GetComponentInChildren<AudioSource>();

		}

		public void cryOut()
		{
			sound.clip = cry;

			sound.pitch = 0.4f;

			sound.loop = false;

			sound.PlayDelayed( 0.0f );
		}

		public void garuruOut()
		{
			sound.clip = garuru;

			sound.pitch = 0.7f;

			sound.loop = true;

			sound.PlayDelayed( 0.0f );
		}

	}



}
