using UnityEngine;
using System.Collections;

public class LandingList : ScriptableObject
{


	public _Emitter3	armorScrape;	// 装甲を削る

	public _Emitter3	bodyPiercing;	// 軟組織を貫通してしまう

	public _Emitter3	fragmentation;	// 軟組織を破壊する

	public _Emitter3	ricochet;		// 弾く 避弾経始

	public _Emitter3	armorPenetrate;	// 貫徹した
	
	public _Emitter3	erosion;		// 流体化貫徹


	public _Emitter3	flame;	// 炎

	public _Emitter3	erode;	// 酸

}
