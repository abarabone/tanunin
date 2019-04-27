using UnityEngine;
using System.Collections;

public class Hit3Effective : Hit3BodyPart
{



	
	public float		limit;
	
	protected	float	life;

	
	public bool	isBroken	{ get { return life <= 0.0f; } }


	
	
	
	
	
	
	
	
	
	
	
	
	// 初期化 -------------------------------
	
	
	protected void OnEnable()
	{
		
		life = limit;
		
	}

}
