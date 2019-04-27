using UnityEngine;
using System.Collections;

public class DarogaAction3 : _Action3Enemy
{
#if false

	protected MotionStateHolder	ms;

	protected RagdollState		ragdoll;

	protected TentaclesState	tentacles;
	

	HingeJoint	jtSholder;

	HingeJoint	jtChinko;


	Rigidbody[]	rbBones;

	ParticleSystem[]	psBeams;


	Rigidbody	rbCrotch;

	Transform	tfCrotch;


	//Rigidbody[]	rbSholderUppers;

	Rigidbody[]	rbCrotchLowers;

	Rigidbody	rbOuterBody;
	Rigidbody	rbInnerBody;


	Rigidbody[] rbFullBodies;



	/*
	Hit3BodyPart[,]	hitterTentacles;
	
	Hit3BodyPart[,]	hitterLegs;

	Hit3BodyPart	hitterOuterBody;

	Hit3BodyPart	hitterChinko;
	*/



	//Quaternion	rotMoveDir;



	Animation	anim;

	MotionFader		motion;
	
	
	

	
	// 初期化 ------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();
		//base.combineMesh( true );


		tfCrotch = tfBody.FindChild( "crotch" );

		rbCrotch = tfCrotch.GetComponent<Rigidbody>();


		var tfOuterBody = tfBody.FindChild( "crotch/outerBody" );

		rbOuterBody = tfOuterBody.GetComponent<Rigidbody>();


		var tfInnerBody = tfBody.FindChild( "crotch/outerBody/innerBody" );

		rbInnerBody = tfInnerBody.GetComponent<Rigidbody>();

		jtChinko = tfInnerBody.GetComponent<HingeJoint>();


		var tfSholder = tfBody.FindChild( "crotch/outerBody/sholder" );

		jtSholder = tfSholder.GetComponent<HingeJoint>();


		psBeams = tfSholder.GetComponentsInChildren<ParticleSystem>();


		rbBones = tfBody.GetComponentsInChildren<Rigidbody>();


		//rbSholderUppers = getSholderUpperRigidbodies( tfSholder );

		rbCrotchLowers = getCrotchLowerRigidbodies( tfCrotch );

		rbFullBodies = tfBody.GetComponentsInChildren<Rigidbody>();


		//bodyHitters = tfBody.GetComponentInChildren<Hit3BodyPart>();


		anim = GetComponentInChildren<Animation>();

		ms.deepInit( anim, tfSholder, tfBody );

		ragdoll.deepInit( anim );


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
		shoot.reload( 1 );
		shoot.reload( 2 );
		shoot.reload( 3 );
		shoot.reload( 4 );


		motion.update();

	}

	bool isKinematicChange;

	new protected void FixedUpdate()
	{

		if( isDead ) return;

		/*
		var angle = Vector3.Angle( rb.rotation * Vector3.forward, migration.targetPoint - rb.position );
		Debug.Log( angle );
		if( angle > 90.0f ) rb.rotation *= Quaternion.AngleAxis( angle - angle % 90.0f, Vector3.up );
		*/
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


		if( figure.scaleRate >= 3.0f )
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
	/*
	void setAnimationMixingInBody( Animation anim, Transform[] tfs )
	{
		foreach( var tfThis in tfs )
		{
			//tfThis
		}
	}
	*/
	Rigidbody[] getSholderUpperRigidbodies( Transform tfShorlder )
	{
		return tfShorlder.GetComponentsInChildren<Rigidbody>();
	}

	Rigidbody[] getCrotchLowerRigidbodies( Transform tfCrotch )
	{
		var rbs = new Rigidbody[ 9 ];

		rbs[ 0 ] = tfCrotch.GetComponent<Rigidbody>();

		var irb = 1;

		for( var i = 0 ; i < 5 ; i++ )
		{
			var tfThis = tfCrotch.GetChild( i );

			if( tfThis.name != "outerBody" )
			{
				rbs[ irb++ ] = tfThis.GetComponent<Rigidbody>();
				rbs[ irb++ ] = tfThis.GetChild( 0 ).GetComponent<Rigidbody>();
			}
		}

		return rbs;
	}

	void rollUpBeamTentacles( float endSpeed, float force )
	{
		//jtSholder.useMotor = false;

		var motor = new JointMotor();
		motor.force = force;
		motor.targetVelocity = endSpeed;
		jtSholder.motor = motor;

		//jtSholder.useMotor = true;
	}
	void rollDownBeamTentacles( float force )
	{
		//jtSholder.useMotor = false;

		var motor = new JointMotor();
		motor.force = force;
		motor.targetVelocity = 0.0f;
		jtSholder.motor = motor;

		//jtSholder.useMotor = true;
	}

	void rightUpBeamTentacles()
	{
		//msWalk.AddMixingTransform( jtSholder.transform, true );
		//foreach( var rbThis in rbSholderUppers ) rbThis.isKinematic = false;

		psBeams[0].Play();
		psBeams[1].Play();
		psBeams[2].Play();
		psBeams[3].Play();
	}
	void rightDownBeamTentacles()
	{
		//msWalk.RemoveMixingTransform( jtSholder.transform );
		//foreach( var rbThis in rbSholderUppers ) rbThis.isKinematic = true;

		psBeams[0].Stop();
		psBeams[1].Stop();
		psBeams[2].Stop();
		psBeams[3].Stop();
	}
	/*
	void setKinematicTentacles( int id, bool isKinematic )
	{

	}

	void setKinematicLegs( int id, bool isKinematic )
	{
		
	}

	public void setkinematicPhys()
	{
 
	}
	*/

	public void setKinematicFullBody( bool iskinematic )
	{

		if( rbCrotch.isKinematic != iskinematic )
		{

			if( !iskinematic ) velocityOff();// アニメで蓄積された？反発か何かの余計な力を解消する


			//return;

			anim.animatePhysics = false;//

			//var rbCrotchLowers = tfBody.GetComponentsInChildren<Rigidbody>();
			foreach( var rbbone in rbCrotchLowers )// rbBones )
			{
				rbbone.isKinematic = iskinematic;
			}
			// 通常からキネマティックをオフにしておく箇所が増えるほど、負荷が高くなる。
			// 地形のみのフィールドで、すべてオフだと５０体もでればＦＰＳが１０程度になるが、
			// 全てオンだと７０体でても２５くらいのＦＰＳはでていた（animationPhysics = false なら３０）。
			// 挙動に関しては、オフだと脚が地形に引っかかってダロガらしい感じでよくなるし、安定もいい気がする。
			// なにより脚が床にめり込まないのはかっこいい。
			// 触手の回転は全部アニメーションにしてしまうとう方法もあると思う。
			// ＦＰＳによって物理を効かす最大数が変化するようにしてもいいが、そこまでするならアニメでもいいかも
			// 仲間と接触したり踏まれて荒ぶることが多いので、敵同士で接触しないレイヤにするべき
			

			rbCrotch.isKinematic = iskinematic;

			anim.enabled = iskinematic;

		}
		
	}


	protected void velocityOff()
	{
		//var rbCrotchLowers = tfBody.GetComponentsInChildren<Rigidbody>();
		foreach( var rbbone in rbFullBodies )// rbCrotchLowers )// rbBones )
		{
			rbbone.velocity = Vector3.zero;
			rbbone.angularVelocity = Vector3.zero;
		}
	}
	protected void setMaxMoveForDepenetration()
	{
		//var rbCrotchLowers = tfBody.GetComponentsInChildren<Rigidbody>();
		foreach( var rbbone in rbFullBodies )// rbCrotchLowers )// rbBones )
		{

			rbbone.maxDepenetrationVelocity = 0.1f;
			// めりこみを解消しようとする反発力の限界を決めてしまう。

		}
		var js = tfBody.GetComponentsInChildren<Joint>();
		foreach( var j in js )// rbBones )
		{
			j.enablePreprocessing = false;
			// よくわからんけど安定するらしい
		}
	}
	/*
	protected void turnToQuarterFront()
	{
 
		//var t = new Vector2( migration.targetPoint.x, mig

	}
	*/
	
	// アクション ================================================================================
	
	// stay ----------------------------------------------------
	
	void stay( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
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

		lookAtTarget();

		
		move.lerp( 0.0f, 3.0f );
		
		//rotMoveDir = Quaternion.LookRotation( finder.target.position - rb.position );
		
		
		
		
		if( finder.target.isExists )
		{
			
			state.changeTo( walk );

		}
		else
		{

		}

	}


	// walk --------------------------------------------------

	void walk( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( walking );
			
						
			motion.fadeIn( ms.walk, 0.5f );
			
			//msStand.speed = quickness * figure.scaleRateR;
			
			
			rollUpBeamTentacles( 2000.0f, 1000.01f );

		};
		
		action.last = () =>
		{

			rollDownBeamTentacles( 1000.0f );

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
						connection.notifyAttack( this, finder.target.act );

						//migration.setTimer( 2.0f );	// 準備に２秒かかる
					}

					lookAtTarget();

					state.changeTo( attack );

				}
				break;

		}



		ms.walk.speed = 0.5f * move.speed * figure.scaleRateR;
		//Debug.Log( move.velocity );

		move.lerp( def.quickness, 1.0f );


		output.loudness = move.speed;

	}




	// attack ------------------------------------------------

	void attack( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( attacking );
			

			motion.fadeIn( ms.walk, 0.3f );
			
			//msStand.speed = quickness * figure.scaleRateR;
			
			
			rollUpBeamTentacles( 3000.0f, 2000.01f );
			
			rightUpBeamTentacles();

		};
		
		action.last = () =>
		{

			rollDownBeamTentacles( 1000.0f );
			
			rightDownBeamTentacles();

		};
		
	}
	
	
	void attacking()
	{
		
		mode = finder.toMode( finder.search( 0 ) );


		migration.routine( this, ref finder.target );

		lookAtTarget( 3.0f );


		if( finder.target.isReach( def.reach, ref finder ) )
		{

			if( finder.target.isReach( def.reach * 0.2f, ref finder ) )
			{


				if( shoot.isReady( 4, 0 ) ) shootTarget( 4, 0 );


				if( migration.isGoal( figure.bodyRadius, rb.position ) )
				{
					
					state.changeTo( stay );
					
				}
			}
			else
			{

				if( shoot.isReady( 0, 0 ) ) shootTarget( 0, 0 );
				if( shoot.isReady( 1, 0 ) ) shootTarget( 1, 0 );
				if( shoot.isReady( 2, 0 ) ) shootTarget( 2, 0 );
				if( shoot.isReady( 3, 0 ) ) shootTarget( 3, 0 );

			}

		}


		if( mode.state != BattleState.EnMode.decision )
		{

			state.changeTo( walk );

		}


		ms.walk.speed = 0.5f * move.speed * figure.scaleRateR;
		//Debug.Log( move.velocity );

		move.lerp( def.quickness, 1.0f );


		output.loudness = move.speed;


	}


	// damage ----------------------------------------------------
	
	void damage( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( damaging );

			
			//motion.crossFade( msDamage, 0.2f );
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
			
			RagdollCliper.clipPose( tfCrotch, ragdoll.ms.clip );

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

			RagdollCliper.clipPose( tfCrotch, ragdoll.ms.clip );

			motion.play( ragdoll.ms );

		}
		
		
		output.loudness = 2.0f;
		
	}
	
	
	
	// dead -------------------------------------------------------
	
	void dead( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( deadingStay );

			
			move.velocity = 0.0f;
			

			
			migration.setTimer( 30.0f );
			
			
			//rb.constraints = RigidbodyConstraints.None;
			


			rollDownBeamTentacles( 0.1f );

			
		//	figure.moveCollider.enabled = false;
			
			//rb.isKinematic = false;

		//	rbBones[0].isKinematic = false;

			//switchEnvelopeDeadOrArive( UserLayer._enemyEnvelope, UserLayer._enemyEnvelopeDead );

			//tfBody.animation.enabled = false;
			setKinematicFullBody( false );


			deadize();

		};
		
		action.last = () =>
		{
			
			figure.moveCollider.enabled = true;
			
			//rb.isKinematic = false;
			
			rbBones[1].isKinematic = false;


			velocityOff();
			setKinematicFullBody( true );


			rb.detectCollisions = true;
			foreach( var irb in rbBones ) irb.detectCollisions = true;
			
			//switchEnvelopeDeadOrArive( UserLayer._enemyEnvelopeDead, UserLayer._enemyEnvelope );
			
			rb.constraints = figure.rbDefaultConstraints;
			
			
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

			foreach( var irb in rbBones )
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



	// ------------------------------------------

	protected struct MotionStateHolder
	{

		public AnimationState	stand;

		public AnimationState	walk;

	

		public void deepInit( Animation anim, Transform tfSholder, Transform tfBody )
		{

			stand = anim[ "wait" ];
			stand.blendMode = AnimationBlendMode.Blend;

			walk = anim[ "walk" ];
			walk.blendMode = AnimationBlendMode.Blend;
			walk.speed = 0.5f;



			setMixing( walk, tfBody, tfSholder );

			setMixing( stand, tfBody, tfSholder );


			anim.animatePhysics = false;// true;

		}


		void setMixing( AnimationState ms, Transform tfApply, Transform tfOmit )
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

			//ms.speed = 0.0f;

		}
	}

	protected struct TentaclesState
	{

		public AnimationState	ms;


		public void deepInit( Animation anim )
		{

		}
 
	}
#endif
}
