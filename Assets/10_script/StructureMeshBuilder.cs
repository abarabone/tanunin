using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;

namespace ModelGeometry
{
	public class StructureMeshBuilder
	{
		
		public ResultDraw draw { get; private set; }

		public List<ResultHit> hits { get; private set; }


		public StructureMeshBuilder( _StructurePart3[] parts, Transform tfParent )
		{

			draw = buildDraw( parts, tfParent );

			hits = new List<ResultHit>();// (int)_StructurePart3.enType.length );
			hits.AddRange( new ResultHit[ 10 ] );

			hits[ (int)_StructurePart3.enType.common ] = buildCommon( parts, tfParent );
			hits[ (int)_StructurePart3.enType.fence ] = buildFence( parts, tfParent );
			hits[ (int)_StructurePart3.enType.invisible ] = buildInvisivle( parts, tfParent );
			hits[ (int)_StructurePart3.enType.occlusion ] = buildOcculusion( parts, tfParent );
			hits[ (int)_StructurePart3.enType.inhittable ] = buildInhittable( parts, tfParent );

		}


		static public ResultDraw buildDraw( _StructurePart3[] parts, Transform tfParent )
		{

			var res = new ResultDraw();


			var meshCreator = new StructureMeshCreator();

			var mtInvParent = tfParent.worldToLocalMatrix;
		

			var qDrawParts = parts.Where( p => p.type != _StructurePart3.enType.invisible );


			var qMesh = 
				from part in qDrawParts
				select part.GetComponent<MeshFilter>().sharedMesh into mesh
				select (v:mesh.vertexCount, i:mesh.triangles.Length)
				;
			var mechCount = qMesh.Aggregate( (pre, cur) => (pre.v + cur.v, pre.i + cur.i) );

			var counter = qDrawParts.Aggregate( new MeshElementsCounter(), (s, p) => s.count( p.GetComponent<MeshFilter>().sharedMesh ) );

			meshCreator.alloc( ref counter );

			if( counter.idxLength == 0 ) return res;


			foreach( var p in qDrawParts )
			{

				var mesh = p.GetComponent<MeshFilter>().sharedMesh;
				var mt = mtInvParent * p.transform.localToWorldMatrix;
				
				meshCreator.addGeometory( mesh, p.partId, 0, ref mt );


				//var mr = p.GetComponent<MeshRenderer>();

				res.isNeedTransparentShader |= true;//((Texture2D)mr.material.mainTexture).format == 
			
			}


			res.mesh = meshCreator.create( EnMode.writeOnly );

			res.texture = (Texture2D)qDrawParts.First().GetComponent<MeshRenderer>().material.mainTexture;

			return res;

		}

		static public ResultHit buildCommon( _StructurePart3[] parts, Transform tfParent )
		{
		
			var q = parts
				.Where( p => p.type == _StructurePart3.enType.common )
				.Select( p => new PartInfo( p ) );

			return _buildHit( q, parts, tfParent );

		}

		static public ResultHit buildFence( _StructurePart3[] parts, Transform tfParent )
		{

			var q = parts
				.Where( p => p.type == _StructurePart3.enType.fence )
				.Select( p => new PartInfo( p ) );

			return _buildHit( q, parts, tfParent );

		}

		static public ResultHit buildInvisivle( _StructurePart3[] parts, Transform tfParent )
		{

			var q = parts
				.Where( p => p.type == _StructurePart3.enType.invisible )
				.Select( p => new PartInfo( p ) );

			return _buildHit( q, parts, tfParent );

		}

		static public ResultHit buildOcculusion( _StructurePart3[] parts, Transform tfParent )
		{

			var q = parts
				.Where( p => p.type == _StructurePart3.enType.occlusion )
				.Select( p => new PartInfo( p ) );

			return _buildHit( q, parts, tfParent );

		}

		static public ResultHit buildInhittable( _StructurePart3[] parts, Transform tfParent )
		{

			var mtInvParent = tfParent.worldToLocalMatrix;


			var q = parts
				.Where( p => p.type == _StructurePart3.enType.inhittable )
				.Select( p => new PartInfo( p ) );

			foreach( var pi in q )
			{
			
				pi.part.hitLocalBounds.set( pi.mesh.bounds, ref mtInvParent, pi.tf );
			
			}


			return new ResultHit();

		}



		static ResultHit _buildHit( IEnumerable<PartInfo> q, _StructurePart3[] parts, Transform tfParent )
		{

			var meshCreator = new CollisionMeshCreator();

			var mtInvParent = tfParent.worldToLocalMatrix;

		
			var counter = q.Aggregate( new MeshElementsCounter(), (s, pi) => s.count( pi.mesh ) );

			meshCreator.alloc( ref counter );

			if( counter.idxLength == 0 ) return new ResultHit();


			var indexList = new TriangleIndexToPartIdList( counter.idxLength );


			var triIndex = 0;

			foreach( var pi in q )
			{

				var triLength = pi.mesh.triangles.Length / 3;

				pi.part.hitLocalBounds.set( pi.mesh.bounds, ref mtInvParent, pi.tf );

				pi.part.hitIndexInfo.set( triIndex, triLength );

				indexList.setTriIndexToPartIdList( pi.part.partId, triIndex, triLength );

				triIndex += triLength;


				var mt = mtInvParent * pi.tf.localToWorldMatrix;

				meshCreator.addGeometory( pi.mesh, ref mt );

			}


			return new ResultHit()
			{
				mesh = meshCreator.create( EnMode.dynamic ),

				triIndexToPartIdList = indexList.triIndexToPartIdList
			};

		}


		// ----------------------------------
	

		struct PartInfo
		{
			public Transform		tf;
			public Mesh				mesh;
			public _StructurePart3	part;

			public PartInfo( _StructurePart3 p )
			{
				part = p;
				tf = p.transform;
				//mesh = p.GetComponent<MeshCollider>()?.sharedMesh ?? p.GetComponent<MeshFilter>().sharedMesh;

				var mc = p.GetComponent<MeshCollider>();
				mesh = mc ? mc.sharedMesh : p.GetComponent<MeshFilter>().sharedMesh;
			}
		}

		struct TriangleIndexToPartIdList
		{
			public ushort[] triIndexToPartIdList { get; private set; }

			public TriangleIndexToPartIdList( int length )
			{
				triIndexToPartIdList = new ushort[ length / 3 ];
			}

			public void setTriIndexToPartIdList( int partId, int st, int len )
			{
				var end = st + len;

				for( var i = st ; i < end ; i++ )
				{
					triIndexToPartIdList[ i ] = (ushort)partId;
				}
			}
		}


		public struct ResultDraw
		{
			public Mesh			mesh;
			public Texture2D	texture;
		
			public bool isNeedTransparentShader;
		}

		public struct ResultHit
		{
			public Mesh		mesh;

			public ushort[] triIndexToPartIdList;
		}

	}
}

