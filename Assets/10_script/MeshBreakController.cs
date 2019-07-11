using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abss.StructureObject
{
	
	public class StructureMeshBreakHitLinker //: HitLinker
	{

		public _Structure3 structure { get; private set; }//
		public _StructurePart3.enType type { get; private set; }


		MeshCollider	collider;

		Mesh hitmesh;

		TriangleIndexToPartIdList	triangleToPartId;

	
		int[]	colliderTriangles;// 破壊メッシュのインデックス配列（破壊があって初めて確保される）
	
		bool	isColliderBrokenInFrame;




		protected new void Awake()
		{

			collider = GetComponent<MeshCollider>();


		
		}

	

	
		//public void init( MeshCollider c, byte[] idList, _StructurePart3.enType t )
		public void init( MeshCollider c, ushort[] idList, _StructurePart3.enType t )
		{

			collider = c;

			triangleToPartId.init( idList );

			type = t;


			if( !structure.isReplicated )
			{
				// コライダのメッシュをシェアしてないものは、最初からメッシュ作成済みとする。

				colliderTriangles = collider.sharedMesh.triangles;

				triangleToPartId.alloc( structure.parts );

				hitmesh = collider.sharedMesh;

			}
			else if( false )
			{
				// 最初からメッシュ作ってしまうモード

				colliderTriangles = collider.sharedMesh.triangles;

				triangleToPartId.alloc( structure.parts );


				var oldmesh = collider.sharedMesh;
				var newmesh = new Mesh();

				newmesh.MarkDynamic();
				newmesh.bounds = oldmesh.bounds;

				newmesh.vertices = oldmesh.vertices;

				newmesh.triangles = colliderTriangles;

				collider.enabled = false;
				collider.sharedMesh = newmesh;
				collider.enabled = true;

				hitmesh = newmesh;

			}
		}



		public void downMeshCollider()
		{

			collider.enabled = false;

		}



		public int triIdToPartId( int triId )
		{

			return triangleToPartId.getPartId( triId );

		}
	
	
	
		/// <summary>
		/// あたり判定メッシュ上において、パーツを構成する面の大きさをゼロにする。
		/// </summary>
		/// <param name="part">破壊したいパーツ</param>
		public void breakPartInHitMesh( _StructurePart3 part )
		{

			if( part.hitIndexInfo.isEmpty ) return;
		

		
			if( colliderTriangles == null )
			{
				// 初回はメッシュ生成
				// 案：パーツコンテンツがホルダーの場合はメッシュ生成なしでいけるんじゃないのか？
				// 　　あと結構メッシュ生成が重い気がするので、あらかじめ全部作ってしまうのはダメだろうか…

				var oldmesh = collider.sharedMesh;  // これはちゃんと同じメッシュを受け取ってるっぽい
				var newmesh = new Mesh();

				newmesh.MarkDynamic();
				newmesh.bounds = oldmesh.bounds;

				newmesh.vertices = oldmesh.vertices;

				colliderTriangles = oldmesh.triangles;  // これは実際には get で配列生成されてるっぽい（クローン不要）

				hitmesh = newmesh;
				collider.sharedMesh = newmesh;


				triangleToPartId.alloc( structure.parts );

			}


			Array.Clear( colliderTriangles, part.hitIndexInfo.startTriangle * 3, part.hitIndexInfo.triangleLength * 3 );
			// 三角形の大きさをゼロにする（全インデックスが 0 を指すようにする）。


			triangleToPartId.remarkOffsets( part );
			// PhysX と Mesh のずれを解消する。ずれない仕様に戻ればなくしてよい。


			if( !triangleToPartId.isFaceExist )
			// 全破壊された場合、コライダをオフにするだけで済ませる。
			{

				collider.enabled = false;
				//Component.Destroy( collider );

			}
			else if( !isColliderBrokenInFrame )
			// フレーム中に複数回更新されるのをふせぐ
			{

				//GM.startCoroutine( applyBrokenCollider() );
				structure.StartCoroutine( applyBrokenCollider() );

			}


		}
	

		/// <summary>
		/// 更新された破壊済みメッシュを適用する。
		/// </summary>
		IEnumerator applyBrokenCollider()
		{

			isColliderBrokenInFrame = true;

			yield return new WaitForSeconds( UnityEngine.Random.Range( 0.02f, 0.25f ) );
			//yield return new WaitForFixedUpdate();


			if( collider.enabled )
			{
				//Debug.Log(hitter.tfStructure.name);

				if( triangleToPartId.isFaceExist )// 完全破壊時のテンソル計算エラーの防止用
				{

					collider.enabled = false;

					hitmesh.triangles = colliderTriangles;

					collider.enabled = true;

				}

			
				triangleToPartId.flush();

			}

			isColliderBrokenInFrame = false;

		}
	


		/// <summary> 
		/// 面インデックスをパーツＩＤに変換する。
		/// 破壊済みのパーツは渡されない前提でよい。
		/// </summary>
		struct TriangleIndexToPartIdList
		{

			ushort[]	partIds;		// パーツＩＤリスト[ 面インデックス ]
			ushort[]	partIds_back;	// 更新用バックバッファ（同フレーム内ヒットはバックバッファで処理する）

			int			faceLength;	// 破壊されていない面の数

			ushort[]	offsets;	// そのパーツの面格納位置[ パーツ数 ]


			public void init( ushort[] srcPartIds )
			{
				// 破壊されるまでは、参照で済ませる。

				partIds = srcPartIds;

				offsets = null;
			
				faceLength = partIds.Length;
			}

			/// <summary>
			/// 領域を確保する。
			/// </summary>
			/// <param name="parts">全パーツの配列</param>
			public void alloc( _StructurePart3[] parts )
			{

				partIds		= (ushort[])partIds.Clone();
				partIds_back= (ushort[])partIds.Clone();

				offsets = new ushort[ parts.Length ];

				foreach( var ipart in parts )
				{
					offsets[ ipart.partId ] = (ushort)ipart.hitIndexInfo.startTriangle;
				}

			}

			public ushort getPartId( int triangleId )
			{
				return partIds[ triangleId ];
			}
		
			public bool isFaceExist
			{
				get { return faceLength > 0; }
			}

			/// <summary>
			///  PhysX と Mesh のずれを解消する。
			/// </summary>
			/// <param name="part">破壊されたパーツ</param>
			public void remarkOffsets( _StructurePart3 part )
			{

				faceLength -= part.hitIndexInfo.triangleLength;
			

				var ofsBytes = offsets[ part.partId ] * 2;

				var lenBytes = part.hitIndexInfo.triangleLength * 2;

				// パーツＩＤリストから破壊された面を削除（後ろが前に詰められる）。
				Buffer.BlockCopy( partIds_back, ofsBytes + lenBytes, partIds_back, ofsBytes, partIds.Length * 2 - ( ofsBytes + lenBytes ) );
			

				// 面格納位置を破壊されたパーツの分だけ前にずらす（後ろのパーツ全てに影響する）。
				// 破壊されたパーツの分は修正しなくてもいいが、その判定をとるなら全て修正したほうがよさそう？
				for( var i = part.partId + 1 ; i < offsets.Length ; i++ )
				{
					offsets[ i ] -= (ushort)part.hitIndexInfo.triangleLength;
				}

			}

			/// <summary>
			/// バックバッファをフロントに反映する。
			/// </summary>
			public void flush()
			{
				Buffer.BlockCopy( partIds_back, 0, partIds, 0, faceLength * 2 );
			}

		}

	}










	public class StructureMeshBreakController3
	{

		public _Structure3 structure { get; private set; }


		public _StructurePart3.enType type { get; private set; }


		MeshCollider    collider;

		Mesh			hitmesh;


		ushort[]    triangleToPartIdList;	// 面インデックスからパーツＩＤへの変換リスト

		int[]		colliderTriangles;		// 破壊メッシュのインデックス配列（破壊があって初めて確保される）

		int			triangleRemaining;		// 未破壊の残三角形数


		bool    isColliderBrokenInFrame;



		public StructureMeshBreakController3( _Structure3 s )
		{

			structure = s;

		}


		//public void init( MeshCollider c, byte[] idList, _StructurePart3.enType t )
		public void init( MeshCollider c, ushort[] idList, _StructurePart3.enType t )
		{

			collider = c;

			triangleToPartIdList	= idList;

			triangleRemaining		= idList.Length;

			type = t;


			if( !structure.isReplicated )
			{
				// コライダのメッシュをシェアしてないものは、最初からメッシュ作成済みとする。

				colliderTriangles = collider.sharedMesh.triangles;
			
				hitmesh = collider.sharedMesh;

			}
			else if( false )
			{
				// 最初からメッシュ作ってしまうモード

				colliderTriangles = collider.sharedMesh.triangles;
			

				var oldmesh = collider.sharedMesh;

				var newmesh = new Mesh();

				newmesh.MarkDynamic();

				newmesh.bounds		= oldmesh.bounds;
				newmesh.vertices	= oldmesh.vertices;
				newmesh.triangles	= colliderTriangles;

				collider.enabled = false;
				collider.sharedMesh = newmesh;
				collider.enabled = true;

				hitmesh = newmesh;

			}
		}


		public bool isBroken
		{

			get { return triangleRemaining <= 0; }

		}

		public void downMeshCollider()
		{

			collider.enabled = false;

		}

		public int triIdToPartId( int triId )
		{

			return triangleToPartIdList[ triId ];

		}
	
	
	
		/// <summary>
		/// あたり判定メッシュ上において、パーツを構成する面の大きさをゼロにする。
		/// </summary>
		/// <param name="part">破壊したいパーツ</param>
		public void breakPartInHitMesh( _StructurePart3 part )
		{

			if( part.hitIndexInfo.isEmpty ) return;


			if( colliderTriangles == null )
			{
				// 初回はメッシュ生成
				// 案：パーツコンテンツがホルダーの場合はメッシュ生成なしでいけるんじゃないのか？
				// 　　あと結構メッシュ生成が重い気がするので、あらかじめ全部作ってしまうのはダメだろうか…

				var oldmesh = collider.sharedMesh;  // これはちゃんと同じメッシュを受け取ってるっぽい
													//Debug.Log( oldmesh.GetInstanceID() );

				var newmesh = new Mesh();

				newmesh.MarkDynamic();

				newmesh.bounds		= oldmesh.bounds;
				newmesh.vertices	= oldmesh.vertices;

				collider.sharedMesh = newmesh;// enable をオフオンするまで実際には切り替わらなかったように思う
			
				hitmesh = newmesh;


				colliderTriangles = oldmesh.triangles;  // これは実際には get で配列生成されてるっぽい（クローン不要）

			}


			// 三角形の大きさをゼロにする（全インデックスが 0 を指すようにする）。

			Array.Clear( colliderTriangles, part.hitIndexInfo.startTriangle * 3, part.hitIndexInfo.triangleLength * 3 );
		

			// 残面数を割り出す。

			triangleRemaining -= part.hitIndexInfo.triangleLength;



			if( isBroken )
			{
				// 全破壊された場合、コライダをオフにするだけで済ませる。

				downMeshCollider();

			}
			else if( !isColliderBrokenInFrame )
			{
				// フレーム中に複数回更新されるのをふせぐ
			
				structure.StartCoroutine( applyBrokenCollider() );

			}

		}


		/// <summary>
		/// 更新された破壊済みメッシュを適用する。
		/// </summary>
		IEnumerator applyBrokenCollider()
		{

			isColliderBrokenInFrame = true;

			yield return new WaitForSeconds( UnityEngine.Random.Range( 0.02f, 0.25f ) );
			//yield return new WaitForFixedUpdate();


			if( collider.enabled )
			{
				//Debug.Log(hitter.tfStructure.name);

				if( isBroken )
				{

					// 全破壊された場合、コライダをオフにするだけで済ませる。
					// （完全破壊時のテンソル計算エラーの防止でもある）

					downMeshCollider();

				}
				else
				{

					collider.enabled = false;

					hitmesh.triangles = colliderTriangles;

					collider.enabled = true;

				}

			}

			isColliderBrokenInFrame = false;

		}


	}


}
