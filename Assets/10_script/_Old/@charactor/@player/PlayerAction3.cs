using UnityEngine;
using System.Collections;
using System;

public partial class PlayerAction3 : _Action3
{

	public Rigidbody	rbPswicth;//仮






	//public PlayerCamera3	playerCamera	{ get; protected set; }
	public PlayerCamera		cam	{ get; protected set; }

	public _PlayerShoot3	playerShoot		{ get; protected set; }



	[SerializeField]
	PhysicMaterialHolder physMats;




	//public PlayerRagdoll	ragdoll	{ get; protected set; }
	public RagdollSwitcher	ragdoll { get; protected set; }




	MotionCrossFader2D	motion;

	MotionFader	motionUpperBody;

	//Quaternion	rotBodyLocal;	// 論理的な tfBody の向き



	FigureInfo		figure;
	
	PostureState	posture;


	FootGroundUnit	grounder;

	[SerializeField]
	OverBarrieUnit	barrier;

	[SerializeField]
	WallUnit		wall;


	[SerializeField]
	MoveStanceUnit	stance;

	public override float speedRate	{ get { return stance.speedRate; } }



	//public bool isJumpable { get; protected set; }// これあったほうが、二段ジャンプ防げていいかも　空中ジャンプ回数カウントでもいいかも

	public bool	isRunable		{ get; protected set; }

	public bool	isMoveHeadInFps	{ get; protected set; }

	public bool isAvoiding		{ get; protected set; }

	public bool isSmallDash		{ get; protected set; }




	[System.Serializable]
	public struct MoveThresholdDefines
	{
		public float    min;
		public float    max;
		public float    threshold;

		public float    stand;
		public float    walk;
		public float    crawl;
		public float    run;
		public float    avoidance;
	}

	[SerializeField]
	public MoveThresholdDefines moveThresholds;



	PushTimeUnit	dashButton;

	LeftStickUnit	ls;


	
	int	downLevel;

	enum EnDamageLevel
	{
		down,
		panic,
		faint
	}


	public enum EnStance
	{
		stand,
		crawl
	}




	// 初期化 ---------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();



		var meshbody = tf.Find("body/Mesh").gameObject;
		var tpacker = new TexturePacker();//3();
		tpacker.regist( meshbody );
		tpacker.packTextures();
		var	csm = new CombineSkinnedMesh();
		csm.combine( meshbody );
		csm.addRenderer( meshbody, Shader.Find( "Custom/SkinnedMesh_1bone_nolit" ) );
		tfBody.gameObject.SetActive( false );
		tfBody.gameObject.SetActive( true );

		//ragdoll = GetComponentInChildren<PlayerRagdoll>();
		ragdoll = tfBody.getComponentInDirectChildren<RagdollSwitcher>();

		//ragdoll.setup( this );
		ragdoll.deepInit( tfBody.getComponentInDirectChildren<SkinnedMeshRender>(), ragdoll.transform );


		wall.setup();
		

		figure.setup( this );




		cam = GameObject.Find( "players/camera base" ).GetComponent<PlayerCamera>();

		cam.init( rb, tfBody.Find("base/chest/head").GetComponent<Rigidbody>(), rb.rotation.eulerAngles.y, 0.0f );
		

		initMotions();



		playerShoot = GetComponentInChildren<_PlayerShoot3>();

		playerShoot.deepInit( this );


	}

	public override void init()
	{

		base.init();
		
		attachedTeam.setPlayerTeam( 0 );//
		targetTeam.flags = enTeam.all;//


		

		state.init( walk );
		
		dashButton.init();//


		posture.init( rb.rotation, 16.0f );


		figure.init( this );


		//moveThresholds.max = moveThresholds.threshold * runSpeed;//
		move.init( moveThresholds.min, moveThresholds.max, moveThresholds.threshold );



		isMoveHeadInFps = true;

		isRunable = true;


		playerShoot.init();

		playerShoot.available <<= ShootOpenFlag.playerReady;

	}




	// 更新処理 ---------------------------------

	new protected void Update()
	{
		

		grounder.checkOnGround( rb, ref figure );


		if( !isDead )
		{

			verticalPosing();

			playerShoot.checkWapons();

			ls.set();

			move.update();

		}

		//GM.enemys.view.text=(rb.velocity.magnitude*rb.mass).ToString();
		if( !isDead & GamePad._batsu ) { if( ragdoll.isRagdollMode ) { ragdoll.switchToAnimation(); state.changeTo( down ); } else ragdoll.switchToRagdoll(); }



		
		motion.update();

		motionUpperBody.update();



		base.Update();



		checkToRun();


		//if( !isDead )
		if( !cam )
		{

			Radar.setLocation( Radar.enType.second, rb.position, Radar.enClass.normal );

		}


		if( GamePad._start ) init();//



		rbPswicth.position = tfBody.position;//
		//rbPswicth.rotation = tf.rotation;//
		//playerCamera.tf.position = 
	}



	Vector3 prepos;

	new void FixedUpdate()
	{

		stance.setMoveDistancePerSec( rb.position, prepos );

		prepos = rb.position;
		

		base.FixedUpdate();


	}






	// 状態遷移 --------------------------------

	public override void changeToDeadMode()
	{

		state.changeTo( dead );

	}

	public override void changeToBlowingMode( _Action3 attacker, int level )
	{
		
		downLevel = level;

		state.changeTo( blow );
		
	}

	public override void changeToDamageMode( _Action3 attacker )
	{

		//stamina.weary( -time, -5.0f );

		//state.changeTo( down );

	}



	void changeToJumpMode( float force )
	{

		changeToFallMode();

		rb.AddForce( Vector3.up * force, ForceMode.Impulse );

	}

	void changeToFallMode()
	{
		
		if( move.isOver )
		{
			state.changeTo( runJumpAndFall );
		}
		else
		{
			state.changeTo( fall );
		}

	}




	// ポージング関連 ------------------------

	//public bool nowMoving()
	//{
	//	return move.speed > 0.0f;
	//}

	
	public override void playMotion( AnimationState ms )
	{
		
		motion.play( ms );

	}


	public void activateCollider( bool isActive )
	{

		figure.activateCollider( isActive );

	}




	void upperStanceToWapon( float time = 0.1f )
	{

		msNowWaponUpperStance = msWaponUpperStance;

		motionUpperBody.fadeIn( msNowWaponUpperStance, time );

	}

	void upperStanceToFree( float time = 0.1f )
	{
		
		msNowWaponUpperStance = null;

		motionUpperBody.fadeOut( time );

	}



	void verticalPosing()
	{

		// 暫定 そのうちリロード専用の上半身を作る予定
		if( playerShoot.weapons.current.isReloading )
		{

			motionUpperBody.fadeOut( 0.3f );
			
		}
		else
		{

			if( !msNowWaponUpperStance ) return;

			if( msNowWaponUpperStance != msWaponCrawlUpperStance )
			{
				motionUpperBody.fadeIn( msNowWaponUpperStance, 0.3f );
			}
		}

		if( msNowWaponUpperStance && msNowWaponUpperStance.enabled )
		{
			
			var v = cam.verticalAngle * ( 1.0f / 90.0f ) * 0.5f;
			
			msNowWaponUpperStance.time = 1.0f + v;
			
		}

	}



	// （レバーによる）移動方向とカメラ向きのセット ----------------------------------

	// lookAt○○は体の向き	Forwrd = カメラ向き, Direction = 移動方向向き
	// 移動方向				OnGround = 地面の傾きに合わせる, 無記述 = 常に直立（ワールド水平前後左右へ移動）
	// 移動方向の場合分け	Stand = 立ち姿の時は直立, Fit = 常に OnGround, Upright = 常に直立
	// その他、引数なしの場合は moveDir = Vector3.zero と同等。


	/// <summary>
	/// カメラ向きを向いて、立ち姿は直立だが、移動だけ接地面に沿う。
	/// </summary>
	/// <param name="moveDir"></param>
	void lookAtForwardStandOnGround( Vector3 moveDir )
	{
		posture.rot = cam.rotHorizontal;

		stance.direction = ( grounder.rotGroundMovableOnWorld( wall ) * posture.rot ) * moveDir;
	}

	void lookAtForwardStandOnGround()
	{
		posture.rot = cam.rotHorizontal;

		stance.direction = Vector3.zero;
	}


	/// <summary>
	/// カメラ向きを向いて、立ち姿・移動ともに接地面に沿う。
	/// </summary>
	void lookAtForwardFitOnGround( Vector3 moveDir )
	{
		posture.rot = grounder.rotGroundMovableOnWorld( wall ) * cam.rotHorizontal;

		stance.direction = posture.rot * moveDir;
	}

	void lookAtForwardFitOnGround()
	{
		posture.rot = grounder.rotGroundMovableOnWorld( wall ) * cam.rotHorizontal;

		stance.direction = Vector3.zero;
	}


	/// <summary>
	/// カメラ向きを向いて、立ち姿・移動ともに常に直立。
	/// </summary>
	void lookAtForwardUpright( Vector3 moveDir )
	{
		posture.rot = cam.rotHorizontal;

		stance.direction = posture.rot * moveDir;
	}


	/// <summary>
	/// 進行方向を向いて、立ち姿・移動ともに常に直立。
	/// </summary>
	/// <param name="moveDir"></param>
	void lookAtDirectionUpright( Vector3 moveDir )
	{
		stance.direction = cam.rotHorizontal * moveDir;

		posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );
	}



	/// <summary>
	/// 進行方向を向いて、立ち姿は直立だが、移動だけ接地面に沿う。
	/// </summary>
	void lookAtDirectionStandOnGround( Vector3 moveDir )
	{
		var forward = cam.rotHorizontal * moveDir;

		posture.rot = Quaternion.LookRotation( forward, Vector3.up );

		stance.direction = grounder.rotGroundMovableOnWorld( wall ) * forward;
	}


	/// <summary>
	/// 進行方向を向いて、立ち姿は直立、移動は継続する。
	/// </summary>
	void lookAtDirection()
	{
		posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );
	}

	void lookAtDirectionWithZeroCheck()
	{
		if( stance.direction.sqrMagnitude > 0.0f )
		{
			lookAtDirection();
		}
	}



	// カメラ調整 -------------------

	public void changeToStand()
	{

		cam.changeStance( EnStance.stand, 10.0f );

		physMats.change( figure.currentCollider, physMats.stand );
		
	}

	public void changeToCrawl()
	{

		cam.changeStance( EnStance.crawl, 10.0f );
		
	}


	public override void zooming( float ratioR, bool isFps )
	{

		cam.zooming( ratioR );

		cam.changeFpsMode( isFps, isFps ? 15.0f : 5.0f );

	}




	// -----------------------

	void checkToRun()
	{

		if( GamePad._l1 )
		{
			
			dashButton.set( 0.3f );//0.5f );
			
		}
		else if( dashButton.over(GamePad.l1) & grounder.isGround & isRunable & ls.isAbleToRun )
		{
			
			state.changeTo( run );
			
		}
		else if( GamePad.l1_ )
		{
			
			dashButton.reset();
			
		}

	}
	

	bool checkOverBarrier()
	// ちょっとキャスト使い過ぎかもな…
	{
		//if( wall.isTouchWall )
		{

			//var c = colliders[ (int)enColliderPose.crawl ];// 着地姿勢は匍匐と同じコライダ

			var bodyRadius = figure.bodyRadius;//c.radius;
			//Debug.Log( "barria start " + bodyRadius +" "+ c.radius + " " + c.height + " " + c.center );

			var pos = rb.position;// + Vector3.up * moveHeight;//tfBody.position;//


			//var checkOrigin = pos + stance.direction * bodyRadius + Vector3.up * moveHeight;

			//var checkEnd = checkOrigin + stance.direction * bodyRadius;

			//var checkcenter = pos + stance.direction * bodyRadius + Vector3.up * moveHeight;

			//if( Physics.CheckCapsule( checkOrigin, checkEnd, moveHeight * 0.5f, UserLayer.groundForPlayer ) )
			//if( Physics.CheckSphere( checkcenter, moveHeight * 0.7f, UserLayer.groundForPlayer ) )
			// 前方に障害物があるかどうか（いらないねこれ）
			{

				var endpos = pos + stance.direction * bodyRadius;
				//GM.defaultLandings.bodyPiercing.emit( endpos, Vector3.up );
				
				var startpos = endpos + Vector3.up * ( barrier.overHeight - bodyRadius * 1.0f );
				
				var ray = new Ray( startpos, Vector3.down );
				//var ray = new Ray( endpos + Vector3.up * ( barrierLowestHeight + bodyRadius ), Vector3.up );

				var dist = barrier.overHeight - barrier.lowestHeight - bodyRadius * 2.0f;
				/*
				var hit = new RaycastHit();
				for( ; ; )
				{
 
					var isHit = Physics.SphereCast( ray, bodyRadius, out hit, dist, UserLayer.groundForPlayer );
					
					if( !isHit ) break;

					if( isHit ) GM.defaultLandings.bodyPiercing.emit( hit.point, hit.normal );

					ray.origin = new Vector3( startpos.x, hit.point.y - 0.1f, startpos.z );

					dist = hit.point.y - endpos.y - barrierLowestHeight - 0.1f;// -bodyRadius * 1.0f;

				}
				return false;
				 */

				var hits = Physics.SphereCastAll( ray, bodyRadius * 0.9f, dist, UserLayer.groundForPlayer );
				//var hits = Physics.CapsuleCastAll( ray.origin, ray.origin, bodyRadius, ray.direction, dist, UserLayer.groundForPlayer );
				
				if( hits == null ) return false;
				//Debug.Log( hits.Length );

				for( var i = hits.Length ; i-- > 0 ; )
				{
					
					//Debug.Log( i +" "+ hits[ i ].point +" "+ hits[ i ].normal );
					if( Vector3.zero == hits[ i ].point | float.IsNaN( hits[ i ].point.y ) ) continue;

					GM.defaultLandings.bodyPiercing.emit( hits[ i ].point, hits[ i ].normal );
					

					var h = hits[i].point.y;


					// 天井チェック
					var jumpstartpos = pos + Vector3.up * bodyRadius;

					var jumpendpos = new Vector3( pos.x, h + bodyRadius, pos.z );

					var isMaxHeadRoomOk = !Physics.CheckCapsule( jumpstartpos, jumpendpos, bodyRadius * 0.8f, UserLayer.groundForPlayer );

					if( !isMaxHeadRoomOk ) continue;


					// 侵入可能チェック
					var forwardpos = jumpendpos + stance.direction * ( bodyRadius * 1.0f );

					var isForwardingOk = !Physics.CheckSphere( forwardpos, bodyRadius * 0.8f, UserLayer.groundForPlayer );

					if( !isForwardingOk ) continue;


					//Debug.Log( "isMaxHeadRoomOk:" + isMaxHeadRoomOk + " isForwardingOk:" + isForwardingOk );
					//if( !isForwardingOk | !isMaxHeadRoomOk ) continue;//

					barrier.height = h;

					return true;

				}

			}

		}

		return false;
	}



	
	// 移動処理 ==============================================================================


	void physFree()
	{

		//stance.clearMoveDistancePerSec();

		wall.clearTouchWall();


		posture.fitting( rb );

	}


	void physHitCheckWall()
	{


		var moveRadius = figure.moveRadius;//colliders[ nowCollider ].radius;

		var movedist = ( stance.moveSpeed * GM.t.delta );// -bodyRadius * 2.0f;

		
		var up = Vector3.up;//posture.rot * Vector3.up;
		
		var bodypos = rb.position + up * stance.moveHeight;


		if( move.isMovable & stance.direction.sqrMagnitude > 0.0f )
		{
			
			var res =	new RaycastHit();


			var fowardRay = new Ray( bodypos/* + stance.direction * bodyRadius*/, stance.direction );

			if( Physics.SphereCast( fowardRay, moveRadius * 0.8f, out res, movedist, UserLayer.groundForPlayer | UserLayer.outerWall ) )
			//if( Physics.Raycast( fowardRay, out res, movedist + moveRadius, UserLayer.groundForPlayer | UserLayer.outerWall ) )
			{
				// 前方の壁に当る
				

				var n = res.normal;


				var otherLayer = 1 << res.collider.gameObject.layer;
					
				if( ( otherLayer & UserLayer.groundForPlayer ) != 0 )
				{
					wall.checkTuchWall( n );
				}


				//rb.MovePosition( rb.position + stance.direction * ( res.distance - moveRadius ) );// 登れない壁の手前まで

				var pos = res.point + n * ( moveRadius * 1.1f ) + ( rb.position - bodypos );
				// 接触点をコライダ半径と高さ位置で補正する。

				/*
				if( move.speed <= 3.33f )
				{
					var distSride = Time.fixedDeltaTime;//Mathf.Min( movedist - dist, moveRadius );

					var mov = stance.direction * distSride;

					var moveSride = mov - Vector3.Dot( mov, n ) * n;

					pos += moveSride;
				}
				*/

				
				rb.MovePosition( pos );

				
			}
			else
			{

				wall.clearTouchWall();


				rb.MovePosition( rb.position + stance.direction * movedist );

				//stance.setMoveDistancePerSec( movedist );

			}


		}
		else
		{

			//stance.clearMoveDistancePerSec();

		}

		
		posture.fitting( rb );

	}









	// ユーティリティ ================================================

	
	public struct PushTimeUnit
	{
		
		float	time;
		
		
		public void init()
		{
			reset();
		}
		
		public void reset()
		{
			time = float.PositiveInfinity;
		}

		
		public void set( float t )
		{
			time = t;
		}

		/// <summary>
		/// ボタンが押し続けられていれば設定時間を超えたかを返し、離されていれば押下時間をリセットする。
		/// </summary>
		public bool over( bool push )
		{
			if( push )
			{
				time -= GM.t.delta;

				return isOverTime;//0.0f >= time;
			}
			else
			{
				reset();

				return false;
			}
		}
		
		/// <summary>
		/// ボタンが押され続けているかどうかだけを返す。その後、常に時間はリセットされる。
		/// </summary>
		public bool keptOver( bool push )
		{
			var res = push && isProgressing;//!float.IsPositiveInfinity( time );

			reset();

			return res;
		}


		bool isProgressing
		{
			get { return !float.IsPositiveInfinity( time ); }
		}

		bool isOverTime
		{
			get { return 0.0f >= time; }
		}

	}
	
	
	public struct LeftStickUnit
	{
		
		public Vector2	stick;
		
		public float	sqrmag;

		public float	bentTime;

		
		public void set()
		{

			stick = GamePad.ls;
			
			sqrmag = stick.sqrMagnitude;

			if( sqrmag < 0.01f )
			{
				bentTime = Time.time;
			}

		}
		
		public bool isAbleToRun
		{
			get { return sqrmag > 0.5f * 0.5f; }
		}

		public bool isShortBent( float limit )
		{
			return bentTime > Time.time - limit;
		}

	}
	
	public struct MoveUtilityUnit
	{
		
		public Vector3	hdir;
		
		public float	mag;

		
		public MoveUtilityUnit( ref LeftStickUnit ls )
		{
			
			mag = Mathf.Sqrt( ls.sqrmag );

			if( mag == 0.0f ) { hdir = Vector3.zero; return; }

			var mls = ls.stick / mag;
			
			hdir = new Vector3( mls.x, 0.0f, mls.y );

		}
		
	}





	// 子クラス ================================

	protected struct PostureState
	// 体勢の状況
	{

		public Quaternion	rot;	// 回転変更は、この値に行う。フレームの最後に、「一度だけ」この値を rb へ落とし込む。

		public float	fittingRate;	// 実姿勢を論理姿勢にあわせる度合い。dext と異なり、表現上の補間的な意味合い。


		public void init( Quaternion rot, float fittingRate )
		{
			this.rot = rot;

			this.fittingRate = fittingRate;
		}

		public void fitting( Rigidbody rb, float rate = 1.0f )
		{
			var rotRb = rb.rotation;

			if( rotRb != rot )
			{
				rb.MoveRotation( Quaternion.Lerp( rotRb, rot, GM.t.delta * rate * fittingRate ) );
			}
			//else Debug.Log("asigned");

		}

		public Vector3 forward { get { return rot * Vector3.forward; } }
		public Vector3 up { get { return rot * Vector3.up; } }
		public Vector3 right { get { return rot * Vector3.right; } }

		
	}


	public struct FigureInfo
	// 身体情報
	{

		CapsuleCollider[]	colliders;	// 立ち・伏せのコライダー

		int nowCollider;

		public CapsuleCollider currentCollider { get { return colliders[ nowCollider ]; } }


		public RigidbodyConstraints rbDefaultConstraints;	// 活動時の Rigidbody 姿勢拘束。保存用。
		


		public float bodyRadius { get { return currentCollider.radius * scaleRate; } }

		public float moveRadius { get { return currentCollider.radius * scaleRate; } }

		public float groundHookReach { get; private set; }// 足を延ばして足場に引っ付ける距離（胴体の中心から）


		public float	scaleRate;	// 体の大きさから来るスピード等の補正用に

		public float	scaleRateR;


		

		public void changeCollider( EnStance i )
		{

			nowCollider = (int)i;

			//if( !ragdoll.isRagdollMode )
			{

				colliders[ (int)i ].enabled = true;

				colliders[ (int)i ^ 1 ].enabled = false;

			}

		}

		public void activateCollider( bool isActive )
		{

			colliders[ nowCollider ].enabled = isActive;

		}


		public void setup( PlayerAction3 act )
		{

			colliders = act.GetComponents<CapsuleCollider>();
			

			rbDefaultConstraints = act.rb.constraints;

		}

		public void init( PlayerAction3 act )
		{

			scaleRate = Mathf.Max( act.tf.localScale.y, act.tf.localScale.z );

			scaleRateR = 1.0f / scaleRate;


			groundHookReach = bodyRadius * 1.0f;//act.def.groundHookReachRate;

		}
		
	}



	[System.Serializable]
	struct MoveStanceUnit
	{

		public Vector3 direction { get; set; }	// 移動の方向（正規化法線だが、移動してない場合はゼロ）

		public float moveDistancePerSec { get; set; }	// 実際に移動した距離を、一秒あたりに換算した値

		public float	moveHeight; // 移動時のレイを飛ばす高さ

		public float	moveRate;// move.SpeedRate は、歩きを 1.0f としているので、その補正　移動計算時に使う


		/// <summary>
		/// スピードの実効率を取得する。
		/// </summary>
		public float	speedRate { get; private set; }


		/// <summary>
		/// 最終的に計算された移動速度。スティック移動等で使用する。
		/// </summary>
		public float	moveSpeed { get { return moveRate * speedRate; } }


		/// <summary>
		/// 最終的な移動速度をセットする。
		/// </summary>
		/// <param name="moveStickMagnitude">移動スティック操作の強さ</param>
		/// <param name="movableRate">ストッピングの影響率</param>
		public void setSpeed( float moveStickMagnitude, float movableRate )
		{
			speedRate = moveStickMagnitude * movableRate;
		}

		/// <summary>
		/// 最終的な移動速度をゼロにセットする。
		/// </summary>
		public void setSpeedZero()
		{
			speedRate = 0.0f;
		}


		public void clearMoveDistancePerSec()
		{
			moveDistancePerSec = 0.0f;
		}

		public void setMoveDistancePerSec( Vector3 pos, Vector3 prepos )
		{
			//moveDistancePerSec = MathOpt.Sqrt( ( pos - prepos ).sqrMagnitude ) * GM.t.deltaR;
			moveDistancePerSec = ( pos - prepos ).magnitude * GM.t.deltaR;
		}

		public void setMoveDistancePerSec( float dist )
		{
			moveDistancePerSec = dist * GM.t.deltaR;
		}

	}



	struct FootGroundUnit
	{

		/// <summary>
		/// 接地しているかどうかを取得する。
		/// </summary>
		public bool isGround { get; private set; }

		/// <summary>
		/// 接地している面の法線を取得する。接地していなければ Vector3.up を得る。
		/// </summary>
		public Vector3 normal { get; private set; }
		

		/// <summary>
		/// 上方を地面の法線に合わせる回転を取得する。
		/// </summary>
		public Quaternion rotGroundOnWorld { get { return Quaternion.FromToRotation( Vector3.up, normal ); } }

		/// <summary>
		/// 上方を地面の法線に合わせる回転を取得する。ただし、壁とみなせる場合は無回転が返る。
		/// </summary>
		public Quaternion rotGroundMovableOnWorld( WallUnit wall )
		{
			return wall.isMovable( normal ) ? rotGroundOnWorld : Quaternion.identity;
		}


		/// <summary>
		/// 現在接地しているコライダーを取得する。
		/// </summary>
		public Collider groundCollider { get; private set; }


		/// <summary>
		/// 接地情報をメンバに取得する。
		/// </summary>
		/// <param name="rb">キャラクターの Rigidbody</param>
		/// <param name="figure">身体情報</param>
		/// <returns>このオブジェクトのコピー</returns>
		public FootGroundUnit checkOnGround( Rigidbody rb, ref FigureInfo figure )
		{

			var hit = new RaycastHit();

			var r = figure.moveRadius * 0.8f;

			var ray = new Ray( rb.position + Vector3.up * r, Vector3.down );


			isGround = Physics.SphereCast( ray, r, out hit, r + figure.groundHookReach, UserLayer.groundForPlayer );

			normal = isGround ? hit.normal : Vector3.up;	// 接地してない場合は真上


			groundCollider = hit.collider;

			return this;
		}

	}



	[System.Serializable]
	struct OverBarrieUnit
	{
		public float	overHeight; // 障害物をひょい越えで登れる高さ

		public float	lowestHeight;	// これ以上を障壁とみなす

		public float height { get; set; }

	}



	[System.Serializable]
	struct WallUnit
	{

		[SerializeField]
		float	slopeLimit; // 登れない壁の角度

		float cosSlopeLimit { get; set; }

		public bool	 isTouchWall;//		{ get; protected set; }


		public void setup()
		{
			//sinSlopeLimit = Mathf.Sin( Mathf.Deg2Rad * slopeLimit );
			cosSlopeLimit = Mathf.Cos( Mathf.Deg2Rad * slopeLimit );
		}

		public void checkTuchWall( Vector3 n )
		{
			var d = Vector3.Dot( n, Vector3.up );//new Vector3( n.x, 0.0f, n.z ).normalized );

			isTouchWall = d < cosSlopeLimit;//d > cosSlopeLimit;
		}

		public bool isMovable( Vector3 n )
		{
			var d = Vector3.Dot( n, Vector3.up );

			return Mathf.Abs( d ) >= cosSlopeLimit;
		}

		public void clearTouchWall()
		{
			isTouchWall = false;
		}

	}








	// モーション ================================================


	AnimationState	msStand;
	
	AnimationState	msStandStance;
	
	AnimationState	msCrawl;
	
	
	AnimationState	msWalk;
	
	AnimationState	msWalkStance;
	
	AnimationState	msWalkSide;
	
	AnimationState	msCrawlMove;
	
	
	AnimationState	msJump;
	
	AnimationState	msFall;
	
	
	AnimationState	msDash;
	
	AnimationState	msRoll;


	AnimationState	msLanding;

	AnimationState	msBarrierOver;

	
	AnimationState	msDead;

	AnimationState	msDown;

	AnimationState	msJitabata;
	
	AnimationState	msStandup;
	
	AnimationState	msYoroke;


	AnimationState	msSitdown;

	AnimationState	msKneesit;


	
	AnimationState	msWaponUpperStance;
	
	AnimationState	msWaponCrawlUpperStance;
	
	
	
	//AnimationState	msNowWalk;
	
	//AnimationState	msNowStand;
	
	AnimationState	msNowWaponUpperStance;


	//AnimationState	msRagdoll;



	void initMotions()
	{

		var anim = tfBody.GetComponent<Animation>();


		msStand = anim[ "stand standard" ];
		msStand.blendMode = AnimationBlendMode.Blend;
		//msStand.speed = 0.0f;
		msStand.weight = 1.0f;
		msStand.layer = 0;

		msStandStance = anim[ "stand stance" ];
		msStandStance.blendMode = AnimationBlendMode.Blend;
		//msStandStance.speed = 0.0f;
		msStandStance.weight = 1.0f;
		msStandStance.layer = 0;


		msWalk = anim[ "walk" ];
		msWalk.blendMode = AnimationBlendMode.Blend;
		//msWalkNoWapon.speed = 0.0f;
		msWalk.weight = 1.0f;
		msWalk.layer = 0;

		msWalkStance = anim[ "walk stance" ];
		msWalkStance.blendMode = AnimationBlendMode.Blend;
		//msWalkStance.speed = 0.0f;
		msWalkStance.weight = 1.0f;
		msWalkStance.layer = 0;

		msWalkSide = anim[ "walk side" ];
		msWalkSide.blendMode = AnimationBlendMode.Blend;
		//msWalkSide.speed = 0.0f;
		msWalkSide.weight = 1.0f;
		msWalkSide.AddMixingTransform( tfBody.Find( "base/hip" ) );//
		msWalkSide.AddMixingTransform( tfBody.Find( "base" ), false );//
		msWalkSide.layer = 0;


		msCrawl = anim[ "crawl" ];
		msCrawl.blendMode = AnimationBlendMode.Blend;
		//msCrawl.speed = 0.0f;
		msCrawl.weight = 1.0f;
		
		msCrawlMove = anim[ "crawl move" ];
		msCrawlMove.blendMode = AnimationBlendMode.Blend;
		//msCrawlMove.speed = 0.0f;
		msCrawlMove.weight = 1.0f;
		
		
		msJump = anim[ "jump" ];
		msJump.blendMode = AnimationBlendMode.Blend;
		
		msFall = anim[ "fall" ];
		msFall.blendMode = AnimationBlendMode.Blend;
		
		msDash = anim[ "dash" ];
		msDash.blendMode = AnimationBlendMode.Blend;
		msDash.speed = 2.0f;
		
		msRoll = anim[ "rolling" ];
		msRoll.blendMode = AnimationBlendMode.Blend;
		msRoll.speed = 2.0f;
		
		
		msLanding = anim[ "landing" ];
		msLanding.blendMode = AnimationBlendMode.Blend;

				
		msBarrierOver = anim[ "wallover" ];
		msBarrierOver.blendMode = AnimationBlendMode.Blend;

		
		msDead = anim[ "dead" ];
		msDead.blendMode = AnimationBlendMode.Blend;
		
		msDown = anim[ "down" ];
		msDown.blendMode = AnimationBlendMode.Blend;
		
		msJitabata = anim[ "jitabata" ];
		msJitabata.blendMode = AnimationBlendMode.Blend;
		
		msSitdown = anim[ "sitdown" ];
		msSitdown.blendMode = AnimationBlendMode.Blend;
		
		msKneesit = anim[ "kneesit" ];
		msKneesit.blendMode = AnimationBlendMode.Blend;
		
		msStandup = anim[ "standup" ];
		msStandup.blendMode = AnimationBlendMode.Blend;

		msYoroke = anim[ "yoroke" ];
		msYoroke.blendMode = AnimationBlendMode.Blend;



		//msRagdoll = RagdollCliper.createState( anim );
		//msRagdoll.blendMode = AnimationBlendMode.Blend;
		//msRagdoll.speed = 0.0f;


		
		var tfChest = tfBody.Find( "base/chest" );
		
		msWaponUpperStance = anim[ "wapon stance" ];
		msWaponUpperStance.AddMixingTransform( tfChest );
		msWaponUpperStance.layer = 1;
		msWaponUpperStance.speed = 0.0f;
		msWaponUpperStance.weight = 1.0f;


		var tfBase = tfBody.Find( "base" );

		msWaponCrawlUpperStance = anim[ "wapon crawl stance" ];
		msWaponCrawlUpperStance.AddMixingTransform( tfBase );//tfChest );
		msWaponCrawlUpperStance.layer = 1;
		msWaponCrawlUpperStance.speed = 0.0f;
		msWaponCrawlUpperStance.weight = 1.0f;

	}






	// =============================================================-


	[System.Serializable]
	struct PhysicMaterialHolder
	{

		public PhysicMaterial   stand;

		public PhysicMaterial   crawl;

		public PhysicMaterial   dash;

		public PhysicMaterial   rolling;

		public PhysicMaterial   jump;


		public void change( Collider c, PhysicMaterial mat )
		{

			c.material = mat;

		}

	}









}
