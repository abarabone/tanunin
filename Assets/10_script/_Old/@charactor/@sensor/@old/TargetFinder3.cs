using UnityEngine;
using System.Collections;

public class TargetFinder3 : MonoBehaviour
// 敵を（発見・通知・被弾などで）想定した時点から、索敵を始める。索敵活動には時間限界があり、敵を発見するたびにリセットされる。
// ターゲットには、敵がセットされていればその「敵」を、敵自体がセットされていなければ記録されている「場所」を結果とする 
// 　敵を決定していない場合、現在位置からの誤差加算を繰り返す。
// search() をよぶと、いい感じに scan()/capture()/cocapture() してくれる（設定されたインターバルを考慮する）。直接呼ぶこともできる（未テスト）。
// 　結果は target に入っているので、それをもとにうんたらする。
// 　conconcentrate() は現在未実装。search() よりも限定されたピーキーな索敵となる。
// 　search() で false が返るなら、敵を捉えていない。ただし時間限界が来るまで、ターゲット位置には「想定」した位置が入り続ける。
// 観測の中心は tfSensor 対象は tfObserbedCenter 。
// perceptions[] は感覚セットの配列。感覚セットの中に各感覚がある。
{


	public Sencor3Holder[]	perceptions;


	public Transform	tfSensor;


	public int	targetLayerMask		{ get; protected set; }




	public Rigidbody	rb	{ get; protected set; }	


	float	researchBaseTime;//nextTimeCount;


	public float	sensitiveRate	{ get; protected set; }	// 感覚の鋭敏さ（現時点では範囲が広がるだけ）
	//public float	sensitiveRateR	{ get; protected set; }
	



	public TargetInfoUnit	target;


	public bool		isRealTargetLost	{ get; protected set; }		// キャプチャに失敗している

	public bool		isVirtualTargetLost	{ get { return Time.time > targetReleaseTime; } }	// 時間限界を過ぎた

	float	targetReleaseTime;	// 索敵限界時間


	public const float	targetReleaseLimit = 60.0f * 3.0f;// 敵を探し続けるしつこさ　敵性格依存にするかも


	


	// 初期化 ------------------------------

	void Awake()
	{

		rb = GetComponent<Rigidbody>();
		
		targetLayerMask = UserLayer.players;//UserLayer.enemyEnvelope;//UserLayer.players | UserLayer.enemyEnvelope;

	}

	void OnEnable()
	{target = new TargetInfoUnit( rb.position, 30.0f ); targetReleaseTime=1000.0f;

		setSensitiveRate( 1.0f );

		isRealTargetLost = true;

		researchBaseTime = Time.time - Random.value * perceptions[0].scanInterval;

	}
	



	// サーチ ------------------------------------

	public bool search( int id )
	{

		if( isRealTargetLost )
		{

			if( Time.time > researchBaseTime + perceptions[id].scanInterval )
			{

				return scan( id );

			}

		}
		else
		{

			if( Time.time > researchBaseTime + perceptions[id].captureInterval )
			{

				return capture( id );

			}

		}


		target.cocapture();


		return !isRealTargetLost;

	}


	// スキャン -------------------------------------------------

	public bool scan( int id )
	{

		researchBaseTime = Time.time;


		var pos = tfSensor.position;

		var maxRadius = perceptions[id].maxDistance * sensitiveRate;


		var targ = new TargetInfoUnit();


		var cs = Physics.OverlapSphere( pos, maxRadius, targetLayerMask );


		if( cs.Length > 1 )
		{

			var others = new _Action3[ cs.Length ];

			var sqrDists = new float[ cs.Length ];


			for( var i = 0; i < cs.Length; i++ )
			{

				var thisRb = cs[i].attachedRigidbody;

				if( thisRb != rb )
				{

					var other = thisRb.GetComponent<_Action3>();

					sqrDists[i]	= ( other.tfObservedCenter.position - pos ).sqrMagnitude;

					others[i] = other;

				}
				else
				{

					sqrDists[i]	= float.PositiveInfinity;

				}

			}


			System.Array.Sort( sqrDists, others );


			targ = perceptions[id].scan( this, others, sqrDists );

		}
		else if( cs.Length == 1 && cs[0].attachedRigidbody != rb )
		{
			
			var otherRb = cs[0].attachedRigidbody;

			var other = otherRb.GetComponent<_Action3>();

			var sqrDist = ( other.tfObservedCenter.position - pos ).sqrMagnitude;


			targ = perceptions[id].capture( this, other, sqrDist );

		}
		else
		{

			//targ = new TargetInfoUnit();

		}


		return isTargetFound( ref targ );

	}


	// キャプチャ ---------------------------

	public bool capture( int id )
	{

		var resTarget = new TargetInfoUnit();


		if( target.isExists() )
		{

			researchBaseTime = Time.time;
			
			
			var sqrDist = ( target.act.tfObservedCenter.position - tfSensor.position ).sqrMagnitude;
			
			
			resTarget = perceptions[id].capture( this, target.act, sqrDist );

		}

		
		return isTargetFound( ref resTarget );

	}

	public bool capture( int id, _Action3 thisTarget, bool forceChangeTarget = false )
	{

		if( thisTarget != null && !thisTarget.isDead )
		{

			target.act = thisTarget;

			var isFound = capture( id );

			if( !isFound && forceChangeTarget )
			{
				target.act = thisTarget;

				isRealTargetLost = true;
			}


			return isFound;

		}
		else
		{

			if( forceChangeTarget )
			{
				target = new TargetInfoUnit();

				isRealTargetLost = true;
			}
			
			return false;

		}

	}







	// ユーティリティ ------------------------------------

	public void setSensitiveRate( float rate )
	{

		sensitiveRate = rate;

		//sensitiveRateR = 1.0f / rate;

	}

	
	public enum enTargetCategory
	{
		player,
		enemy,
		all
	}

	public void setTargetCategory( enTargetCategory team )
	{

		switch( team )
		{
			case enTargetCategory.player :	targetLayerMask = UserLayer.players; break;
			case enTargetCategory.enemy :	targetLayerMask = UserLayer.enemyEnvelope; break;
			default :						targetLayerMask = UserLayer.players | UserLayer.enemyEnvelope; break;
		}

	}
	
	
	public void forceTarget( _Action3 act, float errRadius = 30.0f )
	{
		
		target.cocapture( act, errRadius );

		targetReleaseTime = Time.time + targetReleaseLimit;

	}

	public void forceTarget( Vector3 point, float errRadius = 30.0f )
	{

		target = new TargetInfoUnit( point, errRadius );
		
		targetReleaseTime = Time.time + targetReleaseLimit;

	}

	public void forceUntarget()
	{

		target = new TargetInfoUnit();

		targetReleaseTime = 0.0f;

	}

	

	bool isTargetFound( ref TargetInfoUnit targ )
	{
		
		var isFound = targ.isExists();


		if( isFound )
		{

			target = targ;

			targetReleaseTime = Time.time + targetReleaseLimit;

		}
		else
		{

			if( isVirtualTargetLost )
			{

				target = new TargetInfoUnit();

			}
			else
			{
				// 敵を発見できなくても、時間限界内なら敵のだいたい（半径３０ｍ）の位置を特定し続ける。
				// ただし、残り３０秒を切ると時間とともに誤差が大きくなっていく。

				var remainingTime = targetReleaseTime - Time.time;
				
				var errRadius = remainingTime > 30.0f ? 30.0f : 30.0f + 30.0f - remainingTime;
				
				target.cocapture( errRadius );

			}

		}
		
		isRealTargetLost = !isFound;


		return isFound;

	}




	// ターゲット所持 ---------------------------------

	public struct TargetInfoUnit
	{
		
		public _Action3	act;

		public Vector3	position;

		float	errRadius;


		public TargetInfoUnit( _Action3 a, float r )
		{
			
			act = a;

			errRadius = r;
			
			position = act.rb.position + Random.insideUnitSphere * errRadius;

		}
		
		public TargetInfoUnit( Vector3 point, float r )
		{
			
			act = null;
			
			errRadius = r;

			position = point + Random.insideUnitSphere * errRadius;
			
		}
		
		public void cocapture()
		{
			
			var targpos = isExists() ? act.rb.position : position;
			
			position = targpos + Random.insideUnitSphere * errRadius;
			
		}

		public void cocapture( _Action3 a, float r )
		{

			act = a;

			errRadius = r;
			
			cocapture();

		}
		
		public void cocapture( float r )
		{
			errRadius = r;

			cocapture();
		}


		public float sqrDistance( Vector3 ownpos )
		{
			return ( position - ownpos ).sqrMagnitude;
		}


		
		public bool isExists()
		{
			
			if( act != null )
			{
				if( act.isDead )
				{
					this = new TargetInfoUnit();
				}
				else
				{
					return true;
				}
			}
			
			return false;
			
		}
		
	}
	







}
