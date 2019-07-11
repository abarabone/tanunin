using UnityEngine;
using System.Collections;

public class BeamBullet3 : _LinearBullet3
{


	public float	damage;


	
	protected override void onHit( _Hit3 hitter, ref RaycastHit hit, Vector3 dir )
	{

		var landing = hitter.landings ? hitter.landings.fragmentation : GM.defaultLandings.fragmentation;
		//var landing = hitter.landings.fragmentation;
		
		landing.emit( hit.point, hit.normal );
		
		
		var ds = new DamageSourceUnit( damage, 0.8f, 1.0f, 0.0f, 0.0f ); 
		
		hitter.shot( ref ds, dir * damage, ref hit, owner );

	}


}