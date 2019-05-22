using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abss.Common.Extension;
using Abss.StructureObject;

namespace Abss.Geometry
{
	
	/// <summary>
	/// Mesh 要素を格納する。並列処理させた結果を格納し、最後に Mesh を作成するために使用する。
	/// </summary>
	public struct MeshElements
	{　
		// Mesh 要素
		public List<Vector3>	Vertecies;
		public List<Vector3>	Normals;
		public List<Vector2>	Uvs;
		public List<int>		Indecies;
		public List<Vector3>	Tangents;
		public List<Color32>	Colors;

		// 間接的に必要な要素
		public IEnumerable<Mesh>	MeshesQuery;
		public List<Matrix4x4>		MtParts;
		public Matrix4x4			MtBaseInv;

		/// <summary>
		/// Mesh を生成する。
		/// </summary>
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



	/// <summary>
	/// メッシュを各要素ごとに結合する。
	/// 
	/// </summary>
	public static class MeshCombiner
	{
		
		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶのみ。
		/// </summary>
		static public Func<MeshElements>
			BuildUnlitMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var qMesh = QueryUtility.QueryMeshEveryObjects( parts );

			var vtxss = ( from mesh in qMesh select mesh.vertices ).ToList();
			var uvss = ( from mesh in qMesh select mesh.uv ).ToList();
			var idxss = ( from mesh in qMesh select mesh.triangles ).ToList();

			var mtBaseInv = tfBase.worldToLocalMatrix;
			var mtParts = ( from pt in parts select pt.transform.localToWorldMatrix ).ToList();

			return () => new MeshElements
			{
				Vertecies = ConvertUtility.ToVerticesList( vtxss, mtParts, mtBaseInv ),
				Uvs = uvss.SelectMany( uvs => uvs ).ToList(),
				Indecies = ConvertUtility.ToIndeicesList( vtxss, idxss, mtParts ),

				MtBaseInv = mtBaseInv,
				MtParts = mtParts,
			};
		}
		
		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶと法線。
		/// </summary>
		static public Func<MeshElements>
			BuildNormalMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var f = BuildUnlitMeshElements( parts, tfBase );

			var qMesh = QueryUtility.QueryMeshEveryObjects( parts );

			var nmss = QueryUtility.QueryNormalsEveryMesh( qMesh ).ToList();
			
			return () =>
			{
				var me = f();

				me.Normals = ConvertUtility.ToNormalsList( nmss, me.MtParts, me.MtBaseInv );

				return me;
			};
		}
		
		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。Structure オブジェクト用。
		/// </summary>
		static public Func<MeshElements>
			BuildStructureWithPalletMeshElements( IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{
			var f = BuildNormalMeshElements( parts, tfBase );

			var qMesh = QueryUtility.QueryMeshEveryObjects( parts );


			// パーツＩＤとパレットＩＤをそれぞれ生成し、Color32 にパックする。
			
			// 全パーツから、パーツＩＤをすべてクエリする。
			var partIds = ( from pt in parts select pt.partId ).ToList();

			// サブメッシュ単位で、頂点数を取得。
			var vertexCountEverySubmeshes = QueryUtility.QueryVertexCountEverySubmeshes( qMesh ).ToList();

			// 各メッシュごとに頂点数を取得。
			var vertexCountEveryMeshes = ( from mesh in qMesh select mesh.vertexCount ).ToList();

			// パーツごとに、mat hath の配列を取得。
			var mathashArrayEveryParts = QueryUtility.QueryMatHashArraysEveryParts( parts ).ToList();

			return () => 
			{
				// mat hash → combined mat index に変換する辞書を生成する。
				var mathashToIndexDict = ConvertUtility.ToDictionaryForMaterialHashToIndex( mathashArrayEveryParts );

				// 頂点ごとのパーツＩＤをすべてクエリする。
				var qPidPerVertex = QueryUtility.QueryStructureIndexEveryVertices( vertexCountEveryMeshes, partIds );

				// パレットＩＤをすべて。
				var palletsEveryVertices = QueryUtility
					.QueryePalletEveryVertices( vertexCountEverySubmeshes, mathashArrayEveryParts, mathashToIndexDict )
					.ToList();
				
				var me = f();

				me.Colors = ConvertUtility.ToColor32List( qPidPerVertex, palletsEveryVertices );

				return me;
			};
		}



		// ---------------------------------------------------------------------------------------------------

		/// <summary>
		/// メッシュ要素（頂点、ＵＶ、インデックスなど別々）ごとのコンバートを行い、list<> につめて返す。
		/// 関数の引数はすべて Unity オブジェクトではないため、このクラスの全関数はサブスレッドで処理できる。
		/// </summary>
		static class ConvertUtility
		{
			/// <summary>
			/// 
			/// </summary>
			public static List<Vector3> ToVerticesList
				( IEnumerable<IEnumerable<Vector3>> verticesPerMeshes, IEnumerable<Matrix4x4> mtParts, Matrix4x4 mtBaseInv )
			{
				var qVertex =
					from xy in (verticesPerMeshes, mtParts).Zip( (x,y)=>(vtxs:x, mt:y) )
					let mt = xy.mt * mtBaseInv
					from vtx in xy.vtxs
					select mt.MultiplyPoint3x4( vtx )
					;
				
				return qVertex.ToList();
			}

			/// <summary>
			/// 
			/// </summary>
			public static List<Vector3> ToNormalsList
				( IEnumerable<IEnumerable<Vector3>> normalsPerMeshes, IEnumerable<Matrix4x4> mtParts, Matrix4x4 mtBaseInv )
			{
				var qNormal =
					from xy in Enumerable.Zip( normalsPerMeshes, mtParts, (x,y)=>(nms:x, mt:y) )
					let mt = xy.mt * mtBaseInv
					from nm in xy.nms
					select mt.MultiplyVector( nm )
					;
				
				return qNormal.ToList();

				//Vector3[] recalculateNormals_( Mesh mesh_ )
				//{
				//	mesh_.RecalculateNormals();
				//	return mesh_.normals;
				//}
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
			public static List<int> ToIndeicesList
				( IEnumerable<IEnumerable<Vector3>> verticesPerMeshes, IEnumerable<IEnumerable<int>> indicesPerMeshes, IEnumerable<Matrix4x4> mtParts )
			{
				// 各メッシュに対する、頂点インデックスのベースオフセットをクエリする。
				var qVtxCount = verticesPerMeshes								// まずは「mesh n の頂点数」の集合をクエリする。
					.Select( vtxs => vtxs.Count() )
					.Scan( seed:0, (pre,cur) => pre + cur )
					;
				var qBaseVertex = Enumerable.Repeat(0,1).Concat( qVtxCount );	// { 0 } + { mesh 0 の頂点数, mesh 1 の頂点数, ... }
				
				var qIndex =
					from (IEnumerable<int> idxs, Matrix4x4 mt, int baseVtx) xyz in (indicesPerMeshes, mtParts, qBaseVertex).Zip()
					from index in reverseEvery3_IfMinusScale_( xyz.idxs, xyz.mt )
					select xyz.baseVtx + index;

				return qIndex.ToList();


				IEnumerable<int> reverseEvery3_IfMinusScale_( IEnumerable<int> indices_, Matrix4x4 mtPart_ )
				{
					if( isMinusScale_(in mtPart_) ) return reverseEvery3_(indices_);

					return indices_;
				}

				bool isMinusScale_( in Matrix4x4 mt )
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
		/// 引数に Unity オブジェクトをとらないものは、サブスレッドでも処理できる。
		/// </summary>
		static class QueryUtility
		{
			
			/// <summary>
			/// 
			/// </summary>
			public static IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
				QueryStructureIndexEveryVertices
				( IEnumerable<int> vertexCountEveryMeshes, IEnumerable<int> partIds )
			{
				var qPidEveryParts =	
					from pid in partIds
					select (
					//	int4Index:		pid >> 5 >> 2,
					//	memberIndex:	pid >> 5 & 0b_11,	// 0 ~ 3
					//	bitIndex:		pid & 0b_1_1111		// 0 ~ 31
						int4Index:		pid / 24 >> 2,
						memberIndex:	pid / 24 & 0b_11,	// 0 ~ 23
						bitIndex:		pid % 24			// 0 ~ 23
						// unity で int4[] を転送する手段がないので、float の実数部範囲で妥協する。
					);
				var qPidEveryVertices =
					from xy in Enumerable.Zip( vertexCountEveryMeshes, qPidEveryParts, (x,y)=>(vtxCount:x, pid:y) )
					from pidsEveryVtxs in Enumerable.Repeat<(int,int,int)>( xy.pid, xy.vtxCount )
					select pidsEveryVtxs
					;

				return qPidEveryVertices;
			}
			
			/// <summary>
			/// 全パーツから、それぞれのマテリアルハッシュ配列をすべてクエリする。
			/// 名前順にソートされる。
			/// </summary>
			public static IEnumerable<IEnumerable<int>>
				QueryMatHashArraysEveryParts( IEnumerable<_StructurePartBase> parts )
			{
				var qMatHashArrayEveryParts =
					from pt in parts
					select pt.GetComponent<MeshRenderer>()?.sharedMaterials into mats
					from mat in mats ?? Enumerable.Empty<Material>()//mats.DefaultIfEmpty()
					orderby mat.name
					group mat.GetHashCode() by mats into matHashs
					select matHashs.ToArray()
					;

				return qMatHashArrayEveryParts;
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
			/// 
			/// </summary>
			public static IEnumerable<int> QueryVertexCountEverySubmeshes( IEnumerable<Mesh> meshes )
			{
				var qVertexCountEverySubmeshes =
					from mesh in meshes
					from subIdx in Enumerable.Range( 0, mesh.subMeshCount )
					select calculateVertexCount_( subIdx, mesh )
					;

				return qVertexCountEverySubmeshes;

				int calculateVertexCount_( int subIdx_, Mesh mesh_ )
				{
					if( subIdx_ < mesh_.subMeshCount - 1 ) return (int)mesh_.GetBaseVertex(subIdx_ + 1);

					return mesh_.vertexCount;
				}
			}

			/// <summary>
			/// 頂点ごとの pallet index のクエリを返す。
			/// </summary>
			public static IEnumerable<int> QueryePalletEveryVertices
			(
				IEnumerable<int> vertexCountEverySubmeshes,
				IEnumerable<IEnumerable<int>> matHashArrayEveryMeshes,
				Dictionary<int,int> mathashToIndexDict
			)
			{
				// 頂点ごとに pallet index
				// pallet index は 結合後のマテリアルへの添え字
				// mat idx は、辞書でＩＤを取得して振る
				// submesh ごとの vertex count で、src mat idx を
				var qPalletIdx =
					from matHashEverySubmeshes in matHashArrayEveryMeshes
					from (int matHash, int vtxCount) xy in (matHashEverySubmeshes, vertexCountEverySubmeshes).Zip()
					let matIdx = mathashToIndexDict[xy.matHash]
					from matIdxEveryVertices in Enumerable.Repeat(matIdx, xy.vtxCount)
					select matIdxEveryVertices
					;

				return qPalletIdx;
			}
		}
	}
	
}
