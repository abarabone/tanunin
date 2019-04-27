using UnityEngine;
using System.Collections;

public class _StructureSwitchOfDetail3 : _Structure3
{

	public GameObject	far		{ get; private set; }
	


	public override void deepInit()
	{
		
		base.deepInit();

		far = findOutlineObject();

	}



	public override void build()
	{

		base.build();


		var envelope	= tf.findWithLayerInDirectChildren( UserLayer._bgEnvelope ).gameObject;
		
		envelope.layer = UserLayer._bgSleepEnvelope;

	}

	
	public override void switchToNear()
	{
		far.SetActive( false );

		near.SetActive( true );
	}
	
	public override void switchToFar()
	{
		near.SetActive( false );

		far.SetActive( true );
	}

}
