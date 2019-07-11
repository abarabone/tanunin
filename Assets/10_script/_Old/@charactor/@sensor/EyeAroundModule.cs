using UnityEngine;
using System.Collections;


public class EyeAroundModule : _SensorModule
{

	public float	deadAngleTop;	// 上側死角 ※半角度ではなく全角度
	
	public float	deadAngleUnder;	// 下側死角 ※半角度ではなく全角度

	float	cosDeadTop;

	float	cosDeadUnder;


	



	new void OnEnable()
	{

		base.OnEnable();

		cosDeadTop = Mathf.Cos( Mathf.Deg2Rad * deadAngleTop * 0.5f );

		cosDeadUnder = -Mathf.Cos( Mathf.Deg2Rad * deadAngleUnder * 0.5f );
		
	}




	public override bool scan( ref TargetFinder finder, Collider[] cs )
	{

		//var sqrDists = base.sortNearDistance( cs, ref finder );
		base.shuffle( cs );// むしろシャッフルで

		
		for( var i = 0; i < cs.Length; i++ )
		{

			var irb = cs[ i ].attachedRigidbody;

			if( irb == finder.owner.rb ) continue;


			var act = irb.GetComponent<_Hit3>().getAct();//_Action3>();

			if( perceive( ref finder, act, scanRefreshTime ) ) return true;

		}

		return false;
	}


	public override bool capture( ref TargetFinder finder, _Action3 act )
	{
		return perceive( ref finder, act, captureRefreshTime );
	}


	bool perceive( ref TargetFinder finder, _Action3 act, float refreshTime )
	{

		var senspos = finder.tfSensor.position;

		var targpos = act.tfObservedCenter.position;
		
		var line = targpos - senspos;

		
		var sqrDist = line.sqrMagnitude;

		if( sqrDist > distance * distance ) return false;


		var dist = Mathf.Sqrt( sqrDist );


		if( isLookable( ref finder, line, dist ) )
		{

			var isVisible = !Physics.Linecast( senspos, targpos, UserLayer.sensorEyeOcculusion );
			// 遮蔽チェック

			if( isVisible )
			{
				var hateRate = 1.0f - dist / distance;

				finder.target.keep( finder.owner.character, act, hateUpRatio * hateRate, refreshTime, errRadius );

				return true;
			}

		}

		return false;
	}


	bool isLookable( ref TargetFinder finder, Vector3 line, float dist )
	{
		
		if( deadAngleTop == 0.0f & deadAngleUnder == 0.0f ) return true;


		var dir = line / dist;


		var up = finder.tfSensor.up;

		var cosAngle = Vector3.Dot( up, dir );


		return cosAngle <= cosDeadTop & cosAngle >= cosDeadUnder;

	}

}
