using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;

namespace ModelGeometry
{
	static public class StructureNearObjectBuilder
	{

		static public GameObject BuildNearObject( this _StructurePartBase[] parts )
		{

			return null;
		}

		
		static GameObject BuildMeshAndGameObject( this _StructurePartBase[] parts )
		{
			

			return null;
		}


	}
	
	/// <summary>
	/// 並列に作成することを考えて、各要素を Mesh と独立させる。
	/// </summary>
	public struct MeshElements
	{
		public List<Vector3>	Vertecies;
		public List<Vector3>	Normals;
		public List<Vector2>	Uvs;
		public List<int>		Indecies;
		public List<Vector3>	Tangents;
		public List<Color32>	Colors;

		public Mesh CreateUnlitMesh()
		{
			var mesh = new Mesh();
			if( this.Vertecies != null ) mesh.SetVertices( this.Vertecies );
			if( this.Normals != null ) mesh.SetNormals( this.Normals );
			if( this.Uvs != null ) mesh.SetUVs( 0, this.Uvs );
			if( this.Colors != null ) mesh.SetColors( this.Colors );
			if( this.Indecies != null ) mesh.SetTriangles( this.Indecies, submesh:0, calculateBounds:false );

			return mesh;
		}
	}



	public static class MeshCombiner
	{

		static public Task<MeshElements> BuildUnlitMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var qMesh = QueryUtility.QueryMeshEveryObjects( parts );
			var mtParts = ( from pt in parts select pt.transform.localToWorldMatrix ).ToList();

			var vtxss = ( from mesh in qMesh select mesh.vertices ).ToList();
			var uvss = ( from mesh in qMesh select mesh.uv ).ToList();
			var idxss = ( from mesh in qMesh select mesh.triangles ).ToList();

			var mtBaseInv = tfBase.worldToLocalMatrix;

			return Task.Run(
				() => new MeshElements
				{
					Vertecies = ConvertUtility.BuildVerteces( vtxss, mtParts, mtBaseInv ),
					Uvs = uvss.SelectMany( uvs => uvs ).ToList(),
					Indecies = ConvertUtility.BuildIndeices( vtxss, idxss, mtParts ),
				}
			);
		}

		static public Task<MeshElements> BuildNormalMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var qMesh = QueryUtility.QueryMeshEveryObjects( parts );
			var mtParts = ( from pt in parts select pt.transform.localToWorldMatrix ).ToList();

			var vtxss = ( from mesh in qMesh select mesh.vertices ).ToList();
			var nmss = QueryUtility.QueryNormalsEveryMesh( qMesh ).ToList();
			var uvss = ( from mesh in qMesh select mesh.uv ).ToList();
			var idxss = ( from mesh in qMesh select mesh.triangles ).ToList();

			var mtBaseInv = tfBase.worldToLocalMatrix;

			return Task.Run(
				() => new MeshElements
				{
					Vertecies = ConvertUtility.BuildVerteces( vtxss, mtParts, mtBaseInv ),
					Normals = ConvertUtility.BuildNormals( nmss, mtParts, mtBaseInv ),
					Uvs = uvss.SelectMany( uvs => uvs ).ToList(),
					Indecies = ConvertUtility.BuildIndeices( vtxss, idxss, mtParts ),
				}
			);
		}
		
		static public Task<MeshElements>
			BuildStructureWithPalletMeshElements( IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{
			var qMesh = QueryUtility.QueryMeshEveryObjects( parts );
			var mtParts = ( from pt in parts select pt.transform.localToWorldMatrix ).ToList();

			var vtxss = ( from mesh in qMesh select mesh.vertices ).ToList();
			var nmss = QueryUtility.QueryNormalsEveryMesh( qMesh ).ToList();
			var uvss = ( from mesh in qMesh select mesh.uv ).ToList();
			var idxss = ( from mesh in qMesh select mesh.triangles ).ToList();
			

			// パーツＩＤとパレットＩＤをそれぞれ生成し、Color32 にパックする。
			
			// 全パーツから、パーツＩＤをすべてクエリする。
			var partIds = ( from pt in parts select pt.partId ).ToList();

			var qMathashArrayEveryParts = QueryUtility.QueryMathashArraysEveryParts( parts );
			var mathashToIndexDict = ConvertUtility.ToDictionaryForMaterialHashToIndex( qMathashArrayEveryParts );

			// 頂点ごとのパレットＩＤをすべてクエリする。
			var palletsEveryVertices =
				QueryUtility.QueryePalletEveryVertices( qMesh, qMathashArrayEveryParts, mathashToIndexDict )
					.ToList();
			
			var mtBaseInv = tfBase.worldToLocalMatrix;

			return Task.Run(
				() => 
				{
					// 頂点ごとのパーツＩＤをすべてクエリする。
					var qPidPerVertex = QueryUtility.QueryStructureIndexEveryVertices( vtxss, partIds );

					return new MeshElements
					{
						Vertecies = ConvertUtility.BuildVerteces( vtxss, mtParts, mtBaseInv ),
						Normals = ConvertUtility.BuildNormals( nmss, mtParts, mtBaseInv ),
						Uvs = uvss.SelectMany( uvs => uvs ).ToList(),
						Indecies = ConvertUtility.BuildIndeices( vtxss, idxss, mtParts ),
						Colors = ConvertUtility.ToColor32List( qPidPerVertex, palletsEveryVertices ),
					};
				}
			);
		}

		

		/// <summary>
		/// メッシュ要素（頂点、ＵＶ、インデックスなど別々）ごとのコンバートを行い、list<> につめて返す。
		/// 関数の引数はすべて Unity オブジェクトではないため、このクラスの全関数はサブスレッドで処理できる。
		/// </summary>
		static class ConvertUtility
		{
			/// <summary>
			/// 
			/// </summary>
			public static List<Vector3> BuildVerteces
				( IEnumerable<IEnumerable<Vector3>> verticesPerMeshes, IEnumerable<Matrix4x4> mtParts, Matrix4x4 mtBaseInv )
			{
				var qVertex =
					from xy in Enumerable.Zip( verticesPerMeshes, mtParts, (x,y)=>(vtxs:x, mt:y) )
					let mt = xy.mt * mtBaseInv
					from vtx in xy.vtxs
					select mt.MultiplyPoint3x4( vtx )
					;
				
				return qVertex.ToList();
			}

			/// <summary>
			/// 
			/// </summary>
			public static List<Vector3> BuildNormals
				( IEnumerable<IEnumerable<Vector3>> normalsPerMeshes, IEnumerable<Matrix4x4> mtParts, Matrix4x4 mtBaseInv )
			{
				var qNormal =
					from xy in Enumerable.Zip( normalsPerMeshes, mtParts, (x,y)=>(nms:x, mt:y) )
					let mt = xy.mt * mtBaseInv
					from nm in xy.nms
					select mt.MultiplyVector( nm )
					;
			
				return qNormal.ToList();

				Vector3[] recalculateNormals_( Mesh mesh_ )
				{
					mesh_.RecalculateNormals();
					return mesh_.normals;
				}
			}
			
			/// <summary>
			/// マテリアルを GameObject 階層からすべて取得、重複削除してから index を採番する。
			/// </summary>
			public static Dictionary<int,int>
				ToDictionaryForMaterialHashToIndex( IEnumerable<IEnumerable<int>> matHashArrayEveryParts )
			{
				return matHashArrayEveryParts
					.SelectMany( mathash => mathash )
					.Distinct()
					.Select( (mathash,i) => (mathash,i) )
					.ToDictionary( x => x.mathash, x => x.i )
					;
			}
			
			/// <summary>
			/// 
			/// </summary>
			public static List<Color32> ToColor32List(
				IEnumerable<(int int4Index, int memberIndex, int bitIndex)> partsIdsEveryVertices,
				IEnumerable<int> palletsEveryVertices
			)
			{
				var qPidPalletPerVertex =
					from xy in Enumerable.Zip( partsIdsEveryVertices, palletsEveryVertices, (x,y)=>(pid:x, pallet:y) )
					select new Color32
					(
						r:	(byte)xy.pid.int4Index, 
						g:	(byte)xy.pid.memberIndex, 
						b:	(byte)xy.pid.bitIndex, 
						a:	(byte)xy.pallet
					);

				return qPidPalletPerVertex.ToList();
			}
			
			/// <summary>
			/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
			/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
			/// また、マテリアル別のサブメッシュも、ひとつに統合される。
			/// </summary>
			public static List<int> BuildIndeices
				( IEnumerable<IEnumerable<Vector3>> verticesPerMeshes, IEnumerable<IEnumerable<int>> indicesPerMeshes, IEnumerable<Matrix4x4> mtParts )
			{
				// 各メッシュに対する、頂点インデックスのベースオフセットをクエリする。
				var qVtxCount = verticesPerMeshes
					.Select( vtxs => vtxs.Count() )
					.Scan( seed:0, (pre,cur) => pre + cur )
					;
				var qBaseVertex = Enumerable.Range(0,1).Concat( qVtxCount );// 先頭に 0 を追加する。

				var qIndex =
					from xyz in indicesPerMeshes
						.Zip( mtParts, (x,y)=>(idxs:x, mt:y) )
						.Zip( qBaseVertex, (xy,z)=>(xy.idxs, xy.mt, baseVtx:z) )
					from index in isReverseScale_( xyz.mt )// mesh.triangles は、サブメッシュを地続きに扱う。
						? reverseEvery3_(xyz.idxs) 
						: xyz.idxs
					select xyz.baseVtx + index;

				return qIndex.ToList();


				bool isReverseScale_( Matrix4x4 mt )
				{
					var up = Vector3.Cross( mt.GetRow( 0 ), mt.GetRow( 2 ) );
					return Vector3.Dot( up, mt.GetRow( 1 ) ) > 0.0f;
					//var scl = tf.lossyScale;
					//return scl.x * scl.y * scl.z < 0.0f;
				}

				IEnumerable<int> reverseEvery3_( IEnumerable<int> indecies_ )
				{
					using( var e = indecies_.GetEnumerator() )
					{
						while( e.MoveNext() )
						{
							var i0 = e.Current; e.MoveNext();
							var i1 = e.Current; e.MoveNext();
							var i2 = e.Current;
							yield return i0;
							yield return i2;
							yield return i1;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// パーツやメッシュ等から、要素の集合を抽出するクエリを生成する。
		/// 引数が Unity オブジェクトでないものは、サブスレッドでも処理できる。
		/// </summary>
		static class QueryUtility
		{
			
			/// <summary>
			/// 
			/// </summary>
			public static IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
				QueryStructureIndexEveryVertices
				( IEnumerable<IEnumerable<Vector3>> verticesEveryMeshes, IEnumerable<int> partIds )
			{
				var qIndex =
					from xy in Enumerable.Zip( verticesEveryMeshes, partIds, (x,y)=>(vtxs:x, pid:y) )
					from vtx in xy.vtxs
					select (
						int4Index:		xy.pid >> 5 >>2,
						memberIndex:	xy.pid >> 5 & 0b_11,	// 0 ~ 3
						bitIndex:		xy.pid & 0b_1_1111		// 0 ~ 31
					);

				return qIndex;
			}
			
			/// <summary>
			/// 全パーツから、それぞれのマテリアルハッシュ配列をすべてクエリする。
			/// </summary>
			public static IEnumerable<IEnumerable<int>>
				QueryMathashArraysEveryParts( IEnumerable<_StructurePartBase> parts )
			{
				var qMathashArrayEveryParts =
					from pt in parts
					select pt.GetComponent<MeshRenderer>()?.sharedMaterials into mats
					from mat in mats ?? Enumerable.Empty<Material>()//.DefaultIfEmpty()
					group mat.GetHashCode() by mats into matHashs
					select matHashs.ToArray()
					;

				return qMathashArrayEveryParts;
			}

			/// <summary>
			/// 
			/// </summary>
			public static IEnumerable<Mesh> QueryMeshEveryObjects( IEnumerable<MonoBehaviour> parts )
			{
				var qMesh =
					from pt in parts
					select
						pt.GetComponent<MeshFilter>()?.sharedMesh
						??
						pt.GetComponent<SkinnedMeshRenderer>()?.sharedMesh
					;

				return qMesh;
			}

			/// <summary>
			/// 
			/// </summary>
			public static IEnumerable<IEnumerable<Vector3>>
				QueryNormalsEveryMesh( IEnumerable<Mesh> meshes )
			{
				return
					from mesh in meshes
					select mesh.normals ?? recalculateNormals_( mesh )
					;
				
				Vector3[] recalculateNormals_( Mesh mesh_ )
				{
					mesh_.RecalculateNormals();
					return mesh_.normals;
				}
			}
			
			/// <summary>
			/// 頂点ごとの pallet index のクエリを返す。
			/// ・
			/// ・
			/// </summary>
			public static IEnumerable<int> QueryePalletEveryVertices
			(
				IEnumerable<Mesh> meshesEveryParts,
				IEnumerable<IEnumerable<int>> matHashArraysEveryParts,
				Dictionary<int,int> mathashToIndexDict
			)
			{
				// 全頂点分のバッファを確保。
				var palletIdxArrayPerVertex = new int [ meshesEveryParts.Sum( mesh => mesh.vertexCount ) ];

				// 
				var qPalletIdxPerIndex =
					from xy in Enumerable.Zip( meshesEveryParts, matHashArraysEveryParts, (x,y)=>(mesh:x, mathash:y))
					from submesh in xy.mathash.Select( (matHash,i)=>(matHash,i) )
					from idx in xy.mesh.GetTriangles( submesh.i, applyBaseVertex:true )
					select (idx, submesh.matHash)
					;

				// 
				foreach( var x in qPalletIdxPerIndex )
				{
					palletIdxArrayPerVertex[x.idx] = mathashToIndexDict[x.matHash];
				}
				
				return palletIdxArrayPerVertex;
			}
			public static (IEnumerable<Material>, IEnumerable<int>)
				QueryePalletsEveryVertices2( IEnumerable<Mesh> qMesh, IEnumerable<Renderer> qRender, IEnumerable<Material[]> qMatArray )
			{
				var matToIndexDict = qMatArray
					.SelectMany( mat => mat )
					.Distinct()
					.Select( (mat,i) => (mat,i) )
					.ToDictionary( x => x.mat, x => x.i )
					;
				
				var palletIdxsPerVertex = new int [ qMesh.Sum( mesh => mesh.vertexCount ) ];
				var qPalletIdxPerIndex =
					from xy in qMesh.Zip( qRender, (x,y)=>(mesh:x, r:y))
					from submesh in xy.r.sharedMaterials.Select( (mat,i)=>(mat,i) )
					from idx in xy.mesh.GetTriangles( submesh.i, applyBaseVertex:true )
					group submesh.mat by idx
					;
				foreach( var x in qPalletIdxPerIndex )
				{
					palletIdxsPerVertex[x.Key] = matToIndexDict[x.First()];
				}
				
				return (matToIndexDict.Keys, palletIdxsPerVertex);
			}
			public static (IEnumerable<Material>, IEnumerable<int>)
				QueryePalletsEveryVertices3( IEnumerable<Mesh> qMesh, IEnumerable<Renderer> qRender, IEnumerable<Material[]> qMatArray )
			{
				var matToIndexDict = qMatArray
					.SelectMany( mat => mat )
					.Distinct()
					.Select( (mat,i) => (mat,i) )
					.ToDictionary( x => x.mat, x => x.i )
					;

				var qPalletIdxsPerVertex =
					from xy in qMesh.Zip( qRender, (x,y)=>(mesh:x, r:y))
					from submesh in xy.r.sharedMaterials.Select( (mat,i)=>(mat,i) )
					from idx in xy.mesh.GetTriangles( submesh.i, applyBaseVertex:true )
					group submesh.mat by idx into matsPerIdx
					orderby matsPerIdx.Key
					select matToIndexDict[matsPerIdx.First()]
					;

				return (matToIndexDict.Keys, qPalletIdxsPerVertex);
			}
		}
	}
	
	public static class MeshUtility
	{
		
		static public GameObject AddToNewGameObject( this Mesh mesh, Material mat )
		{
			var go = new GameObject();
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			go.AddComponent<MeshRenderer>().sharedMaterial = mat;

			return go;
		}

		static public Mesh ToWriteOnly( this Mesh mesh )
		{
			mesh.UploadMeshData( markNoLongerReadable: true );
			return mesh;
		}
		static public Mesh ToDynamic( this Mesh mesh )
		{
			mesh.MarkDynamic();
			return mesh;
		}


		//static public IEnumerable<(int i0, int i1, int i2)> AsTriangleTupple( this int[] indexes )
		//{
		//	for( var i=0; i<indexes.Length; i+=3 )
		//	{
		//		yield return ( indexes[i+0], indexes[i+1], indexes[i+2] );
		//	}
		//}

	}

}

