using UnityEngine;
using System.Collections;

public class CablePiece3 : MonoBehaviour
{

	
	//public Transform	tfStartSide;
	
	//public Transform	tfEndSide;
	

/*	
	public void adjustTransform()
	{
		
		var tf = transform;

		tf.position = tfStartSide.position;


		var ray = tfEndSide.position - tfStartSide.position;


		var scale = tf.localScale;
		
		scale.z = ray.magnitude;
		
		tf.localScale = scale;
		
		
		tf.rotation = Quaternion.LookRotation( ray );
		
	}
*/
	public void adjustTransform( Vector3 start, Vector3 end )
	{
		
		var tf = transform;
		
		tf.position = start;
		
		
		var ray = end - start;
		
		
		var scale = tf.localScale;
		
		scale.z = ray.magnitude;
		
		tf.localScale = scale;
		
		
		tf.rotation = Quaternion.LookRotation( ray );
		
	}


}
