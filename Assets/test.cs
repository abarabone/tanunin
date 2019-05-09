using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelGeometry;

public class test : MonoBehaviour
{
	[SerializeField] Material mat;

    void Start()
    {
		var parts = this.GetComponentsInChildren<test>();
        MeshCombiner.BuildNormalMeshFrom( parts, this.transform )
			.ToWriteOnly()
			.AddToNewGameObject( mat )
			.transform.position = Vector3.one;
    }
	
}
