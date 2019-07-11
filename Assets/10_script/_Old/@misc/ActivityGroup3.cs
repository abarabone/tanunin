using UnityEngine;
using System.Collections;

using EnObedience = _Action3.CharacterInfo.EnObedience;


public class ActivityGroup3 : _PoolingObject3<ActivityGroup3>, MEvent.IEventable
{


	/// <summary>
	/// 属するチーム。
	/// </summary>
	public TeamLayer	team;
	

	/// <summary>
	/// 領域形状。円か球か。
	/// </summary>
	public enShape		shape;

	public enum enShape { circle, sphere }


	/// <summary>
	/// リーダー。リーダーが存在すれば、グループの実質中心はリーダーの位置に同期する。
	/// </summary>
	public _Action3	leader	{ set; get; }


	/// <summary>
	/// 実質中心。ＴＦを移動させると子（群れ全員）が動いてしまうので。
	/// </summary>
	public Vector3	center	{ get; private set; }
	

	/// <summary>
	/// 領域半径。
	/// </summary>
	public float	territory;
	

	/// <summary>
	/// 同時に存在できる最大定員
	/// </summary>
	public int maxStayCapacity;

	/// <summary>
	/// 現在のメンバー数。
	/// </summary>
	public int	memberCount;//	{ get; private set; }



	/// <summary>
	/// キャラクター生成に関する情報。
	/// </summary>
	public SpawnRefCounter spawns;

	



	public MEvent.TriggerHolder<ActivityGroup3> memberEvent { get; private set; }





	
	public struct SpawnRefCounter
	{

		/// <summary>
		/// キャラクター生成が現在進行中かどうか
		/// </summary>
		public bool isSpawning { get { return refCounter > 0; }	}

		int refCounter; // キャラクタ発生コルーチンの現存数


		/// <summary>
		/// キャラクター生成開始通知
		/// </summary>
		public void notifyBegin()
		{
			refCounter++;
		}

		/// <summary>
		/// キャラクター生成終了通知
		/// </summary>
		public void notifyEnd()
		{
			refCounter--;
		}

	}










	void Update()
	{
		// リーダーがいない場合は、gameObject をアクティブにしない。

		center = leader.rb.position;//.tf.position;
		
	}
	
	

	// 初期化 -------------------------------------------

	public override void init()
	{

		this.enabled = false;


		base.init();Debug.Log("gi");


		team.flags = (enTeam)( 1 << (int)enTeamShift.enemy );//
		
		GM.groups.increase();
		
		
		center = tf.position;


		memberCount = 0;

	}

	public override void final()
	{

		GM.groups.decrease();

		releaseSelfToPoolOrDestroy();

/*
		for( var i = 0; i < tf.childCount; i++ )
		{
			var act = tf.GetChild(i).GetComponent<_Action3>();
			
			if( act != null ) leave( act );
		}
*/


		base.final();Debug.Log("gf");
		
	}





	// ユーティリティ ---------------------------------------------

	// メンバーは、_Action3 の配列に保管してもいいな　ああ、でも数が増えるとあれだからいまいちか

	/// <summary>
	/// メンバーへのデリゲート配送システム。
	/// </summary>
	/// <param name="proc">配送されるデリゲート</param>
	public void sendProcToAllMembers( MessageProc proc )
	{
		
		for( var i = tf.childCount; i-- > 0; )
		{
			
			var iAct = tf.GetChild(i).GetComponent<_Action3>();

			if( iAct != null ) proc( iAct );

		}

	}
	
	public delegate void MessageProc( _Action3 act );


	/// <summary>
	/// テリトリー内のメンバに警戒通知を送信する。
	/// </summary>
	/// <param name="host">メンバー</param>
	public void notifyAlert( _Action3 host )
	{
		Debug.Log( "notify alert to territory" + host.GetInstanceID() );

		for( var i = tf.childCount; i-- > 0; )
		{
			var iAct = tf.GetChild( i ).GetComponent<_Action3>();

			if( iAct != host & iAct != null )
			if( !isOutOfTerritory( iAct.rb.position, iAct.character.notifyRecvRanges ) )
			{
				iAct.changeToAlertMode();
			}
		}
	}


	/// <summary>
	/// テリトリー内のメンバーに被攻撃通知を送信する。
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="host">メンバー</param>
	public void notifyAlert( _Action3 attacker, _Action3 host )
	{
		Debug.Log("notify be attacked to territory" + host.GetInstanceID());

		for( var i = tf.childCount; i-- > 0; )
		{
			var iAct = tf.GetChild( i ).GetComponent<_Action3>();

			if( iAct != host & iAct != null )
			if( !isOutOfTerritory( iAct.rb.position, iAct.character.notifyRecvRanges ) )
			{
				iAct.changeToAttackMode( attacker );
			}
		}
	}



	/// <summary>
	/// 発信個体周辺のグループメンバーにのみ被攻撃通知を送信する。
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="host">メンバー</param>
	public void notifyAlertToNearMate( _Action3 attacker, _Action3 host )
	{
		Debug.Log( "notify be attacked to near mate" + host.GetInstanceID() );

		var center = host.rb.position;

		for( var i = tf.childCount ; i-- > 0 ; )
		{

			var iAct = tf.GetChild( i ).GetComponent<_Action3>();

			if( iAct != host & iAct != null )
			{
				var radius = iAct.character.notifyRecvOuterRanges;

				if( ( center - iAct.rb.position ).sqrMagnitude < radius * radius )
				{
					iAct.changeToAttackMode( attacker );
				}
			}

		}
	}



	// 領域判定 -------------------

	/// <summary>
	/// メンバーがテリトリーから出ているかどうか。性格により、許容度が異なる。
	/// </summary>
	/// <param name="self">メンバー</param>
	/// <returns>テリトリー外なら True</returns>
	public bool isOutOfTerritorySelf( _Action3Enemy host )
	{
		//if( act.character.obedience == EnObedience.none ) return false;

		var margin = host.figure.bodyRadius * 2.0f * host.character.outMargins;

		return isOutOfTerritory( host.rb.position, margin );
	}


	/// <summary>
	/// ある地点はテリトリーの外にあるか。テリトリーの形態（球・円）も考慮する。
	/// </summary>
	/// <param name="pos">調べたい地点</param>
	/// <param name="margin">許容範囲</param>
	/// <returns></returns>
	public bool isOutOfTerritory( Vector3 pos, float margin = 0.0f )
	{

		var terr = territory + margin;

		switch( shape )
		{

			case enShape.sphere:

				return ( pos - center ).sqrMagnitude > terr * terr;

			case enShape.circle:

				var range = tf.InverseTransformDirection( pos - center );
				// いちおう上はトランスフォームの上側になるように

				return new Vector2( range.x, range.z ).sqrMagnitude > terr * terr;

		}


		return false;

	}



	// 位置取得 -----------

	/// <summary>
	/// テリトリーの中でランダムな位置を返す。テリトリーの形態（球・円）も考慮する。
	/// テリトリー形状が円の場合、高さは中心位置と同じとなる。
	/// </summary>
	/// <returns>テリトリー内でのランダム位置</returns>
	public Vector3 getRandomPoint()
	{

		switch( shape )
		{
			case enShape.circle:

				return getRandomPointOnCircle();

			case enShape.sphere:

				return getRandomPointInSphere();

		}

		return center;

	}

	public Vector3 getRandomPointOnCircle()
	{

		var range = Random.insideUnitCircle * territory;

		return center + tf.TransformDirection( new Vector3( range.x, 0.0f, range.y ) );

	}

	public Vector3 getRandomPointInSphere()
	{

		return center + Random.insideUnitSphere * territory;

	}





	// 加入・離脱 ----------------------------------------

	public void enter( _Action3 act, bool isLeader = false )
	{

		if( act.connection.group != null )
		{
			act.connection.group.leave( act );
		}


		act.connection.group = this;

		act.tf.parent = tf;

		if( isLeader )
		{
			leader = act;

			this.enabled = true;
		}



		memberEvent.check( this );

		memberCount++;//Debug.Log( GetInstanceID() +" "+ memberCount +" "+ leader );
		
	}
	
	public void leave( _Action3 act )
	{

		act.connection.group = null;

		act.tf.parent = null;


		if( leader == act )
		{
			// とりあえず現段階ではリーダー抜けたら単純に不在に

			leader = null;

			this.enabled = false;
		}


		memberCount--;//Debug.Log( GetInstanceID() +" "+ memberCount +" " );
		
		memberEvent.check( this );
		
		
		if( !spawns.isSpawning & memberCount < 1 )
		{

			memberEvent.destruct();

			final();

			this.releaseSelfToPoolOrDestroy();

		}
		
	}




	// 生成 -------------------------------------------------------------

	public ActivityGroup3 instantiate( Vector3 pos, Quaternion rot )
	{

		var template = this;

		var instance = (ActivityGroup3)template.instantiateWithPoolingGameObject( pos, rot, true );

		instance.init();

		return instance;

	}
	




}
