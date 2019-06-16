using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Abss.StructureObject
{
	public interface IStructurePart
	{

		Task BuildAsync();

		bool FallDown( _StructureHit3 hitter, Vector3 force, Vector3 point );

	}

	public enum StructurePartType
	{
		inhittable,	// ヒットメッシュは必要ない、ドローメッシュだけでいい物体。	※視界を遮らない。
		massive,	// 最も一般的な、触れて見える物体。						※視界を遮る。
		occlusion,	// 見えるけど、触れない物体。							※視界を遮る。
		invisible,	// 触れるけど、見えないもの。ガラスなど。					※視界を遮らない。
		fence,		// 見えて触れるけど、弾丸がヒットしない。					※視界を遮る。
	}
	// 描画／可視／点接触／面接触という要素で分類すると、複数のヒットメッシュに同じ面が存在するため非効率。
	// 例えば「可視＆点接触」といった場合に、その両方のヒットメッシュに面を存在させるのか？ということ。
	// 複数のヒットメッシュに存在させるということは、データの重複だけでなく、検出も二度発生する。
}
