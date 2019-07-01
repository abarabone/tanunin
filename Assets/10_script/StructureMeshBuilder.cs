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

		static public async Task<GameObject> BuildNearObjectAsync( this _StructurePartBase[] parts, Transform tfBase )
		{
			
			await Task.WhenAll( from pt in parts select pt.CombinePartMeshesAsync() );

			var meshElement = await Task.Run( MeshCombiner.BuildNormalMeshElements( parts.Select(x=>x.gameObject), tfBase ) );//.BuildStructureWithPalletMeshElements( parts, tfBase ) );

			var go = new GameObject();
			go.AddComponent<MeshFilter>().mesh = meshElement.CreateMesh();
			go.AddComponent<MeshRenderer>().materials = meshElement.materials;

			return go;
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
		
		/// <summary>
		/// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
		/// </summary>
		public static async Task CombinePartMeshesAsync( this _StructurePartBase part )
		{

			// 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
			// （ただしパーツだった場合は、結合対象から除外する）
			var buildTargets = queryTargets_Recursive_( part.gameObject ).ToArray();
			if( buildTargets.Length == 1 ) return;

			var meshElements = await combineChildMeshesAsync_( buildTargets, part.transform );

			// 
			replaceOrAddComponents_CombinedMeshAndMaterials_( part.gameObject, meshElements );
			removeOrigineComponents_( buildTargets.Skip(1) );
			
			return;

			
			IEnumerable<GameObject> queryTargets_Recursive_( GameObject go_ ) =>
				(
					from child in go_.Children()
					where child.GetComponent<_StructurePartBase>() == null
					from x in queryTargets_Recursive_(child)
					select x
				)
				.Prepend( go_ );

			async Task<MeshElements> combineChildMeshesAsync_( IEnumerable<GameObject> targets_, Transform tf_ )
			{
				var combineElementFunc =
					MeshCombiner.BuildNormalMeshElements( targets_, tf_, isCombineSubMeshes:true );

				return await Task.Run( combineElementFunc );
			}

			void removeOrigineComponents_( IEnumerable<GameObject> targets_ )
			{
				foreach( var go in targets_ )
				{
					go.DestroyComponentIfExists<MeshFilter>();
					go.DestroyComponentIfExists<Renderer>();
				}
			}

			void replaceOrAddComponents_CombinedMeshAndMaterials_( GameObject gameObject_, MeshElements me_ )
			{
				var mf = gameObject_.GetComponent<MeshFilter>().As() ?? gameObject_.AddComponent<MeshFilter>().As();
				mf.sharedMesh = me_.CreateMesh();

				var mr = gameObject_.GetComponent<MeshRenderer>().As() ?? gameObject_.AddComponent<MeshRenderer>().As();
				mr.materials = me_.materials;
			}
		}

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

