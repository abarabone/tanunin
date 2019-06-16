using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abss.Common.Extension;
using Abss.Geometry;

namespace Abss.StructureObject
{

	/// <summary>
	/// 
	/// </summary>
	static public class StructureNearObjectBuilder
	{

		static public GameObject BuildNearObject( this _StructurePartBase[] parts )
		{

			return null;
		}

		
		static GameObject BuildMeshAndGameObject( this _StructurePartBase[] parts )
		{
			

			return null;
		}



		static private GameObject buildNearMeshForDraw( _StructureBase structure, _StructurePartBase[] parts )
		{

			foreach( var part in parts )
			{
				part.BuildAsync();
			}

			MeshCombiner.BuildStructureWithPalletMeshElements( parts, structure.transform );

			return null;
		}

		static private GameObject buildNearMeshForHit( IStructurePart[] parts )
		{

			return null;
		}

	}


	
	static class NearObjectBuilder
	{
		
		public static GameObject BuildNearObject( Mesh mesh, Material material )
		{
			var go = new GameObject( name: "near" );

			addRigidBody_IfNoHave_( go );
			addRenderer_( go, mesh, material );
			addStructure_( go );

			go.SetActive( false );

			return go;

			
			void addRigidBody_IfNoHave_( GameObject go_ )
			{
				var rb = go.AddComponent<Rigidbody>();
				rb.isKinematic = true;
			}
			void addRenderer_( GameObject go_, Mesh mesh_, Material mat_ )
			{
				var mf = go_.AddComponent<MeshFilter>();
				mf.sharedMesh = mesh_;

				var mr = go_.AddComponent<MeshRenderer>();
				mr.sharedMaterial = mat_;
			}
			void addStructure_( GameObject go_ )
			{
				var sr = go.AddComponent<StructureNearRenderingController>();
				
			}
		}

		//GameObject BuildChildWithCollider( _StructurePart3.enType hitType, Transform tfParent )
		//{

		//	var mesh = meshBuilder.hits[ (int)hitType ].mesh;


		//	if( mesh != null )
		//	{

		//		var go = new GameObject( hitType.ToString() );

		//		go.layer = getPartLayer( hitType );


		//		go.transform.SetParent( tfParent, false );

		//		var cd = go.AddComponent<MeshCollider>();

		//		cd.sharedMesh = mesh;


		//		return go;

		//	}

		//	return null;

		//}
	}

	

}

