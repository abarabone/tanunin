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
		public List<List<int>>	IndeciesPerSubmesh;
		public List<Vector3>	Tangents;
		public List<Color32>	Colors;

		// 間接的に必要な要素
		public List<Matrix4x4>		mtObjects;
		public Matrix4x4			MtBaseInv;

		// 結合後の材質（
		public Material[] materials;

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
			if( this.IndeciesPerSubmesh != null )
			{
				foreach( var x in this.IndeciesPerSubmesh.Select( (idxs,i) => (idxs,i) ) )
				{
					mesh.SetTriangles( x.idxs, submesh:x.i, calculateBounds:true );
				}
			}

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
			BuildUnlitMeshElements( IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true )
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToList();

			return BuildUnlitMeshElements( mmts, tfBase, isCombineSubMeshes );
		}

		static Func<MeshElements> BuildUnlitMeshElements
			( List<(Mesh mesh,Material[] mats,Transform tf)> mmts, Transform tfBase, bool isCombineSubMeshes )
		{

			var vtxss = ( from x in mmts select x.mesh.vertices ).ToList();
			var uvss = ( from x in mmts select x.mesh.uv ).ToList();

			var idxsss = ( from x in mmts select x.mesh )
				.To(PerSubMeshPerMesh.QueryIndices).ToListRecursive2();
			
			var mtBaseInv = tfBase.worldToLocalMatrix;
			var mtObjects = ( from x in mmts select x.tf.localToWorldMatrix ).ToList();


			return () =>
			{
				var materialsCombined = ( from x in mmts select x.mats ).To(MaterialCombined.Combine).ToArray();

				return new MeshElements
				{
					Vertecies = ConvertUtility.ToVerticesList( vtxss, mtObjects, mtBaseInv ),
					Uvs = uvss.SelectMany( uvs => uvs ).ToList(),

					IndeciesPerSubmesh = isCombineSubMeshes
						? ConvertUtility.ToIndicesList( vtxss, idxsss, mtObjects )
						: ConvertUtility.ToIndicesList( vtxss, idxsss, mtObjects, mmts, materialsCombined ),

					MtBaseInv = mtBaseInv,
					mtObjects = mtObjects,

					materials = materialsCombined,
				};
			};
		}
		
		
		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶと法線。
		/// </summary>
		static public Func<MeshElements>
			BuildNormalMeshElements( IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true )
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToList();

			return BuildUnlitMeshElements( mmts, tfBase, isCombineSubMeshes );
		}
		
		static Func<MeshElements>
			BuildNormalMeshElements( List<(Mesh mesh,Material[] mats,Transform tf)> mmts, Transform tfBase, bool isCombineSubMeshes )
		{
			var f = BuildUnlitMeshElements( mmts, tfBase, isCombineSubMeshes );
			
			var nmss = ( from x in mmts select x.mesh ).To(PerMesh.QueryNormals).ToList();
			
			return () =>
			{
				var me = f();

				me.Normals = ConvertUtility.ToNormalsList( nmss, me.mtObjects, me.MtBaseInv );

				return me;
			};
		}

		
		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。Structure オブジェクト用。
		/// </summary>
		static public Func<MeshElements>
			BuildStructureWithPalletMeshElements ( IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{
			var gameObjects = from part in parts select part.gameObject;
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToList();

			return BuildStructureWithPalletMeshElements( mmts, tfBase );
		}
		
		static Func<MeshElements>
			BuildStructureWithPalletMeshElements ( List<(Mesh mesh,Material[] mats,Transform tf)> mmts, Transform tfBase )
		{

			var f = BuildNormalMeshElements( mmts, tfBase, isCombineSubMeshes:true );
			
			
			// 全パーツから、パーツＩＤをすべてクエリする。
			var partId_PerMesh = ( from x in mmts select x.tf.GetComponent<_StructurePartBase>().partId ).ToList();
			
			// サブメッシュ単位で、頂点数を取得。
			var vertexCount_PerSubmeshPerMesh =
				( from x in mmts select x.mesh ).To(PerSubMeshPerMesh.QueryVertexCount).ToListRecursive2();
			

			return () =>
			{
				var me = f();
				
				me.Colors = ConvertUtility.ToColor32List( vertexCount_PerSubmeshPerMesh, partId_PerMesh, mmts, me.materials );

				return me;
			};
			
		}
		
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
		public static List<Vector3> ToVerticesList
			( IEnumerable<IEnumerable<Vector3>> verticesPerMeshes, IEnumerable<Matrix4x4> mtObjects, Matrix4x4 mtBaseInv )
		{
			var qVertex =
				from xy in (verticesPerMeshes, mtObjects).Zip( (x,y)=>(vtxs:x, mt:y) )
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
			( IEnumerable<IEnumerable<Vector3>> normalsPerMeshes, IEnumerable<Matrix4x4> mtObjects, Matrix4x4 mtBaseInv )
		{
			var qNormal =
				from xy in (normalsPerMeshes, mtObjects).Zip( (x,y)=>(nms:x, mt:y) )
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
		/// 
		/// </summary>
		public static List<Color32> ToColor32List
			(
				IEnumerable<IEnumerable<int>> vertexCount_PerSubmeshPerMesh,
				IEnumerable<int> partsIds_PerMesh,
				IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
				IEnumerable<Material> materialsCombined
			)
		{
			
			var vtxCount_PerMesh =
				from vtxCounts in vertexCount_PerSubmeshPerMesh select vtxCounts.Sum();

			// 頂点ごとのパーツＩＤをすべてクエリする。
			var qPid_PerVtx =
				queryPartIdx_PerVtx_( vtxCount_PerMesh, partsIds_PerMesh );
			

			var qDstMatHashes = from mat in materialsCombined select mat.GetHashCode();
			var qSrcMatHashes = ( from x in mmts select x.mats ).To(PerSubMeshPerMesh.QueryMaterialHash);
			
			// パレットＩＤをすべての頂点ごとにクエリする。
			var qPallets_PerVtx =
				queryePallet_PerVtx_( vertexCount_PerSubmeshPerMesh, qSrcMatHashes, qDstMatHashes );
			

			// パーツＩＤとパレットＩＤを頂点カラーに落とし込む。
			var qPidPallet_PerVtx =
				from xy in (qPid_PerVtx, qPallets_PerVtx).Zip( (x,y)=>(pid:x, pallet:y) )
				select new Color32
				(
					r:	(byte)xy.pid.int4Index, 
					g:	(byte)xy.pid.memberIndex, 
					b:	(byte)xy.pid.bitIndex, 
					a:	(byte)xy.pallet
				);

			return qPidPallet_PerVtx.ToList();
			
			
			/// <summary>
			/// パレットＩＤをすべての頂点ごとにクエリする。
			/// </summary>
			IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
				queryPartIdx_PerVtx_( IEnumerable<int> vtxCount_PerMesh_, IEnumerable<int> partId_PerMesh_ )
			{
				var qPidEveryParts =
					from pid in partId_PerMesh_
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
					from xy in (vtxCount_PerMesh_, qPidEveryParts).Zip( (x,y)=>(vtxCount:x, pid:y) )
					from pidsEveryVtxs in Enumerable.Repeat<(int,int,int)>( xy.pid, xy.vtxCount )
					select pidsEveryVtxs
					;

				return qPidEveryVertices;
			}
			
			/// <summary>
			/// パレットＩＤをすべての頂点ごとにクエリする。
			/// </summary>
			IEnumerable<int> queryePallet_PerVtx_
				(
					IEnumerable<IEnumerable<int>> vtxCount_PerSubmesh_,
					IEnumerable<IEnumerable<int>> matHash_PerSubmeshPerMesh_,
					IEnumerable<int> matHashesCombined_
				)
			{
				var qDstHashAndIdx = matHashesCombined_.Select( (hash,i) => (hash,i) );
				var qPalletIdx_PerSubmesh =
					from hashes in matHash_PerSubmeshPerMesh_
					select
						from srcMatHash in hashes
						join dstMat in qDstHashAndIdx on srcMatHash equals dstMat.hash
						select dstMat.i
					;
				// 頂点ごとに pallet index
				// pallet index は 結合後のマテリアルへの添え字
				// mat idx は、辞書でＩＤを取得して振る
				// submesh ごとの vertex count で、src mat idx を
				var qPalletIdx =
					from meshxy in (qPalletIdx_PerSubmesh, vtxCount_PerSubmesh_).Zip()
					from (int palletIdx, int vtxCount) xy in meshxy.Zip()
					from palletIdx in Enumerable.Repeat(xy.palletIdx, xy.vtxCount)
					select palletIdx
					;

				return qPalletIdx;
			}
		}

		
		/// <summary>
		/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		public static List<List<int>> ToIndicesList
			(
				IEnumerable<Vector3[]> vertices_PerMesh,
				IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
				IEnumerable<Matrix4x4> mtPart_PerMesh
			)
		{
			var mts = mtPart_PerMesh;

			var qBaseVtxs = PerMesh.QueryBaseVertex( vertices_PerMesh );
			
			var qIndex =
				from xyz_ in (indices_PerSubmeshPerMesh, mts, qBaseVtxs).Zip()
				select (idxss: xyz_.x, mt: xyz_.y, baseVtx: xyz_.z) into src
				from idxs in src.idxss
				from index in IndexUtility.ReverseEvery3_IfMinusScale( idxs, src.mt )
				select src.baseVtx + index;

			return Enumerable.Repeat( qIndex.ToList(), 1 ).ToListRecursive2();
		}
		
		/// <summary>
		/// 各パーツメッシュの持つインデックスを結合し、最終的なサブメッシュごとの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// </summary>
		public static List<List<int>> ToIndicesList
			(
				IEnumerable<Vector3[]> vertices_PerMesh,
				IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
				IEnumerable<Matrix4x4> mtPart_PerMesh,
				IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
				IEnumerable<Material> materialsCombined
			)
		{
			var qDstMatHashes = from mat in materialsCombined select mat.GetHashCode();
			var qSrcMatHashes = ( from x in mmts select x.mats ).To(PerSubMeshPerMesh.QueryMaterialHash);
			
			var idxsss = indices_PerSubmeshPerMesh;
			var mts = mtPart_PerMesh;
			var qBaseVtxs = PerMesh.QueryBaseVertex( vertices_PerMesh );

			var qPerMesh = (idxsss, qSrcMatHashes, mts, qBaseVtxs)
				.Zip( (x,y,z,w) => (idxss:x, hashes:y, mt:z, baseVtx:w) );
			
			var qSrcMatGroups =
				from src in qPerMesh
				from perSub in (src.idxss, src.hashes).Zip( (x,y)=>(idxs:x, hash:y, src.mt, src.baseVtx) )
				group perSub by perSub.hash
				;

			var qIdxsPerDstMat =
				from dstHash in qDstMatHashes
				join srcs in qSrcMatGroups on dstHash equals srcs.Key
				select
					from src in srcs
					from idx in src.idxs.ReverseEvery3_IfMinusScale(src.mt)
					select src.baseVtx + idx;
			
			return qIdxsPerDstMat.ToListRecursive2();
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public static class FromObject
	{
		
		/// <summary>
		/// メッシュと材質配列を持つオブジェクトを抽出し、その組を列挙して返す。片方でも null であれば、除外される。
		/// </summary>
		public static IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>
			QueryMeshMatsTransform_IfHaving( IEnumerable<GameObject> gameObjects )
		{
			return
				from obj in gameObjects
				let r = obj.GetComponent<SkinnedMeshRenderer>().As()
				let mesh = r?.sharedMesh ?? obj.GetComponent<MeshFilter>().sharedMesh
				let mats = r?.sharedMaterials ?? obj.GetComponent<Renderer>().sharedMaterials
				select (mesh, mats, obj.transform) into x
				where x.mesh != null || x.mats != null
				select x
				;
		}
		
	}

	/// <summary>
	/// 
	/// </summary>
	public static class PerMesh
	{

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<Vector3[]> QueryNormals( IEnumerable<Mesh> meshes )
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
		/// メッシュごとに、インデックスのベースオフセットをクエリする。
		/// </summary>
		public static IEnumerable<int> QueryBaseVertex( IEnumerable<Vector3[]> vtxsEveryMeshes )
		{
			var qVtxCount = vtxsEveryMeshes // まずは「mesh n の頂点数」の集合をクエリする。
				.Select( vtxs => vtxs.Count() )
				.Scan( seed: 0, ( pre, cur ) => pre + cur )
				;
			return Enumerable.Repeat( 0, 1 ).Concat( qVtxCount );
			// { 0 } + { mesh 0 の頂点数, mesh 1 の頂点数, ... }
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public static class PerSubMeshPerMesh
	{

		/// <summary>
		/// メッシュ、サブメッシュごとにインデックス配列をクエリする。
		/// </summary>
		public static IEnumerable<IEnumerable<int[]>> QueryIndices( IEnumerable<Mesh> meshes )
		{
			return
				from mesh in meshes
				select
					from isubmesh in Enumerable.Repeat( 0, mesh.subMeshCount )
					select mesh.GetTriangles( isubmesh )
				;
		}

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<IEnumerable<int>> QueryMaterialHash( IEnumerable<Material[]> materials_PerMesh )
		{
			return
				from mats in materials_PerMesh
				select
					from mat in mats
					select mat.GetHashCode()
				;
		}
		

		/// <summary>
		/// メッシュ、サブメッシュごとの頂点数をクエリする。
		/// </summary>
		public static IEnumerable<IEnumerable<int>> QueryVertexCount( IEnumerable<Mesh> meshes )
		{
			return
				from mesh in meshes
				select
					from isubmesh in Enumerable.Range( 0, mesh.subMeshCount )
					select calculateVertexCount_( isubmesh, mesh )
				;
			
			int calculateVertexCount_( int isubmesn_, Mesh mesh_ )
			{
				bool isLastSubmesh_() => ( isubmesn_ == mesh_.subMeshCount - 1 );

				var pre =
					(int)mesh_.GetBaseVertex( isubmesn_ + 0 );
				var cur = isLastSubmesh_()
					? mesh_.vertexCount
					: (int)mesh_.GetBaseVertex( isubmesn_ + 1 );

				return cur - pre;
			}
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public static class MaterialCombined
	{
		/// <summary>
		/// 
		/// </summary>
		public static Material[] Combine( IEnumerable<Material[]> materials_PerObject )
		{
			var qMats =
				from mat in materials_PerObject.SelectMany().Distinct()
			//	orderby mat.name
				select mat
				;

			return qMats.ToArray();
		}
	}
	

	public static class IndexUtility
	{
		
		/// <summary>
		/// 反転（スケールの一部がマイナス）メッシュであれば、三角インデックスを逆順にする。
		/// </summary>
		public static IEnumerable<int> ReverseEvery3_IfMinusScale( this IEnumerable<int> indices, Matrix4x4 mtObject )
		{
			if( isMinusScale_(in mtObject) ) return reverseEvery3_(indices);

			return indices;

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
						yield return i0;//210でもいい？
						yield return i2;
						yield return i1;
					}
				}
			}
		}
	}
}
