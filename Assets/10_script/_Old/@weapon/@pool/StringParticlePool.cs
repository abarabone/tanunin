using UnityEngine;
using System.Collections;

public class StringParticlePool : _LineParticlePool<StringParticlePool.StringUnit>
{


	Vector3[]	velocities;
	

	public class StringUnit : LineUnit
	{

		public Transform	tfMuzzle;	// 射出されたマズルを覚えておく

		public Transform	tfHitPoint;	// くっついた先も保持してもいいんじゃないだろうか


		public override void cleanup()
		{
			base.cleanup();

			tfMuzzle = null;

			tfHitPoint = null;
		}

	}







	// 初期化 ----------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();


		velocities = new Vector3[ unitSize * maxLineLength ];
		
	}

	/// <summary>
	/// 位置配列と速度配列を拡張する。また、補足配列も拡張する。ライン配列拡張時に呼ばれる。
	/// </summary>
	protected override void extentPointArray( int maxLines )
	{

		base.extentPointArray( maxLines );

		System.Array.Resize( ref velocities, maxLines * unitSize );
		
	}



	// 取得／設定ユーティリティ -------------------------------------------------------
	
	public Vector3 getVelocity( StringUnit line, int index )
	{
		return velocities[ line.pointId + index ];
	}
	
	public void setVelocity( StringUnit line, int index, Vector3 v )
	{
		velocities[ line.pointId + index ] = v;
	}


	public Vector3 getVelocityAtFirst( StringUnit line )
	{
		return getVelocity( line, line.pointId );
	}

	public Vector3 getVelocityAtLast( StringUnit line )
	{
		return getVelocity( line, line.pointId + numberOfSegmnts + 1 );
	}








	// 糸のびよんびよんした移動 ------------------------------------------


	/// <summary>
	/// 先端と終端を指定して、びよんびよんさせる。
	/// </summary>
	public void moveOnFixedBothEnds( LineUnit line, Vector3 first, Vector3 last, float stiffness, float damping )
	{

		var ifirst = line.pointId;

		var ilast = ifirst + numberOfSegmnts + 1;

		
		velocities[ ifirst ] = ( first - positions[ ifirst ] ) * GM.t.deltaR;
		
		velocities[ ilast ] = ( last - positions[ ilast ] ) * GM.t.deltaR;

		positions[ ifirst ] = first;
		
		positions[ ilast ] = last;


		moveOnFixedBothEnds( line, stiffness, damping );

	}


	/// <summary>
	/// 先端を指定して、びよんびよんさせる。終端は動かない。
	/// </summary>
	public void moveOnFixedBothEndsAtFirst( LineUnit line, Vector3 first, float stiffness, float damping )
	{

		var ifirst = line.pointId;


		velocities[ ifirst ] = ( first - positions[ ifirst ] ) * GM.t.deltaR;

		positions[ ifirst ] = first;


		moveOnFixedBothEnds( line, stiffness, damping );

	}


	/// <summary>
	/// 終端を指定して、びよんびよんさせる。先端は動かない。
	/// </summary>
	public void moveOnFixedBothEndsAtLast( LineUnit line, Vector3 last, float stiffness, float damping )
	{

		var ilast = line.pointId + numberOfSegmnts + 1;


		velocities[ ilast ] = ( last - positions[ ilast ] ) * GM.t.deltaR;

		positions[ ilast ] = last;


		moveOnFixedBothEnds( line, stiffness, damping );

	}


	/// <summary>
	/// 先端と終端を現在の位置のまま、びよんびよんさせる。
	/// </summary>
	public void moveOnFixedBothEnds( LineUnit line, float stiffness, float damping )
	{
		
		var ifirst = line.pointId;
		
		var ilast = ifirst + numberOfSegmnts + 1;


		var dt = GM.t.delta;// > 0.1f ? 0.1f : GM.t.delta;
		
		
		for( var i = ifirst + 1; i < ilast; i++ )
		{
			
			//var desired = positions[i] + ( velocities[i] + ( Physics.gravity * dt ) ) * dt;
			var desired = ( positions[i-1] + positions[i+1] ) * 0.5f;
			//desired = ( desired + ( positions[i] + ( velocities[i] + Physics.gravity * dt ) * dt ) ) * 0.5f;
			
			// バネの力を計算
			Vector3 stretch = positions[i] - desired;
			Vector3 force = -stiffness * stretch - damping * velocities[i];
			
			// 速度
			velocities[i] += ( force + Physics.gravity ) * dt;
			
		}
		
		for( var i = ifirst + 1; i < ilast; i++ )
		{	
			// 移動
			positions[i] += velocities[i] * dt;
			
		}

	}


	/// <summary>
	/// 先端を指定して、びよんびよんさせる。終端は先端に引っ張られていく。
	/// </summary>
	public void movePulling( LineUnit line, Vector3 start, float stiffness, float damping )
	{
		
		var ifirst = line.pointId;

		var iover = ifirst + numberOfSegmnts + 2;
		
		
		velocities[ ifirst ] = ( start - positions[ ifirst ] ) * GM.t.deltaR;

		positions[ ifirst ] = start;

		
		var dt = GM.t.delta;// > 0.1f ? 0.1f : GM.t.delta;
		
		
		for( var i = ifirst + 1; i < iover; i++ )
		{
			
			//var desired = positions[i] + ( velocities[i] + ( Physics.gravity * dt ) ) * dt;
			var desired = (i == iover - 1 ? positions[i-1] + positions[i+0] : positions[i-1] + positions[i+1]) * 0.5f;
			//desired = ( desired + ( positions[i] + ( velocities[i] + Physics.gravity * dt ) * dt ) ) * 0.5f;
			
			// バネの力を計算
			Vector3 stretch = positions[i] - desired;
			Vector3 force = -stiffness * stretch - damping * velocities[i];

			// 速度
			velocities[i] += ( force + Physics.gravity ) * dt;
			
		}
		
		for( var i = ifirst + 1; i < iover; i++ )
		{	
			// 移動
			positions[i] += velocities[i] * dt;
			
		}
		
	}


	/// <summary>
	/// これはよくわからない。わすれました。両端が引っ張られて縮むのかな…。
	/// いや締まるんじゃないかな、まっすぐに
	/// </summary>
	public void shrink( LineUnit line, Vector3 first, Vector3 last )
	{

		var ifirst = line.pointId;
		
		var ilast = ifirst + numberOfSegmnts + 1;
		
		
		velocities[ ifirst ] = ( first - positions[ ifirst ] ) * GM.t.deltaR;
		
		velocities[ ilast ] = ( last - positions[ ilast ] ) * GM.t.deltaR;
		
		positions[ ifirst ] = first;
		
		positions[ ilast ] = last;

		
		var dt = GM.t.delta;// > 0.1f ? 0.1f : GM.t.delta;
		

		var move = ( last - first );

		var sqrmag = move.sqrMagnitude;

		var magR = MathOpt.invSqrt( sqrmag );

		var unit = magR * sqrmag / ( numberOfSegmnts + 1 );

		var dir = move * magR;


		for( var i = ifirst + 1; i < ilast; i++ )
		{
			
			var desired = first + dir * ( unit * i );

			// 速度
			velocities[i] = ( desired - positions[i] ) * 0.5f * 120.0f;
			
		}
		
		for( var i = ifirst + 1; i < ilast; i++ )
		{	
			// 移動
			positions[i] += velocities[i] * dt;
			
		}
		
	}

	
	/// <summary>
	/// 終端を指定してびよんびよんさせる。先端は引っ張られていく。
	/// </summary>
	public void pullToLast( LineUnit line, Vector3 last, float stiffness, float damping )
	{

		var ifirst = line.pointId;

		var ilast = ifirst + numberOfSegmnts + 1;
		
		
		velocities[ ilast ] = ( last - positions[ ilast ] ) * GM.t.deltaR;
		
		positions[ ilast ] = last;
		
		
		var dt = GM.t.delta;// > 0.1f ? 0.1f : GM.t.delta;
		
		
		for( var i = ifirst; i < ilast; i++ )
		{

			var desired = last;
			
			// バネの力を計算
			Vector3 stretch = positions[i] - desired;
			Vector3 force = -stiffness * stretch - damping * velocities[i];
			
			// 速度
			velocities[i] += ( force + Physics.gravity ) * dt;
			
		}
		
		for( var i = ifirst; i < ilast; i++ )
		{	
			// 移動
			positions[i] += velocities[i] * dt;
			
		}
		
	}






	// 生成 ---------------------------------------------------------------

	public override StringUnit instantiate( Vector3 start, Vector3 end )
	{
		
		var line = base.instantiate( start, end );

		initVelocities( line.pointId );
		
		return line;
		
	}
	
	public override StringUnit instantiate( Vector3 pos )
	{

		var line = base.instantiate( pos );
		
		initVelocities( line.pointId );
		
		return line;

	}

	void initVelocities( int istart )
	{
		var iover = istart + numberOfSegmnts + 2;

		for( var i = istart ; i < iover ; i++ )
		{
			velocities[ i ] = Vector3.zero;
		}
	}


}
