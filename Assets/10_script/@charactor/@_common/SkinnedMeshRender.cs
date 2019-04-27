using UnityEngine;
using System.Collections;
using System.Linq;

public class SkinnedMeshRender : MonoBehaviour
{

	public MeshRenderer	mr;//			{ private get; set; }
	
	public MeshFilter	mf;//			{ private get; set; }
	
	public Transform[]	bones;//		{ private get; set; }
	
//	public Matrix4x4[]	bindposes;//	{ private get; set; }
	
	
//	public Animator		anim;//		{ private get; set; }
	
//	public Transform	tfBody;//		{ private get; set; }
	
	


	public void init( Transform[] bns, /*Matrix4x4[] bps, */MeshRenderer inmr, MeshFilter inmf )
	{
		
		mr = inmr;
		
		mf = inmf;
		
		bones = bns;
		
	//	bindposes = bps;
		
	//	tfBody = transform.parent;
		
	//	anim = tfBody.GetComponent<Animator>();
		
		
	//	anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		
	}
	

	public void switchBones( Transform[] tfNewBones )
	{

		bones = tfNewBones;

	}


	protected void LateUpdate()
	{
		
		if( mr.isVisible )
		{
			var mts = MatrixPool.GetList();

			mts.AddRange( bones.Select( bone => ( bone != null ) ? bone.localToWorldMatrix : Matrix4x4.identity ) );

			mr.material.SetMatrixArray( ShaderId.Matrix, mts );
		}
		
	}

	///// <summary>
	///// 自前モーションで使用（現在は未使用）
	///// </summary>
	///// <param name="mts"></param>
	//public void update( Matrix4x4[] mts )
	//{
	//	if( mr.isVisible )
	//	{
	//		mr.material.SetMatrixArray( ShaderId.Matrix, mts );
	//	}
	//}

	///// <summary>
	///// 自前モーションで使用（現在は未使用）
	///// </summary>
	///// <param name="postures"></param>
	//public void update( MotionPlayer.PostureUnit[] postures )
	//{
	//	if( mr.isVisible )
	//	{
	//		var mts = MatrixPool.GetList();

	//		mts.AddRange( postures.Select( p => Matrix4x4.TRS( p.localPosition, p.localRotation, p.localScale ) ) );
			
	//		mr.material.SetMatrixArray( ShaderId.Matrix, mts );
	//	}
	//}

/*	
	protected void OnBecameVisible()
	{//Debug.Log( gameObject );
		anim.enabled = true;
	}
	protected void OnBecameInvisible()
	{
		anim.enabled = false;
	}
*/	
	
	
}
