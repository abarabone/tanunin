using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abss.Common.Extension;
using Abss.StructureObject;

namespace Abss.Geometry2
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
		public List<List<int>>	IndeciesEverySubmeshes;
		public List<Vector3>	Tangents;
		public List<Color32>	Colors;

		// 間接的に必要な要素
		public List<Matrix4x4>		mtObjects;
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
			if( this.IndeciesEverySubmeshes != null )
			{
				foreach( var x in this.IndeciesEverySubmeshes.Select( (idxs,i) => (idxs,i) ) )
				{
					mesh.SetTriangles( x.idxs, submesh:x.i, calculateBounds:false );
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
				.To(PerSubMeshPerMesh.QueryIndices)
				.ToListRecursive2();

			var mtBaseInv = tfBase.worldToLocalMatrix;
			var mtObjects = ( from x in mmts select x.tf.localToWorldMatrix ).ToList();

			var matNameAndHashLists = isCombineSubMeshes
				? null
				: ( from x in mmts select x.mats )
					.To(MaterialUtility.QueryMatNameAndHash_EverySubmeshesEveryMeshes)
					.ToListRecursive2();
			

			return () =>
			{
				var matHashToIdxDict = isCombineSubMeshes
					? null
					: MaterialUtility.ToDictionaryForMaterialHashToIndex( matNameAndHashLists );
				var idxss = isCombineSubMeshes
					? ConvertUtility.ToIndicesList( vtxss, idxsss, mtObjects )
					: ConvertUtility.ToIndicesList( vtxss, idxsss, mtObjects, matNameAndHashLists, matHashToIdxDict );

				//var idxss =
				//	matNameAndHashLists
				//	?.To(MaterialUtility.ToDictionaryForMaterialHashToIndex)
				//	?.To( x => ConvertUtility.ToIndicesList( vtxss, idxsss, mtObjects, matNameAndHashLists, x ) )
				//	?? ConvertUtility.ToIndicesList( vtxss, idxsss, mtObjects );

				return new MeshElements
				{
					Vertecies = ConvertUtility.ToVerticesList( vtxss, mtObjects, mtBaseInv ),
					Uvs = uvss.SelectMany( uvs => uvs ).ToList(),
					IndeciesEverySubmeshes = idxss,

					MtBaseInv = mtBaseInv,
					mtObjects = mtObjects,
				};
			};
		}
		
		/// <summary>
		/// 各パーツメッシュの持つインデックスを結合し、最終的なサブメッシュごとの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		public static List<List<int>> ToIndicesList(
			IEnumerable<Vector3[]> vertices_PerMesh,
			IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
			IEnumerable<Matrix4x4> mtPart_PerMesh,
			IEnumerable<IEnumerable<int>> materialHash_PerSubmeshPerMesh,
			IEnumerable<int> materialHashesCombined
		)
		{
			var idxsss = indices_PerSubmeshPerMesh;
			var hashess = materialHash_PerSubmeshPerMesh;
			var mts = mtPart_PerMesh;

			var qBaseVtxs = PerMesh.QueryBaseVertex( vertices_PerMesh );

			var qSrcMatGroups =
				from perMesh in (idxsss, hashess, mts, qBaseVtxs).Zip()
				select (idxss:perMesh.x, hashes:perMesh.y, mt:perMesh.z, baseVtx:perMesh.w) into src
				from perSub in (src.idxss, src.hashes).Zip( (x,y)=>(idxs:x, hash:y, src.mt, src.baseVtx) )
				group perSub by perSub.hash
				;

			var qIdxsPerDstMat =
				from dstHash in materialHashesCombined
				join srcs in qSrcMatGroups on dstHash equals srcs.Key
				select
					from src in srcs
					from idx in src.idxs.ReverseEvery3_IfMinusScale(src.mt)
					select src.baseVtx + idx;
			
			return qIdxsPerDstMat.ToListRecursive2();
		}


	}
	
	/// <summary>
	/// メッシュ要素（頂点、ＵＶ、インデックスなど別々）ごとのコンバートを行い、list<> につめて返す。
	/// 関数の引数はすべて Unity オブジェクトではないため、このクラスの全関数はサブスレッドで処理できる。
	/// </summary>
	static class ConvertUtility
	{

		/// <summary>
		/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		public static List<List<int>> ToIndicesList(
			IEnumerable<Vector3[]> vertices_PerMesh,
			IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
			IEnumerable<Matrix4x4> mtPart_PerMesh
		)
		{
			var mts = mtPart_PerMesh;

			var qBaseVtxs = PerMesh.QueryBaseVertex( vertices_PerMesh );
			
			var qIndex =
				from xyz_ in (indices_PerSubmeshPerMesh, mts, qBaseVtxs).Zip()
				let idxss = xyz_.x
				let mt = xyz_.y
				let baseVtx = xyz_.z
				from idxs in idxss
				from index in IndexUtility.ReverseEvery3_IfMinusScale( idxs, mt )
				select baseVtx + index;

			return Enumerable.Repeat( qIndex.ToList(), 1 ).ToListRecursive2();
		}

		public static Material[] ToMaterials( IEnumerable<Material[]> materials_PerObject )
		{
			var qMats =
				from mat in materials_PerObject.SelectMany().Distinct()
				orderby mat.name
				select mat
				;

			return qMats.ToArray();
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
		public static IEnumerable<Vector3[]>
			QueryNormals_EveryMeshes( IEnumerable<Mesh> meshes )
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
			var qVtxCount = vtxsEveryMeshes	// まずは「mesh n の頂点数」の集合をクエリする。
				.Select( vtxs => vtxs.Count() )
				.Scan( seed:0, (pre,cur) => pre + cur )
				;
			return Enumerable.Repeat(0,1).Concat( qVtxCount );
				// { 0 } + { mesh 0 の頂点数, mesh 1 の頂点数, ... }
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public static class PerSubMeshPerMesh
	{
		
		/// <summary>
		/// メッシュごと、サブメッシュごとにインデックス配列をクエリする。
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
		public static IEnumerable<IEnumerable<int>> QueryMaerialHash( IEnumerable<Material[]> materials_PerMesh )
		{
			return
				from mats in materials_PerMesh
				select
					from mat in mats
					select mat.GetHashCode()
				;
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public static class MaterialCombined
	{

		/// <summary>
		/// すべてのサブメッシュ単位の材質名とハッシュ値をクエリする。
		/// </summary>
		public static IEnumerable<(string name,int hash)>
			QueryNameAndHashOfMaterials( IEnumerable<Material[]> materials_PerMesh )
		{
			return
				from mats in materials_PerMesh
				from mat in mats
				select (mat.name, hash:mat.GetHashCode())
				;
		}

		/// <summary>
		/// 結合後の材質ハッシュ値列をクエリする。
		/// </summary>
		public static IEnumerable<int> QueryCombinedMaterialHashes
			( IEnumerable<(string name,int hash)> nameAndHashOfMaterials )
		{
			return
				from x in nameAndHashOfMaterials.Distinct()
				orderby x.name
				select x.hash
				;
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
