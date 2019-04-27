using UnityEngine;
using System.Collections;
using UniRx;

public class StructureBonedRenderer3 : MonoBehaviour
{



//	Mesh		mesh;

//	Material	material;


	MeshFilter		mf;

	MeshRenderer	mr;



	bool	scheduleedRecalculateBounds;
	// バウンディングの再計算をうながしているかどうか。何十秒かの猶予を持たせる。




public	_StructureInterArea3[]	structures;

public	int	structuresEntityLength;

	// ↑public はデバッグ用　本来は private

	struct LocationHolder
	{
		public Vector4[]   positions;
		public Vector4[]   rotations;
	}

	LocationHolder				location;

	ISubject<LocationHolder>	observable;


	public void init( Mesh m, Shader s, Texture t, _StructureInterArea3[] ss )
	{

	//	mesh = m;

		var mat	= new Material( s );
		
		mat.mainTexture	= t;

	//	material = mat;


		structures = ss;

		structuresEntityLength = structures.Length;


		var go = new GameObject( "combined area" );

		go.transform.parent = transform;

		mf = go.AddComponent<MeshFilter>();
		mr = go.AddComponent<MeshRenderer>();

		mf.sharedMesh		= m;
		mr.sharedMaterial	= mat;

	}

	public void initBounds( Bounds bounds )
	{

		mf.sharedMesh.bounds = bounds;

	}


	public void destroyChildStructure( int entId )
	{

		var last = structures[ --structuresEntityLength ];
		
		last.entityId = (short)entId;

		structures[ entId ] = last;

		structures[ structuresEntityLength ] = null;
		
	}



	public void setLocation( int partId, Vector3 pos, Quaternion rot, bool visible )
	{

		if( visible )
		{
			setLocationVisible( partId, pos, rot );
		}
		else
		{
			setLocationInvisible( partId );
		}

	}

	public void setLocationVisible( int partId, Vector3 pos, Quaternion rot )
	{

		//mr.sharedMaterial.SetVector( "t" + partId.ToString(), new Vector4( pos.x, pos.y, pos.z, 1.0f ) );

		//mr.sharedMaterial.SetVector( "r" + partId.ToString(), new Vector4( rot.x, rot.y, rot.z, rot.w ) );

		initLocation();

		location.positions[ partId ] = new Vector4( pos.x, pos.y, pos.z, 1.0f );
		location.rotations[ partId ] = new Vector4( rot.x, rot.y, rot.z, rot.w );

		observable.OnNext( location );

	}

	public void setLocationInvisible( int partId )
	{

		//mr.sharedMaterial.SetVector( "t" + partId.ToString(), Vector4.zero );

		initLocation();

		location.positions[ partId ] = Vector4.zero;
		
		observable.OnNext( location );

	}

	void initLocation()
	{
		if( location.positions == null )
		{
			location.positions = new Vector4[ structures.Length ];
			location.rotations = new Vector4[ structures.Length ];

			observable = new Subject<LocationHolder>();

			Observable.ZipLatest( observable, Observable.EveryUpdate(), ( vcs, _ ) => vcs )
				.Subscribe(
					vcs => {
						mr.sharedMaterial.SetVectorArray( ShaderId.Transrate, vcs.positions );
						mr.sharedMaterial.SetVectorArray( ShaderId.Rotation, vcs.rotations );
					} )
				.AddTo( gameObject );
		}
	}



	public bool isVisible()
	{
		return mr.isVisible;
	}

	public bool isOutOfBounds( Collider[] cols )
	{
		var areaBounds = mf.mesh.bounds;

		var res = false;

		foreach( var c in cols )
		{
			var structureBounds = c.bounds;

			res |= structureBounds.min.x < areaBounds.min.x | structureBounds.max.x > areaBounds.max.x;
			res |= structureBounds.min.y < areaBounds.min.y | structureBounds.max.y > areaBounds.max.y;
			res |= structureBounds.min.z < areaBounds.min.z | structureBounds.max.z > areaBounds.max.z;

			if( res ) return true;
		}

		return false;//res;
	}

	public bool isMoveToNear( Vector3 prepos, Vector3 nowpos )//, float rate )
	{

		var center = mf.mesh.bounds.center;

		return (nowpos - center).sqrMagnitude < (prepos - center).sqrMagnitude;// * rate * rate;

	}



	public void encapsulateBounds( Bounds bounds )
	{
		//	Debug.Log("バウンディング拡張");

		var newBounds = mf.mesh.bounds;
		
		newBounds.Encapsulate( bounds );
		
		mf.mesh.bounds = newBounds;

	}
	public void encapsulateBounds( Collider[] cols )
	{
		//	Debug.Log("バウンディング拡張");

		var newBounds = mf.mesh.bounds;

		foreach( var col in cols )
		{

			newBounds.Encapsulate( col.bounds );

		}
		
		mf.mesh.bounds = newBounds;

	}



	public void requestRecalculateAllBounds( float delayTime )
	{
		//	Debug.Log("再計算を促す");
		if( !scheduleedRecalculateBounds )
		{

			StartCoroutine( recalculateBounds( delayTime ) );

		}
	}

	IEnumerator recalculateBounds( float delayTime )
	{

		scheduleedRecalculateBounds = true;


		//	Debug.Log("再計算受理");
		yield return new WaitForSeconds( delayTime );
		
		
		//	Debug.Log("再計算開始");
		//	var structures = GetComponentsInChildren<_StructureInterArea3>();
		
		
		if( structuresEntityLength > 0 )//structures.Length > 0 )
		{
			
			// 本当は far の MeshCollider のほうがいいが、アクティブでないと bounds が消えてしまう
			var newBounds = structures[0].envelopeColliders[0].bounds;


			for( var i = 0; i < structuresEntityLength; i++ )//structures.Length; i++ )
			{
				
				foreach( var col in structures[i].envelopeColliders )
				{
					newBounds.Encapsulate( col.bounds );
				}
				
			}


			mf.sharedMesh.bounds = newBounds;
			
		}
		else
		{
			
			//	Debug.Log("エリア破棄");
			Destroy( gameObject );	// area 消滅

		}
		
		
		//	Debug.Log("再計算終了");
		scheduleedRecalculateBounds = false;

	}


/*
	void LateUpdate()
	{

	//	if( SystemManager.playerCamera.isVisible(mesh.bounds) ) Debug.Log( mesh );
		Graphics.DrawMesh( mesh, Vector3.zero, Quaternion.identity, material, 0 );
	//	Graphics.DrawMesh( mesh, Vector3.zero, Quaternion.identity, new Material(Shader.Find("Transparent/Diffuse")), 0);//, SystemManager.playerCamera.camera, 0, null );

	}
*/


#if UNITY_EDITOR

	void OnDrawGizmos()
	{

	//	var structures = GetComponentsInChildren<_StructureInterArea3>();

	//	var newBounds = ss[0].farCollider.bounds;
		
		for( var i = 0; i < structuresEntityLength; i++ )//structures.Length; i++ )
		{
	//		newBounds.Encapsulate( s.farCollider.bounds );

			var sbounds = structures[i].envelopeColliders[0].bounds;

			for( var ii = 1; ii < structures[i].envelopeColliders.Length; ii++ )
			{
				sbounds.Encapsulate( structures[i].envelopeColliders[ii].bounds );
			}

			drawBounds( sbounds, Color.blue );
		}

	//	drawBounds( newBounds, Color.cyan );


		drawBounds( mf.sharedMesh.bounds, Color.red );

	}

	
	void drawBounds( Bounds bounds, Color c )
	{
		
		var u = bounds.max.y;
		var d = bounds.min.y;
		var r = bounds.max.x;
		var l = bounds.min.x;
		var f = bounds.max.z;
		var b = bounds.min.z;

		var ulf = new Vector3( l, u, f );
		var urf = new Vector3( r, u, f );
		var ulb = new Vector3( l, u, b );
		var urb = new Vector3( r, u, b );
		
		var dlf = new Vector3( l, d, f );
		var drf = new Vector3( r, d, f );
		var dlb = new Vector3( l, d, b );
		var drb = new Vector3( r, d, b );
		
		Debug.DrawLine( ulb, ulf, c );
		Debug.DrawLine( ulf, urf, c );
		Debug.DrawLine( urf, urb, c );
		Debug.DrawLine( urb, ulb, c );
		
		Debug.DrawLine( dlb, dlf, c );
		Debug.DrawLine( dlf, drf, c );
		Debug.DrawLine( drf, drb, c );
		Debug.DrawLine( drb, dlb, c );
		
		Debug.DrawLine( ulb, dlb, c );
		Debug.DrawLine( ulf, dlf, c );
		Debug.DrawLine( urf, drf, c );
		Debug.DrawLine( urb, drb, c );
		
	}

#endif


}
