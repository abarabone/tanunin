using UnityEngine;
using System.Collections;

public class EarModule : _SensorModule
{


	public float	threshold;	// 聞こえる音の閾値。人間の通常状態（立ち）を 1.0f とする。loudness は、種別を超えた対象の興味度を加味した値とする。



	public override bool scan( ref TargetFinder finder, Collider[] cs )
	{

		var resLoudness = threshold - float.Epsilon;

		_Action3	act = null;


		for( var i = 0; i < cs.Length; i++ )
		{

			var irb = cs[i].attachedRigidbody;

			if( irb == finder.owner.rb ) continue;


			var other = irb.GetComponent<_Hit3>().getAct();// _Action3>();


			if( other.output.loudness > resLoudness )
			// 音は全候補で最大のものを返す。
			{

				act = other;

				resLoudness = other.output.loudness;

			}
			
		}


		if( act != null )
		{
			var uphate = hateUpRatio * resLoudness;

			finder.target.keep( finder.owner.character, act, resLoudness, scanRefreshTime, errRadius );
		}

		return act != null;
	}

	public override bool capture( ref TargetFinder finder, _Action3 act )
	{

		var isHit = isIn( ref finder, act ) & ( act.output.loudness >= threshold );// * finder.sensitiveRateR;


		if( isHit )
		{
			var uphate = hateUpRatio * act.output.loudness;

			finder.target.keep( finder.owner.character, act, uphate, captureRefreshTime, errRadius );
		}

		return isHit;
	}

}
