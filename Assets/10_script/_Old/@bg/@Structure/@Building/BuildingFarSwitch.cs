using UnityEngine;
using System.Collections;

public class BuildingFarSwitch : MonoBehaviour
{
	/*
	PlayerAction3	act;
	
	void Awake()
	{
		act = GetComponent<Collider>().attachedRigidbody.GetComponent<PlayerAction3>();
	}
	*/


	void OnTriggerExit( Collider otherCollider )
	{
		
	//	if( otherCollider.gameObject.layer == UserLayer.bgEnvelope )
		{
			//if( !act.nowMoving() ) return;
			
			var	s	= otherCollider.attachedRigidbody.GetComponent<_Structure3>();
			
			if( s != null ) s.switchToFar();// if( s != null && s.name== "plot hodou (4)" ) Debug.Log( s.name );
		}
		
	}
}
