using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abss.StructureObject;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Abss.Common.Extension;
using Abss.Geometry;

public class test : _StructurePartBase
{
	[SerializeField] Material mat;

    async void Start()
    {
		var parts = this.GetComponentsInChildren<test>();
		
		var tex = Abss.Geometry.TexturePacker.PackTextureAndTranslateUv( from x in parts select x.gameObject );

		var results = await Task.WhenAll( Task.Run( MeshCombiner.BuildStructureWithPalletMeshElements( parts, this.transform ) ) );
		//var parts = from x in this.GetComponentsInChildren<test>() select x.gameObject;
		//var results = await Task.WhenAll( Task.Run( MeshCombiner.BuildNormalMeshElements( parts, this.transform ) ) );

		mat.mainTexture = tex;

		var go = results
			.First()
			.CreateUnlitMesh().ToWriteOnly()
			.AddToNewGameObject( mat );
		
		go.transform.position = Vector3.one;

		//var mpb = new MaterialPropertyBlock();
		//var r = go.GetComponent<Renderer>();
		//var vs = new Vector4[] { new Vector4(0b_1111_1111_1111_1111_1111_1111,0,0,0) };
		//Debug.Log(vs[0].x);
		//Debug.Log(vs[0].x.AsInt());
		////mpb.SetVectorArray( "isVisibleFlags", vs );
		//var c = new Vector4[] { new Vector4(1,0,0,1), new Vector4(1,1,0,1), new Vector4(1,1,1,1), new Vector4(0,1,1,1) };
		//mpb.SetVectorArray( "_Pallet", c );
		////var a = mpb.GetVectorArray( "isVisibleFlags" )[ 0 ];
		////Debug.Log( a.x.AsInt() );
		//mpb.SetTexture( "_MainTex", mat.GetTexture("_MainTex") );
		////r.SetPropertyBlock( mpb );

		var smrc = go.AddComponent<StructureNearRenderingController>();
		smrc.SetVisibilityFlags( Enumerable.Repeat( -1, 4 * 8 ).ToArray() );
	}

}

