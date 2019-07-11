using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class TexturePackerSub : ITexturePacker
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
			.SelectMany(
				mr => mr.sharedMaterials.Select( (mat, subId) => new { mat, subId } ),
				(mr, x) => new { mr, x.mat, x.subId }
			)
			.ToLookup( x => (Texture2D)x.mat.mainTexture, x => new { x.mr, x.subId } );

		var texs = textureLookup.Select( x => x.Key ).ToArray();

		// みっぷマップ６以降の真っ黒（バグ？）回避
		var atlas = new Texture2D( 0, 0, TextureFormat.ARGB32, false );
		var rects = atlas.PackTextures( texs, 0, 4096, false );
		dstTexture.Resize( atlas.width, atlas.height, TextureFormat.ARGB32, true );
		dstTexture.SetPixels32( atlas.GetPixels32( 0 ), 0 );
		dstTexture.Apply( true, false );
		dstTexture.Compress( true );
		dstTexture.Apply( false, noLongerReadable );


		var dstMaterials = new Material[] { new Material( textureLookup.First().First().mr.material.shader ) };

		dstMaterials.First().mainTexture = dstTexture;


		var q = textureLookup
			.Select( (gr, texId) => new { gr, texId } )
			.SelectMany( x => x.gr, (x, mrsub) => new { mrsub.mr, x.texId, mrsub.subId } )
			.GroupBy(
				x => x.mr,
				x => new { x.texId, x.subId },
				(mr, ids) => new {
					mr,
					texIds = string.Join( ":", ids.OrderBy( id => id.subId ).Select( id => id.texId.ToString() ).ToArray() )
				}
			)
			.GroupBy( x => new {
				x.texIds,
				mesh = x.mr is SkinnedMeshRenderer ? ((SkinnedMeshRenderer)x.mr).sharedMesh : x.mr.GetComponent<MeshFilter>().sharedMesh
			}, x => x.mr );

		foreach( var meshGroup in q )
		{
			//Debug.Log( string.Format( "{0} : {1}", meshGroup.Key.mesh, meshGroup.Key.texIds ) );
			
			var srcMesh = meshGroup.Key.mesh;
			
			var dstMesh = new Mesh();


			dstMesh.vertices = srcMesh.vertices;
			
			dstMesh.normals = srcMesh.normals;

			dstMesh.tangents = srcMesh.tangents;

			dstMesh.uv = convertUvs( srcMesh.uv, meshGroup.Key.texIds, srcMesh, rects );

			dstMesh.uv2 = srcMesh.uv2;

			dstMesh.colors32 = srcMesh.colors32;


			dstMesh.triangles = srcMesh.triangles;


			dstMesh.bindposes = srcMesh.bindposes;

			dstMesh.boneWeights = srcMesh.boneWeights;



			foreach( var mr in meshGroup )
			{
				//Debug.Log( mr );	
				mr.sharedMaterials = dstMaterials;


				if( mr is SkinnedMeshRenderer )
				{
					( (SkinnedMeshRenderer)mr ).sharedMesh = dstMesh;
				}
				else
				{
					mr.GetComponent<MeshFilter>().sharedMesh = dstMesh;
				}

			}

		}


		clear();
		
	}


	Vector2[] convertUvs( Vector2[] dstuvs, string texIdsString, Mesh srcMesh, Rect[] rects )
	{

		var subIdxInfos = texIdsString.Split( ':' )
			.Select( ( x, id ) => new { id, texId = int.Parse( x ) } );

		foreach( var isub in subIdxInfos )
		{

			var idxs = srcMesh.GetTriangles( isub.id );

			var rect = rects[ isub.texId ];
			var offset = new Vector2( rect.x, rect.y );
			var wide = new Vector2( rect.width, rect.height );
			//Debug.Log( string.Format("subId/texId {0}/{1} : {2}, {3}", isub.id, isub.texId, offset, wide) );

			foreach( var idx in idxs.Distinct() )
			{

				//Debug.Log( string.Format( "uv {0} : {1}, {2}", idx, dstuvs[idx], offset + Vector2.Scale( dstuvs[ idx ], wide ) ) );
				dstuvs[ idx ] = offset + Vector2.Scale( dstuvs[ idx ], wide );

			}

		}


		return dstuvs;

	}


}
