using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Abss.StructureObject
{
	public class StructureContentSource : MonoBehaviour, IStructureContent
	{

		private IStructurePart[]	parts;
		private GameObject			near;
		

		
		public GameObject GetNear()
		{
			if( this.near != null ) return this.near;

			return null;
		}

		public IStructurePart[] GetParts()
		{
			if( this.parts != null ) return this.parts;

			return null;
		}
	}
}
