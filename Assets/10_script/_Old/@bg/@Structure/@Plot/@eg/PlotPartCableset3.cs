using UnityEngine;
using System.Collections;

public class PlotPartCableset3 : PlotPart3
{

	
	public Transform	tfStartSideHub;	// はじめはハブにつながっているが、初期化されてポールにつながる。

	public Transform	tfEndSideHub;	// 同上


	CableSideLocations3[]	sidePositions;





	public override void init( int id )
	{

		base.init( id );


		tfStartSideHub.GetComponent<PlotPartUtlPole3>().cablesetLink.add( this );
		//tfStartSideHub.GetComponentInParent<PlotPartUtlPole3>().cablesetLink.add( this );

		tfEndSideHub.GetComponent<PlotPartUtlPole3>().cablesetLink.add( this );
		//tfEndSideHub.GetComponentInParent<PlotPartUtlPole3>().cablesetLink.add( this );

	}
	
	
	public void cut( _StructureHit3 hitter )
	{
		
		var tf = transform;
		
		if( !hitter.breakManager.isPartBroken( partId ) )// breaker.structure.nearRender.isVisible( partId ) )
		{
			//fallDownAllChildren( hitter, 0.0f, Vector3.down, tf.position + tf.forward * ( tf.localScale.z * 0.5f ) );
			fallDownEntity( hitter, Vector3.down, tf.position + tf.forward * ( tf.localScale.z * 0.5f ) );
		}
		
	}






	protected override bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
	{

		showCutcables( hitter );


		fallDownDestructionSurface( hitter );


		return true;

	}

	void showCutcables( _StructureHit3 hitter )
	{
		
		var tf = transform;


		if( tfStartSideHub != null )
		{
			var rot	= tf.rotation;

			foreach( var pos in sidePositions )
			{
				createCutcableInstance( tfStartSideHub, pos.start, rot, hitter );
			}
		}

		if( tfEndSideHub != null )
		{
			var rot	= Quaternion.Inverse( tf.rotation );

			foreach( var pos in sidePositions )
			{
				createCutcableInstance( tfEndSideHub, pos.end, rot, hitter );
			}
		}

	}

	void createCutcableInstance( Transform tfHub, Vector3 pos, Quaternion rot, _StructureHit3 hitter )
	{

		//pos = structure.tf.TransformPoint( pos );

		//rot *= structure.tf.rotation;



		var cutcableInstance = (CutCable3)Instantiate( GM.cutcablePrefab, pos, rot );

		cutcableInstance.transform.parent = hitter.structure.near.transform;//hitter.transform;



		tfHub.GetComponent<PlotPartUtlPole3>().cutcableLink.addCutCable( cutcableInstance );
		//tfHub.GetComponentInParent<PlotPartUtlPole3>().cutcableLink.addCutCable( cutcableInstance );

	}







	// ケーブルコンバイン -------------------------------------------------------

	public override void buildFromPieces()
	{
		
		var cables = GetComponentsInChildren<CablePiece3>();//adjustCablePositions();
		
		
		setCableSideLocations( cables );


		reparentChildrenOfCables( cables );


		tfStartSideHub = tfStartSideHub.GetComponentInParent<PlotPartUtlPole3>().transform;

		tfEndSideHub = tfEndSideHub.GetComponentInParent<PlotPartUtlPole3>().transform;


		base.buildFromPieces();
		
	}
	
	void setCableSideLocations( CablePiece3[] cables )
	{
		
		if( cables.Length > 0 )
		{
			
			sidePositions = new CableSideLocations3[ cables.Length ];
			
			
			for( int i = 0; i < cables.Length; i++ )
			{
				
				sidePositions[i] = new CableSideLocations3( cables[i] );
				
			}
			
		}
		
	}

	void reparentChildrenOfCables( CablePiece3[] cables )
	{

		var tf = transform;
		
		foreach( var cable in cables )
		{
			
			var tfc = cable.transform;
			
			for( var i = tfc.childCount; i-- > 0; )
			{
				tfc.GetChild(i).parent = tf;
			}
			
		}

	}
	
	struct CableSideLocations3
	{

		public Vector3	start;

		public Vector3	end;


		public CableSideLocations3( CablePiece3 cable )//, Transform tfPlot )
		{
			//var mtInv = tfPlot.worldToLocalMatrix;
			var tfc = cable.transform;

			start	= tfc.position;//cable.tfStartSide.position;//mtInv.MultiplyPoint3x4( cable.tfStartSide.position );

			end		= tfc.position + tfc.forward * tfc.lossyScale.z;//cable.tfEndSide.position;//mtInv.MultiplyPoint3x4( cable.tfEndSide.position );
		}

	}



#if UNITY_EDITOR
	// エディタ上で ------------------------------------------------------------

	public void adjustCablePositions( Transform _tfStartSideHub, Transform _tfEndSideHub )
	{

		tfStartSideHub = _tfStartSideHub;
		
		tfEndSideHub = _tfEndSideHub;
		
		
		adjustCablePositions();

	}

	public void adjustCablePositions()
	{

		if( tfStartSideHub == null || tfEndSideHub == null ) return;



		var cables = GetComponentsInChildren<CablePiece3>();



		var mt = tfStartSideHub.localToWorldMatrix;
		
		var mtInv = tfStartSideHub.worldToLocalMatrix;
		

		var vcsts = calculateLinkPoint( tfStartSideHub, ref mtInv );

		var vceds = calculateLinkPoint( tfEndSideHub, ref mtInv );

		

		var center = new Vector3();


		var len = Mathf.Min( vcsts.Length, vceds.Length );

		var i = 0;
		
		foreach( var cable in cables )
		{

			cable.transform.parent = null;


			if( i < len )
			{

				var vcst = mt.MultiplyPoint3x4( vcsts[ i ] );

				var vced = mt.MultiplyPoint3x4( vceds[ i ] );

				center += vcst + vced;

				cable.adjustTransform( vcst, vced );

			}
			else
			{
				break;
			}
			
			i++;

		}




		var tf = transform;

		tf.rotation = tfStartSideHub.rotation;

		tf.position = center / (float)(i * 2);




		foreach( var cable in cables )
		{
			
			cable.transform.parent = tf;


			var tfc = cable.transform;
			
			for( var ip = tfc.childCount; ip-- > 0; )
			{
				tfc.GetChild(ip).localScale = new Vector3( 1.0f, 1.0f, 1.0f / tfc.lossyScale.z );
			}
			
		}
		
	}


	Vector3[] calculateLinkPoint( Transform tfHub, ref Matrix4x4 mtInv )
	{
		

		var tfAnchors = tfHub.getComponentsInDirectChildren<Transform>();

		
		var vcs = new Vector3[ tfAnchors.Length ];
		
		for( var i = 0; i < tfAnchors.Length; i++ )
		{
			
			vcs[ i ] = mtInv.MultiplyPoint3x4( tfAnchors[ i ].position );
			
		}

		if( vcs.Length > 1 )
		{

			var hdist = Mathf.Abs( vcs[ 1 ].x - vcs[ 0 ].x );
			var vdist = Mathf.Abs( vcs[ 1 ].y - vcs[ 0 ].y );

			if( hdist > vdist )
			{//Debug.Log(vcs.Length);foreach( var vc in vcs ) Debug.Log( tfHub.name );
				System.Array.Sort( vcs, (a, b) => a.x >= b.x ? 1: -1 );//foreach( var vc in vcs ) Debug.Log( vc );
			}
			else
			{
				System.Array.Sort( vcs, (a, b) => a.y >= b.y ? 1: -1 );
			}

		}

		return vcs;

	}
#endif

}
