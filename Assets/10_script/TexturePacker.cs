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
		
		//public static Texture PackTextureAndTranslateUv( IEnumerable<GameObject> targetObjects )
		//{
		//	var mmts = MeshExtractionUtility.QueryMeshMatsTransform_IfHaving( targetObjects ).ToList();

		//	var matHashToIdxDict = ( from x in mmts select x.mats )
		//		.To(MaterialUtility.QueryMatNameAndHash_EverySubmeshesEveryMeshes)
		//		.To(MaterialUtility.ToDictionaryForMaterialHashToIndex);

		//	var vtxCount_EverySubmeshes = ( from x in mmts select x.mesh )
		//		.To(VertexUtility.QueryVertexCount_EverySubmeshes);

		//	var qFlatMats =
		//		from x in mmts
		//		from mat in x.mats
		//		select mat
		//		;

		//	var qTextures =
		//		from mat in qFlatMats
		//		select mat.mainTexture
		//		;
		//	//var atlas = new Texture2D( width:0, height:0, textureFormat:TextureFormat.ARGB32, mipChain:false );
		//	var dstTexture = new Texture2D( 0, 0 );
		//	var flatUvRects = dstTexture.PackTextures(
		//		qTextures.Cast<Texture2D>().ToArray(), padding: 0, maximumAtlasSize: 4096,
		//		makeNoLongerReadable: false
		//	);

		//	//var q =
		//	//	from x in (qFlatMats, flatUvRects).Zip( (x,y)=>(mat:x, uvs:y) )
		//	//	select 


		//	return null;
		//}

	}
}

