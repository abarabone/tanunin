using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TexturePacker3Sub : ITexturePacker3
{

	Dictionary< Material, Material >	srcMaterials;

	List<Texture2D>	srcTextures;


	Texture2D	dstTexture;

	Material	dstMaterial;
	Material	dstMaterialAlpha;//

	Dictionary< string, MeshTextureNumberingUnit >	meshInfos;


	class MeshTextureNumberingUnit
	{

		public Mesh	srcMesh;

		public Mesh	dscMesh;

		public int[]	ids;


		public MeshTextureNumberingUnit( Mesh sm, Mesh dm, Material[] mats )
		{

			srcMesh = sm;

			dscMesh = dm;

			ids = new int[ mats.Length ];

		}

		public void registTexId( int matId, int id )
		{

			ids[ matId ] = id;

		}

	}


	public TexturePacker3Sub()
	{

		reset();

	}


	public void reset()
	{
		if( meshInfos == null )
		{

			srcMaterials = new Dictionary< Material, Material >( 64 );
			
			srcTextures = new List<Texture2D>( 64 );
			
			
			dstTexture = new Texture2D( 0, 0 );

			dstMaterial = new Material( Shader.Find( "Diffuse" ) );
			
			dstMaterial.mainTexture = dstTexture;


			meshInfos	= new Dictionary< string, MeshTextureNumberingUnit >( 256 );
		
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

				//registSkinnedMesh( (SkinnedMeshRenderer)r );

			}

		}

	}

	void registSolidMesh( MeshRenderer mr )
	{

		var srcMats = mr.sharedMaterials;

		mr.sharedMaterial = dstMaterial;//registMaterial( srcMat );


		var mf = mr.GetComponent<MeshFilter>();
		
		var srcMesh = mf.sharedMesh;

		var mi = registMesh( mf, srcMesh, srcMats );


		if( mi != null )
		{
			for( var i = 0; i < srcMats.Length; i++ )
			{

				var srcTex = (Texture2D)srcMats[ i ].mainTexture;

				if( srcTex != null )
				{

					var id = registTexture( srcTex );

					mi.registTexId( i, id );

				}

			}
		}

	}
	/*
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
*/
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

	MeshTextureNumberingUnit registMesh( MeshFilter mf, Mesh srcMesh, Material[] srcMats )
	{

		var meshKey = srcMesh.GetInstanceID().ToString() + ":";

		for( var i = 0; i < srcMats.Length; i++ )
		{
			meshKey += ( srcMats[i].mainTexture != null ? srcMats[i].mainTexture.GetInstanceID().ToString() : "" ) + ":";
		}


		if( meshInfos.ContainsKey(meshKey) )
		{
			
			mf.sharedMesh = meshInfos[ meshKey ].dscMesh;


			return null;

		}
		else
		{
			
			var dstMesh = new Mesh();
			
			var mi = new MeshTextureNumberingUnit( srcMesh, dstMesh, srcMats );
			
			meshInfos[ meshKey ] = mi;

			mf.sharedMesh = dstMesh;


			return mi;

		}

	}



	public void packTextures( bool noLongerReadable = true, bool addToAsset = false )// addToAsset はまだ未実装
	{

		var texs = srcTextures.ToArray();

		//var recs = dstTexture.PackTextures( texs, 0, 4096, false );
		
		// みっぷマップ６以降の真っ黒（バグ？）回避
		var atlas = new Texture2D( 0, 0, TextureFormat.ARGB32, false );//
		var rects = atlas.PackTextures( texs, 0, 4096, false );//
		dstTexture.Resize( atlas.width, atlas.height, TextureFormat.ARGB32, true );//
		dstTexture.SetPixels32( atlas.GetPixels32( 0 ), 0 );//
		dstTexture.Apply( true, false );//
		dstTexture.Compress( true );
		dstTexture.Apply( false, noLongerReadable );


		foreach( var mi in meshInfos.Values )
		{

			var srcMesh = mi.srcMesh;
			
			var dstMesh = mi.dscMesh;


			dstMesh.vertices = srcMesh.vertices;
			
			dstMesh.normals = srcMesh.normals;
			
			dstMesh.tangents = srcMesh.tangents;


			
			dstMesh.uv = getUvs( srcMesh, mi, rects );

			dstMesh.uv2 = srcMesh.uv2;//

			dstMesh.colors32 = srcMesh.colors32;

						
			dstMesh.triangles = srcMesh.triangles;

			
			dstMesh.bindposes = srcMesh.bindposes;
			
			dstMesh.boneWeights = srcMesh.boneWeights;

		}


		clear();

	}

	Vector2[] getUvs( Mesh srcMesh, MeshTextureNumberingUnit mi, Rect[] rects )
	{
		
		var uvs = new Vector2[ srcMesh.vertexCount ];Debug.Log(mi.ids.Length);
		
		for( var i = 0; i < mi.ids.Length; i++ )
		{
			
			convertUvs( srcMesh.GetTriangles(i), rects[ mi.ids[i] ], srcMesh.uv, uvs );
			
		}
		
		return uvs;
		
	}

	Vector2[] convertUvs( int[] srcIdxs, Rect uvOffset, Vector2[] srcUvs, Vector2[] dstUvs )
	{

		for( var i = 0; i < srcIdxs.Length; i++ )
		{

			var srcUv = srcUvs[ srcIdxs[i] ];

			if( !float.IsPositiveInfinity(srcUv.x) )
			{

				var u = uvOffset.x + srcUv.x * uvOffset.width;

				var v = uvOffset.y + srcUv.y * uvOffset.height;

				dstUvs[ srcIdxs[i] ] = new Vector2( u, v );

				srcUvs[ srcIdxs[i] ] = new Vector2( float.PositiveInfinity, float.PositiveInfinity );

			}

		}

		return dstUvs;

	}


}
