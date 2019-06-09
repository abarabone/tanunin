using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abss.Common.Extension;
using System.Linq;
using UniRx;
using System;
using System.Runtime;

namespace Abss.StructureObject
{
	/// <summary>
	/// StructureNear オブジェクトを描画するために必要なマテリアルプロパティをセットする。
	/// ・テクスチャ
	/// ・カラーパレット
	/// ・パーツオンオフ
	/// </summary>
	sealed public class StructureNearRenderingController : MonoBehaviour
	{

		Renderer rendererOfStructure;
		MaterialPropertyBlock mpb;


		// ビットフラグ更新通知用
		ISubject<Unit> visibilityFlagObserver = new Subject<Unit>();

		readonly static int propId_isVisibleFlags = Shader.PropertyToID("isVisibleFlags");
		

		// パーツの可視フラグをシェーダーへ転送するために、クッションとして使用。
		BitFlagVector4Holder visibilityFlags = new BitFlagVector4Holder( 1024 / 4 * 3 );
		// 最初から Vector4[] で管理するより、
		// パーツ単位でのフラグオンオフは int ／転送時に Vector4 に全コピー、としたほうがコストが低いとふんでのこと。
		// 32bit flag * 4 * 8 で 1024 bit を想定（現状 24bit だが）
		// ただし、SetInt4Array() が存在しないため、int イメージでは転送できない。
		// float にキャストしないといけない。つまり、0 ~ -1(32bit) ではなく、0 ~ 0xffffff(24bit) の値に限定される。


		/// <summary>
		/// 
		/// </summary>
		public void SetVisibilityFlags( int[] visibilityBitFlags )
		{
			// ビットフラグをベクターバッファにコピーする。
			this.visibilityFlags.CopyFromIntArray( visibilityBitFlags );

			// ＭＰＢに転送する。
			this.mpb.SetVectorArray( propId_isVisibleFlags, this.visibilityFlags.BitFlags );

			// 更新を通知する。
			this.visibilityFlagObserver.OnNext( Unit.Default );
		}


		public void Awake()
		{
			
			this.rendererOfStructure = this.GetComponent<Renderer>();

			this.mpb = new MaterialPropertyBlock();
			var c = new Vector4[] { new Vector4(1,0,0,1), new Vector4(1,1,0,1), new Vector4(1,1,1,1), new Vector4(0,1,1,1) };
			mpb.SetVectorArray( "_Pallet", c );

			this.visibilityFlagObserver
				.BatchFrame()// 3, FrameCountType.EndOfFrame )
				.Subscribe( _ => this.rendererOfStructure.SetPropertyBlock(this.mpb) )
				.AddTo( this.gameObject )
				;

		}



	}



	// 本来は 32bit flag としたいのだが、下記の理由により 24bit にせざるを得ない。
	// 理由：mpb に SetInt4Array() が存在しないため。int イメージでは転送できない。
	// また内部イメージを int にして SetVectorArray() で転送しても、
	// シェーダーに届く際には (float)int のキャスト的変換が行われてしまうようだ。（ (int)float なのかも、正確には不明） 
	// そのため、float の実数表現ぎりぎりの 24bit までを扱う。
	// つまり 0 ~ -1(32bit) ではなく、0 ~ 0xffffff(24bit) の値に限定する。

	/// <summary>
	/// シェーダーに転送するためのビットフラグを保持・操作する。
	/// </summary>
	public struct BitFlagHolder
	{
		public int[] BitFlags { get; private set; }

		/// <summary>
		/// 必要なビット要素の分だけ領域を確保する。その際、すべてのビットは真となる。
		/// </summary>
		public BitFlagHolder( int elementLegth )
		{
			this.BitFlags = allocAndFill_( elementLegth );
			return;

			int[] allocAndFill_( int elementLength_ )
			{
				var wordLength = elementLegth / 24;
				//var wordLength = elementLegth >> 5;// 32 bit

				return Enumerable.Repeat( 0xffffff, wordLength ).ToArray();
			}
		}

		public void flagOn( int elementId )
		{
			var wordIdx = elementId / 24;
			this.BitFlags[ wordIdx ] |= elementId << (elementId - wordIdx * 24);
		}
		public void flagOff( int elementId )
		{
			var wordIdx = elementId / 24;
			this.BitFlags[ wordIdx ] &= ~( elementId << (elementId - wordIdx * 24) );
		}
	}
	
	/// <summary>
	/// シェーダー転送に SetInt4Array() がないため、間のクッションとして Vector4 を使用する。
	/// </summary>
	public struct BitFlagVector4Holder
	{
		public Vector4[] BitFlags { get; private set; }

		public BitFlagVector4Holder( int elementLegth )
		{
			this.BitFlags = alloc_( elementLegth );
			return;

			Vector4[] alloc_( int elementLength_ )
			{
				var wordLength = elementLegth / 24;

				var vectorLength = wordLength >> 2;
				var isFit4 = ( wordLength & 0b11 ) == 0;
				if( !isFit4 ) vectorLength++;

				return new Vector4[ vectorLength ];
			}
		}

		public void CopyFromIntArray( int[] intBitFlags )
		{
			//Buffer.BlockCopy( intBitFlags, 0, this.BitFlags, 0, intBitFlags.Length * 4 >> 2 );
			for( var i = 0; i < intBitFlags.Length; i += 4 )
			{
				this.BitFlags[ i >> 2 ] = intBitFlags.ToVector4( i );
			}
		}
	}
}
