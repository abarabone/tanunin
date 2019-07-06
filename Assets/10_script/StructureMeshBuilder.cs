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

		/// <summary>
		/// 
		/// </summary>
		static public async Task<GameObject> BuildNearObjectAsync( this IEnumerable<_StructurePartBase> parts, Transform tfBase )
		{

			var goes = await buildObjectsAsync_( parts );

			setParent_( parent_:goes[0], children_:goes.Skip(1) );

			return goes[0];
			

			async Task<GameObject[]> buildObjectsAsync_( IEnumerable<_StructurePartBase> parts_ )
			{
				var buildAsyncs = new Task<GameObject> []
				{
					buildNearObjectAsync_( parts ),
					buildHitObjectAsync_( parts.Where(pt => pt.partType == StructurePartType.massive) ),
				};
				return await Task.WhenAll( buildAsyncs );
			}
			
			void setParent_( GameObject parent_, IEnumerable<GameObject> children_ )
			{
				var tfNear = parent_.transform;
				foreach( var tfHit in children_.Select(go=>go.transform) )
				{
					tfHit.SetParent( tfNear, worldPositionStays:true );
				}
			}

			async Task<GameObject> buildNearObjectAsync_( IEnumerable<_StructurePartBase> parts_ )
			{
				Debug.Log($"build near pre : {tfBase.GetHashCode()}");

				var q = from pt in parts_ select pt.combinePartMeshesAsync();
				await Task.WhenAll( q );
				
				Debug.Log($"build near part combined : {tfBase.GetHashCode()}");

				var f = MeshCombiner.BuildNormalMeshElements( parts.Select(x=>x.gameObject), tfBase );//.BuildStructureWithPalletMeshElements( parts, tfBase );
				var meshElement = await Task.Run( f );
				
				Debug.Log($"build near combined : {tfBase.GetHashCode()}");

				var go = new GameObject("near");
				go.AddComponent<MeshFilter>().mesh = meshElement.CreateMesh();
				go.AddComponent<MeshRenderer>().material = new Material( meshElement.materials[0] );
				
				( go.GetComponent<Rigidbody>().As() ?? go.AddComponent<Rigidbody>() ).isKinematic = true;

				var sr = go.AddComponent<StructureNearRenderingController>();

				return go;
			}

			async Task<GameObject> buildHitObjectAsync_( IEnumerable<_StructurePartBase> parts_ )
			{
				var q = from pt in parts_ select pt.combinePartMeshesAsync();
				await Task.WhenAll( q );

				var f = MeshCombiner.BuildBaseMeshElements( parts.Select(x=>x.gameObject), tfBase );
				var meshElement = await Task.Run( f );

				var go = new GameObject("near hit");
				go.AddComponent<MeshCollider>().sharedMesh = meshElement.CreateMesh();
				
				return go;
			}
			
		}

		
		/// <summary>
		/// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
		/// </summary>
		static async Task combinePartMeshesAsync( this _StructurePartBase part )
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
				var mf = gameObject_.GetComponent<MeshFilter>().As() ?? gameObject_.AddComponent<MeshFilter>();
				mf.sharedMesh = me_.CreateMesh();

				var mr = gameObject_.GetComponent<MeshRenderer>().As() ?? gameObject_.AddComponent<MeshRenderer>();
				mr.materials = me_.materials;
			}
		}
		
	}

}
	
