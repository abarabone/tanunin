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
			var qMesh = VertexUtility.QueryMesh_EveryObjects( gameObjects );

			var vtxss = ( from mesh in qMesh select mesh.vertices ).ToList();
			var uvss = ( from mesh in qMesh select mesh.uv ).ToList();
			var idxsss = IndexUtility.QueryIndices_EverySubmeshesEveryMeshes( qMesh ).ToListRecursive2();

			var mtBaseInv = tfBase.worldToLocalMatrix;
			var mtObjects = ( from pt in gameObjects select pt.transform.localToWorldMatrix ).ToList();

			var matNameAndHashLists = isCombineSubMeshes
				? null
				: MaterialUtility.QueryMatNameAndHash_EverySubmeshesEveryMeshes( gameObjects ).ToListRecursive2();
			

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
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶと法線。
		/// </summary>
		static public Func<MeshElements>
			BuildNormalMeshElements( IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes )
		{
			var f = BuildUnlitMeshElements( gameObjects, tfBase, isCombineSubMeshes );

			var qMesh = VertexUtility.QueryMesh_EveryObjects( gameObjects );

			var nmss = VertexUtility.QueryNormals_EveryMeshes( qMesh ).ToList();
			
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
		static public Func<MeshElements> BuildStructureWithPalletMeshElements
			( IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{
			var gos = from part in parts select part.gameObject;

			var f = BuildNormalMeshElements( gos, tfBase, isCombineSubMeshes:true );

			var qMesh = VertexUtility.QueryMesh_EveryObjects( gos );


			// パーツＩＤとパレットＩＤをそれぞれ生成し、Color32 にパックする。
			// （ほかのメッシュ要素は f で取得されている。ここでは、Color32 の生成だけに集中すればよい。）
			

			// 全パーツから、パーツＩＤをすべてクエリする。
			var partIds = ( from pt in parts select pt.partId ).ToList();

			// 各メッシュごとに頂点数を取得。
			var vertexCount_EveryMeshes = ( from mesh in qMesh select mesh.vertexCount ).ToList();


			// サブメッシュ単位で、頂点数を取得。
			var vertexCount_EverySubmeshes = VertexUtility.QueryVertexCount_EverySubmeshes( qMesh ).ToList();

			// mat name and hath の配列を取得。
			var mats_EveryMeshes =
				MaterialUtility.QueryMatNameAndHash_EverySubmeshesEveryMeshes( gos ).ToListRecursive2();


			return () => 
			{

				// 頂点ごとのパーツＩＤをすべてクエリする。
				var qPid_EveryVertices =
					VertexUtility.QueryStructurePartIndex_EveryVertices( vertexCount_EveryMeshes, partIds );


				// mat hash → combined mat index に変換する辞書を生成する。
				var matHashToIndexDict =
					MaterialUtility.ToDictionaryForMaterialHashToIndex( mats_EveryMeshes );

				// パレットＩＤをすべての頂点ごとにクエリする。
				var qPallets_EveryVertices = VertexUtility
					.QueryePallet_EveryVertices( vertexCount_EverySubmeshes, mats_EveryMeshes, matHashToIndexDict );
				

				var me = f();

				me.Colors = ConvertUtility.ToColor32List( qPid_EveryVertices, qPallets_EveryVertices );

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
		public static List<Color32> ToColor32List(
			IEnumerable<(int int4Index, int memberIndex, int bitIndex)> partsIdsEveryVertices,
			IEnumerable<int> palletsEveryVertices
		)
		{
			var qPidPalletPerVertex =
				from xy in (partsIdsEveryVertices, palletsEveryVertices).Zip( (x,y)=>(pid:x, pallet:y) )
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
		/// 各パーツメッシュの持つインデックスを結合し、最終的なサブメッシュごとの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		public static List<List<int>> ToIndicesList(
			IEnumerable<Vector3[]> vertices_EveryMeshes,
			IEnumerable<IEnumerable<int[]>> indices_EverySubmeshesEveryMeshes,
			IEnumerable<Matrix4x4> mtPart_EveryMeshes,
			IEnumerable<IEnumerable<(string,int hash)>> matNameAndHash_EverySubmeshesEveryMeshes,
			Dictionary<int,int> matHashToIndexDict
		)
		{
			var idxsss = indices_EverySubmeshesEveryMeshes;
			var matss = matNameAndHash_EverySubmeshesEveryMeshes;
			var mts = mtPart_EveryMeshes;

			var qBaseVtxs = IndexUtility.QueryBaseVertex_EveryMeshes( vertices_EveryMeshes );

			var qIdxs =
			// メッシュ
				from xyzw_ in (idxsss, matss, mts, qBaseVtxs).Zip()
				let src = (idxss:xyzw_.x, mats:xyzw_.y, mt:xyzw_.z, baseVtx:xyzw_.w)
			// サブメッシュ（結合後の材質をベースに巡回）
				from idxs in findIdxs_( src.idxss, src.mats, matHashToIndexDict ).EmptyIfNull()
			// インデックス
				select
					from idx in IndexUtility.ReverseEvery3_IfMinusScale( idxs, src.mt )
					select src.baseVtx + idx;

			return qIdxs.ToListRecursive2();

			IEnumerable<int[]> findIdxs_
				( IEnumerable<int[]> idxss, IEnumerable<(string,int hash)> mats, Dictionary<int,int> hashToIdxDict )
			{
				return
					from dstSubmesh in hashToIdxDict// index 順にソートされているとする
					select (mats, idxss).Zip( (x,y)=>(mat:x, idxs:y) )
						.FirstOrDefault( x => x.mat.hash == dstSubmesh.Key )// 見つからなければ idxs:null
						.idxs
					;
			}
		}


		/// <summary>
		/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		public static List<List<int>> ToIndicesList(
			IEnumerable<Vector3[]> verticesEveryMeshes,
			IEnumerable<IEnumerable<int[]>> indicesEverySubmeshesEveryMeshes,
			IEnumerable<Matrix4x4> mtPartEveryMeshes
		)
		{
			var idxss =
				from submeshes in indicesEverySubmeshesEveryMeshes
				select submeshes.SelectMany( idxss_ => idxss_ )
				;
			var mts = mtPartEveryMeshes;

			var qBaseVtxs = IndexUtility.QueryBaseVertex_EveryMeshes( verticesEveryMeshes );
				
			var qIndex =
				from xyz_ in (idxss, mts, qBaseVtxs).Zip()
				let xyz = (idxs:xyz_.x, mt:xyz_.y, baseVtx:xyz_.z)
				from index in IndexUtility.ReverseEvery3_IfMinusScale( xyz.idxs, xyz.mt )
				select xyz.baseVtx + index;

			return Enumerable.Repeat( qIndex.ToList(), 1 ).ToListRecursive2();
		}

	}
	
		
	/// <summary>
	/// 
	/// </summary>
	public static class IndexUtility
	{
		/// <summary>
		/// メッシュごと、サブメッシュごとにインデックス配列をクエリする。
		/// </summary>
		public static IEnumerable<IEnumerable<int[]>>
			QueryIndices_EverySubmeshesEveryMeshes( IEnumerable<Mesh> meshes )
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
		public static IEnumerable<int> QueryBaseVertex_EveryMeshes( IEnumerable<Vector3[]> vtxsEveryMeshes )
		{
			// 各メッシュに対する、頂点インデックスのベースオフセットをクエリする。
			var qVtxCount = vtxsEveryMeshes	// まずは「mesh n の頂点数」の集合をクエリする。
				.Select( vtxs => vtxs.Count() )
				.Scan( seed:0, (pre,cur) => pre + cur )
				;
			return Enumerable.Repeat(0,1).Concat( qVtxCount );
				// { 0 } + { mesh 0 の頂点数, mesh 1 の頂点数, ... }
		}

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<int> ReverseEvery3_IfMinusScale( IEnumerable<int> indices, Matrix4x4 mtObject )
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
	public static class VertexUtility
	{
			
		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<Mesh> QueryMesh_EveryObjects( IEnumerable<GameObject> gameObjects )
		{
			var qMesh =
				from pt in gameObjects
				select
					pt.GetComponent<MeshFilter>()?.sharedMesh
					??
					pt.GetComponent<SkinnedMeshRenderer>()?.sharedMesh
				;

			return qMesh.Where( mesh => mesh != null );
		}

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
		/// 
		/// </summary>
		public static IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
			QueryStructurePartIndex_EveryVertices
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
				from xy in (vertexCountEveryMeshes, qPidEveryParts).Zip( (x,y)=>(vtxCount:x, pid:y) )
				from pidsEveryVtxs in Enumerable.Repeat<(int,int,int)>( xy.pid, xy.vtxCount )
				select pidsEveryVtxs
				;

			return qPidEveryVertices;
		}
			

		/// <summary>
		/// サブメッシュごとの頂点数をクエリする。
		/// </summary>
		public static IEnumerable<int> QueryVertexCount_EverySubmeshes( IEnumerable<Mesh> meshes )
		{
			var qVertexCountEverySubmeshes =
				from mesh in meshes
				from isubmesh in Enumerable.Range( 0, mesh.subMeshCount )
				select calculateVertexCount_( isubmesh, mesh )
				;

			return qVertexCountEverySubmeshes;

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

		/// <summary>
		/// 頂点ごとの pallet index のクエリを返す。
		/// </summary>
		public static IEnumerable<int> QueryePallet_EveryVertices
		(
			IEnumerable<int> vertexCount_EverySubmeshes,
			IEnumerable<IEnumerable<(string name, int hash)>> nameAndHashOfMaterials_EveryMeshes,
			Dictionary<int,int> matHashToIndexDict
		)
		{
			var qHash_EverySubmeshes =
				from matEverySubmeshes in nameAndHashOfMaterials_EveryMeshes
				from mat in matEverySubmeshes
				select mat.hash
				;
			// 頂点ごとに pallet index
			// pallet index は 結合後のマテリアルへの添え字
			// mat idx は、辞書でＩＤを取得して振る
			// submesh ごとの vertex count で、src mat idx を
			var qPalletIdx =
				from (int matHash, int vtxCount) xy in (qHash_EverySubmeshes, vertexCount_EverySubmeshes).Zip()
				let matIdx = matHashToIndexDict[xy.matHash]
				from matIdxEveryVertices in Enumerable.Repeat(matIdx, xy.vtxCount)
				select matIdxEveryVertices
				;

			return qPalletIdx;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static class MaterialUtility
	{
			
		/// <summary>
		/// パーツごとに、それぞれのマテリアルハッシュ配列をすべてクエリする。
		/// </summary>
		public static IEnumerable<IEnumerable<(string name, int hash)>>
			QueryMatNameAndHash_EverySubmeshesEveryMeshes( IEnumerable<GameObject> gameObjects )
		{
			var qMatNameAndHashEveryParts =
				from pt in gameObjects
				select pt.GetComponent<MeshRenderer>()?.sharedMaterials into mats
				select
					from mat in mats.EmptyIfNull()
					select (mat.name, mat.GetHashCode())
					;

			return qMatNameAndHashEveryParts;
		}
			
		/// <summary>
		/// マテリアルの全列挙より重複削除しつつ index を採番し、mat hash -> index の対応辞書を生成する。
		/// </summary>
		public static Dictionary<int,int> ToDictionaryForMaterialHashToIndex
			( IEnumerable<IEnumerable<(string name,int hash)>> matNameAndHash_EverySubmeshesEveryMeshes )
		{
			var qOrderedHash =
				from mat in matNameAndHash_EverySubmeshesEveryMeshes
					.SelectMany( mats => mats )
					.Distinct( mat => mat.hash )
				orderby mat.name
				select mat.hash
				;
			return qOrderedHash
				.Select( (hash,i) => (hash,i) )
				.ToDictionary( x => x.hash, x => x.i )
				;
		}
			
	}
	
}
