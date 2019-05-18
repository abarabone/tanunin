using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelGeometry;
using System.Linq;
using System.Threading.Tasks;

public class test : _StructurePartBase
{
	[SerializeField] Material mat;

    async void Start()
    {
		var parts = this.GetComponentsInChildren<test>();
        var results = await Task.WhenAll( Task.Run(MeshCombiner.BuildNormalMeshElements( parts, this.transform )) );
		results
			.First()
			.CreateUnlitMesh().ToWriteOnly()
			.AddToNewGameObject( mat )
			.transform.position = Vector3.one;
    }
	
}
