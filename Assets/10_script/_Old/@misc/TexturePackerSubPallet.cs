using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



/// <summary>
/// 登録したゲームオブジェクト以下の全レンダラの持つテクスチャをパックする。
/// メッシュのＵＶも修正するが、その時テクスチャとパレット０～３を共有するメッシュごとに新規メッシュを作成する。
/// </summary>

public class TexturePackerSubPallet : ITexturePacker
{

	HashSet<GameObject>	gameObjects = new HashSet<GameObject>();
	//List<GameObject>	gameObjects = new List<GameObject>();//( 256 );

	
	readonly int propIdForPallet = Shader.PropertyToID( "_PalletIdOverride" );


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

		//var rects = dstTexture.PackTextures( texs, 0, 4096, noLongerReadable );
		
		
		// みっぷマップ６以降の真っ黒（バグ？）回避
		var atlas = new Texture2D( 0, 0, TextureFormat.ARGB32, false );
		var rects = atlas.PackTextures( texs, 0, 4096, false );
		dstTexture.Resize( atlas.width, atlas.height, TextureFormat.ARGB32, true );
		dstTexture.SetPixels32( atlas.GetPixels32( 0 ), 0 );
		dstTexture.Apply( true, false );
		dstTexture.Compress( true );
		dstTexture.Apply( false, noLongerReadable );
		


		//var dstMaterials = new Material[] { new Material( textureLookup.First().First().mr.material.shader ) };
		var dstMaterials = new Material[] { new Material( Shader.Find( "Diffuse" ) ) };

		dstMaterials.First().mainTexture = dstTexture;



		var q = textureLookup
			.Select( (gr, texId) => new { gr, texId } )
			.SelectMany( x => x.gr,
				(x, mrsub) => {

					var mat = mrsub.mr.sharedMaterials[ mrsub.subId ];

					return new
					{
						mrsub.mr,
						x.texId,
						mrsub.subId,
						palletId = mat.HasProperty( propIdForPallet ) ? (int)mat.GetFloat( propIdForPallet ) : -1
					};

				}
			)
			.GroupBy(
				x => x.mr,
				x => new { x.texId, x.subId, x.palletId },
				(mr, ids) => new {
					mr,
					texIds = string.Join( ":",
						ids.OrderBy( id => id.subId )
						.Select( id => id.texId +"@"+ id.palletId )
						.ToArray()
					)// t0@p0:t1@p1: ... :tn@pn わかりづらいけど
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


			var qsub = meshGroup.Key.texIds.Split( ':' )
				.Select( ( x, id ) => {

					var idset = x.Split('@');

					return new SubMeshInfo
					{
						idxs		= srcMesh.GetIndices( id ).Distinct(),
						texId		= int.Parse( idset[0] ),
						palletId	= int.Parse( idset[1] )
					};

				} ).ToArray();
			


			dstMesh.vertices = srcMesh.vertices;
			
			dstMesh.normals = srcMesh.normals;

			dstMesh.tangents = srcMesh.tangents;
			
			dstMesh.uv = convertUvs( srcMesh.uv, qsub, rects );

			dstMesh.uv2 = srcMesh.uv2;
			
			dstMesh.colors32 = convertColors( qsub, srcMesh.vertexCount );


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


	struct SubMeshInfo
	{
		public IEnumerable<int>	idxs;
		public int	texId;
		public int	palletId;
	}


	Vector2[] convertUvs( Vector2[] dstuvs, IEnumerable<SubMeshInfo> qsub, Rect[] rects )
	{

		var quv = qsub
			.Select( x =>
			{
				var rect = rects[ x.texId ];
				var offset = new Vector2( rect.x, rect.y );
				var wide = new Vector2( rect.width, rect.height );

				return new { offset, wide, x.idxs };
			} )
			.SelectMany( x => x.idxs, ( x, idx ) => new {
				idx,
				newuv = x.offset + Vector2.Scale( dstuvs[ idx ], x.wide )
			} );

		foreach( var iuv in quv )
		{
			dstuvs[ iuv.idx ] = iuv.newuv;
		}


		return dstuvs;

	}

	Color32[] convertColors( IEnumerable<SubMeshInfo> qsub, int vertexLength )
	{

		var qcolor = qsub
			.SelectMany( x => x.idxs, ( x, idx ) => new {
				idx,
				color = new Color32( (byte)x.palletId, 0, 0, 0 )
			} );


		var dstColors = new Color32[ vertexLength ];

		foreach( var icolor in qcolor )
		{
			dstColors[ icolor.idx ] = icolor.color;
		}


		return dstColors;

	}


}
