using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abss.Common.Extension;
using Abss.StructureObject;

namespace Abss.Geometry2
{

	/// <summary>
	/// Mesh 要素を格納する。並列処理させた結果を格納し、最後に Mesh を作成するために使用する。
	/// </summary>
	public struct MeshElements
	{　
		// Mesh 要素
		public List<Vector3>	Vertecies;
		public List<Vector3>	Normals;
		public List<Vector2>	Uvs;
		public List<List<int>>	IndeciesEverySubmeshes;
		public List<Vector3>	Tangents;
		public List<Color32>	Colors;

		// 間接的に必要な要素
		public List<Matrix4x4>		mtObjects;
		public Matrix4x4			MtBaseInv;

		/// <summary>
		/// Mesh を生成する。
		/// </summary>
		public Mesh CreateUnlitMesh()
		{
			var mesh = new Mesh();

			if( this.Vertecies != null ) mesh.SetVertices( this.Vertecies );
			if( this.Normals != null ) mesh.SetNormals( this.Normals );
			if( this.Uvs != null ) mesh.SetUVs( 0, this.Uvs );
			if( this.Colors != null ) mesh.SetColors( this.Colors );
			if( this.IndeciesEverySubmeshes != null )
			{
				foreach( var x in this.IndeciesEverySubmeshes.Select( (idxs,i) => (idxs,i) ) )
				{
					mesh.SetTriangles( x.idxs, submesh:x.i, calculateBounds:false );
				}
			}

			return mesh;
		}
	}
	

	public static class QueryPerMesh
	{
		public static IEnumerable<> QueryIndex
	}
	public static class QueryPerSubMeshInMeshes
	{

	}
}
