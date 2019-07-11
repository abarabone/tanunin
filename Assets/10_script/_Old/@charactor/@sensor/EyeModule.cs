using UnityEngine;
using System.Collections;


public class EyeModule : _SensorModule
{

	public float	directivity;	// 感覚の指向性角度 ※半角度ではなく全角度
	
	float	cosDirectivity;


	



	new void OnEnable()
	{

		base.OnEnable();

		cosDirectivity = Mathf.Cos( Mathf.Deg2Rad * directivity * 0.5f );
		
	}




	public override bool scan( ref TargetFinder finder, Collider[] cs )
	{

		//var sqrDists = base.sortNearDistance( cs, ref finder );
		base.shuffle( cs );// むしろシャッフルで

		
		for( var i = 0; i < cs.Length; i++ )
		{

			var irb = cs[ i ].attachedRigidbody;

			if( irb == finder.owner.rb ) continue;


			var act = irb.GetComponent<_Hit3>().getAct();// _Action3>();

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


		var forward = finder.tfSensor.forward;

		var sqrDist = ( targpos - senspos ).sqrMagnitude;
		
		if( sqrDist > distance * distance ) return false;


		var dist = Mathf.Sqrt( sqrDist );

		var dir = ( targpos - senspos ) / dist;
		
		if( Vector3.Dot( forward, dir ) >= cosDirectivity )// 視野チェック
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
	

}
