using UnityEngine;
using System.Collections;




public class CombineSkinnedMesh
// ボーンの参照先の TF が、トリミング（削除）されてる場合はボーンから外す、ということをやってる
// よって、ボーン
{
	
	Mesh	mesh;
	
	BoneIdTrimmer	reBoneIds;
	
	
	
	public void combine( GameObject go )
	// 子のスキンメッシュを統合する
	{
		
		var smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
		
		combine( smrs );
		
	}
	
	public void combine( SkinnedMeshRenderer[] smrs )
	{
		
		var i = 0;
		
		var totalVtxLength = 0;
		
		var cis = new CombineInstance[ smrs.Length ];
		
		foreach( var smr in smrs )
		// メッシュ取りまとめ、頂点数計算
		{//Debug.Log( smr.name + smrs.Length );
			
			var srcmesh = smr.sharedMesh;
			
			cis[i++].mesh = srcmesh;
			
			totalVtxLength += srcmesh.vertexCount;
		//	Debug.Log( srcmesh.boneWeights.Length +" "+ totalVtxLength +" "+ srcmesh.vertexCount );
			
			smr.enabled = false;
		//	GameObject.Destroy( smr.gameObject );
		}
		
		
		reBoneIds.init( smrs[0] );
		
		var dstvtxs	= new Vector3[ totalVtxLength ];
		var dstbids = new Color32[ totalVtxLength ];
		var dstweis = new Vector2[ totalVtxLength ];
		
		i = 0;
		
		foreach( var smr in smrs )
		// ウェイト関係配列構築
		{
			
			var srcmesh = smr.sharedMesh;
			
			var srcweis = srcmesh.boneWeights;
		//	Debug.Log( srcweis.Length +" "+ totalVtxLength +" "+ srcmesh.vertexCount );
			
			for( var iv = 0 ; iv	< srcmesh.vertexCount ; iv++, i++ )
			{
				
				var bid0 = srcweis[iv].boneIndex0;
				var wei0 = srcweis[iv].weight0;
				
				var bid1 = srcweis[iv].boneIndex1;
				var wei1 = srcweis[iv].weight1;
				
				var weirate	= 1.0f / ( wei0 + wei1 );
				
				dstbids[i] = new Color32( reBoneIds[bid0], reBoneIds[bid1], 0, 0 );
				dstweis[i] = new Vector2( wei0 * weirate, wei1 * weirate );
				
				dstvtxs[i] = srcmesh.bindposes[bid0].MultiplyPoint3x4( srcmesh.vertices[iv] );
				
			}
			
		}
		
		
		
		var dstmesh = new Mesh();
		
		dstmesh.CombineMeshes( cis, true, false );
		
		dstmesh.vertices	= dstvtxs;
		dstmesh.uv2			= dstweis;
		dstmesh.colors32	= dstbids;
		
		dstmesh.RecalculateNormals();
		
		
		
		;
		
		
		mesh = dstmesh;
		
		
	}
	
	
	public void addRenderer( GameObject go, Shader shader )
	{
		var smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
		
		addRenderer( go, smrs, shader );
	}
	
	public void addRenderer( GameObject go, SkinnedMeshRenderer[] smrs, Shader shader )
	{
		
		var dstmat = new Material( smrs[0].sharedMaterial );
		dstmat.shader = shader;
		
		
		var dstbones = reBoneIds.getReBones( smrs[0].bones );
		
	//	Debug.Log( smrs[0].bones.Length +"=>"+ dstbones.Length );
		
		
		foreach( var smr in smrs )
		{
			GameObject.Destroy( smr.gameObject );
			smr.transform.parent = null;//
	//		smr.enabled = false;
		}
		
			
		var mr = go.AddComponent<MeshRenderer>();
		var mf = go.AddComponent<MeshFilter>();
	//	var sm = go.transform.parent.gameObject.AddComponent<SkinnedMeshRender>();
		var sm = go.AddComponent<SkinnedMeshRender>();
		
		mr.sharedMaterial	= dstmat;
		
		mf.sharedMesh		= mesh;
		
		sm.init( dstbones, mr, mf );
		
	}
	
}



struct BoneIdTrimmer
{
	
	public int	boneLength	{ get; private set; }
	
	byte[]	reIds;
	
	int[]	oldIds;
	
	
	public void init( SkinnedMeshRenderer smr )
	{
		
		reIds = new byte[ smr.bones.Length ];
		
		boneLength = 0;
		
		foreach( var bone in smr.bones ) if( bone ) boneLength++;
		
		oldIds = new int[ boneLength ];
		
		
		byte idstbone = 0;
		
		for( var ibone=0; ibone<smr.bones.Length; ibone++ )
		{

			if( smr.bones[ibone] )
			{
			
				reIds[ibone]		= idstbone;
				oldIds[idstbone]	= ibone;

				idstbone++;
			}
			else
			{

				reIds[ibone]		= 0;//-1;

			}

			//	Debug.Log( smr.bones[ibone] +"=>"+ oldIds[idstbone] );

		}
		
	}
	
	public byte this[ int i ]
	{
		get { return reIds[ i ]; }
	}
	
	public Transform[] getReBones( Transform[] srcBones )
	{
		var dstBones = new Transform[ boneLength ]; 
		
		for( var i=boneLength; i-->0; )
		{
			dstBones[ i ] = srcBones[ oldIds[i] ];
		}
		
		return dstBones;
	}
	
	
}




