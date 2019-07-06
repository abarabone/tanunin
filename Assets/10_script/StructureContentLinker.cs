using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Abss.StructureObject
{
	public class StructureContentLinker : MonoBehaviour, IStructureContent
	{
		public StructureContentSource	Source;
		
		public Task<GameObject> GetOrBuildNearAsync() => this.Source.GetOrBuildNearAsync();		
	}
}

