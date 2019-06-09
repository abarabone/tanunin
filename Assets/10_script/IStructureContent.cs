using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abss.StructureObject
{
	public interface IStructureContent
	{
		
		GameObject GetNear();
		
		IStructurePart[] GetParts();

	}
}
