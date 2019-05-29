using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abss.StructureObject;
using System.Linq;
using Abss.Common.Extension;

namespace Abss.Geometry
{
	public static class TexturePacker
	{

		public static Texture PackTextureAndReUv( IEnumerable<GameObject> targetObjects )
		{
			var matHashToIdxDict = targetObjects
				.To(MaterialUtility.QueryMatNameAndHash_EverySubmeshesEveryMeshes)
				.To(MaterialUtility.ToDictionaryForMaterialHashToIndex);

			var vtxCount_EverySubmeshes = targetObjects
				.To(VertexUtility.QueryMesh_EveryObjects)
				.To(VertexUtility.QueryVertexCount_EverySubmeshes);

			var q =
				from obj in targetObjects
				select (mats: obj.GetComponent<Renderer>()?.sharedMaterials, uvss:obj.GetComponent)

			return null;

			(Material[],Vector2[]) getMatAndUvs_( GameObject obj_ )
			{
				var smr = obj_.GetComponent<SkinnedMeshRenderer>();
				
				var mats = obj_.GetComponent<Renderer>().sharedMaterials;

				var mesh = smr != null
					? smr.sharedMesh
					: obj_.GetComponent<MeshFilter>()?.sharedMesh;
				var uvss =
					from i in Enumerable.Range( 0, mats.Length )
					select mesh.GetUVs(()

				return (mats, mesh.uvs)
			}

		}

		/// <summary>
		/// 全パーツから、それぞれのマテリアルハッシュ配列をすべてクエリする。
		/// 名前順にソートされる。
		/// </summary>
		public static IEnumerable<IEnumerable<int>>
			QueryMatHashArraysEveryParts( IEnumerable<_StructurePartBase> parts )
		{
			var qMatHashArrayEveryParts =
				from pt in parts
				select pt.GetComponent<MeshRenderer>()?.sharedMaterials into mats
				from mat in mats ?? Enumerable.Empty<Material>()//mats.DefaultIfEmpty()
				orderby mat.name
				group mat.GetHashCode() by mats into matHashs
				select matHashs.ToArray()
				;

			return qMatHashArrayEveryParts;
		}
	}
}

