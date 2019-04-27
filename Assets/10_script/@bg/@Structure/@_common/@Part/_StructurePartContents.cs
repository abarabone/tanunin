using UnityEngine;
using System.Collections;

public abstract class _StructurePartContents : MonoBehaviour
{



	public abstract GameObject getContentsObject();
	// コンテンツ本体を取得する。




	public abstract StructureNearObjectBuilder build( Transform tfParent );
	// ビルドでは、使用側にとっては near オブジェクトが返れば問題ない。本体のパーツコンテンツの管理は隠ぺいできる。
	


	public abstract void clean();
	// ビルドに使用したメモリとかオブジェクトとかのうち、プレイ中に不要のものは消してしまおうという魂胆。
	

}
