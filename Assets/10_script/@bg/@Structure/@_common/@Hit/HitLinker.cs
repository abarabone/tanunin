using UnityEngine;
using System.Collections;



public interface IHitLinker
{
	_Hit3 hitter { get; }
}


public class HitLinker : MonoBehaviour
{

	public _Hit3 hitter { get; private set; }


	protected void Awake()
	{
		
		hitter = GetComponentInParent<_Hit3>();

	}

}
