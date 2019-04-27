using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public interface ITexturePacker
{

	void clear();
	

	void regist( GameObject go );

	void packTextures( bool noLongerReadable = true, bool addToAsset = false );

}

public class TexturePacker : ITexturePacker
{

	HashSet<GameObject>	gameObjects = new HashSet<GameObject>();
	//List<GameObject>	gameObjects = new List<GameObject>();//( 256 );


	public void clear()
	{
		gameObjects.Clear();
	}
	
	public void regist( GameObject go )
	{
		if( !gameObjects.Contains( go ) ) gameObjects.Add( go );
	}

	public void packTextures( bool noLongerReadable = true, bool addToAsset = false )
	{

		var dstTexture = new Texture2D( 0, 0 );

		var textureLookup = gameObjects
			.SelectMany( go => go.GetComponentsInChildren<Renderer>() )
			.Distinct()// 要らないのが理想
			.ToLookup( mr => (Texture2D)mr.sharedMaterial.mainTexture );

		var texs = textureLookup.Select( g => g.Key ).ToArray();

		// みっぷマップ６以降の真っ黒（バグ？）回避
		var atlas = new Texture2D( 0, 0, TextureFormat.ARGB32, false );
		var recs = atlas.PackTextures( texs, 0, 4096, false );
		dstTexture.Resize( atlas.width, atlas.height, TextureFormat.ARGB32, true );
		dstTexture.SetPixels32( atlas.GetPixels32( 0 ), 0 );
		dstTexture.Apply( true, false );
		dstTexture.Compress( true );
		dstTexture.Apply( false, noLongerReadable );


		var dstMaterial = new Material( textureLookup.First().First().sharedMaterial.shader );

		dstMaterial.mainTexture = dstTexture;

		var q = textureLookup
			.Select( (gr, itex) => new { itex, gr } )
			.SelectMany( x => x.gr, (x, mr) => new { x.itex, mr } )
			.GroupBy( x => new { x.itex, mesh = x.mr is SkinnedMeshRenderer ? ((SkinnedMeshRenderer)x.mr).sharedMesh : x.mr.GetComponent<MeshFilter>().sharedMesh } );

		foreach( var meshInfoGroup in q )
		{

			var key = meshInfoGroup.Key;

			var srcMesh = key.mesh;
			
			var dstMesh = new Mesh();


			dstMesh.vertices = srcMesh.vertices;
			
			dstMesh.normals = srcMesh.normals;
			
			dstMesh.tangents = srcMesh.tangents;

			dstMesh.uv = convertUvs( recs[key.itex], srcMesh.uv );

			dstMesh.uv2 = srcMesh.uv2;

			dstMesh.colors32 = srcMesh.colors32;


			dstMesh.triangles = srcMesh.triangles;


			dstMesh.bindposes = srcMesh.bindposes;
			
			dstMesh.boneWeights = srcMesh.boneWeights;


			foreach( var x in meshInfoGroup )
			{

				x.mr.sharedMaterial = dstMaterial;


				if( x.mr is SkinnedMeshRenderer )
				{
					( (SkinnedMeshRenderer)x.mr ).sharedMesh = dstMesh;
				}
				else
				{
					x.mr.GetComponent<MeshFilter>().sharedMesh = dstMesh;
				}

			}

		}


		clear();
		
	}

	Vector2[] convertUvs( Rect uvOffset, Vector2[] dstuvs )
	{
		
		var offset	= new Vector2( uvOffset.x, uvOffset.y );

		var wide	= new Vector2( uvOffset.width, uvOffset.height );

		foreach( var uv in dstuvs.Select( (x, i) => new { srcvalue = x, i } ) )
		{

			dstuvs[ uv.i ] = offset + Vector2.Scale( uv.srcvalue, wide );

		}

		return dstuvs;

	}
	

}
