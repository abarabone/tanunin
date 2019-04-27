using UnityEngine;
using System.Collections;


public struct MeshElementsCounter
{

	public int	vtxLength	{ private set; get; }
	public int	idxLength	{ private set; get; }

	public MeshElementsCounter count( Mesh srcmesh )
	{
		vtxLength	+= srcmesh.vertexCount;
		idxLength	+= srcmesh.triangles.Length;

		return this;
	}

	public MeshElementsCounter( MeshFilter[] mfs )
	{
		vtxLength	= 0;
		idxLength	= 0;

		foreach( var mf in mfs )
		{
			count( mf.sharedMesh );
		}
	}

	public MeshElementsCounter( MonoBehaviour[] components )
	{
		vtxLength	= 0;
		idxLength	= 0;

		foreach( var component in components )
		{
			var mf = component.GetComponentInChildren<MeshFilter>();
			
			count( mf.sharedMesh );
		}
	}

}


// **************************************************************

public enum EnMode
{
	normal,
	dynamic,
	writeOnly
}


// ---------------------------------------------------

public class CollisionMeshCreator
{
	public int[]		idxs	{ protected set; get; }
	
	public Vector3[]	vtxs	{ protected set; get; }
	
	protected int	idxProgress;
	protected int	vtxProgress;
	
	public virtual void alloc( ref MeshElementsCounter mec )
	{
		
		idxs	= new int[ mec.idxLength ];
		
		vtxs	= new Vector3[ mec.vtxLength ];
		
	}
	
	protected void addIndices( Mesh srcmesh, ref Matrix4x4 mt )
	{

		var isReverse = MeshUtility.isReverse( ref mt );//( mt[0,0] < 0.0f ) ^ ( mt[1,1] < 0.0f ) ^ ( mt[2,2] < 0.0f );//Debug.Log( srcmesh.name +" "+ mt[0,0] +" "+ mt[1,1] +" "+ mt[2,2] );


		var ii = idxProgress;

		var	srcidxs = srcmesh.triangles;

		if( isReverse )
		{
			for( var i = 0; i < srcmesh.triangles.Length; i += 3 )
			{
				idxs[ ii++ ]	= vtxProgress + srcidxs[ i + 0 ];
				idxs[ ii++ ]	= vtxProgress + srcidxs[ i + 2 ];
				idxs[ ii++ ]	= vtxProgress + srcidxs[ i + 1 ];
			}
		}
		else
		{
			for( var i = 0; i < srcmesh.triangles.Length; )
			{
				idxs[ ii++ ]	= vtxProgress + srcidxs[ i++ ];
				idxs[ ii++ ]	= vtxProgress + srcidxs[ i++ ];
				idxs[ ii++ ]	= vtxProgress + srcidxs[ i++ ];
			}
		}

	}
	
	protected virtual void addVertices( Mesh srcmesh, ref Matrix4x4 mt )
	{
		
		var iv = vtxProgress;

		var srcvtxs = srcmesh.vertices;

		for( var i = 0; i < srcvtxs.Length; i++, iv++ )
		{
			vtxs[ iv ]	= mt.MultiplyPoint3x4( srcvtxs[ i ] );
		}

	}

	public virtual void addGeometory( Mesh srcmesh, ref Matrix4x4 mt )
	{

		addIndices( srcmesh, ref mt );

		addVertices( srcmesh, ref mt );


		idxProgress += srcmesh.triangles.Length;

		vtxProgress += srcmesh.vertexCount;

	}

	public virtual Mesh create( EnMode mode = EnMode.normal )
	{

		var dstmesh = new Mesh();

		if( mode == EnMode.dynamic ) dstmesh.MarkDynamic();

		dstmesh.vertices = vtxs;

		dstmesh.triangles = idxs;

		if( mode == EnMode.writeOnly ) dstmesh.UploadMeshData( true );

		return dstmesh;

	}
	
}

// ---------------------------------------------------

public class SimpleMeshCreator : CollisionMeshCreator
{

	public Vector2[]	uvs		{ protected set; get; }


	public override void alloc( ref MeshElementsCounter mec )
	{

		base.alloc( ref mec );

		uvs		= new Vector2[ mec.vtxLength ];

	}

	protected override void addVertices( Mesh srcmesh, ref Matrix4x4 mt )
	{

		var iv = vtxProgress;

		var srcvtxs = srcmesh.vertices;
		var srcuvs	= srcmesh.uv;

		for( var i = 0; i < srcmesh.vertexCount; i++, iv++ )
		{
			vtxs[ iv ]	= mt.MultiplyPoint3x4( srcvtxs[ i ] );
			uvs[ iv ]	= srcuvs[ i ];
		}

	}

	public override Mesh create( EnMode mode = EnMode.normal )
	{
		
		var dstmesh = new Mesh();
		
		if( mode == EnMode.dynamic ) dstmesh.MarkDynamic();

		dstmesh.vertices	= vtxs;
		dstmesh.uv			= uvs;
		
		dstmesh.triangles	= idxs;
		
		if( mode == EnMode.writeOnly ) dstmesh.UploadMeshData( true );

		return dstmesh;

	}
	
}

// ---------------------------------------------------

public class NormalMeshCreator : SimpleMeshCreator
{
	
	public Vector3[]	nms		{ protected set; get; }
	
	protected bool	isNoNormal;

	public override void alloc( ref MeshElementsCounter mec )
	{
		
		base.alloc( ref mec );
		
		nms	 = new Vector3[ mec.vtxLength ];

	}
	
	protected override void addVertices( Mesh srcmesh, ref Matrix4x4 mt )
	{

		base.addVertices( srcmesh, ref mt );

		
		var iv = vtxProgress;

		var srcnms	= srcmesh.normals;

		isNoNormal = srcnms.Length == 0;
		
		if( isNoNormal )
		{

		}
		else
		{
			for( var i = 0; i < srcmesh.vertexCount; i++, iv++ )
			{
				nms[ iv ]	= mt.MultiplyVector( srcnms[ i ] );
			}
		}

	}
	
	public override Mesh create( EnMode mode = EnMode.normal )
	{
		
		var dstmesh = new Mesh();
		
		if( mode == EnMode.dynamic ) dstmesh.MarkDynamic();

		dstmesh.vertices	= vtxs;
		dstmesh.uv			= uvs;
		dstmesh.normals		= nms;
		
		dstmesh.triangles	= idxs;

		if( true || isNoNormal )
		{
			dstmesh.RecalculateNormals();
		}

		if( mode == EnMode.writeOnly ) dstmesh.UploadMeshData( true );

		return dstmesh;
		
	}
	
}

// ---------------------------------------------------

public class ColoredNormalMesthCreator : NormalMeshCreator
{
	
	public Color32[]	cols	{ protected set; get; }
	
	
	public override void alloc( ref MeshElementsCounter mec )
	{
		
		base.alloc( ref mec );
		
		cols = new Color32[ mec.vtxLength ];
		
	}
	
	protected override void addVertices( Mesh srcmesh, ref Matrix4x4 mt )
	{

		base.addVertices( srcmesh, ref mt );

		
		var iv = vtxProgress;

		var srccols	= srcmesh.colors32;
		
		for( var i = 0; i < srcmesh.vertexCount; i++, iv++ )
		{
			cols[ iv ]	= srccols.Length != 0 ? srccols[ i ] : new Color32( 0, 0, 0, 255 );//if( cols[ iv ].r > 4 ) Debug.Log( cols[ iv ] );
		}
		
	}
	
	public override Mesh create( EnMode mode = EnMode.normal )
	{
		
		var dstmesh = new Mesh();
		
		if( mode == EnMode.dynamic ) dstmesh.MarkDynamic();
		
		dstmesh.vertices	= vtxs;
		dstmesh.uv			= uvs;
		dstmesh.normals		= nms;
		dstmesh.colors32	= cols;
		
		dstmesh.triangles	= idxs;
		
		if( true || isNoNormal )
		{
			dstmesh.RecalculateNormals();
		}

		if( mode == EnMode.writeOnly ) dstmesh.UploadMeshData( true );

		return dstmesh;
		
	}
	
}

// ---------------------------------------------------

public class StructureBonedMeshCreator : NormalMeshCreator
{

	public Color32[] bids { protected set; get; }


	public override void alloc( ref MeshElementsCounter mec )
	{

		base.alloc( ref mec );

		bids = new Color32[ mec.vtxLength ];

	}


	public virtual void addGeometory( Mesh srcmesh, int id, ref Matrix4x4 mt, StructurePallet3 pallets )
	{

		addIndices( srcmesh, ref mt );

		addVertices( srcmesh, id, ref mt, pallets );


		idxProgress += srcmesh.triangles.Length;

		vtxProgress += srcmesh.vertexCount;

	}

	protected virtual void addVertices( Mesh srcmesh, int id, ref Matrix4x4 mt, StructurePallet3 pallets )
	{

		base.addVertices( srcmesh, ref mt );// NormalMeshCreator のもの


		var palletList = makePalletList( pallets );

		
		var iv = vtxProgress;

		var srccols	= srcmesh.colors32;
		
		for( var i = 0; i < srcmesh.vertexCount; i++, iv++ )
		{
			
			var c = srccols.Length != 0 ? srccols[ i ] : new Color32( 0, 0, 0, 0 );//255 );
			
			var palletId = c.r;//( c.r & c.g & c.b ) != 0 ? 0 : 4 - (( ( c.r & 1 ) * 3 ) | ( ( c.g & 1 ) * 2 ) | ( ( c.b & 1 ) * 1 ));
			
			if( palletId > 4 ) palletId = 0;


			var color = palletList[ palletId ];

			color.a = (byte)( id & 0xff );

			bids[ iv ] = color;
			
		}
		
	}

	Color32[] makePalletList( StructurePallet3 pallets )
	{

		var palletColors = new Color32[ 5 ];

		palletColors[ 0 ] = new Color32( 255, 255, 255, 255 );
		palletColors[ 1 ] = pallets.color0;
		palletColors[ 2 ] = pallets.color1;
		palletColors[ 3 ] = pallets.color2;
		palletColors[ 4 ] = pallets.color3;

		return palletColors;

	}


	public override Mesh create( EnMode mode = EnMode.normal )
	{

		var dstmesh = new Mesh();

		if( mode == EnMode.dynamic ) dstmesh.MarkDynamic();

		dstmesh.vertices = vtxs;
		dstmesh.uv = uvs;
		dstmesh.normals = nms;
		dstmesh.colors32 = bids;

		dstmesh.triangles = idxs;

		if( true || isNoNormal )
		{
			dstmesh.RecalculateNormals();
		}

		if( mode == EnMode.writeOnly ) dstmesh.UploadMeshData( true );

		return dstmesh;

	}

}

// ---------------------------------------------------

public class StructureMeshCreator : StructureBonedMeshCreator
{

	protected Vector2[]	flagOffsets;


	public override void alloc( ref MeshElementsCounter mec )
	{

		base.alloc( ref mec );

		flagOffsets = new Vector2[ mec.vtxLength ];

	}


	public virtual void addGeometory( Mesh srcmesh, int partId, int boneId, ref Matrix4x4 mt )
	{

		addIndices( srcmesh, ref mt );

		addVertices( srcmesh, partId, boneId, ref mt );


		idxProgress += srcmesh.triangles.Length;

		vtxProgress += srcmesh.vertexCount;

	}


	protected virtual void addVertices( Mesh srcmesh, int partId, int boneId, ref Matrix4x4 mt )
	{

		base.addVertices( srcmesh, ref mt );// NormalMeshCreator のもの


		var iv = vtxProgress;

		var srccols	= srcmesh.colors32;

		for( var i = 0 ; i < srcmesh.vertexCount ; i++, iv++ )
		{

			var c = srccols.Length != 0 ? srccols[ i ] : new Color32( 0, 0, 0, 0 );//255 );

			var palletId = c.r;//( c.r & c.g & c.b ) != 0 ? 0 : 4 - (( ( c.r & 1 ) * 3 ) | ( ( c.g & 1 ) * 2 ) | ( ( c.b & 1 ) * 1 ));

			if( palletId > 4 ) palletId = 0;


			var r = (byte)palletId;
			var a = (byte)( boneId & 0xff );// ボーン機能はオミットされたのでなくてもいい

			var b = (byte)( partId >> 4 >> 2 );							// top level
			var v = (float)( (partId >> 4) & 0x3 );                     // 4 simds
			var u = (float)( 1.0 / (double)( 1 << (partId & 0xf) ) );	// 1 / exp2(16 bit flags)（exp2 は 2 のナントカ乗のこと）

			bids[ iv ] = new Color32( r, 0, b, a );
			flagOffsets[ iv ] = new Vector2( u, v );
			//Debug.Log( bids[ iv ] );

		}

	}


	public override Mesh create( EnMode mode = EnMode.normal )
	{

		var dstmesh = new Mesh();

		if( mode == EnMode.dynamic ) dstmesh.MarkDynamic();

		dstmesh.vertices = vtxs;
		dstmesh.uv = uvs;
		dstmesh.uv2 = flagOffsets;
		dstmesh.normals = nms;
		dstmesh.colors32 = bids;

		dstmesh.triangles = idxs;

		if( true || isNoNormal )
		{
			dstmesh.RecalculateNormals();
		}

		if( mode == EnMode.writeOnly ) dstmesh.UploadMeshData( true );

		return dstmesh;

	}

}

// ---------------------------------------------------



