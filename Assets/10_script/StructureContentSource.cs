using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

namespace Abss.StructureObject
{
	public class StructureContentSource : MonoBehaviour, IStructureContent
	{

		GameObject	near;
		

		
		public async Task<GameObject> GetOrBuildNearAsync()
		{
			if( this.near != null ) return this.near;
			
			if( isPrefab_(this.gameObject) )
			{
				this.near = await Instantiate<StructureContentSource>( this ).GetOrBuildNearAsync();
				return this.near;
			}
			
			var parts = this.GetComponentsInChildren<_StructurePartBase>();
			this.near = await StructureNearObjectBuilder.BuildNearObjectAsync( parts, this.transform );
			return this.near;


			bool isPrefab_( GameObject go_ ) => PrefabUtility.GetPrefabAssetType(go_) != PrefabAssetType.NotAPrefab;
		}
	}
}
