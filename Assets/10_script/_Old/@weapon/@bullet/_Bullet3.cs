using UnityEngine;
using System.Collections;

public abstract class _Bullet3 : _Emitter3
{



	public _Action3	owner	{ get; protected set; }







	// 静的 ----------------------------------


	static protected RaycastHitWork  hits;

	static _Bullet3()
	{
		hits = new RaycastHitWork( 64 );
	}




	/// <summary>
	/// コライダを担当する _Hit3 を取得する。attachedRigidbody -> 親 GameObject の順番で探すの注意。
	/// </summary>
	static public _Hit3 getHitter( Collider c )
	{

		if( c.attachedRigidbody != null )
		// Rigidbody がついているならその hitter を優先する。
		// 本来ならコライダの hitter をまず見るほうが自然だが、速度的に有利なほうを選択する。
		{
			return c.attachedRigidbody.GetComponentInParent<_Hit3>();
			//return c.attachedRigidbody.GetComponent<_Hit3>() ?? c.GetComponent<_Hit3>();
		}
		else
		{
			return c.GetComponent<_Hit3>();
		}

	}


	/// <summary>
	/// factor のかかり具合を rate で調整するユーティリティ。
	/// </summary>
	static protected float effectFactor( float factor, float rate )
	{
		return 1.0f + ( factor - 1.0f ) * rate;
	}



	/// <summary>
	/// オウンヒットを除外して、至近距離でもヒット判定を成立させたい場合に使用する。
	/// ヒットしていれば、一番近距離のＩＤを返す。ヒットしていない・オウンヒットのみ、の場合は -1 を返す。
	/// </summary>
	static public int getOtherHitIdForOwnHittable( int hitLength, RaycastHit[] hits, _Action3 owner )
	{

		for( var i = 0 ; i < hitLength ; i++ )
		{

			var hitter = getHitter( hits[ i ].collider );

			if( hitter == null || hitter.getAct() != owner ) return i;

		}

		return -1;

	}











	// オブジェクト ---------------------------------------------


	public struct DamageSourceUnit
	// hitter などにダメージを渡す時に一時的に使用する。
	{
		
		public float	damage;				// 威力。
		
		public float	heavyRate;			// 回復不可能な割合。

		public float	fragmentationRate;	// 内部耐久力への威力倍率。軟弾は断裂して高くなり、硬弾は貫通して低くなる。
		
		public float	moveStoppingDamage;	// 移動へのダメージ。歩行が 1.0f で、緊急回避などは初期値で 20.0f とかある。

		public float	moveStoppingRate;	// 移動ダメージが適用された場合の、威力から差し引かれる割合。

		//public _Hit3.enMaterial	penetratableArmorClass;	// 貫通可能なアーマークラス。


		public DamageSourceUnit( float dmg )
		{
			damage = dmg;
			heavyRate = 1.0f;
			fragmentationRate = 1.0f;
			moveStoppingDamage = 0.0f;
			moveStoppingRate = 0.0f;
		}
		
		public DamageSourceUnit( float dmg, float fmr )
		{
			damage = dmg;
			heavyRate = 1.0f;
			fragmentationRate = fmr;
			moveStoppingDamage = 0.0f;
			moveStoppingRate = 0.0f;
		}

		public DamageSourceUnit( float dmg, float hvr, float fmr, float msd, float msr )
		{
			damage = dmg;
			heavyRate = hvr;
			fragmentationRate = fmr;
			moveStoppingDamage = msd;
			moveStoppingRate = msr;
		}

		public DamageSourceUnit( float dmg, float msd, float msr )
		{
			damage = dmg;
			heavyRate = 1.0f;
			fragmentationRate = 1.0f;
			moveStoppingDamage = msd;
			moveStoppingRate = msr;
		}

	}







	/// <summary>
	/// RaycastHit のワーク。NonAlloc タイプのキャストと併せて使用する。
	/// </summary>
	protected struct RaycastHitWork
	{

		public RaycastHit[] array { get; private set; }	// あたり判定用に使用していい


		public RaycastHitWork( int length )
		{

			array = new RaycastHit[ length ];  

        }
		

		/// <summary>
		/// 至近距離でもヒット判定を成立させたい弾丸で使用する
		/// （オウンヒットしたら無効でもよい場合は普通のキャストでＯＫ）。
		/// this.array から、オウンヒットしていない一番近距離のＩＤを返す。
		/// ヒットしていない・オウンヒットのみ、の場合は -1 を返す。
		/// </summary>
		public int getOtherHitIdForOwnHittable( int hitLength, _Action3 owner )
		{

			return _Bullet3.getOtherHitIdForOwnHittable( hitLength, array, owner );

		}

	}










}
