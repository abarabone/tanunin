using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abss.StructureObject
{
	public class StructureContentSource : MonoBehaviour, IStructureContent
	{

		IStructureContent	instancedContent;
		
		
		public async Task<GameObject> GetOrBuildNearAsync()
		{
			if( this.instancedContent != null ) this.instancedContent = Instantiate<( this );

			return this.instancedContent.GetOrBuildNearAsync();
		}

	}
}
