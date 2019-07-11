using UnityEngine;
using System.Collections;


public class LineParticlePool<LineUnit>
{}

public class _LineParticlePool<TLineUnit> : _PoolingObject3<_LineParticlePool<TLineUnit>> where TLineUnit : _LineParticlePool<TLineUnit>.LineUnit, new()
{


	public int	numberOfSegmnts;	// 一本のラインに継ぎ目をいくつ入れるか

	public int	maxLineLength;		// 最大ライン数

	public int	increaseAtOverMax;



	public Material	material;

	
	
	Mesh	mesh;

	int		freq;

	int     registedDefineLengths;  // 色とサイズを登録した数。maxDefineLength 個まで。

	protected int	unitSize;



	protected LineHolder	lines;

	protected Vector3[]		positions;




	public const int	maxShaderPoints = 240;  // シェーダで描画できる最大の「ポイント」数

	public const int    maxDefineLength = 10;

	MaterialPropertyBlock	mpb;// = new MaterialPropertyBlock();

	Vector4[]   drawPositions;







	// 初期化 ----------------------------------------------

	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();

		drawPositions = new Vector4[ maxShaderPoints ];

		mpb = new MaterialPropertyBlock();

		material = new Material( material );


		unitSize = numberOfSegmnts + 2;


		freq = maxLineLength * unitSize > maxShaderPoints ? maxShaderPoints / unitSize : maxLineLength;


		mesh = new LineMeshCreator( numberOfSegmnts, freq ).create();

		mesh.bounds = new Bounds( Vector3.zero, Vector3.one );

		
		positions = new Vector3[ unitSize * maxLineLength ];

		lines.init( this );

	}

	/// <summary>
	/// 位置配列を拡張する。ライン配列拡張時に呼ばれる。
	/// </summary>
	protected virtual void extentPointArray( int maxLines )
	{

		System.Array.Resize( ref positions, maxLines * unitSize );

		maxLineLength = maxLines;

	}


	/// <summary>
	/// ライン定義（の色・サイズ）を登録する。
	/// </summary>
	/// <param name="lineDefine">ILineParticle を継承したライン定義</param>
	/// <returns>登録したＩＤ</returns>
	public int registLineDefine( StringParticlePool.ILineParticle lineDefine )
	{

		if( registedDefineLengths >= maxDefineLength ) return -1;   // 飽和


		var c = lineDefine.color;

		var s = lineDefine.size;

		var define = new Color( c.r, c.g, c.b, s );


		var defs = material.GetColorArray( "d" ) ?? new Color[8];

		defs[ registedDefineLengths ] = define;

		material.SetColorArray( "d", defs );


		return registedDefineLengths++;

	}




	// 更新処理 -------------------------------------------------

	void Update()
	{

		update();

		draw();
		
	}


	void update()
	{

		for( var il = 0; il < lines.entityLength; il++ )
		{
			
			var line = lines.get( il );

			line.time += GM.t.delta;

			var isDestroyed = line.update( line );

			if( isDestroyed )
			{
				lines.back( il );
			}

		}

	}



	// 位置の取得／設定 ------------------------------------


	public Vector3 getPosition( TLineUnit line, int index )
	{
		return positions[ line.pointId + index ];
	}

	public void setPosition( TLineUnit line, int index, Vector3 pos )
	{
		positions[ line.pointId + index ] = pos;
	}


	public Vector3 getPositionAtFirst( TLineUnit line )
	{
		return positions[ line.pointId ];
	}

	public Vector3 getPositionAtLast( TLineUnit line )
	{
		return positions[ line.pointId + numberOfSegmnts + 1 ];
	}




	// ラインの取得・返却 ------------------------------------------------


	/// <summary>
	/// ラインの取得。始点・終点を指定する。間の点は平均的に算出される。
	/// </summary>
	/// <param name="start">始点</param>
	/// <param name="end">終点</param>
	/// <returns>ライン情報オブジェクト</returns>
	public virtual TLineUnit instantiate( Vector3 start, Vector3 end )
	{

		var line = lines.rent();
		

		var unitLength = numberOfSegmnts + 2;

		var numOfSegR = 1.0f / ( numberOfSegmnts + 1 );


		positions[ line.pointId ] = start;

		positions[ line.pointId + unitLength - 1 ] = end;

		var move = end - start;


		var sqrmag = move.sqrMagnitude;

		var	totalR = MathOpt.invSqrt( sqrmag );
		
		var dir = move * totalR;
		
		var span = totalR * sqrmag * numOfSegR;
		

		var ist = line.pointId + 1;

		var iover = ist + numberOfSegmnts;

		for( var i = ist; i < iover ; i++ )
		{
			positions[i] = start + dir * ( span * i );
		}


		return line;

	}
	

	/// <summary>
	/// ラインの取得。
	/// </summary>
	/// <param name="pos">全点の位置</param>
	/// <returns>ライン情報オブジェクト</returns>
	public virtual TLineUnit instantiate( Vector3 pos )
	{

		var line = lines.rent();


		var unitLength = numberOfSegmnts + 2;


		var ist = line.pointId;
		
		var iover = ist + unitLength;
		
		for( var i = ist; i < iover ; i++ )
		{
			positions[i] = pos;
		}

		
		return line;

	}
	

	/// <summary>
	/// ラインの返却。
	/// </summary>
	/// <param name="lineId">ライン情報オブジェクトのインデックス</param>
	public void releaseToPool( int lineId )
	{

		lines.back( lineId );

	}








	// 描画 -------------------------------------------------

	void draw()
	{
		
		var campos = GM.cameras.player.transform.position;


		var il = 0;

		var loopLimit = 0;
		
		var remain = lines.entityLength;

		do
		{
			
			mpb.Clear();


			var i = 0;

			loopLimit += remain > freq ? freq : remain;
			
			for( ; il < loopLimit; il++ )
			{

				var line = lines.get( il );//Debug.Log(il+" "+line.pointId);

				var updater = (ILineParticle)line.update.Target;
				

				var istart = line.pointId;

				var iover = istart + unitSize;
				
				for( var ip = istart; ip < iover; ip++ )
				{

					var t = new Vector4( positions[ip].x, positions[ip].y, positions[ip].z, 0 );// (float)updater.registedId );

					//mpb.SetVector( "p" + i.ToString(), t );
					drawPositions[ i ] = t;

					i++;

				}

			}


			System.Array.Clear( drawPositions, i, maxShaderPoints - i );

			mpb.SetVectorArray( ShaderId.Position, drawPositions );

			Graphics.DrawMesh( mesh, campos, Quaternion.identity, material, 0, GM.cameras.player, 0, mpb, false, false );

			remain -= freq;

		}
		while( remain > 0 );

	}




	// ライン情報保持 -----------------------------------------------------

	public interface ILineParticle
	{

		int registedId { get; }
		
		float size { get; }
		
		Color color { get; }

	}


	public delegate bool UpdateProc( TLineUnit line );



	public class LineUnit
	{
		
		public int	pointId	{ get; private set; }


		public _Action3		owner;
		
		public UpdateProc	update;


		public Quaternion	rotation;


		public float	time;
		
		public float	barrelFactor;



		public void setId( int i )
		{
			pointId = i;
		}
		
		public virtual void init( UpdateProc updateProc, Quaternion rot, _Action3 act )
		{

			update = updateProc;

			owner = act;

			rotation = rot;

			time = 0.0f;
			
			barrelFactor = 1.0f;

		}

		public virtual void cleanup()
		{

			owner = null;

			update = null;

		}

	}
	

	/// <summary>
	/// ラインを保管する。
	/// </summary>
	protected struct LineHolder
	{

		TLineUnit[]	lines;	// ライン一本単位の情報。位置・速度へのオフセットを格納する。
		
		public int	entityLength	{ get; private set; }	// lines をポイントする

		_LineParticlePool<TLineUnit> pool;


		public void init( _LineParticlePool<TLineUnit> p )
		{

			lines = new TLineUnit[ p.maxLineLength ];

			entityLength = 0;

			pool = p;

		}

		/// <summary>
		/// 既存ラインの取得。
		/// </summary>
		/// <param name="i">ラインＩＤ</param>
		/// <returns>ライン情報オブジェクト</returns>
		public TLineUnit get( int i )
		{

			return lines[ i ];

		}

		/// <summary>
		/// ラインの取得。新しく貸与される。
		/// </summary>
		/// <returns>ライン情報オブジェクト</returns>
		public TLineUnit rent()
		{
			
			if( entityLength >= lines.Length )
			{

				var maxLines = lines.Length + pool.increaseAtOverMax;
				
				Debug.Log( "resize line pool : " + lines.Length.ToString() + " => " + maxLines.ToString() +" / now points : "+ entityLength * pool.unitSize );
				
				System.Array.Resize( ref lines, maxLines );

				pool.extentPointArray( maxLines );

			}
			
			if( lines[ entityLength ] == null )
			{
				lines[ entityLength ] = new TLineUnit();

				lines[ entityLength ].setId( entityLength * pool.unitSize );
			}
			

			return lines[ entityLength++ ];

		}

		/// <summary>
		/// ラインの返却。
		/// </summary>
		/// <param name="id">ラインＩＤ</param>
		public void back( int id )
		{

			var blank = lines[ id ];

			blank.cleanup();


			var ilast = --entityLength;

			lines[ id ] = lines[ ilast ];


			lines[ ilast ] = blank;	// 一度確保したヒープはとっておく

		}

	}

	
	
	// メッシュ生成 -----------------------------------------------------
	
	struct LineMeshCreator
	{
		
		Vector3[]	vtxs;	// なくてもいいんだが、Mesh に必須らしい…。color に仕込んでもいけるのだが。
		Color32[]	bids;
		Vector2[]	uvs;
		int[]		idxs;
		
		int	unitSize;
		int	vtxofs;
		int	idxofs;
		
		float	uvd;
		
		int		freq;
		
		
		public LineMeshCreator( int numSegments, int repeat )
		{
			
			unitSize = numSegments + 2;
			
			freq = repeat;
			
			
			var vsize = unitSize * 2 * freq;
			
			vtxs = new Vector3[ vsize ];
			
			bids = new Color32[ vsize ];
			
			uvs	 = new Vector2[ vsize ];
			
			
			uvd = 1.0f / ( numSegments + 1 );
			
			
			
			var isize = 3 * ( 2 + 2 * numSegments ) * freq;
			
			idxs = new int[ isize ];
			
			
			vtxofs = 0;
			
			idxofs = 0;
			
		}
		
		public Mesh create()
		{
			
			for( var i = 0; i < freq; i++ )
			{
				buildLine( i );
			}
			
			
			var mesh = new Mesh();
			mesh.vertices = vtxs;
			mesh.uv = uvs;
			mesh.colors32 = bids;
			mesh.triangles = idxs;
			mesh.RecalculateNormals();
			mesh.UploadMeshData( true );
			
			return mesh;
			
		}
		
		void buildLine( int ifreq )
		{
			
			var ivtx = vtxofs;
			
			for( var i = 0; i < unitSize; i++ )
			{
				vtxs[ ivtx++ ] = new Vector3( 0.0f, 0.0f, +0.5f );
				vtxs[ ivtx++ ] = new Vector3( 0.0f, 0.0f, -0.5f );
				//vtxs[ ivtx++ ] = new Vector3( -0.5f, 0.0f, 0.0f );
				//vtxs[ ivtx++ ] = new Vector3( +0.5f, 0.0f, 0.0f );
			}
			
			
			var ibid = vtxofs;
			
			for( var i = 0; i < unitSize - 1; i++ )
			{
				var pid = (byte)( i + ifreq * unitSize );

				bids[ ibid++ ] = new Color32( pid, (byte)( pid + 1 ), 0, pid );
				bids[ ibid++ ] = new Color32( pid, (byte)( pid + 1 ), 0, pid );
			}
			for( var i = unitSize - 1 ; i < unitSize ; i++ )
			{
				var pid = (byte)( i + ifreq * unitSize );

				bids[ ibid++ ] = new Color32( (byte)( pid - 1 ), pid, 0, pid );
				bids[ ibid++ ] = new Color32( (byte)( pid - 1 ), pid, 0, pid );
			}


			var iuv = vtxofs;
			
			var v = 0.0f;
			
			for( var i = 0; i < unitSize; i++, v += uvd )
			{
				uvs[ iuv++ ] = new Vector2( 1.0f, v );
				uvs[ iuv++ ] = new Vector2( 0.0f, v );
			}
			
			
			
			for( var i = 0; i < unitSize - 1; i++, vtxofs += 2 )
			{
				idxs[ idxofs++ ] = vtxofs + 0;
				idxs[ idxofs++ ] = vtxofs + 1;
				idxs[ idxofs++ ] = vtxofs + 2;
				idxs[ idxofs++ ] = vtxofs + 2;
				idxs[ idxofs++ ] = vtxofs + 1;
				idxs[ idxofs++ ] = vtxofs + 3;
			}
			
			vtxofs += 2;
			
		}
		
	}

}
