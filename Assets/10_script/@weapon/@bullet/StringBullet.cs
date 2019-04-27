using UnityEngine;
using System.Collections;


using UpdateProc = StringParticlePool.UpdateProc;
using System;

public class StringBullet : _Bullet3, StringParticlePool.ILineParticle
{

	public StringParticlePool	poolTemplate;

	StringParticlePool	pool	{ get { return (StringParticlePool)poolTemplate.getBaseInstance(); } }


	public float	damage;
	
	
	public float	moveStoppingDamage;
	
	public float	moveStoppingRate;	// 目標が移動していた場合に、威力から停止力へ変換される率（0 ～ 1）
	

	public float	bulletSpeed;
	
	public Color	bulletColor;
	
	public float	bulletSize;
	
	public float    stickingTime;	// ひっついた時に時間に猶予が加算される。

	public float	lifeTime;		// 消えるまでの時間。ひっつきで増える。
	
	public float    maxStretchDistance;	// 最大伸張距離。単純に、始点と終点の距離。

	
	public float	stiffness;

	public float	damping;



	/// <summary>
	/// キャラクターにひっついている時に他のキャラクターに接触した場合の挙動を示す。
	/// 接触したキャラクター全てにダメージを与えるか、または一番近いものにひっつきなおすか。
	/// </summary>
	public bool    isMultiHitOnTrackCharacter;


	HitCheckOnTrackCharacterProc    hitCheckOnTrackCharacter;

	delegate EnHitState HitCheckOnTrackCharacterProc( StringParticlePool.StringUnit line, ref Vector3 posfirst, ref Vector3 poslast, Vector3 force, int hitmask, bool isOwnHittable, float hitRate = 1.0f );



	const float hitTimingSpan = 0.1f;   // 連続ヒットの秒間隔（実際にはランダムの目安）

	const float hitDistanceBg = 1.0f;   // 背景ヒット成立条件として、始点 - 終点間の最低距離

	const float hitForceRateCh = 0.0f;//80.0f;	// キャラクターにヒットした時に力にかける倍率


	enum EnHitState
	{
		none,
		ch,
		bg
	}


	public float size { get { return bulletSize; } }

	public Color color { get { return bulletColor; } }
	
	public int registedId { get; protected set; }
	



	

	// 初期化 ---------------------------------
	
	new void Awake()
	{

		base.Awake();

		
		var pool = (StringParticlePool)poolTemplate.getBaseInstance();

		registedId = pool.registLineDefine( this );

		if( registedId == -1 ) registedId = 0;  // とりあえず登録失敗しても０のを使おう。
		

		hitCheckOnTrackCharacter = isMultiHitOnTrackCharacter ? (HitCheckOnTrackCharacterProc)hitCheckForMultiCollision : hitCheckForFirstCollision;

	}





	// 生成 -----------------------------------------

	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle )
	{

		var bs = (StringBullet)this.getBaseInstance();

		var line = pool.instantiate( pos );
		
		line.init( bs.flying, rot, act );
		
		line.tfMuzzle = tfMuzzle;

		line.barrelFactor = 0.0f;

    }






	// 更新処理 ==============================================
	
	
	// ライフフェイズ ----------------------------

	
	// 飛行 - - - - - - -

	/// <summary>
	/// 飛行時（マズル着）
	/// </summary>
	public bool flying( StringParticlePool.StringUnit line )
	{

		
		var firstpos    = progressBullet( line );

		var lastpos     = line.tfMuzzle.position;




		if( isOverStretch( firstpos, lastpos ) ) line.update = flyingAway;



		var res = hitCheckOnFlying( line, ref firstpos );

		if( res != EnHitState.none )
		{
			line.update = res == EnHitState.ch ? (UpdateProc)hittingCh : hittingBg;

			line.time -= stickingTime;
		}



		checkToShiftToEndPhaseOnOverTimeLimit( line, pullFlying );




		pool.moveOnFixedBothEnds( line, firstpos, lastpos, stiffness, damping );


		return false;

	}


	/// <summary>
	/// 飛行時（マズル脱）
	/// </summary>
	public bool flyingAway( StringParticlePool.StringUnit line )
	{


		var firstpos    = progressBullet( line );
		



		var res = hitCheckOnFlying( line, ref firstpos );

		if( res != EnHitState.none ) line.update = res == EnHitState.ch ? (UpdateProc)pullingCh : pullingBg;



		checkToShiftToEndPhaseOnOverTimeLimit( line, pullFlying );




		//pool.movePulling( line, firstpos, stiffness * 0.5f, damping * 0.5f );
		pool.movePulling( line, firstpos, stiffness, damping );


		return false;

	}




	// 引きずり - - - - - -


	/// <summary>
	/// キャラクター - マズル　で引きずる
	/// </summary>
	bool hittingCh( StringParticlePool.StringUnit line )
	{
		

		var firstpos	= line.tfHitPoint.position;

		var lastpos		= line.tfMuzzle.position;



		var    res = EnHitState.none;

		if( isHitTiming )
		{
			res = hitCheckOnTrackCharacter(
				line, ref firstpos, ref lastpos, Vector3.zero, UserLayer.bulletHittable, false, hitTimingSpan );
		}


		if( res == EnHitState.bg )
		{

			// 背景に接触した場合、先端が千切れて相手を離す。


			line.update = hittingBg;


			checkToShiftToEndPhaseOnOverTimeLimit( line, pullingMuzzle );


		}
		else
		{
			
			checkToShiftToEndPhaseOnOverTimeLimit( line, pullingCh );

			
			checkToShiftToEndPhaseInOverStretch( line, firstpos, lastpos, pullingCh );

		}
		

		pool.moveOnFixedBothEnds( line, firstpos, line.tfMuzzle.position, stiffness, damping );


		return false;

	}


	/// <summary>
	/// 背景 - マズル　で引きずる
	/// </summary>
	bool hittingBg( StringParticlePool.StringUnit line )
	{


		var firstpos	= getPositionAtFirstWithDown( line );

		var lastpos		= line.tfMuzzle.position;



		var    res = EnHitState.none;

		if( isHitTiming )
		{
			res = hitCheckForFirstCollision(
				line, ref firstpos, ref lastpos, Vector3.zero, UserLayer.bulletHittable, false, hitTimingSpan );
		}



		if( res == EnHitState.bg )
		{

			// 背景にくっついていた場合は、後ろが切れる。


			line.update = stickingBg;


			lastpos = firstpos;

			pool.moveOnFixedBothEndsAtLast( line, lastpos, stiffness, damping );

		}
		else
		{

			pool.moveOnFixedBothEnds( line, firstpos, line.tfMuzzle.position, stiffness, damping );

			
			checkToShiftToEndPhaseInOverStretch( line, firstpos, lastpos, pullingBg );

		}



		checkToShiftToEndPhaseOnOverTimeLimit( line, pullingBg );


		return false;

	}




	// 貼りつきフェイズ - - - - - -


	/// <summary>
	/// キャラクター - 背景　で貼りつく。
	/// </summary>
	bool stickingCh( StringParticlePool.StringUnit line )
	{

		var firstpos	= line.tfHitPoint.position;

		var lastpos		= getPositionAtLastWithDown( line );


		modifyOwnHitableState( lastpos, line );



		if( isHitTiming )
		{
			hitCheckForMultiCollision(
				line, ref firstpos, ref lastpos, Vector3.zero, UserLayer.bulletHittableCh, isOwnHittable( line ), hitTimingSpan );
		}

		
		pool.moveOnFixedBothEndsAtFirst( line, firstpos, stiffness, damping );


		
		checkToShiftToEndPhaseInOverStretch( line, firstpos, lastpos, pullingCh );

		checkToShiftToEndPhaseOnOverTimeLimit( line, pullingCh );


		return false;

	}


	/// <summary>
	/// 背景 - 背景　で貼りつく。
	/// </summary>
	bool stickingBg( StringParticlePool.StringUnit line )
	{

		var firstpos	= getPositionAtFirstWithDown( line );

		var lastpos		= getPositionAtLastWithDown( line );


		modifyOwnHitableState( lastpos, line );



		if( isHitTiming )
		{
			hitCheckForMultiCollision(
				line, ref firstpos, ref lastpos, Vector3.zero, UserLayer.bulletHittableCh, isOwnHittable( line ), hitTimingSpan );
		}



		pool.moveOnFixedBothEnds( line, firstpos, lastpos, stiffness, damping );


		
		checkToShiftToEndPhaseOnOverTimeLimit( line, pullingBg );


		return false;

	}





	// エンドフェイズ ------------------------------------------


	/// <summary>
	/// 先端に引っ張られて飛んでいく
	/// </summary>
	bool pullFlying( StringParticlePool.StringUnit line )
	{

		var posnext = progressBullet( line );


		pool.movePulling( line, posnext, stiffness, damping );


		return line.time > 0.5f;

	}

	/// <summary>
	/// 先端のキャラクターに引っ張られる
	/// </summary>
	bool pullingCh( StringParticlePool.StringUnit line )
	{


		var firstpos    = line.tfHitPoint.position;


		pool.movePulling( line, firstpos, stiffness, damping );


		return line.time > 0.5f;

	}

	/// <summary>
	/// 先端の背景に引っ張られる
	/// </summary>
	bool pullingBg( StringParticlePool.StringUnit line )
	{

		var firstpos    = pool.getPositionAtFirst( line );


		pool.movePulling( line, firstpos, stiffness, damping );


		return line.time > 0.5f;

	}

	/// <summary>
	/// 終端のマズルに引っ張られる
	/// </summary>
	bool pullingMuzzle( StringParticlePool.StringUnit line )
	{


		pool.pullToLast( line, line.tfMuzzle.position, stiffness, damping );


		return line.time > 0.5f;

	}

	/// <summary>
	/// 両端が引っ張られる？←ちがうっぽい
	/// </summary>
	bool shulink( StringParticlePool.StringUnit line )
	{
		
		var firstpos    = pool.getPositionAtFirst( line );

		var lastpos     = pool.getPositionAtLast( line );


		pool.shrink( line, firstpos, lastpos );


		return line.time > 0.5f;

	}

	








	// 各種処理 ===============================================





	// 弾丸の飛行処理 -----------------------------

	/// <summary>
	/// 飛行中の弾丸位置を進める。
	/// </summary>
	Vector3 progressBullet( StringParticlePool.StringUnit line )
	{


		var dir = line.rotation * Vector3.forward;

		var speed = bulletSpeed - bulletSpeed * line.time * line.time * 2.0f;


		var d = speed > 0.0f ? speed * GM.t.delta : 0.0f;

		var dg = Physics.gravity * ( GM.t.delta * line.time );

		var posnow = pool.getPositionAtFirst( line );

		var posnext = posnow + dir + dg;


		return posnext;

	}



	/// <summary>
	/// 飛行処理中のヒットチェックを呼び出す。
	/// </summary>
	EnHitState hitCheckOnFlying( StringParticlePool.StringUnit line, ref Vector3 posnext )
	{

		var posnow = pool.getPositionAtFirst( line );


		// 衝突した時のノックバックは、サイズに比例する
		var forceSeed = posnext - posnow;


		return hitCheckForFirstCollision( line, ref posnext, ref posnow, forceSeed, UserLayer.bulletHittable, false );

	}







	// ヒットチェック --------------------

	/// <summary>
	/// 始点から一番近い衝突物のみを判定する。接触していた場合、firstpos に接触位置が返る。
	/// また、接触物がキャラクターだった場合は、line.tfHitPoint にそのオブジェクトが入る。
	/// 戻り値は、ヒットしたものを示す列挙子。
	/// </summary>
	EnHitState hitCheckForFirstCollision(

		StringParticlePool.StringUnit line, ref Vector3 posfirst, ref Vector3 poslast, Vector3 forceSeed,

		int hitmask, bool isOwnHittable, float hitRate = 1.0f

	)
	{
		

		var ihit = 0;
		

		if( isOwnHittable )
		{
			
			var res = Physics.Linecast( poslast, posfirst, out hits.array[0], hitmask );
			

			if( !res ) return EnHitState.none;

		}
		else
		{
			
			var m = (posfirst - poslast).magnitude;

			var ray = new Ray( poslast, (posfirst - poslast) / m );


			var hitLength = Physics.RaycastNonAlloc( ray, hits.array, m, hitmask );
			

			ihit = hits.getOtherHitIdForOwnHittable( hitLength, line.owner );

			if( ihit < 0 ) return EnHitState.none;
			
		}



		var hitter = getHitter( hits.array[ ihit ].collider );

		var act = hitter ? hitter.getAct() : null;


		if( act )
		{

			// キャラクター


			damageTohitCh( line, ref hits.array[ ihit ], act, hitter, forceSeed, hitRate );


			line.tfHitPoint = hitter.getRandomBone();

			posfirst = line.tfHitPoint.position;


			return EnHitState.ch;

		}



		// 背景・その他

		var p = hits.array[ ihit ].point;

		var n = hits.array[ ihit ].normal;

		posfirst = p + n * bulletSize;


		return EnHitState.bg;
		
	}


	/// <summary>
	/// 始点から一番近い背景と、線上のすべてのキャラクターとのヒットチェックをする。
	/// 接触したキャラクターすべてにダメージを与える。
	/// 背景に接触していた場合は、その位置を firstpos に返してそこで抜ける。
	/// </summary>
	EnHitState hitCheckForMultiCollision(
		
		StringParticlePool.StringUnit line, ref Vector3 posfirst, ref Vector3 poslast, Vector3 dir,
		
		int hitmask, bool isOwnHitable, float hitRate = 1.0f

	)
	{

		var v = posfirst - poslast;

		var m = v.magnitude;

		var ray = new Ray( poslast, v / m );


		var hitLength = Physics.RaycastNonAlloc( ray, hits.array, m, hitmask );


		for( var i = 0 ; i < hitLength ; i++ )
		{


			var hitter = getHitter( hits.array[ i ].collider );

			var act = hitter ? hitter.getAct() : null;


			if( act )
			{

				// キャラクタ


				damageTohitCh( line, ref hits.array[ i ], act, hitter, Vector3.zero, hitRate, isOwnHitable );


			}
			else
			{

				var p = hits.array[ i ].point;

				if( ( p - posfirst ).sqrMagnitude > hitDistanceBg * hitDistanceBg )
				{

					// 背景・その他


					posfirst = p;// + hits.array[ i ].normal * bulletSize;


					return EnHitState.bg;

				}
				
			}

		}


		return EnHitState.none;

	}






	// ヒット関連処理 ------------------------
	
	
	/// <summary>
	/// キャラクターにヒットしていればダメージを与え、act を返す。
	/// オウンヒット判定はダメージが入るかどうかのみに関与し、戻り値はオウンヒットに関係なく act が返る。
	/// </summary>
	void damageTohitCh(

		StringParticlePool.StringUnit line, ref RaycastHit hit, _Action3 act, _Hit3 hitter,

		Vector3 forceSeed, float damageRate, bool isOwnHittable = true

	)
	{

		if( !isOwnHittable && act == line.owner ) return;	// オウンヒットなら抜ける

		if( line.owner && !line.owner.targetTeam.isMate( act.attachedTeam ) ) return;	// 攻撃対象以外なら抜ける


		var landing = hitter.landings ? hitter.landings.erode : GM.defaultLandings.erode;//暫定
		//var landing = hitter.landings.erode;//本式はこっち

		landing.emit( hit.point, hit.normal );



		var force = forceSeed *( GM.t.deltaR * bulletSize * hitForceRateCh );


		var d = damage * damageRate;

		var ds = new DamageSourceUnit( d, 0.5f, 1.0f, moveStoppingDamage * damageRate, moveStoppingRate );


		hitter.shot( ref ds, force, ref hit, line.owner );

	}









	// 両端が接地していなければ、落下させて位置を返す -----------------------------


	/// <summary>
	/// 背景に触れるまで、落下する。戻り値は落下処理した位置。
	/// </summary>
	Vector3 getPositionAtFirstWithDown( StringParticlePool.StringUnit line )
	{
		return getPositionWithDown( line, 0 );
    }

	/// <summary>
	/// 背景に触れるまで、落下する。戻り値は落下処理した位置。
	/// </summary>
	Vector3 getPositionAtLastWithDown( StringParticlePool.StringUnit line )
	{
		return getPositionWithDown( line, pool.numberOfSegmnts + 1 );
	}


	Vector3 getPositionWithDown( StringParticlePool.StringUnit line, int index )
	{

		var pos = pool.getPosition( line, index );
		
		var velocity = pool.getVelocity( line, index );


		var isNoHit = false;

		if( velocity.sqrMagnitude != 0.0f || UnityEngine.Random.value < GM.t.delta * ( 1.0f / 0.5f ) )
		{
			
			// 適当に負荷を抑えたランダムなタイミングでヒットチェックを行う。


			if( Physics.CheckSphere( pos, 0.2f, UserLayer.bulletHittableBg ) )
			{

				if( line.barrelFactor != 0.0f ) line.barrelFactor = 0.0f;

				return pos;

			}


			isNoHit = true;

		}


		if( isNoHit || line.barrelFactor != 0.0f )
		{

			// 位置を計算し、落下速度を更新する。


			velocity += Physics.gravity * GM.t.delta;

			pos += velocity * GM.t.delta;


			pool.setVelocity( line, index, velocity );
			
		}


		return pos;
		
	}





	// オウンヒット関連 ----------------


	/// <summary>
	/// オウンヒットを許可するか否かを取得する。具体的には、オーナーへのリンクを保持しているかどうかで決まる。
	/// </summary>
	bool isOwnHittable( StringParticlePool.StringUnit line )
	{

		return line.owner != null;

	}

	/// <summary>
	/// プレイヤーのみ、終端から３ｍ離れたら、オウンヒット可能となるようにする。
	/// 敵の場合、チームへのフレンドリーファイアの防止のためもあり、line.owner を保持する必要がある。
	/// </summary>
	void modifyOwnHitableState( Vector3 endpos, StringParticlePool.StringUnit line )
	{

		if( !line.owner ) return;

		if( line.owner.attachedTeam.isEnemyTeam ) return;

		if( ( line.owner.tf.position - endpos ).sqrMagnitude > 3.0f * 3.0f )
		{

			line.owner = null;

		}

	}






	// 時間・伸張の限界をチェックし、エンドフェイズへ進める -----------------------------
	
	
	
	/// <summary>
	/// 制限時間を越えていたら、指定されたエンドフェイズ（糸が消えゆく処理）へ移行する。
	/// </summary>
	void checkToShiftToEndPhaseOnOverTimeLimit( StringParticlePool.StringUnit line, UpdateProc endPhase )
	{
		
		if( isOverLifeTime( line ) )
		{
			line.time = 0.0f;

			line.update = endPhase;
		}
		
    }


	/// <summary>
	/// 最大伸張を越えていたら、指定されたエンドフェイズ（糸が消えゆく処理）へ移行する。
	/// </summary>
	void checkToShiftToEndPhaseInOverStretch( StringParticlePool.StringUnit line, Vector3 startpos, Vector3 endpos, UpdateProc endPhase )
	{

		if( isOverStretch( startpos, endpos ) )
		{
			line.time = 0.0f;

			line.update = endPhase;
		}

	}






	// 条件のチェック --------------------------------


	/// <summary>
	/// 最大伸張距離を超えているかどうかを取得する。
	/// </summary>
	bool isOverStretch( Vector3 startpos, Vector3 endpos )
	{

		return ( startpos - endpos ).sqrMagnitude > maxStretchDistance * maxStretchDistance;

	}



	/// <summary>
	/// 生存時間を過ぎたかどうかを取得する。
	/// </summary>
	bool isOverLifeTime( StringParticlePool.StringUnit line )
    {

		return line.time >= lifeTime;

	}



	/// <summary>
	/// 連続ヒットのチェックタイミングになったかどうかを取得する。
	/// </summary>
	bool isHitTiming
	{

		get { return UnityEngine.Random.value < GM.t.delta * ( 1.0f / hitTimingSpan ); }

	}







}
