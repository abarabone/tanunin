using UnityEngine;
using System.Collections;

public class BuildingHit3 : _StructureHit3//Templatable
{


	// 初期化・更新 ----------------------------

	public override void init( StructureNearObjectBuilder builder )
	{

		base.init( builder );


		durability = (float)structure.parts.Length * 0.5f;

	}

	// --------------------




	void checkMoving( Building3 building )
	{

		if( building.binder != null )
		{
			building.binder.bindOff();
		}
		else
		{
			building.startToMove();
		}

	}





	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, _Action3 attacker )
	{
		
		base.blasted( ref ds, pressure, boringFactor, center, radius, collider, attacker );


		var building = (Building3)structure;


		checkMoving( building );


		building.rb.AddForceAtPosition( (building.rb.worldCenterOfMass - center).normalized * pressure * 10.0f, center, ForceMode.Impulse );

	}



}
