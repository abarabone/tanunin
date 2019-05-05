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

			var qPartMesh =
				from part in parts
				select
				(
					mesh	: part.GetComponent<MeshFilter>().sharedMesh,
					tf		: part.transform
				)
				;
			var qBaseVtx = Enumerable.Range(0,1).Concat
				(
					qPartMesh
						.Select( x => x.mesh.vertexCount )
						.Scan( seed:0, (pre,cur) => pre + cur )
				);

			var qIndex =
				from x in Enumerable.Zip( qBaseVtx, qPartMesh, (x,y)=>(baseVtx:x,partMesh:y) )
				from index in x.partMesh.mesh.triangles
				select index + x.baseVtx;

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

