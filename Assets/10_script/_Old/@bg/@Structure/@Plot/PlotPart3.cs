using UnityEngine;
using System.Collections;

public class PlotPart3 : _StructurePart3
{



	protected override bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
	{
		
		fallDownDestructionSurface( hitter, force, point );
		
		return true;

	}





}
