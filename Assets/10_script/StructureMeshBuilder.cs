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

		static public Transform Combine( IEnumerable<MonoBehaviour> parts, Transform tfBase )
		{
			var qMesh = parts.Select( x => x.GetComponent<MeshFilter>().sharedMesh );
			var qTf = parts.Select( x => x.transform );

			var mesh = new Mesh();
			mesh.SetVertices( buildVerteces(qMesh,qTf,tfBase) );
			mesh.SetTriangles( buildIndeices(qMesh), submesh:0, calculateBounds:false );
			mesh.SetUVs( 0, qMesh.SelectMany( x => x.uv ).ToList() );

			//var mat = new Material(  );
			
			var go = new GameObject("new");
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			go.AddComponent<MeshRenderer>();//.sharedMaterial = 

			return go.transform;
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
			var qVertex =
				from xy in Enumerable.Zip( qMesh, qTf, (x,y)=>(mesh:x, tf:y) )
				from vtx in xy.mesh.vertices
				select xy.tf.TransformPoint( vtx ) into wvtx
				select tfBase.InverseTransformPoint( wvtx )
				;
			
			return qVertex.ToList();
		}
	}

	public static class MeshUtility
	{

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

