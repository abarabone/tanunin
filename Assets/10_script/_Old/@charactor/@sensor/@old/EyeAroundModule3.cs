using UnityEngine;
using System.Collections;


public class EyeAroundModule3 : _SensorModule3
{


	





	
	public override int scan( TargetFinder3 finder, _Action3[] others, float[] sqrDists )
	{

		var sqrLimitDist = distance * distance;

		
		for( var i = 0; i < others.Length; i++ )
		{
			
			if( sqrDists[i] <= sqrLimitDist )
			{

				if( capture( finder, others[i] ) )
				{
					return i;
				}
				
			}
			else
			{
				
				// これ以上遠いものは打ち切ってＯＫ

				break;
				
			}
			
		}
		
		return -1;

	}
	
	
	public override bool capture( TargetFinder3 finder, _Action3 target )
	{

		var visible = false;


		var senspos = finder.tfSensor.position;

		var targpos = target.tfObservedCenter.position;


		visible = !Physics.Linecast( senspos, targpos, UserLayer.sensorEyeOcculusion );


		return visible;
		
	}
	
	public override bool concentrate( TargetFinder3 finder )
	{
		
		return false;
		
	}


}
