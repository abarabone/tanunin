using UnityEngine;
using System.Collections;

public class DarogaDefinition : _CharacterClassDefinition
{
	




	//public _Armor3	armor;

	//public float	outerArmor;

	//public float	innerArmor;



	//public _Wapon3	wapon;









	public override void init( _Action3Enemy action )
	{

		//var act = (DarogaAction3)action;
		//var act = (DarogaAction)action;


		action.tf.localScale = Vector3.one * sizeScale;
		

	}



}
