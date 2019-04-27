using UnityEngine;
using System.Collections;

public class BuildingNearSwitch : MonoBehaviour
{
	
	void OnTriggerEnter( Collider otherCollider )
	{
		
	//	if( otherCollider.gameObject.layer == UserLayer.bgEnvelope )
		{
			var	s	= otherCollider.attachedRigidbody.GetComponent<_Structure3>();

			if( s != null ) s.switchToNear();//if( s != null && s.name == "plot hodou (4)" ) Debug.Log(s.name);
		}
		
	}
	
}
