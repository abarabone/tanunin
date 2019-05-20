using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abss.StructureObject
{
	public interface IStructureContent
	{
		





		/// <summary>
		/// コンテンツ本体を取得する。
		/// </summary>
		/// <returns></returns>
		GameObject GetContentsObject();

		
		
		/// <summary>
		/// ビルドでは、使用側にとっては near オブジェクトが返れば問題ない。本体のパーツコンテンツの管理は隠ぺいできる。
		/// </summary>
		/// <param name="tfParent"></param>
		/// <returns></returns>
		StructureNearObjectBuilder Build( Transform tfParent );
		
		
		///// <summary>
		///// ビルドに使用したメモリとかオブジェクトとかのうち、プレイ中に不要のものは消してしまおうという魂胆。
		///// </summary>
		//void Clean();
		
	}
}
