using UnityEngine;
using System.Collections;

public class BuildingBinder3 : MonoBehaviour
{


	Building3[]	bodys;


	void Awake()
	{
		
		bodys = GetComponentsInChildren<Building3>();

		foreach( var body in bodys )
		{

			body.binder = this;

		}
		
	}
	
	public void bindOff()
	{
		
	//	var tf = transform;
		
	//	var bodys = GetComponentsInChildren<Building3>();
		
		
		foreach( var body in bodys )
		{
			
			if( body != null )// 破壊されているかチェック
			{
				
				body.binder = null;
				
			//	body.tf.parent = tf.parent;
				
				body.startToMove();
				
			}
			
		}
		
		
		Destroy( gameObject );
		
	}
}
