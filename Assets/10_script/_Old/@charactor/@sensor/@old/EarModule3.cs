using UnityEngine;
using System.Collections;

public class EarModule3 : _SensorModule3
{


	public float	threshold;	// 聞こえる音の閾値。人間の通常状態（立ち）を 1.0f とする。loudness は、種別を超えた対象の興味度を加味した値とする。


	
	public override int scan( TargetFinder3 finder, _Action3[] others, float[] sqrDists )
	{
		
		var sqrLimitDist = distance * distance;
	

		var ires = -1;

		var resLoudness = 0.0f;


		for( var i = 0; i < others.Length; i++ )
		{
			
			if( sqrDists[i] <= sqrLimitDist )
			{
				
				if( capture( finder, others[i] ) && others[i].output.loudness > resLoudness )// 音は全候補で最大のものを返す。
				{

					ires = i;

					resLoudness = others[i].output.loudness;

				}
				
			}
			else
			{

				// これ以上遠いものは打ち切ってＯＫ

				break;

			}
			
		}
		
		return ires;
		
	}

	public override bool capture( TargetFinder3 finder, _Action3 target )
	{

		return target.output.loudness >= threshold;// * finder.sensitiveRateR;

	}
	
	public override bool concentrate( TargetFinder3 finder )
	{

		return false;
		
	}
	
	
}
