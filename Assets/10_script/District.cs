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
			var tex = Geometry.TexturePacker.PackTextureAndTranslateUv( a );
			foreach( var r in this.GetComponentsInChildren<Renderer>() ) foreach( var m in r.materials ) m.mainTexture = tex;

			var structures = this.GetComponentsInChildren<_StructureBase>();
			
			await Task.WhenAll( from st in structures select st.BuildAsync() );

		}

	}
}

