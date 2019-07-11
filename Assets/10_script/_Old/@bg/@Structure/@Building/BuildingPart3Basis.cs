using UnityEngine;
using System.Collections;

public class BuildingPart3Basis : _StructurePart3Replicatable//_StructurePart3
// 建物の基礎部分の破壊不可能パーツ。柱や基礎のコンクリートなど。
{


	
	protected override bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
	{
		
		//fallDownDestructionSurface( hitter, pressure, direction, point );

		return false;

	}


}
