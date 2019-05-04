using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelGeometry
{
	public class _StructurePartBase : MonoBehaviour, IStructurePart
	{
		public void Build()
		{
			throw new System.NotImplementedException();
		}

		public bool FallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
		{
			throw new System.NotImplementedException();
		}

		public void Init()
		{
			throw new System.NotImplementedException();
		}

		public void Reset()
		{
			throw new System.NotImplementedException();
		}
	}

}

