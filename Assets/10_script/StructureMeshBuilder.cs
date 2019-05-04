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
	
	public class MeshUtility
	{

		static public bool IsReverseScale( ref Matrix4x4 mt )
		{

			//var up = Vector3.Cross( mt.GetColumn(0), mt.GetColumn(2) );

			//return Vector3.Dot( up, mt.GetColumn(1) ) > 0.0f;


			var up = Vector3.Cross( mt.GetRow(0), mt.GetRow(2) );
		
			return Vector3.Dot( up, mt.GetRow(1) ) > 0.0f;

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

			var qIndex =
				from pm in qPartMesh
				select pm.


		}

	}
}

