using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelGeometry
{
	public interface IStructurePart
	{
		
		void Init();

		void Reset();


		void Build();

		bool FallDown( _StructureHit3 hitter, Vector3 force, Vector3 point );

	}
}
