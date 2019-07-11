using UnityEngine;
using System.Collections;

public abstract class _StructurePart3Replicatable : _StructurePart3
{

	
	//protected int	refCount;
	// 別に参照カウントつけなくてもいいかもとも思う、ピーク時（面開始時）の量が減らせるわけじゃないんだし。
	// 　雛形が残り続けても問題ないし、複雑さと参照カウント分のメモリを増やすだけかも…。

	/*
	public void setRefCount( int c )
	{
		refCount = c;
	}
	*/

	
	protected override _StructurePart3 getDestructionSurfaceInstance( Transform tfParent )
	{

		var tf = transform;

		var pos = tfParent.TransformPoint( tf.position );
		
		var rot = tfParent.rotation * tf.rotation;
		
		
		//if( refCount-- > 0 )
		{
			
			var partInstance = (_StructurePart3)Instantiate( this, pos, rot );

			partInstance.transform.localScale.Scale( tfParent.lossyScale );//

			return partInstance;
			
		}
		/*else
		{
			
			tf.position = pos;
			
			tf.rotation = rot;
			
			return this;
			
		}*/

	}


}
