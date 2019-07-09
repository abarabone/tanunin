using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Abss.StructureObject
{
	public class StructureContent : MonoBehaviour, IStructureContent
	{

		GameObject	near;
		

		
		public async Task<GameObject> GetOrBuildNearAsync()
		{
			if( this.near != null ) return this.near;


			var parts = this.GetComponentsInChildren<_StructurePartBase>();
			
			this.near = await StructureNearObjectBuilder.BuildNearObjectAsync( parts, this.transform );
			
			return this.near;
		}
	}
}

