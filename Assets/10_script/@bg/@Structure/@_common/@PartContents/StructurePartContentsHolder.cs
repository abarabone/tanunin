using UnityEngine;
using System.Collections;

public class StructurePartContentsHolder : _StructurePartContents
// プレハブではなく、直にヒエラルキービュー上のパーツをコンテンツとして所持する。
// パーツが _StructurePart3 なら直にパーツを持つが、_Structure3PartReplicatable なら複製可能のパーツ雛形となる。
{


	public override StructureNearObjectBuilder build( Transform tfStructureRoot )
	{

		var holderObject = getContentsObject();


		var nearBuilder = new StructureNearObjectBuilder();

		nearBuilder.build( holderObject, GM.shaders, tfStructureRoot );

		
		flatParentingAllParts( nearBuilder.parts );

		
		return nearBuilder;

	}


	public override void clean()
	{
		Destroy( this );

		gameObject.SetActive( false );
	}


	public override GameObject getContentsObject()
	{
		return gameObject;
	}



	// ---------------------

	void flatParentingAllParts( _StructurePart3[] parts )
	{
		var tf = getContentsObject().transform;

		foreach( var part in parts )
		{
			part.transform.SetParent( tf, true );
		}
	}



}
