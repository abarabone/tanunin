using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Abss.StructureObject
{
	public class District : MonoBehaviour
	{

		async void Awake()
		{
			
			var parts = this.GetComponentsInChildren<_StructurePartBase>();
			
			var qBuildPartsTasks =
				from part in parts
				select part.BuildAsync()
				;

			await Task.WhenAll( qBuildPartsTasks );

		}

	}
}

