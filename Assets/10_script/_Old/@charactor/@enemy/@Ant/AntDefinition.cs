using UnityEngine;
using System.Collections;

public class AntDefinition : _CharacterClassDefinition
{
	


	public enBodyType	bodyType;




	//public _Armor3	armor;

	//public float	outerArmor;

	//public float	innerArmor;



	//public _Wapon3	wapon;









	public override void init( _Action3Enemy action )
	{

		var act = action as AntAction3;//(AntAction3)action;

		if( act )
		{
			act.wriggler.tfHead.localScale = headScales[ (int)bodyType ];

			act.wriggler.tfWeist.localScale = hipScales[ (int)bodyType ];
		}
		
		action.tf.localScale = Vector3.one * sizeScale;
		
	}




	public enum enBodyType
	{
		normal,
		strong
	}

	static readonly Vector3[]	headScales =
	{
		new Vector3( 1.0f, 1.0f, 1.0f ),
		new Vector3( 0.8f, 0.8f, 0.9f )
	};

	static readonly Vector3[]	hipScales =
	{
		new Vector3( 1.0f, 1.0f, 1.0f ),
		new Vector3( 0.8f, 0.8f, 0.9f )
	};

}
