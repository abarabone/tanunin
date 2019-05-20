using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Abss.StructureObject
{
	public class StructureContentSource : MonoBehaviour
	{

		private IStructurePart[]	parts;
		private GameObject			near;
		
		public IStructurePart[] GetOrCreateParts()
		{
			if( this.parts == null )
			{
				this.parts = this.GetComponentsInChildren<IStructurePart>( includeInactive:true );
			}

			return this.parts;
		}

		public GameObject GetOrCreateNearObject()
		{
			if( this.near == null )
			{
				this.near = null;
			}

			return this.near;
		}
		
	}
}
