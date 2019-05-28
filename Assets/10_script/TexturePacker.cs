using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abss.StructureObject;
using System.Linq;

namespace Abss.Geometry
{
	public static class TexturePacker
	{

		public static Texture PackTextureAndReUv( IEnumerable<GameObject> targetObjects )
		{

			var qNameAndHashMats = MaterialUtility.QueryMatNameAndHash_EverySubmeshesEveryMeshes(  );
			var matHashToIdxDict = MaterialUtility.ToDictionaryForMaterialHashToIndex( qNameAndHashMats );

			var q =
				from obj in targetObjects
				select (mats: obj.GetComponent<Renderer>()?.sharedMaterials, uvs)

			return null;

			IEnumerable<> query

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

