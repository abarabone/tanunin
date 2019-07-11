using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TexturePacker3SubPallet : ITexturePacker3
{


	Dictionary< Material, Material >	srcMaterials;

	List<Texture2D>	srcTextures;


	Texture2D	dstTexture;

	Material	dstMaterial;
	Material	dstMaterialAlpha;//

	Dictionary< string, MeshTextureNumberingUnit >	meshInfos;

	HashSet<GameObject>	registedObjects;


	int	spidPalletId = Shader.PropertyToID( "_PalletIdOverride" );








	public TexturePacker3SubPallet()
	{

		reset();

	}


	public void reset()
	{

		if( meshInfos == null )
		{

			srcMaterials = new Dictionary<Material, Material>( 64 );

			srcTextures = new List<Texture2D>( 64 );


			dstTexture = new Texture2D( 0, 0 );

			dstMaterial = new Material( Shader.Find( "Diffuse" ) );//Custom/Pallet Diffuse v3" ) );

			dstMaterial.mainTexture = dstTexture;


			meshInfos = new Dictionary<string, MeshTextureNumberingUnit>( 256 );

			registedObjects = new HashSet<GameObject>();

		}
		else
		{

			clear();

			reset();

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

		registedObjects.Clear();
		registedObjects = null;

	}






	public void regist( GameObject go )
	{

		if( registedObjects.Contains( go ) ) return;

		registedObjects.Add( go );


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

		mr.sharedMaterials = new Material[ 1 ];

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

					var texId = registTexture( srcTex );
					
					var palletId = srcMats[i].HasProperty( spidPalletId ) ? (int)srcMats[i].GetFloat( spidPalletId ) : -1;

					mi.registTexId( i, texId, palletId );

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

		// "meshId/texId@paretId:texId@paretId:..."

		var meshKey = srcMesh.GetInstanceID().ToString() + "/";

		for( var i = 0; i < srcMats.Length; i++ )
		{
			var texId = srcMats[i].mainTexture != null ? srcMats[i].mainTexture.GetInstanceID().ToString() : "";

			var palletId = srcMats[i].HasProperty( spidPalletId ) ? ((int)srcMats[i].GetFloat( spidPalletId )).ToString() : "";

			meshKey += texId + "@" + palletId + ":";
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


	// -------------

	public void packTextures( bool noLongerReadable = true, bool addToAsset = false )// addToAsset はまだ未実装
	{

		if( srcTextures.Count == 0 ) return;


		var texs = srcTextures.ToArray();

		//var rects = dstTexture.PackTextures( texs, 0, 4096, noLongerReadable );

		//var rects = dstTexture.PackTextures( texs, 0, 4096, false );
		//dstTexture.Apply( true, noLongerReadable );
		
		// みっぷマップ６以降の真っ黒（バグ？）回避
		var hasTransparent = false;
		foreach( var tex in texs ) if( tex.format == TextureFormat.DXT5 ){ hasTransparent = true; break; }
		var atlas = new Texture2D( 0, 0, TextureFormat.ARGB32, false );//
		var rects = atlas.PackTextures( texs, 0, 4096, false );//
		dstTexture.Resize( atlas.width, atlas.height, TextureFormat.ARGB32, true );//
		dstTexture.SetPixels32( atlas.GetPixels32( 0 ), 0 );//
		dstTexture.Apply( true, false );//
		dstTexture.Compress( !hasTransparent );// true );//
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

			dstMesh.colors32 = getColors( srcMesh, mi );


						
			dstMesh.triangles = srcMesh.triangles;

			
			dstMesh.bindposes = srcMesh.bindposes;
			
			dstMesh.boneWeights = srcMesh.boneWeights;

		}


		clear();

	}



	Vector2[] getUvs( Mesh srcMesh, MeshTextureNumberingUnit mi, Rect[] rects )
	{

		var uvs = new Vector2[ srcMesh.vertexCount ];
		
		for( var i = 0; i < mi.ids.Length; i++ )
		{

			if( srcMesh.subMeshCount > i )

			convertUvs( srcMesh.GetTriangles(i), rects[ mi.ids[i].texId ], srcMesh.uv, uvs );
			
		}

		return uvs;

	}

	void convertUvs( int[] srcIdxs, Rect uvOffset, Vector2[] srcUvs, Vector2[] dstUvs )
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

	}



	Color32[] getColors( Mesh srcMesh, MeshTextureNumberingUnit mi )
	{

		var colors = srcMesh.colors32;//Debug.Log(colors.Length);
		
		if( mi.hasOverridedPalletId )
		{
			if( colors.Length == 0 ) colors = new Color32[ srcMesh.vertexCount ];
			
			for( var i = 0; i < mi.ids.Length; i++ )
			{
				overridePalletId( srcMesh.GetTriangles(i), mi.ids[i].palletId, colors );
			}
		}

		return colors;
	}

	void overridePalletId( int[] srcIdxs, int palletId, Color32[] dstColors )
	{
		
		for( var i = 0; i < srcIdxs.Length; i++ )
		{
			dstColors[ srcIdxs[i] ] = new Color32( (byte)palletId, 0, 0, 1 );//Debug.Log(dstColors[ srcIdxs[i] ]);
		}
		
	}




	// ======================================

	class MeshTextureNumberingUnit
	{

		public Mesh	srcMesh;

		public Mesh	dscMesh;

		public SubInfo[]	ids;

		public bool	hasOverridedPalletId;


		public struct SubInfo
		{
			public int	texId;
			public int	palletId;

			public SubInfo( int tex, int pal ) { texId = tex; palletId = pal; }
		}


		public MeshTextureNumberingUnit( Mesh sm, Mesh dm, Material[] mats )
		{

			srcMesh = sm;

			dscMesh = dm;

			ids = new SubInfo[ mats.Length ];

		}

		public void registTexId( int matId, int texId, int palId )
		{

			ids[ matId ] = new SubInfo( texId, palId );

			if( palId != -1 ) hasOverridedPalletId = true;//Debug.Log(hasOverridedPalletId);

		}

	}

}
