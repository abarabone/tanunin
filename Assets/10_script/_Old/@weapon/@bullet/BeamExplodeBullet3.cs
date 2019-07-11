using UnityEngine;
using System.Collections;

public class BeamExplodeBullet3 : _LinearBullet3
{


	public _Emitter3	subEmitObject;




	protected override void onHit( _Hit3 hitter, ref RaycastHit hit, Vector3 dir )
	{
		
		subEmitObject.emit( hit.point, hit.normal, owner );

	}




}
