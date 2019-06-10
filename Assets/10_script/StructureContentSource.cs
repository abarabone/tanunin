using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Abss.StructureObject
{
	public class StructureContentSource : MonoBehaviour, IStructureContent
	{

		GameObject	near;
		

		
		public GameObject GetOrBuildNear()
		{
			if( this.near != null ) return this.near;

			var parts = this.GetComponentsInChildren<_StructurePartBase>();

			this.near = StructureNearObjectBuilder.BuildNearObject( parts );
			
			return this.near;
		}

	}
}
