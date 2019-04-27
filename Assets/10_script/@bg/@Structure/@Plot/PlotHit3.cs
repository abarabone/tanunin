using UnityEngine;
using System.Collections;

public class PlotHit3 : _StructureHit3
{


	// 初期化・更新 ----------------------------

	public override void init( StructureNearObjectBuilder builder )
	{

		base.init( builder );


		durability = (float)structure.parts.Length;

	}

	// --------------------


}
