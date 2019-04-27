using UnityEngine;
using System.Collections;

[SelectionBase]
public class PlotPartUtlPole3 : PlotPart3
{

	public CablesetLinkHolder3	cablesetLink;	// 自分につながるケーブルセットのチェイン

	public CutcableLinkHolder3	cutcableLink;	// 自分につながる断線ケーブルのチェイン



	protected override bool fallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
	{

		cablesetLink.cutAllLinkedCablesets( hitter );


		var instance = fallDownDestructionSurface( hitter, force, point );


		cutcableLink.reparentAllCutcables( instance );
		

		return true;

	}

	
}



public struct CablesetLinkHolder3
{

	CablesetLinkUnit3	cablesetLink;


	public class CablesetLinkUnit3// ケーブルセットが破壊される可能性から、直でリスト構造にはできない（リストの間が抜けるため）
	{
		public PlotPartCableset3	cableset;
		
		public CablesetLinkUnit3	next;	
	}

	
	public void add( PlotPartCableset3 newCableset )
	{

		var link = new CablesetLinkUnit3();


		link.cableset	= newCableset;

		link.next		= cablesetLink;


		cablesetLink	= link;

	}

	public void cutAllLinkedCablesets( _StructureHit3 hitter )
	{

		for( var link = cablesetLink; link != null; link = link.next )
		{
			if( link.cableset != null ) link.cableset.cut( hitter );
		}

	}

}




public struct CutcableLinkHolder3
{
	
	CutCable3	cutcableLink;


	public void addCutCable( CutCable3 newCutcable )
	{
		newCutcable.cutcableLink	= this.cutcableLink;

		this.cutcableLink			= newCutcable;
	}


	public void reparentAllCutcables( _StructurePart3 pole )
	{

		//var tfPole = pole.transform;

		var rbPole = pole.GetComponent<Rigidbody>();


		for( var cutcable = cutcableLink; cutcable != null; cutcable = cutcable.cutcableLink )
		{

			cutcable.transform.parent = null;//tfPole;

			cutcable.rb.isKinematic = false;

			cutcable.GetComponent<FixedJoint>().connectedBody = rbPole;//gameObject.AddComponent<FixedJoint>().connectedBody = rbPole;

			GameObject.Destroy( cutcable.gameObject, 2.0f );

		}

	}

}

