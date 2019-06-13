using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Abss.Geometry;
using Abss.Common.Extension;
using System.Linq;
using Unity.Linq;
using System;

namespace Abss.StructureObject
{
	public class _StructurePartBase : MonoBehaviour, IStructurePart
	{

		public int PartId { get; private set; }


		public async Task Build()
		{
			
			var gos =
				from renderer in this.GetComponentsInChildren<Renderer>().EmptyIfNull()
				select renderer.gameObject
				;

			IEnumerable<GameObject> queryTargets_( GameObject go_ ) =>
				from child in go_.Children()
				where queryTargets_(child) != 
				select x
				;
			
			var combineElementFunc =
				MeshCombiner.BuildNormalMeshElements( gos, this.transform, isCombineSubMeshes: false );

			var meshElements = await Task.Run( combineElementFunc );


			var go = this.gameObject;

			var mf = go.GetComponent<MeshFilter>().As() ?? go.AddComponent<MeshFilter>().As();
			mf.sharedMesh = meshElements.CreateMesh();

			var mr = go.GetComponent<MeshRenderer>().As() ?? go.AddComponent<MeshRenderer>().As();
			mr.materials = meshElements.materials;

		}

		public bool FallDown( _StructureHit3 hitter, Vector3 force, Vector3 point )
		{
			throw new System.NotImplementedException();
		}
		
	}

	
	/// <summary>
	/// パーツの軸沿いボックスを管理する。
	/// </summary>
	public struct HitLocalBoundsUnit
	{
		
		/// <summary>モデル座標上のメッシュ境界ボックス中心</summary>
		public Vector3	structureViewCenter	{ get; private set; }

		/// <summary>メッシュ境界ボックスの外接球半径</summary>
		public float	radius				{ get; private set; }
		
		/// <summary>メッシュ境界ボックスのコピー</summary>
		public Bounds	localBounds			{ get; private set; }
		
		
		public void set( Bounds bounds, ref Matrix4x4 mtInvParent, Transform tf )
		{
			
			localBounds = bounds;
			
			radius		= bounds.extents.magnitude;
			
			var mt		= mtInvParent * tf.localToWorldMatrix;
			
			structureViewCenter = mt.MultiplyPoint3x4( bounds.center );
			
		}

		/// <summary>
		/// 球と境界ボックスの接触を判定する。
		/// 判定はローカル座標で行う。そのため雛形式でも実体所持式でも同じ処理で可能。
		/// </summary>
		/// <param name="otherCenter">球の中心（モデル座標）</param>
		/// <param name="otherRadius">球の半径</param>
		public bool check( Vector3 otherCenter, float otherRadius )
		{
			
			// 相手の球をローカル座標に変換

			var localOtherCenter = otherCenter - structureViewCenter;
			

			var hitLimit = otherRadius + radius;
			
			if( localOtherCenter.sqrMagnitude < hitLimit * hitLimit )	// 球 vs 球
			{
				
				return localBounds.SqrDistance( localOtherCenter ) < otherRadius * otherRadius;
				// 球とＡＡＢＢの接触判定。ここまでやらなくても（球vs球でやめても）いい気はする。
				
			}
			
			return false;
			
		}

	}
	
	
	/// <summary>
	/// 全体メッシュインデックス内における位置を三角形単位に直して保管する。
	/// </summary>
	public struct HitIndexInfoUnit
	{

		/// <summary>開始三角形位置</summary>
		public int	startTriangle	{ get; private set; }

		/// <summary>三角形数</summary>
		public int	triangleLength	{ get; private set; }
		
		/// <summary></summary>
		public int[] Triangles { get; private set; }

		
		/// <summary>
		/// インデックス位置を三角形位置として保管する。
		/// 先に三角形位置に直すのは、後に使用する場合のため（除算/乗算のコスト）。
		/// </summary>
		/// <param name="st">三角形開始位置</param>
		/// <param name="len">三角形数</param>
		public void set( int st, int len )
		{
			
			startTriangle	= st;
			
			triangleLength	= len;
			
		}

		public bool isEmpty
		{
			get { return triangleLength == 0; }
		}

	}

}

