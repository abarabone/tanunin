using UnityEngine;
using System.Collections;

public class StructurePartContentsLinker : _StructurePartContents
// _Structure3 に所持され、コンテンツ本体への参照を持ち、橋渡しをする。
{

	public StructurePartContentsSource templatePrefab;




	public override StructureNearObjectBuilder build( Transform tfStructureRoot )
	{
		return templatePrefab.build( tfStructureRoot );
	}


	public override void clean()
	{
		Destroy( this );

		templatePrefab.clean();
	}


	public override GameObject getContentsObject()
	{
		return templatePrefab.getContentsObject();
	}

}
