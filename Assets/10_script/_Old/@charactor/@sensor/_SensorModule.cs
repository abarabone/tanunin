using UnityEngine;
using System.Collections;

public abstract class _SensorModule : ScriptableObject
{



	public float	distance;	// 索敵範囲

	public float	errRadius;	// 捕捉した後の誤差半径

	public float	hateUpTime;	// だいたい何秒でヘイトがたまるか（スキャンとキャプチャの時間の平均で割られる）

	public float	hateUpRatio { get; private set; }


	public float	scanRefreshTime;

	public float	captureRefreshTime;


	public GameObject hint;//



	public abstract bool scan( ref TargetFinder finder, Collider[] cs );


	public abstract bool capture( ref TargetFinder finder, _Action3 act );



	protected void OnEnable()
	{

		hateUpRatio = 1.0f / hateUpTime;
		
	}



	protected void shuffle( Collider[] cs )
	{
		var max = cs.Length - 1;

		for( var i = 0 ; i < cs.Length ; i++ )
		{
			var j = Random.Range( 0, max );

			var tmp = cs[ j ];

			cs[ j ] = cs[ i ];

			cs[ i ] = tmp;
		}
	}

	protected float[] sortNearDistance( Collider[] cs, ref TargetFinder finder )
	{

		var sqrDists = new float[ cs.Length ];

		var myCenter = finder.tfSensor.position;


		for( var i = 0 ; i < cs.Length ; i++ )
		{

			var rbOhter = cs[ i ].attachedRigidbody;


			if( rbOhter != finder.owner.rb )
			{

				var otherCenter = rbOhter.GetComponent<_Action3>().tfObservedCenter.position;

				sqrDists[ i ] = ( otherCenter - myCenter ).sqrMagnitude;

			}
			else
			{

				sqrDists[ i ] = float.PositiveInfinity;

			}

		}


		System.Array.Sort( sqrDists, cs );


		return sqrDists;

	}

	protected bool isIn( ref TargetFinder finder, _Action3 other )
	{
		return ( other.tfObservedCenter.position - finder.tfSensor.position ).sqrMagnitude <= distance * distance;
	}

}
