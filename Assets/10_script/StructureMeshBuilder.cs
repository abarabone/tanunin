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
	
	public static class MeshCombiner
	{

		static public GameObject AddToNewGameObject( this Mesh mesh, Material mat )
		{
			var go = new GameObject();
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			go.AddComponent<MeshRenderer>().sharedMaterial = mat;

			return go;
		}

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
		static public Mesh BuildNormalMeshFrom( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var qMesh = parts.Select( x => x.GetComponent<MeshFilter>().sharedMesh );
			var qTf = parts.Select( x => x.transform );
			
			var mesh = new Mesh();
			mesh.SetVertices( buildVerteces(qMesh,qTf,tfBase) );
			mesh.SetNormals( buildNormals(qMesh,qTf,tfBase) );
			mesh.SetTriangles( buildIndeices(qMesh), submesh:0, calculateBounds:false );
			mesh.SetUVs( 0, qMesh.SelectMany( x => x.uv ).ToList() );

			return mesh;
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
	}

	public static class MeshUtility
	{

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

