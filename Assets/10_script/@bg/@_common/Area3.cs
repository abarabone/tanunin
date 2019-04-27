using UnityEngine;
using System.Collections;

public class Area3 : MonoBehaviour
{


	public void build()
	{
		
		var buildings = this.GetComponentsInChildren<_StructureInterArea3>();

		if( buildings.Length > 0 )
		{
			
			var dsttex	= buildings[0].GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture;
			
			var dstmesh	= combineMesh( buildings );


			var sbr	= gameObject.AddComponent<StructureBonedRenderer3>();

			sbr.init( dstmesh, GM.shaders.area, dsttex, buildings );
			
			
			buildFarBuildings( buildings, sbr );

		}

	}
	

	

	Mesh combineMesh( _StructureInterArea3[] buildings )
	{
		
		var counter	= new MeshElementsCounter( buildings );
		
		
		//var tfArea	= this.transform;
		
		var creator	= new StructureBonedMeshCreator();//StructureMeshCreator();
		
		creator.alloc( ref counter );
		
		for( var i = 0; i < buildings.Length; i++ )
		{
			
			var tfBuilding = buildings[i].tf;
			
			
			var mf = buildings[i].far.GetComponent<MeshFilter>();
			
			var tfDetail = mf.transform;
			
			
			var mtBuidingBase = tfBuilding.worldToLocalMatrix * tfDetail.localToWorldMatrix * Matrix4x4.Scale( tfBuilding.localScale );// 用考慮：スケーリング

			creator.addGeometory( mf.sharedMesh, i, ref mtBuidingBase, buildings[ i ].colorPallets );

		}
		
		return creator.create();
		
	}


	void buildFarBuildings( _StructureInterArea3[] buildings, StructureBonedRenderer3 sbr )
	{
		
		//var tfArea	= this.transform;

		var areaBounds = buildings[0].envelopeColliders[0].bounds;
		
		
		for( var i = 0; i < buildings.Length; i++ )
		{
			
			var building = buildings[i];

			
			var mf = building.far.GetComponent<MeshFilter>();
			var mr = mf.GetComponent<MeshRenderer>();
			

			building.attatchToArea( sbr, i );

			foreach( var col in building.envelopeColliders )
			{
				areaBounds.Encapsulate( col.bounds );
			}

			
			Destroy( mf );
			Destroy( mr );
			
		}
		
		
		sbr.initBounds( areaBounds );
		
	}

}
