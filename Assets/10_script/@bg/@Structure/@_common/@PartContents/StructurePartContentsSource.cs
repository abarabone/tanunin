using UnityEngine;
using System.Collections;

public class StructurePartContentsSource : StructurePartContentsHolder
// パーツ集合のプレハブへのリンクを保存するプレハブ。
// 複数の ContentsLinker から参照され、一つだけインスタンス化し、テクスチャをパックする。
// _Structure3.build() 時に一度だけ this.build() し、ひな形となる。
{


	public GameObject[]	sources;	// コンテンツパートプレハブへのリンク。エディタ上で設定する。


	GameObject templateObject;
	// 雛形インスタンスへのリンク。
	// 雛形インスタンスは、パートプレハブ (sources) をすべてインスタンス化してそれを所持する。




	StructureNearObjectBuilder	templateNearBuilder;




	public override StructureNearObjectBuilder build( Transform tfStructureRoot )
	{
		if( templateNearBuilder == null )
		{// Debug.Log( tfStructureRoot.name+" build" );
			templateNearBuilder = base.build( tfStructureRoot );
		}
		else
		{// Debug.Log(tfStructureRoot.name+" duplicate");
			templateNearBuilder.duplicateNear( tfStructureRoot );
		}

		return templateNearBuilder;
	}

	public override GameObject getContentsObject()
	{
		if( templateObject == null )
		{
			templateObject = instantiateTemplate();
		}

		return templateObject;
	}

	public override void clean()
	{
		if( templateNearBuilder != null )
		{
			//templateNearBuilder = null;

			//gameObject.SetActive( false );
			
			getContentsObject().SetActive( false );
		}
	}


	// ---------------

	private GameObject instantiateTemplate()
	{

		var go = new GameObject( "part contents holder" );


		go.SetActive( false );//

		var tfHolder = go.transform;

		foreach( var source in sources )
		{

			if( source == null ) continue;

			var tfChild = Instantiate( source.transform );

			tfChild.SetParent( tfHolder );

		}

		go.SetActive( true );//


		return go;

	}

}
