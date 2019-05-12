using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;

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

		static public MeshElements BuildUnlitMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var (qMesh, qTf) = queryAboutMeshes( parts );
			
			return new MeshElements
			{
				Vertecies = buildVerteces( qMesh, qTf, tfBase ),
				Uvs = qMesh.SelectMany( x => x.uv ).ToList(),
				Indecies = buildIndeices( qMesh ),
			};
		}
		static public MeshElements BuildNormalMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var (qMesh, qTf) = queryAboutMeshes( parts );
			
			return new MeshElements
			{
				Vertecies = buildVerteces( qMesh, qTf, tfBase ),
				Uvs = qMesh.SelectMany( x => x.uv ).ToList(),
				Normals = buildNormals( qMesh, qTf, tfBase ),
				Indecies = buildIndeices( qMesh ),
			};
		}

		static public MeshElements
			BuildStructureWithPalletMeshElements( IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{
			var (qMesh, qTf) = queryAboutMeshes( parts );
			
			var qPartId = from pt in parts select pt.partId;
			var qMatArray = from pt in parts select pt.GetComponent<MeshRenderer>()?.sharedMaterials;

			var qPid = queryStructureIndecies( qMesh, qPartId );
			var qPallets = queryePallets( qMesh, qMatArray );
			var qPidPallet =
				from xy in Enumerable.Zip( qPid, qPallets, (x,y)=>(pid:x, pallet:y) )
				select new Color32
				(
					r:	(byte)xy.pid.int4Index, 
					g:	(byte)xy.pid.memberIndex, 
					b:	(byte)xy.pid.bitIndex, 
					a:	(byte)xy.pallet
				);
			
			return new MeshElements
			{
				Vertecies = buildVerteces( qMesh, qTf, tfBase ),
				Uvs = qMesh.SelectMany( x => x.uv ).ToList(),
				Normals = buildNormals( qMesh, qTf, tfBase ),
				Indecies = buildIndeices( qMesh ),
				Colors = qPidPallet.ToList(),
			};
		}

		static ( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf )
			queryAboutMeshes( IEnumerable<MonoBehaviour> parts )
		{
			var qMesh =
				from pt in parts
				select pt.GetComponent<MeshFilter>()?.sharedMesh ?? pt.GetComponent<SkinnedMeshRenderer>()?.sharedMesh
				;

			var qTf = from pt in parts select pt.transform;

			return (qMesh, qTf);
		}

		/// <summary>
		/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		static List<int> buildIndeices( IEnumerable<Mesh> qMesh )
		{
			var qVtxCount = qMesh
				.Select( mesh => mesh.vertexCount )
				.Scan( seed:0, (pre,cur) => pre + cur )
				;
			var qBaseVertex = Enumerable.Range(0,1).Concat( qVtxCount );// 先頭に 0 を追加する。

			var qIndex =
				from xy in Enumerable.Zip( qBaseVertex, qMesh, (x,y)=>(baseVtx:x, mesh:y) )
				from index in xy.mesh.triangles // mesh.triangles は、サブメッシュを地続きに扱う。
				select xy.baseVtx + index;

			return qIndex.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		static List<Vector3> buildVerteces( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf, Transform tfBase )
		{
			var mtBaseInv = tfBase.worldToLocalMatrix;

			var qVertex =
				from xy in Enumerable.Zip( qMesh, qTf, (x,y)=>(mesh:x, tf:y) )
				let mt = xy.tf.localToWorldMatrix * mtBaseInv
				from vtx in xy.mesh.vertices
				select mt.MultiplyPoint3x4( vtx )
				;
			
			return qVertex.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		static List<Vector3> buildNormals( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf, Transform tfBase )
		{
			var mtBaseInv = tfBase.worldToLocalMatrix;

			var qNormal =
				from xy in Enumerable.Zip( qMesh, qTf, (x,y)=>(mesh:x, tf:y) )
				let mt = xy.tf.localToWorldMatrix * mtBaseInv
				from nm in xy.mesh.normals ?? recalculateNormals_( xy.mesh )
				select mt.MultiplyVector( nm )
				;
			
			return qNormal.ToList();

			Vector3[] recalculateNormals_( Mesh mesh_ )
			{
				mesh_.RecalculateNormals();
				return mesh_.normals;
			}
		}

		static IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
			queryStructureIndecies( IEnumerable<Mesh> qMesh, IEnumerable<int> qPartId )
		{
			var qIndex =
				from xy in Enumerable.Zip( qMesh, qPartId, (x,y)=>(mesh:x, partId:y) )
				select (
					int4Index:		xy.partId >> 5 >>2,
					memberIndex:	xy.partId >> 5 & 0b_11,	// 0 ~ 3
					bitIndex:		xy.partId & 0b_1_1111	// 0 ~ 31
				);

			return qIndex;
		}
		static IEnumerable<int>
			queryePallets( IEnumerable<Mesh> qMesh, IEnumerable<Material[]> qMatArray )
		{
			var matToIndexDict = qMatArray
				.SelectMany( mat => mat )
				.Distinct()
				.Select( (mat,i) => (mat,i) )
				.ToDictionary( x => x.mat, x => x.i )
				;
			//var qPallet = null;

			return new int [0];
		}
	}
	
	public static class MeshCombiner2
	{


		static public Mesh BuildUnlitMeshFrom( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var qMesh = parts.Select( x => x.GetComponent<MeshFilter>().sharedMesh );
			var qTf = parts.Select( x => x.transform );

			var mesh = new Mesh();
			mesh.SetVertices( buildVerteces(qMesh,qTf,tfBase) );
			mesh.SetTriangles( buildIndeices(qMesh), submesh:0, calculateBounds:false );
			mesh.SetUVs( 0, qMesh.SelectMany( x => x.uv ).ToList() );

			return mesh;
		}

		static public (List<Vector3> vtxs, List<int> tris, List<Vector2> uvs)
			BuildUnlitMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var (qMesh, qTf) = queryAboutMeshes( parts );
			
			var vtxs = buildVerteces( qMesh, qTf, tfBase );
			var tris = buildIndeices( qMesh );
			var uvs = qMesh.SelectMany( x => x.uv ).ToList();

			return (vtxs, tris, uvs);
		}
		static public (List<Vector3> vtxs, List<Vector3> nms, List<int> tris, List<Vector2> uvs)
			BuildNormalMeshElements( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var (qMesh, qTf) = queryAboutMeshes( parts );
			
			var vtxs = buildVerteces( qMesh, qTf, tfBase );
			var nms = buildNormals( qMesh, qTf, tfBase );
			var tris = buildIndeices( qMesh );
			var uvs = qMesh.SelectMany( x => x.uv ).ToList();

			return (vtxs, nms, tris, uvs);
		}

		static public (List<Vector3> vtxs, List<Vector3> nms, List<int> tris, List<Vector2> uvs, List<Color32> cols)
			BuildStructureWithPalletMeshElements( IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{
			var (qMesh, qTf) = queryAboutMeshes( parts );
			
			var vtxs = buildVerteces( qMesh, qTf, tfBase );
			var nms = buildNormals( qMesh, qTf, tfBase );
			var tris = buildIndeices( qMesh );
			var uvs = qMesh.SelectMany( x => x.uv ).ToList();

			var qPartId = from pt in parts select pt.partId;
			var qMatArray = from pt in parts select pt.GetComponent<MeshRenderer>()?.sharedMaterials;

			var qPid = queryStructureIndecies( qMesh, qPartId );
			var qPallets = queryePallets( qMesh, qMatArray );
			var qPidPallet =
				from xy in Enumerable.Zip( qPid, qPallets, (x,y)=>(pid:x, pallet:y) )
				select new Color32( r:(byte)xy.pid.int4Index, g:(byte)xy.pid.memberIndex, b:(byte)xy.pid.bitIndex, a:(byte)xy.pallet );

			return (vtxs, nms, tris, uvs, qPidPallet.ToList() );
		}
		static public Mesh ToMesh( this (List<Vector3> vtxs, List<Vector3> nms, List<int> tris, List<Vector2> uvs, List<Color32> cols) e )
		{
			var mesh = new Mesh();
			mesh.SetVertices( e.vtxs );
			mesh.SetTriangles( e.tris, submesh:0, calculateBounds:false );
			mesh.SetUVs( 0, e.uvs );

			return mesh;
		}

		static ( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf )
			queryAboutMeshes( IEnumerable<MonoBehaviour> parts )
		{
			var qMesh =
				from pt in parts
				select pt.GetComponent<MeshFilter>()?.sharedMesh ?? pt.GetComponent<SkinnedMeshRenderer>()?.sharedMesh
				;

			var qTf = from pt in parts select pt.transform;

			return (qMesh, qTf);
		}

		/// <summary>
		/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		static List<int> buildIndeices( IEnumerable<Mesh> qMesh )
		{
			var qVtxCount = qMesh
				.Select( mesh => mesh.vertexCount )
				.Scan( seed:0, (pre,cur) => pre + cur )
				;
			var qBaseVertex = Enumerable.Range(0,1).Concat( qVtxCount );// 先頭に 0 を追加する。

			var qIndex =
				from xy in Enumerable.Zip( qBaseVertex, qMesh, (x,y)=>(baseVtx:x, mesh:y) )
				from index in xy.mesh.triangles // mesh.triangles は、サブメッシュを地続きに扱う。
				select xy.baseVtx + index;

			return qIndex.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		static List<Vector3> buildVerteces( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf, Transform tfBase )
		{
			var mtBaseInv = tfBase.worldToLocalMatrix;

			var qVertex =
				from xy in Enumerable.Zip( qMesh, qTf, (x,y)=>(mesh:x, tf:y) )
				let mt = xy.tf.localToWorldMatrix * mtBaseInv
				from vtx in xy.mesh.vertices
				select mt.MultiplyPoint3x4( vtx )
				;
			
			return qVertex.ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		static List<Vector3> buildNormals( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf, Transform tfBase )
		{
			var mtBaseInv = tfBase.worldToLocalMatrix;

			var qNormal =
				from xy in Enumerable.Zip( qMesh, qTf, (x,y)=>(mesh:x, tf:y) )
				let mt = xy.tf.localToWorldMatrix * mtBaseInv
				from nm in xy.mesh.normals ?? recalculateNormals_( xy.mesh )
				select mt.MultiplyVector( nm )
				;
			
			return qNormal.ToList();

			Vector3[] recalculateNormals_( Mesh mesh_ )
			{
				mesh_.RecalculateNormals();
				return mesh_.normals;
			}
		}

		static IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
			queryStructureIndecies( IEnumerable<Mesh> qMesh, IEnumerable<int> qPartId )
		{
			var qIndex =
				from xy in Enumerable.Zip( qMesh, qPartId, (x,y)=>(mesh:x, partId:y) )
				select (
					int4Index:		xy.partId >> 5 >>2,
					memberIndex:	xy.partId >> 5 & 0b_11,	// 0 ~ 3
					bitIndex:		xy.partId & 0b_1_1111	// 0 ~ 31
				);

			return qIndex;
		}
		static IEnumerable<int>
			queryePallets( IEnumerable<Mesh> qMesh, IEnumerable<Material[]> qMatArray )
		{
			var matToIndexDict = qMatArray
				.SelectMany( mat => mat )
				.Distinct()
				.Select( (mat,i) => (mat,i) )
				.ToDictionary( x => x.mat, x => x.i )
				;
			//var qPallet = null;

			return new int [0];
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

		static public bool IsReverseScale( ref Matrix4x4 mt )
		{

			//var up = Vector3.Cross( mt.GetColumn(0), mt.GetColumn(2) );

			//return Vector3.Dot( up, mt.GetColumn(1) ) > 0.0f;


			var up = Vector3.Cross( mt.GetRow(0), mt.GetRow(2) );
		
			return Vector3.Dot( up, mt.GetRow(1) ) > 0.0f;

		}

		static public IEnumerable<(int i0, int i1, int i2)> AsTriangleTupple( this int[] indexes )
		{
			for( var i=0; i<indexes.Length; i+=3 )
			{
				yield return ( indexes[i+0], indexes[i+1], indexes[i+2] );
			}
		}

	}

}

