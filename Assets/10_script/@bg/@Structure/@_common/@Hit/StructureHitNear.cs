using UnityEngine;
using System.Collections;

public class StructureHitNear : _Hit3
{

	public Transform tfStructure { get; private set; }

	public _StructureHit3	parentHitter { get; private set; }


	/// <summary>
	/// 適当なボーン位置を返す。
	/// </summary>
	public override Transform getRandomBone()
	{
		return tfStructure;
	}











}
