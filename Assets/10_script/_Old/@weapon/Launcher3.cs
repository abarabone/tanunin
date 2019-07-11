using UnityEngine;
using System.Collections;

public class Launcher3 : _Weapon3
{


	float	speed;

	const float	g = 9.8f;


	/*
	public override void build()
	{

		foreach( var unit in units )
		{

			var fire = unit as _FireUnit3;

			if( fire != null )
			{
 
				fire.bullet.

			}

		}

	}
	*/
	public override void muzzleLookAt( Vector3 line )//, int unitId )
	{

		speed = 30.0f;//


		var t2 = line.sqrMagnitude / ( speed * speed );

		var height = t2 * g;


		tfMuzzle.rotation = Quaternion.LookRotation( line + Vector3.up * height );

	}





}
