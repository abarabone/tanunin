using UnityEngine;
using System.Collections;

public class Sencor3Holder : ScriptableObject
{


	
	public _SensorModule3[]	sensors;



	public float	scanInterval;

	public float	captureInterval;


	public float	maxDistance	{ get; protected set; }




	void OnEnable()
	{

		initMaxDistance();

	}

	void initMaxDistance()
	{

		maxDistance = 0.0f;

		foreach( var m in sensors )
		{

			if( maxDistance < m.distance ) maxDistance = m.distance;

		}

	}


	

	public TargetFinder3.TargetInfoUnit scan( TargetFinder3 finder, _Action3[] others, float[] sqrDists )
	{

		foreach( var s in sensors )
		{

			var i =	s.scan( finder, others, sqrDists );

			if( i != -1 )
			{
				
				Destroy( Instantiate( s.hint, finder.tfSensor.position, finder.rb.rotation ), 1.0f );//
				return new TargetFinder3.TargetInfoUnit( others[i], s.errRadius );

			}

		}


		return new TargetFinder3.TargetInfoUnit();

	}

	
	public TargetFinder3.TargetInfoUnit capture( TargetFinder3 finder, _Action3 other, float sqrDist )
	{

		foreach( var s in sensors )
		{

			if( sqrDist <= s.distance * s.distance && s.capture( finder, other ) )
			{
				
				Destroy( Instantiate( s.hint, finder.tfSensor.position, finder.rb.rotation ), 1.0f );//
				return new TargetFinder3.TargetInfoUnit( other, s.errRadius );

			}

		}


		return new TargetFinder3.TargetInfoUnit();

	}

}
