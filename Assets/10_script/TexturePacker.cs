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

		public static Texture PackTextureAndTranslateUv( IEnumerable<GameObject> targetObjects )
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( targetObjects ).ToList();

			var qUvs_PerMesh =
				from x in mmts
				select x.mesh.uv
				;

			var qDstMats = (from x in mmts select x.mats)
				.To(MaterialCombined.Combine);
			
			var texs = (from mat in qDstMats select mat.mainTexture).Cast<Texture2D>().ToArray();
			
			//var atlas = new Texture2D( width:0, height:0, textureFormat:TextureFormat.ARGB32, mipChain:false );
			var dstTexture = new Texture2D( 0, 0 );
			var uvRects = dstTexture.PackTextures
				( texs, padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true );


			var qPerDstMat =
				from xy in (uvRects, qDstMats).Zip()
				select (rect: xy.x, hash: xy.y.GetHashCode())
				;

			var qBaseVtxss =
				from x in mmts
				select
					from isub in Enumerable.Range(0, x.mesh.subMeshCount)
					select (int)x.mesh.GetBaseVertex(isub)
				;

			var qVtxCountss = (from x in mmts select x.mesh)
				.To(PerSubMeshPerMesh.QueryVertexCount);

			var qHashess = (from x in mmts select x.mats)
				.To(PerSubMeshPerMesh.QueryMaterialHash);

			var qPerSubPerMesh =
				from perMesh in (qVtxCountss, qBaseVtxss, qHashess).Zip()
				select
					from perSub in perMesh.Zip()
					select (vtxCount: perSub.x, baseVtx: perSub.y, hash: perSub.z) into perSub
					join mat in qPerDstMat on perSub.hash equals mat.hash
					select (mat.rect, perSub.vtxCount, perSub.baseVtx)
				;

			var qUvsTranslated_PerMesh =
				from perMesh in (qUvs_PerMesh, qPerSubPerMesh).Zip()
				select (uvs: perMesh.x, perSub: perMesh.y) into perMesh
				select
					from perSub in perMesh.perSub
					from iuv in Enumerable.Range( perSub.baseVtx, perSub.vtxCount )
					let rect = perSub.rect
					let uv = perMesh.uvs[iuv]
					select new Vector2
					{
						x = rect.x + uv.x * rect.width,
						y = rect.y + uv.y * rect.height,
					}
				;

			
			var qMeshTraverse =
				from xy in (from x in mmts select x.tf, qUvsTranslated_PerMesh).Zip()
				select (dstTf: xy.x, srcUvs: xy.y)
				;

			foreach( var perMesh in qMeshTraverse )
			{
				if( perMesh.dstTf.GetComponent<SkinnedMeshRender>()?.mesh)
				var mesh = perMesh.dstMf.mesh;
				mesh.SetUVs( 0, perMesh.srcUvs.ToList() );
				perMesh.dstMf.sharedMesh = mesh;
			}

			return dstTexture;
		}

	}
}

