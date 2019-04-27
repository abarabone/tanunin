using UnityEngine;
using System.Collections;
using ul = UserLayer;

public class _Action3Enemy : _Action3
{

	public float	classBasedSpeed;	// 種族クラスの基本スピード

	public _CharacterClassDefinition	def;
	//public float	dext;		// 最大旋回性能（度／秒）
	//public float	quickness;	// 基本速度倍率、アニメーションの速度もだいたいこれで。
	//public float	reach;		// 攻撃に転ずる間合いの基本値 これは武器ごとでもいいか？




	public TargetFinder		finder;

	public BattleState		mode;		// モード遷移保持



	protected EnemyShoot3	shoot;

	


	protected PostureState	posture;	// 姿勢管理

	public FigureInfo		figure;		// 身体情報


	protected MoveState		migration;  // 移動先管理


	protected MoveStanceUnit    stance;

	public override float speedRate { get { return stance.speedRate; } }


	protected delegate void CollisionProc( Collision clsn );



	




	// 初期化・基本処理 =========================================

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();


		figure = new FigureInfo( this );


		shoot = GetComponentInChildren<EnemyShoot3>();

		if( shoot ) shoot.deepInit( this );


		rb.solverIterations = 1;//

	}


	public override void init()
	{

		base.init();

		def.init( this );
		
		attachedTeam.setEnemyTeam( 0 );//
		targetTeam.flags = enTeam.maskPlayer;// | enTeam.maskEnemy;//

		finder.init( this );//
		finder.filter.setTargetTeam( targetTeam );//TeamLayer.playerTeam( 0 ) );//



		figure.init( this );

		rb.SetDensity( figure.scaleRate );// さすがに三乗はあれなので妥協ｗ
		//rb.mass = definition != null ? definition.template.rigidbody.mass * figure.scaleRate : rb.mass * figure.scaleRate;



		//move.velocity = def.quickness;


		if( shoot ) shoot.available <<= ShootOpenFlag.enemyReady;


		posture.rot = rb.rotation;


		stance.initBaseSpeed( this );


		state.setPhysics( physFree );//
		shiftToPhysicalMode();//


		GM.enemys.increase();

	}

	public override void deadize()
	{

		base.deadize();

		
		GM.enemys.decrease();

	}

	public override void final()
	{

		if( !isDead ) deadize();

		releaseSelfToPoolOrDestroy();


		base.final();
	}


	new protected void Update()
	{


		base.Update();



		if( !isDead )
		{

			Radar.setLocation( Radar.enType.enemy, rb.position, sizeClass );
			
		}

	}

	new protected void FixedUpdate()
	{

		base.FixedUpdate();

		posture.fitting( rb, move.speedRate );
		
	}


	// 衝突処理 ---------------------------------------------

	protected void OnCollisionEnter( Collision c )
	{

		posture.collision( c );

	}



	// ユーティリティ ==========================================

	// 旋回 -----------------------------

	protected bool lookAtTarget( float dextRate = 1.0f, float limitAngle = 30.0f, bool isForceTurn = false )
	// 秒間旋回限界を超えずにターゲット方向へ旋回する。
	{

		var up = posture.rot * Vector3.up;

		var rbpos = rb.position;

		var rbrot = posture.rot;


		var pl = new Plane( up, rbpos );
		
		var projpos = new Vector3( migration.targetPoint.x, rbpos.y, migration.targetPoint.z );


		if( isForceTurn || pl.GetDistanceToPoint( projpos ) > -3.0f )
		// 相手が自分の腹側（３ｍより遠く）にいる場合は、進行方向を変えない（ビルの向こうとか）
		{
			
			var d = pl.GetDistanceToPoint( migration.targetPoint );

			var p = migration.targetPoint - d * up;

			var line = p - rbpos;


			//if( line != Vector3.zero )
			if( line.sqrMagnitude > figure.moveCollider.radius * figure.moveCollider.radius )
			{

				var rotLook = Quaternion.LookRotation( line, up );


				float ang = Quaternion.Angle( rbrot, rotLook );

				//ang = ang >= 0.0f ? ang : -ang;


				var step = def.dext * dextRate;

				//posture.rot = Quaternion.RotateTowards( rbrot, rotLook, step );
				
				posture.rot = Quaternion.Lerp( rbrot, rotLook, ( step / ang ) * GM.t.delta );


				return ang < limitAngle;

			}

		}


		return false;

	}



	// 方向転換 ----------------

	protected void turnDirection( float direction, float bodyRange, float limitTime )
	{

		var newdir = Quaternion.AngleAxis( direction, posture.up ) * posture.forward * figure.bodyRadius * bodyRange;

		migration.setTargetPoint( rb.position + newdir, limitTime );

	}

	protected void turnDirection( float bodyRange, float limitTime )
	{
		var direction = turnBackAngles[ (int)character.aggressive ] * ( Random.value > 0.5f ? 1.0f : -1.0f );

		turnDirection( direction, bodyRange, limitTime );
	}

	static readonly float[] turnBackAngles = { 180.0f, 135.0f, 90.0f, 45.0f, 0.0f };





	// 射撃 -----------------------------

	protected void shootForward( int waponId, int unitId )
	// とくに修正しないで、まずるの向いている方向に射出する。
	{

		shoot.shoot( waponId, unitId );

	}

	protected void shootTarget( int waponId, int unitId, float angleLimit )
	// 限界角度と距離と地面を考慮して、ターゲット位置へ射撃する。角度は半角度。
	{
		
		if( !shoot.isReady( waponId, unitId ) ) return;

		var wapon = shoot.weapons[ waponId ];


		var line = calculateShootLine( wapon );

			
		var forward = rb.rotation * Vector3.forward;


		var dist = line.magnitude;

		var angle = Mathf.Acos( Vector3.Dot( forward, line / dist ) ) * Mathf.Rad2Deg;

		if( angle > angleLimit )
		{
			var forwardLine = forward * dist;

			line = ( line - forwardLine ) * ( angleLimit / angle ) + forwardLine;
		}


		wapon.muzzleLookAt( line );

		shoot.shoot( waponId, unitId );

	}

	protected void shootTarget( int waponId, int unitId )
	// 距離と地面を考慮して、ターゲット位置へ射撃する。角度は全方位へ向ける。
	{

		if( !shoot.isReady( waponId, unitId ) ) return;

		var wapon = shoot.weapons[ waponId ];


		var line = calculateShootLine( wapon );


		wapon.muzzleLookAt( line );

		shoot.shoot( waponId, unitId );

	}


	Vector3 calculateShootLine( _Weapon3 wapon )
	{

		var pos = wapon.tfMuzzle.position;

		var line = migration.targetPoint - pos;



		if( finder.target.isExists )
		{

			var actpos = finder.target.act.tfObservedCenter.position;

			var aline = actpos - pos;

			var reach = figure.bodyRadius * 3.0f;


			if( aline.sqrMagnitude < reach * reach )
			{
				// あまりにも近い場合は誤差なしで

				line = aline;

			}
			else
			{
				
				var up = rb.rotation * Vector3.up;

				var ad = Vector3.Dot( up, aline );

				if( ad >= 0.0f )
				{
					// 実際に相手が地面の下にいるわけではない
					
					var d = Vector3.Dot( up, line );

					if( d < 0.0f ) line -= up * d;	// 地面にめり込む場合は補正する

				}

			}

		}


		return line;

	}




	// 他 -----------------------------

	protected bool isGround()
	{

		var hitSphereRadius = figure.moveRadius;

		return Physics.CheckSphere( rb.position, hitSphereRadius, UserLayer.groundForEnemy );
		
	}





	

	// 移動関連 ========================================================================
	
	// 姿勢・位置関係の処理には、
	// 　・FixedUpdate() で処理される移動ルーチン( physXxxx() )：主に壁移動と物理移動のそれぞれの姿勢・位置決定
	// 　・OnCollision() で処理されるコライダ衝突ルーチン( collisionXxxx() )：主に壁移動中の物理移動時、壁接地を検出する
	// 　・キネマティック・コライダオンオフ等の設定ルーチン( shiftToXxxxMode() )：壁移動と物理移動のシフト
	// がある

	// 壁移動等の非物理移動があるキャラは、最低限 collisionXxxx() をセットすればいい。
	// 　そうすれば、壁に接地した時にそちらで physXxxx() と shiftToXxxxMode() をやってくれる。

	// ちなみに、Stay は Move の限定状態。無駄な処理を省いたモードに過ぎない。



	// shiftPhysicalMode ------------------------------------

	protected void shiftToKinematicMode()
	{
		figure.moveCollider.enabled = false;// detectCollisions だけでなくこれもやると微高速

		rb.isKinematic = true;

	//	rb.detectCollisions = false;

		posture.resetDefermentOnFoothold();
	}

	protected void shiftToPhysicalMode()
	{
		figure.moveCollider.enabled = true;

		rb.isKinematic = false;

	//	rb.detectCollisions = true;
	}


	
	// collision -------------------------------------------

	protected void collisionWallInStay( Collision clsn )
	{

		collisionWall( clsn, physStayOnWall );

	}

	protected void collisionWallInMove( Collision clsn )
	{

		collisionWall( clsn, physMoveOnWall );
		
	}

	void collisionWall( Collision clsn, _Action3.ActionStateUnit.PhysicsProc physProc )
	{

		if( !isDead && !rb.isKinematic )
		{

			var layer = 1 << clsn.collider.gameObject.layer;

			if( ( layer & ul.groundForEnemy ) != 0 )
			// 着地した
			{

				var up = posture.rot * Vector3.up;//tf.up

				posture.rot = Quaternion.FromToRotation( up, clsn.contacts[ 0 ].normal ) * posture.rot;

				rb.MovePosition( clsn.contacts[ 0 ].point );

				state.setPhysics( physProc );
				shiftToKinematicMode();

			}

		}
	}




	// physProc ----------------------------------------------------------

	protected void physFree()
	{}


	protected void physStayOnWall()
	{

		var rc = new UnderRaycaster( ref posture, ref figure, this );


		var res =	new RaycastHit();

		if( rc.raycastUnder( out res ) )
		{
			// 接地してた

			posture.rot = Quaternion.FromToRotation( rc.up, res.normal ) * posture.rot;

			rb.MovePosition( res.point );

		}
		else
		{
			// 落下へ

			state.setPhysics( physFree );
			shiftToPhysicalMode();

		}
		
	}


	protected void physMoveOnWall()
	{

		var rc = new ForwawrdAndUnderRaycaster( ref posture, ref figure, this );


		var res =	new RaycastHit();

		if( rc.raycastForward( out res ) )
		{
			// 前方の壁に当る

			var otherLayer = 1 << res.collider.gameObject.layer;

			if( ( otherLayer & ul.groundForEnemy ) != 0 )
			{
				// 障害物との衝突だった

				posture.rot = Quaternion.FromToRotation( rc.up, res.normal ) * posture.rot;

				//var groundMove = ( res.point - rb.position ) * GM.t.delta * 30.0f;// *figure.scaleRate;

				//rb.MovePosition( rb.position + groundMove );// 前進
				rb.MovePosition( res.point );// 壁まで

			}
			else
			{
				// 敵同士の衝突だった

				posture.rot = Quaternion.LookRotation( Vector3.Reflect( rc.forward, res.normal ), rc.up );//forward - Vector3.Dot(forward, res.normal) * res.normal, up );
				

				//var dir = Vector3.Reflect( rc.forward, res.normal );

				//moves.targetPoint = rb.position + dir * figure.moveCastRadius;// *2.0f;

				//moves.setTimer( 1.0f );


				//rb.MovePosition( rb.position + rc.forward * ( res.distance - rc.moveRadius ) );// 衝突相手の手前まで

			}


			posture.resetDefermentOnFoothold();

		}
		else
		{

			if( rc.raycastUnder( out res ) )
			{
				// 接地してた

				posture.rot = Quaternion.FromToRotation( rc.up, res.normal ) * posture.rot;

				var groundMove = ( res.point - rb.position ) * GM.t.delta * 30.0f;// *figure.scaleRate;
				// 少しずつ地面に近寄る

				rb.MovePosition( rb.position + groundMove + rc.forward * rc.movedist );// 前進


				posture.resetDefermentOnFoothold();

			}
			else
			{
				// 中空

				var right = Vector3.Cross( rc.up, rc.forward );

				if( posture.isNoFoothold )
				{
					// 落下モードへ移行

					posture.rot = Quaternion.AngleAxis( -180.0f, right ) * posture.rot;	// もどす

					state.setPhysics( physFree );
					shiftToPhysicalMode();

				}
				else if( posture.isFindingFoothold() )
				{
					// 足場探して回転

					posture.rot = Quaternion.AngleAxis( 90.0f, right ) * posture.rot;

					//rb.MovePosition( rb.position + rc.forward * rc.movedist );// 前進

				}
				else
				{
					// 落下中	

				}

			}

		}
	}



	struct UnderRaycaster
	{

		public Vector3	up;
		public Vector3	bodypos;

		public float	hookingReach;

		public UnderRaycaster( ref PostureState posture, ref FigureInfo figure, _Action3Enemy act )
		{

			hookingReach = figure.groundHookReach;

			up = posture.rot * Vector3.up;

			bodypos = act.rb.position + up * figure.moveRadius;

		}


		public bool raycastUnder( out RaycastHit hit )
		{

			var underRay = new Ray( bodypos, -up );

			//retunr Physics.SphereCast( underRay, moveCollider.radius, out res, moveCastRadius, ul.groundForEnemy );
			
			return Physics.Raycast( underRay, out hit, hookingReach, ul.groundForEnemy );

		}

	}

	struct ForwawrdAndUnderRaycaster
	{

		UnderRaycaster	r;

		public float	movedist;
		public float	moveRadius;
		
		public Vector3	forward;
		public Vector3	up			{ get { return r.up; } set { r.up = value; } }
		public Vector3	bodypos		{ get { return r.bodypos; } set { r.bodypos = value; } }
		public Vector3	forwardOrigin;

		public ForwawrdAndUnderRaycaster( ref PostureState posture, ref FigureInfo figure, _Action3Enemy act )
		{

			r = new UnderRaycaster( ref posture, ref figure, act );


			movedist = act.stance.moveSpeed * GM.t.delta;


			moveRadius = figure.moveRadius;


			forward = posture.rot * Vector3.forward;

			forwardOrigin = posture.isGotFoothold ? r.bodypos : r.bodypos - forward;

		}


		public bool raycastForward( out RaycastHit hit )
		{

			//if( movedist == 0.0f ) { hit = new RaycastHit(); return false; }

			var fowardRay = new Ray( forwardOrigin, forward );

			return Physics.Raycast( fowardRay, out hit, movedist + moveRadius, ul.groundForEnemy | ul.outerWall | ul.enemyEnvelope | ul.enemyEnvelopeDead );//| ul.enemyEnvelopeMove ) );

		}

		public bool raycastUnder( out RaycastHit hit )
		{

			return r.raycastUnder( out hit );

		}

	}




	// 子クラス =========================================================

	
	protected struct PostureState
	// 体勢の状況
	{

		public Quaternion	rot;	// 回転変更は、この値に行う。フレームの最後に、「一度だけ」この値を rb へ落とし込む。

		public float	fittingRate;	// 実姿勢を論理姿勢にあわせる度合い。dext と異なり、表現上の補間的な意味合い。


		public void fitting( Rigidbody rb, float rate = 1.0f )
		{
			rb.MoveRotation( Quaternion.Lerp( rb.rotation, rot, GM.t.delta * rate * fittingRate ) );	
		}

		public Vector3 forward	{ get { return rot * Vector3.forward; } }
		public Vector3 up		{ get { return rot * Vector3.up; } }
		public Vector3 right	{ get { return rot * Vector3.right; } }


		

		CollisionProc	collisionProc;	// 現在の対障害物ルーチン。


		public void setCollision( CollisionProc cproc )
		{
			collisionProc = cproc;
		}

		public void collision( Collision c )
		{
			if( collisionProc != null ) collisionProc( c );
		}




		int		fallDeferment;	// 進行方向の足場がない時、９０度単位の回り込み壁探査をあと何回残すか。

		const int	defaultFallDeferment = 2;

		public bool isGotFoothold { get { return fallDeferment == defaultFallDeferment; } }

		public bool isNoFoothold { get { return fallDeferment <= 0; } }

		public bool isFindingFoothold() { return fallDeferment-- > 0; }

		public void resetDefermentOnFoothold() { fallDeferment = defaultFallDeferment; }

	}


	public struct FigureInfo
	// 身体情報
	{

		public SphereCollider	moveCollider;	// 対障害物用コライダへの参照キャッシュ。

		//public SphereCollider	bodyCollider;	// 対爆発物コライダへの参照キャッシュ。ボディサイズを参照する。


		public RigidbodyConstraints	rbDefaultConstraints;	// 活動時の Rigidbody 姿勢拘束。保存用。


		public float bodyRadius { get; private set; }//{ get { return bodyCollider.radius * scaleRate; } }

		public float moveRadius { get { return moveCollider.radius * scaleRate; } }

		public float groundHookReach { get; private set; }// 足を延ばして足場に引っ付ける距離（胴体の中心から）


		public float	scaleRate;	// 体の大きさから来るスピード等の補正用に

		public float	scaleRateR;


		public FigureInfo( _Action3Enemy act ) : this()
		{

			moveCollider = act.tfBody.GetComponent<SphereCollider>();

			//bodyCollider = act.tfBody.findWithLayer( UserLayer._enemyEnvelope ).GetComponent<SphereCollider>();


			rbDefaultConstraints = act.rb.constraints;

		}

		public void init( _Action3Enemy act )
		{

			scaleRate = Mathf.Max( act.tf.localScale.y, act.tf.localScale.z );

			scaleRateR = 1.0f / scaleRate;


			var bodyObject = act.tfBody.findWithLayer( UserLayer._enemyEnvelope );//

			bodyRadius = bodyObject != null ? bodyObject.GetComponent<SphereCollider>().radius * scaleRate : moveRadius;//


			groundHookReach = bodyRadius * act.def.groundHookReachRate;

		}

	}


	protected struct MoveState
	// 進行方向・目標
	{

		public Vector3 targetPoint;

		float	limitTime;	// どうしてもたどり着けない場合のための時間制限など



		// 移動先計算 ---------------------

		public bool routine( _Action3Enemy self, ref TargetFinder.Target target )
		{
			if( !isGoalOrLimit( ref self.figure, self.rb.position ) ) return false;

			if( target.isExists )
			{
				setTargetPoint( self, ref target );
			}
			else
			{
				setTargetPoint( self );
			}

			return true;
		}

		public void setTargetPoint( Vector3 newTarget, float limit )
		{
			targetPoint = newTarget;

			setTimer( limit );
		}

		public void setTargetPoint( _Action3Enemy self, ref TargetFinder.Target target )
		{
			targetPoint = target.imaginaryPosition;//self.rb.position + ( target.imaginaryPosition - self.rb.position ) * 0.5f;

			setTimer( Random.Range( 0.5f, 3.0f ) );
		}

		public void setTargetPoint( _Action3Enemy self )
		{

			var group = self.connection.group;

			if( group != null && group.isOutOfTerritorySelf( self ) )
			{
				// グループ外なので内に戻る

				targetPoint = group.getRandomPoint();
				

				setTimer( 8.0f );

			}
			else
			{
				// グループ内部での適当移動（グループに所属しない場合もこちら）

				var maxMovableBodyRange = movableRanges[ (int)self.character.aggressive ] * self.figure.bodyRadius * 2.0f;

				targetPoint = self.rb.position + Random.onUnitSphere * Random.Range( maxMovableBodyRange * 0.1f, maxMovableBodyRange );


				setTimer( Random.Range( 2.0f, movableTimes[ (int)self.character.obedience ] ) );

			}
			
		}

		static readonly float[]	movableTimes = { 20.0f, 10.0f, 7.0f, 5.0f, 3.0f };
		// 移動限界時間。集団性が低いほど長い時間移動できる可能性がある。最低２秒は固定で、最大を列記する。

		static readonly float[]	movableRanges = { 5.0f, 6.0f, 7.0f, 10.0f, 100.0f };
		// 移動範囲の馬身半径。攻撃性が高いほど遠くへ移動する可能性がある。
		


		// 限界時間 -----------------

		public void setTimer( float limitAfter )
		{
			limitTime = Time.time + limitAfter;
		}

		public void resetTimer()
		{
			limitTime = 0.0f;
		}

		public bool isLimitOver
		{
			get { return Time.time > limitTime; }
		}



		// 到着判定 ---------------------------

		public bool isGoal( float limit, Vector3 position )
		{

			return ( targetPoint - position ).sqrMagnitude < limit * limit;

		}

		public bool isGoalOrLimit( ref FigureInfo figure, Vector3 position )
		{
			var bodySize = figure.moveCollider.radius * 2.0f;

			return isLimitOver || isGoal( bodySize, position );
		}

	}


	[System.Serializable]
	public struct BattleState
	// 戦闘モードの状況
	{


		public EnMode	state;


		public BattleState( ref TargetFinder.Target target )
		{
			var decisionOn = target.isDecision ? 4 : 0;//.GetHashCode() << 2;
			var existsOn = target.isExists ? 2 : 0;//.GetHashCode() << 1;
			var battleOn = target.isRelease ? 0 : 1;//(!target.isRelease).GetHashCode();

			state = (EnMode)( decisionOn | existsOn | battleOn );
		}

		/*
		public bool isDecisionTrigger( ref TargetFinder.Target target )
		{
			return isDecisionEnemy ^ target.isDecision;
		}

		public bool isChaseTrigger( ref TargetFinder.Target target )
		{
			return isChaseEnemy ^ target.isChase;
		}
		*/

		public enum EnMode // decision | exists | battle
		{
			dxx100 = 4,
			dxb101 = 5,
			wait = 0,		// 000
			doubt = 2,		// 010
			alert = 1,		// 001
			decision = 7,	// 111
			weary = 6,		// 110
			lost = 3		// 011
		}


		public bool isBattling { get { return ( (int)state & 0x1 ) != 0; } }

		public bool isDecisionEnemy { get { return ( (int)state & 0x4 ) != 0; } }
		
		public bool isExistsEnemy { get { return ( (int)state & 0x2 ) != 0; } }

		public bool isChaseEnemy { get { return ( (int)state & 0x3 ) == 0x3; } }


		public bool isWait { get { return state == EnMode.wait; } }

		public bool isAlert { get { return state == EnMode.alert; } }

		public bool isDoubtEnemy { get { return state == EnMode.doubt; } }

		public bool isLostEnemy { get { return state == EnMode.lost; } }

		public bool isWeary { get { return state == EnMode.weary; } }

	}



	protected struct MoveStanceUnit
	{
		
		/// <summary>
		/// quickness と classBasedSpeed とボディサイズを計算した基本速度値。
		/// </summary>
		public float baseSpeed { get; private set; }
		
		/// <summary>
		/// 最後に設定されたアクションの固有速度。
		/// </summary>
		public float actionSpeed { get; private set; }


		/// <summary>
		/// moveMagnitude * movableRate による、移動の実効値を保存する。
		/// </summary>
		public float speedRate { get; private set; }


		/// <summary>
		/// 最終的に計算された移動速度。物理計算でない移動で使用する。
		/// </summary>
		public float moveSpeed { get { return baseSpeed * actionSpeed * speedRate; } }




		/// <summary>
		/// 初期化時以外変化しない速度値を設定する。
		/// </summary>
		public void initBaseSpeed( _Action3Enemy act )
		{
			baseSpeed = act.classBasedSpeed * act.def.quickness * act.figure.scaleRate;
		}

		/// <summary>
		/// 動作ごとの固有速度を設定する。
		/// </summary>
		public void setActionSpeed( float currentActionSpeed )
		{
			actionSpeed = currentActionSpeed;
		}

		/// <summary>
		/// 各速度関係値を設定する。
		/// </summary>
		/// <param name="moveMagnitude">移動の強さ 0.0f～1.0f</param>
		/// <param name="movableRate">ストッピングの影響率</param>
		public void setSpeed( float moveMagnitude, float movableRate )
		{
			speedRate = moveMagnitude * movableRate;
		}

		/// <summary>
		/// 各速度関係値を設定する。
		/// </summary>
		/// <param name="currentActionSpeed">動作ごとの固有速度</param>
		/// <param name="moveMagnitude">移動の強さ 0.0f～1.0f</param>
		/// <param name="movableRate">ストッピングの影響率</param>
		public void setSpeed( float currentActionSpeed, float moveMagnitude, float movableRate )
		{
			setActionSpeed( currentActionSpeed );

			setSpeed( moveMagnitude, movableRate );
		}


		/// <summary>
		/// 計算移動速度をゼロにセットする。
		/// </summary>
		public void setSpeedZero()
		{
			speedRate = 0.0f;
		}


		/// <summary>
		/// アニメーションスピードを計算する。quickness * scaleRateR を計算する。
		/// </summary>
		public float getMotionSpeed( _Action3Enemy act )
		{
			return act.def.quickness * act.figure.scaleRateR;
		}

		/// <summary>
		/// アニメーションスピードを計算する。
		/// </summary>
		public float getMotionSpeed( _Action3Enemy act, float currentActionSpeed, float movableRate )
		{
			return getMotionSpeed( act ) * currentActionSpeed * movableRate;
		}

	}
}



