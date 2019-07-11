using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public interface ITexturePackable
{

	void registTo( ITexturePacker3 tpacker );

}

public interface ITexturePacker3
{

	void reset();

	void clear();


	void regist( GameObject go );

	void packTextures( bool noLongerReadable = true, bool addToAsset = false );

}


public class TexturePacker3 : ITexturePacker3
{

	Dictionary<Material,Material>	srcMaterials;

	List<Texture2D>	srcTextures;


	Texture2D	dstTexture;

	Dictionary<long,MeshTextureNumberingUnit>	meshInfos;


	struct MeshTextureNumberingUnit
	{

		public Mesh	srcMesh;

		public Mesh	dscMesh;

		public int	id;


		public MeshTextureNumberingUnit( Mesh sm, Mesh dm, int i )
		{
			srcMesh = sm;

			dscMesh = dm;

			id = i;
		}

	}


	public TexturePacker3()
	{

		reset();

	}


	public void reset()
	{
		if( meshInfos == null )
		{

			srcMaterials = new Dictionary<Material,Material>( 64 );
			
			srcTextures = new List<Texture2D>( 64 );
			
			
			dstTexture = new Texture2D( 0, 0 );
			
			meshInfos	= new Dictionary<long,MeshTextureNumberingUnit>( 256 );
		
		}
	}

	public void clear()
	{

		srcMaterials.Clear();
		srcMaterials = null;

		srcTextures.Clear();
		srcTextures = null;

		dstTexture = null;

		meshInfos.Clear();
		meshInfos = null;

	}




	public void regist( GameObject go )
	{

		var rs = go.GetComponentsInChildren<Renderer>();

		foreach( var r in rs )
		{

			if( r is MeshRenderer )
			{

				registSolidMesh( (MeshRenderer)r );

			}
			else if( r is SkinnedMeshRenderer )
			{

				registSkinnedMesh( (SkinnedMeshRenderer)r );

			}

		}

	}

	void registSolidMesh( MeshRenderer mr )
	{

		var srcMat = mr.sharedMaterial;

		var srcTex = (Texture2D)srcMat.mainTexture;

		if( srcTex != null )
		{

			mr.sharedMaterial = registMaterial( srcMat );


			var id = registTexture( srcTex );


			var mf = mr.GetComponent<MeshFilter>();
			
			var srcMesh = mf.sharedMesh;

			mf.sharedMesh = registMesh( srcMesh, srcTex, id );

		}

	}

	void registSkinnedMesh( SkinnedMeshRenderer mr )
	{
		
		var srcMat = mr.sharedMaterial;
		
		var srcTex = (Texture2D)srcMat.mainTexture;
		
		if( srcTex != null )
		{
			
			mr.sharedMaterial = registMaterial( srcMat );
			
			
			var id = registTexture( srcTex );
			

			var srcMesh = mr.sharedMesh;
			
			mr.sharedMesh = registMesh( srcMesh, srcTex, id );
			
		}
		
	}

	Material registMaterial( Material srcMat )
	{

		Material	dstMat;


		if( srcMaterials.ContainsKey(srcMat) )
		{
			
			dstMat = srcMaterials[ srcMat ];
			
		}
		else
		{
			
			dstMat = new Material( srcMat );
			
			dstMat.mainTexture = dstTexture;

			
			srcMaterials[ srcMat ] = dstMat;
			
		}


		return dstMat;

	}

	int registTexture( Texture2D srcTex )
	{

		var id = srcTextures.IndexOf( srcTex );
		
		if( id == -1 )
		{
			
			id = srcTextures.Count;
			
			srcTextures.Add( srcTex );
			
		}

		return id;

	}

	Mesh registMesh( Mesh srcMesh, Texture2D srcTex, int id )
	{

		Mesh dstMesh;


		var meshKey = ((long)srcMesh.GetInstanceID() << 32) + (long)srcTex.GetInstanceID();//((long)srcMesh.GetInstanceID() << 32) | (long)srcTex.GetInstanceID();
		
		if( meshInfos.ContainsKey(meshKey) )
		{
			
			dstMesh = meshInfos[ meshKey ].dscMesh;
			
		}
		else
		{
			
			dstMesh = new Mesh();
			
			var mi = new MeshTextureNumberingUnit( srcMesh, dstMesh, id );
			
			meshInfos[ meshKey ] = mi;

		}

		return dstMesh;

	}



	public void packTextures( bool noLongerReadable = true, bool addToAsset = false )
	{

		var texs = srcTextures.ToArray();

	//	var recs = dstTexture.PackTextures( texs, 0, 4096, false );
		
		// みっぷマップ６以降の真っ黒（バグ？）回避
		var atlas = new Texture2D( 0, 0, TextureFormat.ARGB32, false );
		var recs = atlas.PackTextures( texs, 0, 4096, false );
		dstTexture.Resize( atlas.width, atlas.height, TextureFormat.ARGB32, true );
		dstTexture.SetPixels32( atlas.GetPixels32( 0 ), 0 );
		dstTexture.Apply( true, false );
		dstTexture.Compress( true );
		dstTexture.Apply( false, noLongerReadable );


		foreach( var mi in meshInfos.Values )
		{

			var srcMesh = mi.srcMesh;
			
			var dstMesh = mi.dscMesh;


			dstMesh.vertices = srcMesh.vertices;
			
			dstMesh.normals = srcMesh.normals;
			
			dstMesh.tangents = srcMesh.tangents;

			dstMesh.uv = convertUvs( recs[mi.id], srcMesh.uv, new Vector2[ srcMesh.vertexCount ] );

			dstMesh.uv2 = srcMesh.uv2;

			dstMesh.colors32 = srcMesh.colors32;


			dstMesh.triangles = srcMesh.triangles;


			dstMesh.bindposes = srcMesh.bindposes;
			
			dstMesh.boneWeights = srcMesh.boneWeights;

		}


		clear();

	}

	Vector2[] convertUvs( Rect uvOffset, Vector2[] srcUvs, Vector2[] dstUvs )
	{

		for( var i = 0; i < srcUvs.Length; i++ )
		{

			var srcUv = srcUvs[i];

			var u = uvOffset.x + srcUv.x * uvOffset.width;

			var v = uvOffset.y + srcUv.y * uvOffset.height;

			dstUvs[i] = new Vector2( u, v );

		}

		return dstUvs;

	}


}
