using UnityEngine;
using System.Collections;

public class BuildingPart3Basis : _StructurePart3Replicatable//_StructurePart3
// �����̊�b�����̔j��s�\�p�[�c�B�����b�̃R���N���[�g�ȂǁB
{


	
	protected override bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
	{
		
		//fallDownDestructionSurface( hitter, pressure, direction, point );

		return false;

	}


}
