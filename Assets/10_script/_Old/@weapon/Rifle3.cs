using UnityEngine;
using System.Collections;

public class Rifle3 : _Weapon3
{

	public override void muzzleLookAt( Vector3 line )
	{

		tfMuzzle.rotation = Quaternion.LookRotation( line );

	}





}
