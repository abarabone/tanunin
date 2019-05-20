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
		
		public IStructurePart[] Parts
		{
			get
			{
				if( this.parts != null ) return this.parts;
				
				this.parts = this.GetComponentsInChildren<IStructurePart>( includeInactive:true );
				return this.parts;
			}
		}

		public GameObject Near
		{
			get
			{
				if( this.near != null ) return this.near;
				
				this.near = 
				return this.near;
			}
			
		}
		
	}
}
