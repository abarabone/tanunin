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
			
			var a = from aa in this.GetComponentsInChildren<Renderer>() select aa.gameObject;
			Geometry.TexturePacker.PackTextureAndTranslateUv( a );

			var structures = this.GetComponentsInChildren<_StructureBase>();
			
			await Task.WhenAll( from st in structures select st.BuildAsync() );

		}

	}
}

