using UnityEngine;
using System.Collections;
using System;

public abstract class _Action3 : _PoolingObject3<_Action3>//, IMissionEventable
{

	
	public Radar.enClass	sizeClass;

	public TeamLayer	attachedTeam;	// 自分の所属チーム

	public TeamLayer	targetTeam;		// 攻撃目標のチーム


	public ConnectionLinker	connection;

	public CharacterInfo	character;


	[SerializeField]
	public MoveStoppingUnit	move;



	public _Hit3	hitter		{ get; protected set; }


	public Rigidbody	rb		{ get; protected set; }

	public Transform	tfBody	{ get; protected set; }

	public Transform	tfObservedCenter;	// 観測される中心点（主に視認に使用される点）

	//public Transform	tfVisualMuzzle;		// エフェクト等見た目に使用する銃口



	//public CharactorSoundUnit	sounds;



	
	public OutSourceUnit	output;

	
	
//	public EventTargetLinkerHolder<_ActionBase>	events;
	
	
	
	public bool	isDead	{ get; set; }	// 死んでる


	
	public ActionStateUnit	state;



	public SkinnedMeshRender	smr { get; protected set; }





	public abstract float speedRate { get; }	// スピードの実効値を取得する。ストッピング計算に使用するだけなので、微妙。







	// メッシュ単一化（使わなくてもいい） ----------------------------------------------
	
	public virtual GameObject combineMesh( bool isForcePackTexture = false )
	{

		var tfmeshbody = transform.Find( "body/Mesh" );


		var meshbody = tfmeshbody.gameObject;


		if( isForcePackTexture )
		// 単体でテクスチャをパックしてしまうか
		{
			var tpacker = new TexturePacker();
			
			tpacker.regist( meshbody );
			
			tpacker.packTextures();
		}
		

		var	csm = new CombineSkinnedMesh();
		
		csm.combine( meshbody );
		
		csm.addRenderer( meshbody, Shader.Find( "Custom/SkinnedMesh_1bone_nolit" ) );


		return meshbody;
	}


	// 不要なボーンを破棄する（使わなくてもいい）---------------------------

	public void trimTransforms( Transform[] tfBones )
	// Rigidbody のないボーンオブジェクトを除去する。ジョイント接続されている剛体は階層から外す。
	// 雛形の場合はトリム後に複製してもよい。
	{

		for( var i = tfBones.Length ; i-- > 0 ; )// Unity の仕様では明記されてないと思うが、後ろ＝末端
		{

			var tfBone = tfBones[ i ];

			if( tfBone == null ) continue;


			if( tfBone.GetComponent<Rigidbody>() )
			{
				if( tfBone.GetComponent<Joint>() )
				{
					tfBone.parent = null;
				}
			}
			else
			{
				for( var j = 0 ; j < tfBone.childCount ; j++ )// 念のため
				{
					tfBone.GetChild( j ).SetParent( tfBone.parent, true );
				}

				GameObject.Destroy( tfBone.gameObject );
			}
		}

	}





	// 遷移関数 -----------------------------------------------
	
	public virtual void changeToWaitMode()
	{}
	
	public virtual void changeToAttackMode( _Action3 attacker )
	{}
	
	public virtual void changeToAlertMode()
	{}

	public virtual void changeToFearMode()
	{}

	public virtual void changeToDamageMode( _Action3 attacker )
	{}

	public virtual void changeToBlowingMode( _Action3 attacker, int level )
	{}

	public virtual void changeToDeadMode()
	{}





	public virtual void zooming( float ratioR, bool isFps )
	{}





	// 更新処理 -----------------------------------------------

	protected void Update()
	{

		if( !isDead ) move.update();

		hitter.update();

		state.updateAction();

	}

	protected void FixedUpdate()
	{

		state.updateMove();

	}


	public virtual void playMotion( AnimationState ms )
	{}



	// 初期化 -------------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();


		rb = GetComponent<Rigidbody>();


		tfBody = tf.Find( "body" );


		hitter = GetComponent<_Hit3>();


		state = new ActionStateUnit();

	}

	
	public override void init()
	{

		base.init();

		hitter.init();


		isDead = false;

		smr = tfBody.getComponentInDirectChildren<SkinnedMeshRender>();

		move.setSpeedRate( 1.0f );


		if( !rb.isKinematic )
		{
			rb.isKinematic = true;
			
			rb.isKinematic = false;
		}

	}


	public virtual void deadize()
	// 死亡させる時に呼ぶ（した後ではない）。
	{

		isDead = true;


		if( connection.group != null )
		{
			connection.group.leave( this );
		}

	}


	protected void Start()
	// プールされていないインスタンスのため
	{

		if( poolingLink == null ) init();

	}





	// 小クラス ==================================================================================

	
	public class ActionStateUnit
	//public struct ActionStateUnit
	{
		
		ActionHolder	currentAction;
		
		ActionHolder	nextAction;


		ActionProc	actionProc;
		
		PhysicsProc	physicsProc;


		public struct ActionHolder
		{

			public ActionProc	first;

			public ActionProc	last;


			public void shiftToIdle()
			{
				first = _idle;
				
				last = _idle;
			}

		}
		
		public delegate void ActionInitProc( ref ActionHolder action );

		public delegate void ActionProc();

		public delegate void PhysicsProc();



		public void init( ActionInitProc currentInit )
		{

			currentAction = new ActionHolder();

			nextAction = new ActionHolder();


			currentAction.shiftToIdle();
			
			nextAction.shiftToIdle();

			actionProc = _idle;
			
			physicsProc = _idle;


			currentInit( ref currentAction );


			currentAction.first();

		} 



		
		public void changeTo( ActionInitProc nextInit )
		{

			nextInit( ref nextAction );

			shiftTo( _change );
			
		}
		
		public void shiftTo( ActionProc act )
		{
			
			actionProc = act;
			
		}
		
		public void setPhysics( PhysicsProc phys )
		{
			
			physicsProc = phys;
			
		}
		
		public void updateAction()
		{
			
			actionProc();
			
		}
		
		public void updateMove()
		{
			
			physicsProc();
			
		}


		public void changeToIdle()
		{

			nextAction.first = _idle;

			nextAction.last = _idle;

			
			shiftTo( _change );

		}



		static public void _idle()
		{}

		
		void _change()
		{

			currentAction.last();


			currentAction = nextAction;

			nextAction.shiftToIdle();

			actionProc = _idle;

			physicsProc = _idle;


			currentAction.first();


			actionProc();

		}


	}





	[System.Serializable]
	public struct CharacterInfo
	{

		public EnPersistence	persistence;	// 偏執性　一人の敵を追い続けるか

		public EnObedience		obedience;		// 集団性　グループの傘下に居続けるか

		public EnAggressive		aggressive;		// 攻撃性



		/// <summary>
		/// 偏執性。
		/// </summary>
		public enum EnPersistence : byte
		{ none, low, middle, high, perfect }
		// none は毎回 scan する、perfect はどんな横やりを入れられてもターゲットを変えない（タイムアウトまで）。

		
		/// <summary>
		/// 集団性。
		/// </summary>
		public enum EnObedience : byte
		{ none, low, middle, high, perfect }
		// none は攻撃後は野良になる、perfect は攻撃時も群れから離れない。
		// none は通知を一切受け取らない。※送信は常にする。通知関係の性格影響は、受け取る側の問題。
		// group == null があるので、none でも完全フリーではない。

		
		/// <summary>
		/// 攻撃性。
		/// </summary>
		public enum EnAggressive : byte
		{ none, low, middle, high, perfect }
		// none は攻撃しない状態、perfect は一旦攻撃に移ったら二度と戻らない。



		// 性格による値 --------------

		/// <summary>
		/// アラート通知を受け取るテリトリーのマージン。集団性が高い個体ほど大きいとする。
		/// </summary>
		public float notifyRecvRanges { get { return _notifyRecvRanges[ (int)obedience ]; } }

		static readonly float[] _notifyRecvRanges =
		{ 0.0f, 0.0f, 10.0f, 30.0f, float.PositiveInfinity };


		/// <summary>
		/// 発信個体周辺のグループメンバーが、通知を受け取れる範囲。受信者の集団性が高いほど広い。
		/// </summary>
		public float notifyRecvOuterRanges { get { return _notifyRecvOuterRanges[ (int)obedience ]; } }

		static readonly float[] _notifyRecvOuterRanges =
		{ 0.0f, 30.0f, 60.0f, 120.0f, float.PositiveInfinity };


		/// <summary>
		/// テリトリーから何馬身出ても気にしないか。集団性が高いほど狭いとする。
		/// </summary>
		public float outMargins { get { return _outMargins[ (int)obedience ]; } }

		static readonly float[] _outMargins =
		{ 30.0f, 15.0f, 5.0f, 0.0f, 0.0f };


	}






	public struct ConnectionLinker
	{

		public ActivityGroup3	group;
		
		public TeamLayer		team;



		public void enterToGroup( ActivityGroup3 gr, _Action3 self, bool isLeader = false )
		{
			group = gr;

			group.enter( self, isLeader );
		}

		public void leaveForGroup( _Action3 self )
		{
			if( group != null )
			{
				group.leave( self );
			}
		}



		public void notifyAlert( _Action3 self )
		{
			if( group != null && !group.isOutOfTerritory( self.rb.position ) )
			{
				group.notifyAlert( self );
			}
		}


		public void notifyAttack( _Action3 self, _Action3 attacker )
		{
			if( group != null && !group.isOutOfTerritory( self.rb.position ) )
			{
				group.notifyAlert( attacker, self );
			}
		}

		public void notifyAttack( _Action3 self, _Action3 attacker, _Action3Enemy.BattleState mode )
		{

			if( mode.isChaseEnemy ) return;	// 追跡中は通知を出さない


			if( mode.isAlert )
			{
				// 警戒中は近辺の仲間への通知

				if( group != null )
				{
					group.notifyAlertToNearMate( attacker, self );
				}

			}
			else
			{
				// 疑敵中は無条件で通知、他は自分がテリトリー内にいる場合のみ通知

				if( mode.isDoubtEnemy || group != null && !group.isOutOfTerritory( self.rb.position ) )
				{
					group.notifyAlert( attacker, self );
				}

			}
		}

	}









	public struct OutSourceUnit
	{

		public float	visibility;	// 視認性

		public float	loudness;	// 発している音の大きさ
		
		public float	heatness;	// 発している熱の大きさ
		
		public float	smellness;	// 発している匂いの大きさ
	
		public float	factQ;		// 発しているＱ要素の大きさ
		

		public void set( float v, float l, float h, float s, float q )
		{
			visibility	= v;
			loudness	= l;
			heatness	= h;
			smellness	= s;
			factQ		= q;
		}

	}





	protected struct MotionUnit2D
	{
		
		public AnimationState	motionX;
		
		public AnimationState	motionY;
		
		
		float	nowWeight;
		
		float	targetWeight;
		
		float	deltaWeight;
		
		Vector2	dirWeight;
		
		
		bool isVectorMode;
		
		
		public MotionUnit2D( AnimationState m, float time, float targ, bool isVec ) : this( m, null, time, targ, isVec ){}
		
		public MotionUnit2D( AnimationState mX, AnimationState mY, float time, float targ, bool isVec )
		{
			
			motionX = mX;
			
			motionY = mY;
			
			motionX.time = 0.0f;
			
			
			if( time != 0.0f )
			{
				
				nowWeight = 0.0f;
				
				targetWeight = targ;
				
				deltaWeight = targ / time;
				
			}
			else
			{
				
				nowWeight = targ;
				
				targetWeight = targ;
				
				deltaWeight = 0.0f;
				
			}
			
			
			dirWeight = Vector2.zero;
			
			isVectorMode = isVec;
			
		}
		
		public void move( float dir, float speed )
		{
			
			var sign = dir > 0.0f? 1.0f: -1.0f;
			
			dirWeight.x = dir * sign;
			
			motionX.speed = speed * sign;
			
		}
		
		public void move( Vector2 dir, float speed )
		{
			
			var signx = dir.x > 0.0f? 1.0f: -1.0f;
			
			dirWeight.x = dir.x * signx;
			
			motionX.speed = speed * signx;
			
			
			var signy = dir.y > 0.0f? 1.0f: -1.0f;
			
			dirWeight.y = dir.y * signy;
			
			motionY.time = dir.y > 0.0f ^ dir.x > 0.0f ? ( motionX.length - motionX.time + motionX.length * 0.5f ) % motionX.length : motionX.time;
			
		}
		
		public void antiMove( ref MotionUnit2D other, float dirsMax )
		{
			
			this.nowWeight = ( dirsMax - ( other.dirWeight.x + other.dirWeight.y ) ) * other.targetWeight;
			
		}
		
		public void update()
		{
			
			if( motionX != null ) motionX.enabled = true;
			
			if( motionY != null ) motionY.enabled = true;
			
			
			if( deltaWeight != 0.0f )
			{
				
				nowWeight += deltaWeight * GM.t.delta;
				
				
				if( nowWeight >= targetWeight )
				{
					
					nowWeight = targetWeight;
					
					deltaWeight = 0.0f;
					
				}
				else if( nowWeight <= 0.0f )
				{
					
					kill();
					
					return;
					
				}
				
				
				if( !isVectorMode )
				{
					
					motionX.weight = nowWeight;
					
					return;
					
				}
				
			}
			
			if( isVectorMode )
			{
				if( motionY == null )
				{
					
					motionX.weight = dirWeight.x * nowWeight;
					
				}
				else
				{
					
					motionX.weight = dirWeight.x * nowWeight;
					
					motionY.weight = dirWeight.y * nowWeight;
					
				}
			}
			
		}
		
		public void startFadeOut( float time )
		{
			
			if( isVectorMode )
			{
				if( motionY == null )
				{
					
					nowWeight = motionX.weight;
					
					dirWeight.x = 1.0f;
					
				}
				else
				{
					
					nowWeight = motionX.weight + motionY.weight;
					
					var totalWeightR = 1.0f / nowWeight;
					
					dirWeight.x = motionX.weight * totalWeightR;
					
					dirWeight.y = motionY.weight * totalWeightR;
					
				}
			}
			
			
			deltaWeight = -nowWeight / time;

			if( deltaWeight >= 0.0f )
			{
				kill();
			}

		}
		
		public void stay()
		{
			
			deltaWeight = 0.0f;
			
			
			if( !isVectorMode )
			{
				
				isVectorMode = true;
				
				dirWeight.x = 1.0f;
				
			}
			
		}
		
		public void kill()
		{
			if( motionX != null )
			{
				
				motionX.enabled = false;
				
				motionX = null;
				
				
				if( motionY != null )
				{
					motionY.enabled = false;
					
					motionY = null;
				}
				
			}
			
			deltaWeight = 0.0f;
			
			isVectorMode = false;
		}
		
		public bool isSame( AnimationState m )
		{
			return motionY == null && motionX == m;
		}
		
		public bool isSame( AnimationState mX, AnimationState mY )
		{
			return motionX == mX && motionY == mY;
		}
		
	}
	
	
	protected struct MotionCrossFader2D
	{
		
		MotionUnit2D	pre;
		
		MotionUnit2D	now;
		
		
		public bool crossFade( AnimationState m, float time = 0.3f, float targ = 1.0f )
		{
			
			if( !now.isSame( m ) )
			{
				pre.kill();
				
				pre = now;
				
				pre.startFadeOut( time );
				
				now = new MotionUnit2D( m, time, targ, false );
			}
			
			return now.motionX.time >= now.motionX.length;
			
		}
		
		public bool crossFade1D( AnimationState m, float dir, float speed, float time = 0.3f, float targ = 1.0f )
		{
			
			if( !now.isSame( m ) )
			{
				pre.kill();
				
				pre = now;
				
				pre.startFadeOut( time );
				
				now = new MotionUnit2D( m, time, targ, true );
			}
			
			now.move( dir, speed );
			
			return now.motionX.time >= now.motionX.length;
			
		}
		
		public bool crossFade2D( AnimationState mX, AnimationState mY, Vector2 dir, float speed, float time = 0.3f, float targ = 1.0f )
		{
			
			if( !now.isSame( mX, mY ) )
			{
				pre.kill();
				
				pre = now;
				
				pre.startFadeOut( time );
				
				now = new MotionUnit2D( mX, mY, time, targ, true );
			}
			
			now.move( dir, speed );
			
			return now.motionX.time >= now.motionX.length && now.motionY.time >= now.motionY.length;
			
		}
		
		public bool blend1D( AnimationState m, float dir, float dirsMax, float speed, float time = 0.3f, float targ = 1.0f )
		// dir は正規化
		{
			
			if( !now.isSame( m ) )
			{
				pre.kill();
				
				pre = now;
				
				pre.stay();
				
				now = new MotionUnit2D( m, time, targ, true );
			}
			
			now.move( dir, speed );
			
			pre.antiMove( ref now, dirsMax );
			
			return now.motionX.time >= now.motionX.length;
			
		}
		
		public bool blend2D( AnimationState mX, AnimationState mY, Vector2 dir, float dirsMax, float speed, float time = 0.3f, float targ = 1.0f )
		// dir は正規化
		{
			
			if( !now.isSame( mX, mY ) )
			{
				pre.kill();
				
				pre = now;
				
				pre.stay();
				
				now = new MotionUnit2D( mX, mY, time, targ, true );
			}
			
			now.move( dir, speed );
			
			pre.antiMove( ref now, dirsMax );
			
			return now.motionX.time >= now.motionX.length && now.motionY.time >= now.motionY.length;
			
		}
		
		public bool play( AnimationState m, float targ = 1.0f )
		{
			
			if( !now.isSame( m ) )
			{
				pre.kill();
				
				now.kill();
				
				now = new MotionUnit2D( m, 0.0f, targ, false );
			}
			
			return now.motionX.time >= now.motionX.length;
			
		}
		
		
		public void update()
		{
			
			now.update();
			
			pre.update();
			
		}
		
		
	}







	public struct MotionFader
	{

		AnimationState	msPrePrev;

		AnimationState	msPrev;

		AnimationState	msCurrent;


		float	remainingTime;

		float	totalTimePowerR;

		


		public float preWeight { get { return msPrev ? msPrev.weight : 0.0f; } }

		public float currentWeight { get { return msCurrent.weight; } }




		public void init( AnimationState ms )
		{
			play( ms );
		}

		public void update()
		{

			if( remainingTime <= 0.0f ) return;


			if( remainingTime > 0.0f )
			{

				remainingTime -= GM.t.delta;


				if( remainingTime <= 0.0f )
				{

					if( msCurrent ) msCurrent.weight = 1.0f;

					if( msPrev )
					{
						msPrev.enabled = false;

						msPrev = null;
					}

					if( msPrePrev )
					{
						msPrePrev.enabled = false;

						msPrePrev = null;
					}

				}
				else
				{
					
					var t = remainingTime;

					var _dt = t * t * totalTimePowerR;

					var preprevweight = 0.0f;


					if( msCurrent ) msCurrent.weight = 1.0f - _dt;

					if( msPrePrev )
					{
						msPrePrev.weight -= GM.t.delta * totalTimePowerR;

						preprevweight = msPrePrev.weight;

						if( preprevweight <= 0.0f )
						{
							msPrePrev.enabled = false;

							msPrePrev = null;
						}
					}

					if( msPrev ) msPrev.weight = _dt - preprevweight;

				}
			}

		}
		

		public void fadeIn( AnimationState msNext, float time )
		{

			if( msCurrent == msNext ) return;


			if( msPrePrev )
			{
				msPrePrev.enabled = false;
			}

			if( msPrev == msNext )
			{
				msPrev.enabled = false;

				msPrev = null;
			}

			msPrePrev = msPrev;

			msPrev = msCurrent;


			msCurrent = msNext;

			msCurrent.time = 0.0f;

			msCurrent.weight = 0.0f;

			msCurrent.enabled = true;


			remainingTime = time;
			
			totalTimePowerR = 1.0f / ( time * time );

		}
		

		public void fadeOut( float time )
		{

			if( !msCurrent ) return;


			if( msPrePrev )
			{
				msPrePrev.enabled = false;

				msPrePrev = null;
			}

			if( msPrev ) msPrePrev = msPrev;
			
			msPrev = msCurrent;


			msCurrent = null;


			remainingTime = time;
			
			totalTimePowerR = 1.0f / ( time * time );

		}
		

		public void play( AnimationState ms )
		{

			if( msPrePrev ) msPrev.enabled = false;

			msPrePrev = null;

			if( msPrev ) msPrev.enabled = false;

			msPrev = null;


			if( msCurrent ) msCurrent.enabled = false;

			msCurrent = ms;

			msCurrent.time = 0.0f;

			msCurrent.weight = 1.0f;

			msCurrent.enabled = true;


			remainingTime = 0.0f;

		}

	}

	/*

	public struct MotionBlender2D
	{

		AnimationState	msX;
		AnimationState	msY;




		public void move( float dir, float speed )
		{

			var sign = dir > 0.0f ? 1.0f : -1.0f;

			dirWeight.x = dir * sign;

			msX.speed = speed * sign;

		}

		public void blend2D( AnimationState mX, AnimationState mY, Vector2 dir, float dirsMax, float speed, float time )
		{ 
		}

		public void move( Vector2 dir, float speed )
		{

			var signx = dir.x > 0.0f ? 1.0f : -1.0f;

			dirWeight.x = dir.x * signx;

			msX.speed = speed * signx;


			var signy = dir.y > 0.0f ? 1.0f : -1.0f;

			dirWeight.y = dir.y * signy;

			msY.time = dir.y > 0.0f ^ dir.x > 0.0f ? ( msX.length - msX.time + msX.length * 0.5f ) % msX.length : msX.time;

		}

	}

	*/



	/*
	public struct MoveSpeedUnit
	// 歩くスピードを 1.0f とする
	{

		public float	velocity	{ get; set; }

		public float	speed		{ get { return velocity >= 0.0f ? velocity : 0.0f; } }
		


		public void _inc( float acc, float limit )
		{
			var d = velocity + acc;

			velocity = d > limit ? limit : d;
		}

		public void _dec( float acc, float limit )
		{
			var d = velocity + acc;

			velocity = d < limit ? limit : d;
		}


		public void inc( float acc, float limit )
		{
			_inc( acc * GM.t.delta, limit );
		}

		public void dec( float acc, float limit )
		{
			_dec( acc * GM.t.delta, limit );
		}


		public void _add( float acc, float minLimit, float maxLimit )
		{
			if( acc >= 0.0f )
			{
				_inc( acc, maxLimit );
			}
			else
			{
				_dec( acc, minLimit );
			}
		}

		public void add( float acc, float minLimit, float maxLimit )
		{
			if( acc >= 0.0f )
			{
				inc( acc, maxLimit );
			}
			else
			{
				dec( acc, minLimit );
			}
		}


		public void lerp( float targetSpeed, float rate )
		{

			var acc = ( targetSpeed - speed ) * GM.t.delta * rate;

			_add( acc, targetSpeed, targetSpeed );

		}

	}



	public struct StaminaUnit
	// 最大を 1.0f、最少を 0.0f とする
	// 内部では、マイナスもある。そのため、回復時間が長くなる。
	{

		public float now;

		const float minValue = 0.7f;

		public float get()
		{

			return now < minValue ? minValue : now;

		}


		public void weary( float v, float limit = -1.0f )
		{

			now = now + v < limit ? limit : now + v;

		}


		public void repair( float acc, float limit )
		{
			
			var d = now + acc * GM.t.delta;
			
			now = d > limit ? limit : d;
			
		}

	}
	*/

	
	/// <summary>
	/// 非物理移動系のスピードを制御し、物理移動系にも初期値等への影響を与える。
	/// </summary>
	[System.Serializable]
	public struct MoveStoppingUnit
	{

		float   min;
		float   max;

		float   threshold;
		float   thresholdR;

		public float   value;

		public float   repairPower;


		/// <summary>
		/// ゼロ未満か？
		/// </summary>
		public bool isUnder { get { return 0.0f > value; } }

		/// <summary>
		/// 平常値より上か？（レートで言えば 1.0f より上か？）
		/// </summary>
		public bool isOver { get { return value * thresholdR > 1.0f; } }

		/// <summary>
		/// 移動可能か？（ゼロ以上か？）
		/// </summary>
		public bool isMovable { get { return value > 0.0f; } }

		/// <summary>
		/// ゼロ以上の移動レートを返す（レートは平常値が 1.0f）。
		/// </summary>
		public float speedRate { get { return ( value > 0.0f ? value : 0.0f ) * thresholdR; } }

		/// <summary>
		/// 移動レートを 0.0f～1.0f でクランプして返す（レートは平常値が 1.0f）。
		/// </summary>
		public float speedRate01 { get { return Mathf.Clamp01( value * thresholdR ); } }

		
		/// <summary>
		/// 移動レートを 0.0f～1.0f でクランプして、その２乗を返す（レートは平常値が 1.0f）。
		/// </summary>
		public float sqSpeedRate01 { get { var spd = speedRate01; return spd * spd; } }



		/// <summary>
		/// 初期化する。
		/// </summary>
		/// <param name="minValue">最小値</param>
		/// <param name="maxValue">最大値</param>
		/// <param name="thresholdValue">平常値（平常閾値）</param>
		public void init( float minValue, float maxValue, float thresholdValue )
		{

			min = minValue;
			
			max = maxValue;

			repairPower = 0.0f;

			threshold = thresholdValue;

			thresholdR = 1.0f / threshold;

			value = thresholdValue;

		}


		/// <summary>
		/// 移動レートを設定する（レートは平常値が 1.0f）。
		/// </summary>
		public void setSpeedRate( float newRate )
		{
			value = newRate * threshold;
		}
		

		/// <summary>
		///移動回復値を設定する。何秒で平常値まで回復するかを指定する。
		/// </summary>
		public void setRepairTime( float refreshTime )
		{
			repairPower = ( 1.0f / refreshTime ) * threshold;
		}

		/// <summary>
		/// 移動回復値を設定する。１秒で回復する割合を、平常値を 1.0f としたレートで指定する。
		/// </summary>
		public void setRepairPowerRate( float powRate )
		{
			repairPower = powRate * threshold;
		}


		/// <summary>
		/// 毎フレームでの移動回復処理を行う。
		/// </summary>
		public void update()
		{
			repair( repairPower * GM.t.delta );
        }


		/// <summary>
		/// 移動値をその場でレートで指定して回復する。
		/// </summary>
		public void repairRate( float repairImpulseRate )
		{
			repair( repairImpulseRate * threshold );
		}

		/// <summary>
		/// 移動値をその場で回復する。
		/// </summary>
		public void repair( float repairImpulse )
		{
			var newValue = value + repairImpulse;

			value = max > newValue ? newValue : max;
		}



		/// <summary>
		/// 移動値をその場でレートで指定して減じる。
		/// </summary>
		public void stoppingRate( float damageImpulseRate )
		{
			stopping( damageImpulseRate * threshold );
		}

		/// <summary>
		/// 移動値をその場で減じる。
		/// </summary>
		public void stopping( float damageImpulse )
		{
			var newValue = value - damageImpulse;

			value = newValue > min ? newValue : min ;
		}


		/// <summary>
		/// 移動値を弾丸ダメージ構造体によって、その場で減じる。
		/// </summary>
		/// <param name="speedRate">移動速度の実効率</param>
		public void stopping( ref _Bullet3.DamageSourceUnit ds, float speedRate )
		{

			if( 0.0f >= ds.moveStoppingDamage ) return;

			stopping( ds.moveStoppingDamage );


			ds.moveStoppingRate *= speedRate;// 実行率が少ないほどダメージ大

			//var speed = speedRate * threshold;

			//var rate = ds.moveStoppingDamage > speed ? speed / ds.moveStoppingDamage : 1.0f;

			//ds.moveStoppingRate *= rate;// 実行率が０にならないとダメージを受けない
			
		}



		public void lerpRate( float targetRate, float timeRate )
		{
			value = Mathf.Lerp( value, targetRate * threshold, GM.t.delta * timeRate );
		}

	}





	// ----------------------

	protected interface IActionController
	{



	}

}


