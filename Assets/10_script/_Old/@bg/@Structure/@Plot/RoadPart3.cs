using UnityEngine;
using System.Collections;

public class RoadPart3 : PlotPart3
// ����͂����ĂȂ��ۂ��@���H�� PlotPart3 �g���Ă�Ǝv���@�p�~�\��H
{
	
	
	protected override bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
	{

		//var tf = transform;


		//var smoke = (GameObject)Instantiate( GM.roadBreakPrefab, tf.position, GM.roadBreakPrefab.transform.rotation );

		//Destroy( smoke, 1.0f );

		
		fallDownDestructionSurface( hitter );


		return true;

	}

}
