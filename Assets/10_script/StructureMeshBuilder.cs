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
	
	public class MeshCombiner
	{

		public void combine( _StructurePartBase[] parts )
		{
			var qMesh =
				from pt in parts
				select pt.GetComponent<MeshFilter>().sharedMesh
				;
			var qTf =
				from pt in parts
				select pt.transform
				;


		}
		/// <summary>
		/// 各パーツメッシュの持つインデックスをすべて結合し、ひとつの配列にして返す。
		/// その際各メッシュの頂点数は、次のインデックスのベースとなる。
		/// また、マテリアル別のサブメッシュも、ひとつに統合される。
		/// </summary>
		int[] buildIndeices_( IEnumerable<Mesh> qMesh )
		{
			var qVtxCount = qMesh
				.Select( mesh => mesh.vertexCount )
				.Scan( seed:0, (pre,cur) => pre + cur )
				;
			var qBaseVertex = Enumerable.Range(0,1).Concat( qVtxCount );// 先頭に 0 を追加する。

			var qIndex =
				from xy in Enumerable.Zip( qBaseVertex, qMesh, (x,y)=>(baseVtx:x,mesh:y) )
				from index in xy.mesh.triangles // mesh.triangles は、サブメッシュを地続きに扱う。
				select xy.baseVtx + index;

			return qIndex.ToArray();
		}
		Vector3[] buildVerteces_( IEnumerable<Mesh> qMesh, IEnumerable<Transform> qTf )
		{
			var qVertex =
				from xy in Enumerable.Zip( qMesh, qTf, (x,y)=>(mesh:x, tf:y) )
				select xy.tf.tran
			return null;
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

