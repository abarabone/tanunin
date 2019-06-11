using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Abss.StructureObject
{
	public class _StructurePartBase : MonoBehaviour, IStructurePart
	{
		public int PartId { get; private set; }

		public Task<Mesh> Build()
		{
			
			var meshElementTasks = 

		}

		public bool FallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
		{
			throw new System.NotImplementedException();
		}
		
	}

}

