﻿using System.Collections;
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
			
			var qParts =
				from part in parts.Select( (x,i)=>(x,i) )
				select part.x.Build()
				;

			await Task.WhenAll( qParts );

		}

	}
}

