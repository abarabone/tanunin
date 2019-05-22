using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abss.StructureObject;

namespace Abss.Geometry
{
	public static class TexturePacker : MonoBehaviour
	{

		public static Texture PackTexture( IEnumerable<Material> materials )
		{

			var q =
				from mat in materials


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

