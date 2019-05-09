using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelGeometry;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		var parts = this.GetComponentsInChildren<test>();
        MeshCombiner.Combine( parts, this.transform ).position = Vector3.one;

		//this.gameObject.SetActive( false );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
