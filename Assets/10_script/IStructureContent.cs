using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Abss.StructureObject
{
	public interface IStructureContent
	{
		
		Task<GameObject> GetOrBuildNearAsync();
		
	}
}
