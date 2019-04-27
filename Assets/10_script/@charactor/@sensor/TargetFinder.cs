using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 能動的な会敵 -----------

// 会敵の状態
// 通常→（疑敵）→｜発見→捕捉｜→喪失　（｜｜内が確敵状態)

// 敵の発見や特定は、TargetFinder で扱う。

// target.isDecision が true	(hate >= 1.0)	確敵
// target.isExist が true		(act != null)	疑敵・確敵・喪失・倦怠
// ※ 0.0f <= hate	<= 1.0f

// scan() は不特定の相手を索敵。
// capture() は索敵せず、target を捕捉できているかどうかのみ見る。
// それぞれ発見すれば hate が上昇し、hate >= 1.0f で確敵となる。非発見時は減少し、喪失となる。
// 　※警戒と喪失の状態では、敵を発見した場合３倍の hate 上昇となる。

// 捕捉状態は単純に言えば、確敵時に capture のリフレッシュタイムが来るまではとりあえず確敵状態を維持させよう、という考え。
// 　喪失と捕捉は似た状態だが、大きな違いは「その状態で敵を攻撃するかしないか」にある（性格によっては闇雲に攻撃する）。

// 確敵・喪失（・倦怠）を追跡状態と呼ぶこともある（敵を追い続けるから）。


// 確敵 --------------

// 広義的には hate >= 1.0f の状態を指す。狭義的には hate >= 1.0f & !isRelease & isExist の状態。
// 限界時間が来るまで敵を追跡し、攻撃する。限界時間はひとつの「追跡」状態として喪失と共有する。
// 確敵のまま限界時間が来た場合、倦怠状態となる。


// 倦怠 -------------

// 倦怠は攻撃するが戦闘モードとみなされず、戦闘意欲のない状態。
// 敵を発見し続ければ続く。１回でも非発見すれば（疑敵を経由して）警戒となる（基本的には警戒で帰巣する）。


// 喪失と警戒 -------------

// 喪失と警戒は、確敵でない戦闘モードを指す。
// 性格にもよるが、基本的に攻撃はしない（敵を見失った状態だから）。

// 喪失と警戒においては、限界時間が設定される。
// 喪失は target.isExist が true であり、警戒では false である。
// ともに敵を発見した場合、３倍の hate 上昇となる。

// 喪失へは、非発見により確敵状態から外れた場合に遷移する。
// hate の減少につれて、敵対象位置取得の誤差範囲が拡大していく（おおよその位置を追っていることを表現）。
// 限界時間はひとつの「追跡」状態として確敵と共有し、引き継ぐ。

// 警戒へは、外部からの要因（グループ通知・被攻撃）や、追跡の限界時間超過により遷移する。
// 警戒時に攻撃を受けると近隣の仲間へ攻撃通知する。


// 戦闘モード ------------

// 戦闘モードは限界時間をもつ、確敵・喪失・警戒を指す。
// 状態は性格・パラメータの解釈によって遷移するものだが、戦闘モードは限界時間を超えるか超えないかで明確に決定される。


// 被攻撃の通知 ------------------

// 通常　条件：自分がグループテリトリー内にいる　対象：グループテリトリー内のグループメンバー
// 疑敵　条件：なし　対象：グループテリトリー内のグループメンバー
// 警戒　条件：なし　対象：自分の近隣のグループメンバー

// ※通知を受け取れるかどうかは、受け取る側の character.obedience に依存する。
// このへん変更したかもしんない



[System.Serializable]
public struct TargetFinder
{

#if UNITY_EDITOR
	[SensorHolder]
#endif
	public SensorUnit[]	sensors;


	public Filter	filter;

	public Target	target;


	public Transform		tfSensor;// センサーごとにあってもいいかも

	public _Action3Enemy	owner { get; private set; }




	public void init( _Action3Enemy action )
	{
		owner = action;

		sensors.init();

		target.clear();
	}



	

	public _Action3Enemy.BattleState toMode( bool isFound )
	{
		if( !isFound ) target.lose( owner.character );//, GM.t.delta );

		return new _Action3Enemy.BattleState( ref target );
	}

	public _Action3Enemy.BattleState toMode()
	{
		return new _Action3Enemy.BattleState( ref target );
	}


	public bool search( int id )
	{

		if( Time.time > sensors[ id ].refreshTime )
		{

			if( !target.isExists || !target.isCapturable( owner.character ) )
			{
				//Debug.Log( "scan" );
				return scan( id );

			}
			else
			//if( target.isExists && target.isCapturable( owner.character ) )
			{
				//Debug.Log( "capt" );
				return capture( id );

			}

		}

		
		return target.isExists;

	}



	public bool scan( int id )
	// 新規に索敵する。
	{

		var pos = tfSensor.position;

		var maxRadius = sensors[ id ].module.distance;


		var isHit = false;

		var cs = Physics.OverlapSphere( pos, maxRadius, filter.layerMask );

		if( cs.Length == 1 )
		{

			var rbOhter = cs[ 0 ].attachedRigidbody;

			if( rbOhter != owner.rb )
			{
				isHit = sensors[ id ].module.capture( ref this, cs[ 0 ].GetComponent<_Hit3>().getAct() );// _Action3>() );
			}

		}
		else if( cs.Length > 1 )
		{

			isHit = sensors[ id ].module.scan( ref this, cs );

		}


		sensors[ id ].setRefreshTime( target.isExists );

		if( isHit ) GameObject.Destroy( GameObject.Instantiate( sensors[ id ].module.hint, tfSensor.position, owner.rb.rotation ), 1.0f );//

		
		return isHit;
	}


	public bool capture( int id )	
	// 捕捉している相手にだけ、判定をする。
	{

		if( target.isExists )
		{

			var isHit = sensors[ id ].module.capture( ref this, target.act );


			sensors[ id ].setRefreshTime( target.isExists );

			if( isHit ) GameObject.Destroy( GameObject.Instantiate( sensors[ id ].module.hint, tfSensor.position, owner.rb.rotation ), 1.0f );//

			
			return isHit;
		}

		
		return false;
	}





	public struct Filter
	{

		public int			layerMask;

		public TeamLayer	team;


		public void setTargetTeam( TeamLayer teamLayer )
		{

			team.flags = teamLayer.flags;

			layerMask = UserLayer.players * team.isPlayerTeam.GetHashCode() | UserLayer.enemyEnvelope * team.isEnemyTeam.GetHashCode();

		}

	}


	[System.Serializable]
	public struct Target
	{

		public _Action3		act;

		public 		float	hate;

		public 		float	errRadius;

		public 		float	releaseTime;



		public void clear()
		// これが wait の基本値、hate のみ変動する。errRadius は待機・警戒に戻った時にのみクリアされる。
		{
			hate = 0.0f;

			resetTime();

			errRadius = 0.0f;

			act = null;
		}


		public void keep( _Action3Enemy.CharacterInfo ch, _Action3 a, float h, float time, float er )
		// 感覚でとらえた相手・ヘイトを適用する。
		{
			var preTargetting = hate >= 1.0f;

			hateUp( ch, h, time * (isBattling ? 3.0f : 1.0f) );

			var nowTargetting = hate >= 1.0f;

			if( ( !isExists | isRelease ) & ( preTargetting ^ nowTargetting ) ) refreshChaseTime( ch );
			// 待機・警戒・疑敵・倦怠時に確敵モードに移行した場合、追跡タイマーをセットする。

			errRadius = er;

			act = a;
		}

		public void lose( _Action3Enemy.CharacterInfo ch )//, float pastTime )
		{
			//if( pastTime != 0.0f )
			{
				hateDown( ch, GM.t.delta );// pastTime );

				carmDown( ch );
			}
		}


		public void toDecision( _Action3Enemy.CharacterInfo ch, _Action3 a, float er = 0.0f )
		{
			//Debug.Log( "to decision" );

			hate = 1.0f;

			refreshChaseTime( ch );

			errRadius = er;

			act = a;
		}

		public void toLost( _Action3Enemy.CharacterInfo ch )
		{
			//Debug.Log( "to lost" );

			hateDown( ch, 0.1f );
		}

		public void toWait()
		{
			//Debug.Log( "to wait" );

			resetTime();

			errRadius = 0.0f;

			act = null;
		}

		public void toAlert( _Action3Enemy.CharacterInfo ch )
		{
			//Debug.Log( "to alert" );

			refreshAlertTime( ch );

			errRadius = 0.0f;

			act = null;
		}



		void hateUp( _Action3Enemy.CharacterInfo ch, float up, float time )
		{
			var newHate = hate + up * hateUpRatios[ (int)ch.aggressive ] * time;

			hate = newHate > 1.0f ? 1.0f : newHate;
		}

		void hateDown( _Action3Enemy.CharacterInfo ch, float time )
		{
			hate -= hateDownRatios[ (int)ch.aggressive ] * time;

			hate = hate < 0.0f ? 0.0f : hate;
		}


		void carmDown( _Action3Enemy.CharacterInfo ch )
		// 時間限界時に、確敵・喪失→警戒→待機の順で１つだけ低い状態へ移行する。
		{
			if( !isRelease ) return;

			if( isChaseOver )
			{
				refreshAlertTime( ch );
			}
			else
			{
				resetTime();
			}
			
			if( isExists )
			{
				act = null;
			}
		}



		void resetTime()
		{
			//Debug.Log( "reset time" );
			releaseTime = 0.0f;
		}

		void refreshChaseTime( _Action3Enemy.CharacterInfo ch )
		{
			//Debug.Log( "chase time" );
			releaseTime = Time.time + chaseTimes[ (int)ch.aggressive ];
		}

		void refreshAlertTime( _Action3Enemy.CharacterInfo ch )
		{
			//Debug.Log( "alert time" );
			releaseTime = Time.time + alertTimes[ (int)ch.aggressive ];
		}




		public bool isExists
		{
			get
			{
				if( act != null )
				{
					if( act.isDead )//&& Random.value > 0.9f )
					{
						act = null;
					}
					else
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool isDecision { get { return isExists & hate >= 1.0f & !isRelease; } }

		public bool isChase { get { return isExists & ( !isRelease | hate >= 1.0f ); } }

		public bool isNoHate { get { return isExists & hate <= 0.0f; } }

		public bool isRelease { get { return Time.time > releaseTime; } }

		public bool isChaseOver { get { return isExists & isRelease & releaseTime > 0.0f; } }

		public bool isNoBattle { get { return releaseTime == 0.0f; } }

		public bool isBattling { get { return !isRelease; } }

		public bool isAlert { get { return !isExists & !isRelease; } }

		public bool isWeary { get { return isExists & hate >= 1.0f & isRelease; } }


		public bool isReach( float reach, ref TargetFinder finder )
		{
			if( !isExists ) return false;

			//var sqrDist = ( act.rb.position - finder.owner.rb.position ).sqrMagnitude;
			var sqrDist = ( act.tfObservedCenter.position - finder.tfSensor.position ).sqrMagnitude;

			return sqrDist < reach * reach;
		}


		public bool isCapturable( _Action3Enemy.CharacterInfo ch )
		// キャプチャする際、スキャンしなおすケースの検討のため、キャプチャ可能か（すべきか）どうかを返す。
		{
			//return hate >= capturableLimits[ (int)ch.persistence ];
			return Random.value >= capturableLimits[ (int)ch.persistence ];
		}


		public Vector3 imaginaryPosition
		{
			//get { return act.rb.position + Random.insideUnitSphere * errRadiusInChase; }//( isDecision ? errRadius : errRadiusInLost ); }
			get { return act.tfObservedCenter.position + Random.insideUnitSphere * errRadiusInChase; }
		}

		float errRadiusInChase
		{
			get { return errRadius + ( 1.0f - hate ) * 30.0f; }// 確敵中は hate == 1.0f なので実質 errRadius
		}

		


		static readonly float[] hateUpRatios =
		// ヘイト上昇の性格補正値。ヘイトは攻撃的なほど上がりやすいとする。
		{ 0.0f, 1.0f / 1.5f, 1.0f, 3.0f, float.PositiveInfinity };

		static readonly float[] hateDownRatios =
		// ヘイトがだいたい何秒でなくなるかの逆値。ヘイトは攻撃的なほど減りにくいとする。
		{ 1.0f, 1.0f / 3.0f, 1.0f / 10.0f, 1.0f / 30.0f, 0.0f };

		static readonly float[] chaseTimes =
		// 確敵時に設定される追跡（確敵・喪失）の限界時間。攻撃的なほど長いとする。
		{ 0.0f, 60.0f, 360.0f, 720.0f, float.PositiveInfinity };
		//{ 0.0f, 10.0f, 360.0f, 720.0f, float.PositiveInfinity };

		static readonly float[] alertTimes =
		// 警戒時に設定される喪失限界時間。攻撃的なほど長いとする。
		{ 0.0f, 30.0f, 180.0f, 360.0f, float.PositiveInfinity };
		//{ 0.0f, 10.0f, 180.0f, 360.0f, float.PositiveInfinity };

		static readonly float[] capturableLimits =
		// サーチ時にターゲットが存在した場合においてキャプチャする確率の閾値。執着的なほど低いとする。
		{ 1.0f, 0.5f, 0.25f, 0.0f, 0.0f };// perfect では強制変更や被攻撃時でもターゲットが変更されないレベル

	}

}






[System.Serializable]
public struct SensorUnit
{
	public _SensorModule	module;

	public float			refreshTime;


	public void setRefreshTime( bool isExists )
	{
		refreshTime = Time.time + ( isExists ? module.captureRefreshTime : module.scanRefreshTime );
	}
}

static class SensorsExtension
{
	static public void init( this SensorUnit[] sensors )
	{
		for( var i = 0 ; i < sensors.Length ; i++ )
		{
			sensors[ i ].refreshTime = Random.value * sensors[ i ].module.scanRefreshTime;
		}
	}
}





#if UNITY_EDITOR

public class SensorHolderAttribute : PropertyAttribute
{
	public SensorHolderAttribute()
	{}
}

[CustomPropertyDrawer( typeof( SensorHolderAttribute ) )]
public class SensorModuleDrawer : PropertyDrawer
{
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		var sensorModuleAttribute = (SensorHolderAttribute)attribute;

		var sensor = property.FindPropertyRelative( "module" );

		if( sensor != null )
		{
			sensor.objectReferenceValue = EditorGUI.ObjectField( position, label, sensor.objectReferenceValue, typeof(_SensorModule), false );
		}
	}
}

#endif
