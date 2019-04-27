using UnityEngine;
using System.Collections;

public abstract class _SensorModule3 : ScriptableObject
{


	
	public float	distance;	// 索敵範囲

	public float	errRadius;	// 捕捉した後の誤差半径

//	public float	hate;



	public GameObject hint;



	public virtual int scan( TargetFinder3 finder, _Action3[] others, float[] sqrDists )
	{

		return -1;

	}


	public virtual bool capture( TargetFinder3 finder, _Action3 target )
	{

		return false;

	}


	public virtual bool concentrate( TargetFinder3 finder )
	{

		return false;

	}

}
