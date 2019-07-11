using UnityEngine;
using System.Collections;

public class SpiderDefinition : _CharacterClassDefinition
{
	


	public enBodyType	bodyType;




	//public _Armor3	armor;

	//public float	outerArmor;

	//public float	innerArmor;



	//public _Wapon3	wapon;









	public override void init( _Action3Enemy action )
	{

		var act = (SpiderAction3)action;


		act.tf.localScale = Vector3.one * sizeScale;
		
		
	}




	public enum enBodyType
	{
		stringer,
		hunter
	}


}
