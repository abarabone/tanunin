using UnityEngine;
using System.Collections;
using System;

public class Radar : MonoBehaviour
{
	
	
	public Shader		shader;			// レーダーのシェーダ
	
	public Texture2D	texture;		// 点を描画するための円テクスチャ

	public int			maxPoints;		// 点の最大数（ベースレイヤとプレイヤーを含む）

	public float        sphereRadius;	// レーダー範囲の半径




	// 以下が static なのは、Rader.xxx でアクセスしたいため。

	
	static MeshRenderer		mr;


	
	static DrawPointUnit[]	drawPoints; // 現在のフレームで登録された点の仮情報。ベースレイヤ・プレイヤーを含めない。

	static Vector4[]		positions;	// ベースレイヤ・プレイヤーを含めた、全ての点の配列。この配列がシェーダへ送られる。

	
	static int	nowIndex;	// 現在のフレームで点を登録した個数。フレームごとにクリアされる。ベースレイヤ・プレイヤーは除く。


	static Matrix4x4	mtCenterInv;



	public enum enType
	{
		nullType,

		baseLayer,

		player,
		second,
		third,
		enemy,
		item,
		machine,

		length
	}

	public enum enClass
	{
		nullClass,

		baseLayer,

		normal,
		heavy,
		carrier,
		fortress,

		length
	}




	/// <summary>
	/// 
	/// </summary>
	struct DrawPointUnit
	{

		public enType type;

		public Vector3 position;

		public enClass sizeClass;

		public DrawPointUnit( enType t, Vector3 p, enClass c )
		{
			type		= t;
			position	= p;
			sizeClass	= c;
		}

	}



	void Awake()
	{
		
		drawPoints = new DrawPointUnit[ maxPoints - 2 ];

		positions = new Vector4[ maxPoints ];


		createMesh( maxPoints );


		setViewTypeProperties();

		setViewClassSizeProperties();


		setBaseLayerProperty();

		setPlayerProperty();
		

		nowIndex = 0;

	}

	void OnEnable()
	{

		// カウンタをリセットしないと、非表示の時にカウンタが増加してるのでヤバイ

		nowIndex = 0;

	}

	void LateUpdate()
	{
		
		setPointProperties();
		

		nowIndex = 0;

	}
	


	/// <summary>
	/// レーダー中心点の位置を、ワールド座標でセットする。
	/// </summary>
	/// <param name="tfCenter"></param>
	static public void setCenter( Transform tfCenter )
	{

		mtCenterInv = tfCenter.worldToLocalMatrix;

	}

	/// <summary>
	/// 現在のフレームで、レーダーに表示させたい点を登録する。
	/// </summary>
	/// <param name="type"></param>
	/// <param name="pos"></param>
	/// <param name="sizeClass"></param>
	static public void setLocation( enType type, Vector3 pos, enClass sizeClass )
	{

		if( nowIndex >= drawPoints.Length ) return;

		drawPoints[ nowIndex++ ] = new DrawPointUnit( type, pos, enClass.normal );

	}
	/*
	static public void setViewProperty( enType type, Vector3 pos, float size )//old
	{

		drawPoints[ nowIndex++ ] = new DrawPointUnit( type, pos, enClass.normal );
		
	}
	*/


	/// <summary>
	/// ベースレイヤーのシェーダプロパティのセット（常に定位置なので、一度でよい）
	/// （実際にはシェーダに登録はせず、登録前段階の配列に値をセットする）
	/// </summary>
	void setBaseLayerProperty()
	{

		// ベースレイヤーの座標セット

		var layerinfo = (float)( (int)enClass.baseLayer * 16 + (int)enType.baseLayer );

		var vlayer = new Vector4( 0.0f, 0.0f, 0.3f, layerinfo );

		positions[ 0 ] = vlayer;

	}

	/// <summary>
	/// プレイヤーのシェーダプロパティのセット（常に中央なので、一度でよい）
	/// （実際にはシェーダに登録はせず、登録前段階の配列に値をセットする）
	/// </summary>
	void setPlayerProperty()
	{

		// プレイヤーの座標セット

		var playerinfo = (float)( (int)enClass.normal * 16 + (int)enType.player );

		var vplayer = new Vector4( 0.0f * 0.3f, 0.0f * 0.3f, 0.0f, playerinfo );

		positions[ 1 ] = vplayer;

	}

	/// <summary>
	/// フレーム内に setLocation() で登録された点をシェーダプロパティにセットする。
	/// </summary>
	void setPointProperties()
	{

		var r   = (double)sphereRadius;

		var rxr = sphereRadius * sphereRadius;

		const int playerAndBaseLayerOffset    = 2;
		

		for( var i = 0; i < nowIndex; i++ )
		{

			var dir = mtCenterInv.MultiplyPoint3x4( drawPoints[i].position );
			
			var dir2d = new Vector2( dir.x, dir.z );
			
			dir2d = ( dir2d.sqrMagnitude > rxr )? dir2d.normalized: dir2d * (float)( 1.0 / r );


			// クラスサイズ（点の大きさ）とタイプ（点の色）を設定。

			var info = (float)( (int)drawPoints[i].sizeClass * 16 + (int)drawPoints[i].type );


			// 高さをセットする。

			var height = Mathf.Clamp( dir.y * (float)( 1.0 / r ), -1.0f, 1.0f ) * 0.5f;


			//var p = new Vector4( dir2d.x * 0.3f, dir2d.y * 0.3f, height, info );
			var p = new Vector4( dir2d.x * 0.49f, dir2d.y * 0.49f, height, info );


			positions[ i + playerAndBaseLayerOffset ] = p;
		}


		// シェーダーパラメータから、余剰な点をゼロクリアして、必要分をセットする。

		Array.Clear( positions, playerAndBaseLayerOffset + nowIndex, maxPoints - nowIndex - playerAndBaseLayerOffset );

		mr.material.SetVectorArray( ShaderId.Position, positions );
		
	}

	
	/// <summary>
	/// タイプ（点の色）参照用のシェーダプロパティをセットする。
	/// </summary>
	static void setViewTypeProperties()
	{
		
		var colors = new Color[]
		{
			Color.black,	// ヌルタイプ
			new Color( 0.0f, 0.2f, 0.0f, 0.7f ),	// ベースレイヤ
			Color.yellow,	// プレイヤー
			Color.magenta,	// 友軍
			Color.white,	// 市民
			Color.red,		// 敵
			Color.green,	// アイテム
			Color.blue		// マシーン
		};

		mr.material.SetColorArray( ShaderId.Color, colors );
	}

	/// <summary>
	/// クラスサイズ（点の大きさ）参照用ののシェーダプロパティをセットする。
	/// </summary>
	static void setViewClassSizeProperties()
	{

		var mat = mr.material;

		var s0 = 0.0f;	// ヌルクラス
		var s1 = 1.0f;
		var s2 = 0.04f * ( 0.5f + 1.0f * 0.5f );
		var s3 = 0.04f * ( 0.5f + 2.0f * 0.5f );
		var s4 = 0.04f * ( 0.5f + 3.0f * 0.5f );
		var s5 = 0.04f * ( 0.5f + 5.0f * 0.5f );

		var sizeList = new Vector4[]
		// 本来は float[] でよいが、shader で効率が悪いので Vector4 に押し込めている（float[]はVector4[]の x のみ使用）。
		{
			new Vector4( s0, s1, s2, s3 ),
			new Vector4( s4, s5, 0.0f, 0.0f )
		};

		mr.material.SetVectorArray( ShaderId.ClassSize, sizeList );

	}


	/// <summary>
	/// 点ビルボードの集合メッシュを作成する。
	/// </summary>
	/// <param name="pointLength">点の最大数。ベースレイヤ・プレイヤー分も含む。</param>
	void createMesh( int pointLength )
	{
		
		var fieldLength = pointLength;
		
		var idxs	= new int[ 6 * fieldLength	];
		var vtxs	= new Vector3[ 4 * fieldLength ];
		var uvs		= new Vector2[ 4 * fieldLength ];
		var ids		= new Color32[ 4 * fieldLength ];
		
		
		createPoint( 0, idxs, vtxs, uvs, ids );			// ベースレイヤ
		
		createPoint( 1, idxs, vtxs, uvs, ids, 0.95f );	// プレイヤー

		
		for( var i = 2; i < fieldLength; i++ )
		{
			
			createPoint( i, idxs, vtxs, uvs, ids, 0.95f );
			
		}
		
		
		var mesh = new Mesh();
		
		mesh.vertices	= vtxs;
		mesh.uv			= uvs;
		mesh.colors32	= ids;
		mesh.triangles	= idxs;
		
		
		var mf	= gameObject.AddComponent<MeshFilter>();
		mf.sharedMesh = mesh;
		
		
		var mat = new Material( shader );
		mat.mainTexture	= texture;
		
		
		mr	= gameObject.AddComponent<MeshRenderer>();
		mr.sharedMaterial = mat;
		
	}
	
	
	void createPoint( int i, int[] idxs, Vector3[] vtxs, Vector2[] uvs, Color32[] ids, float uvscale = 1.0f )
	{
		
		var ivtx = i * 4;
		
		vtxs[ ivtx + 0 ]	= new Vector3( -0.5f,	0.5f, 1.0f );	// 左上
		vtxs[ ivtx + 1 ]	= new Vector3(	0.5f,	0.5f, 1.0f );	// 右上
		vtxs[ ivtx + 2 ]	= new Vector3(	0.5f, -0.5f,  1.0f );	// 右下
		vtxs[ ivtx + 3 ]	= new Vector3( -0.5f, -0.5f,  1.0f );	// 左下

		uvs[ ivtx + 0 ]	= Vector2.one * 0.5f + new Vector2( -0.5f, -0.5f ) * uvscale;	// 左上
		uvs[ ivtx + 1 ]	= Vector2.one * 0.5f + new Vector2(	 0.5f, -0.5f ) * uvscale;	// 右上
		uvs[ ivtx + 2 ]	= Vector2.one * 0.5f + new Vector2(	 0.5f,	0.5f ) * uvscale;	// 右下
		uvs[ ivtx + 3 ]	= Vector2.one * 0.5f + new Vector2( -0.5f,	0.5f ) * uvscale;	// 右下

		ids[ ivtx + 0 ]	= new Color32( 0, 0, 0, (byte)i );	// 左上
		ids[ ivtx + 1 ]	= new Color32( 0, 0, 0, (byte)i );	// 右上
		ids[ ivtx + 2 ]	= new Color32( 0, 0, 0, (byte)i );	// 右下
		ids[ ivtx + 3 ]	= new Color32( 0, 0, 0, (byte)i );	// 右下
		
		
		var iidx = i * 6;
		
		idxs[ iidx++ ]	= ivtx + 0;
		idxs[ iidx++ ]	= ivtx + 1;
		idxs[ iidx++ ]	= ivtx + 3;
		idxs[ iidx++ ]	= ivtx + 3;
		idxs[ iidx++ ]	= ivtx + 1;
		idxs[ iidx++ ]	= ivtx + 2;
		
	}

	
}
