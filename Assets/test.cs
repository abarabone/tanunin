using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelGeometry;
using System.Linq;
using System.Threading.Tasks;

public class test : _StructurePartBase
{
	[SerializeField] Material mat;

    void Start()
    {
		var parts = this.GetComponentsInChildren<test>();
        Task.WhenAll( MeshCombiner.BuildUnlitMeshElements( parts, this.transform ) ).Result.First()
			.CreateUnlitMesh().ToWriteOnly()
			.AddToNewGameObject( mat )
			.transform.position = Vector3.one;
    }
	
}
