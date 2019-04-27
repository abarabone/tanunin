using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;



namespace MEvent
{

	// 各種条件・ハンドラ **************************************************************


	// タイマー ========================================

	public class Timer
	{

		// 条件 -------------------------

		/// <summary>
		/// とにかくトリガーチェックされたら成立する。
		/// </summary>
		public class OnOver : Condition<IEventable>
		{

			public float limitTime;


			public IEnumerator setTimer()
			{
				yield return new WaitForSeconds( limitTime );

				e.execute();

				e = null;
			}


			protected override bool isSatisfied( IEventable obj )
			{

				return true;

			}
		}

	}


	// グループ ========================================

	public static class ActivityGroupDefines
	{



		// プロキシ --------------------

		public class Proxy : TargetProxy<ActivityGroup3>
		{
			protected override int triggerCountDefine { get { return 1; } }

			protected override void passTriggersToTarget( ActivityGroup3 obj )
			{
				obj.memberEvent.receiveTrigger( triggers[ 0 ] );
			}
		}




		// ハンドラ ----------------------

		
		/// <summary>
		/// グループ生成。
		/// </summary>
		public class ToCreate : TargetPublishableHandler<Proxy, ActivityGroup3>
		{

			public Vector3		pos;

			public Quaternion	rot;

			public float		territory;

			public ActivityGroup3.enShape	shape;
			
			public int	maxStayCapacity;


			public override void behavior()
			{

				rot = rot == new Quaternion() ? Quaternion.identity : rot;

				target = GM.groups.create( pos, rot );


				target.territory = territory;

				target.shape = shape;
				
				target.maxStayCapacity = maxStayCapacity;


				//proxy.passTriggersTo( target );
				
			}

		}


		/// <summary>
		/// キャラクターを生成を伴うグループ生成。
		/// </summary>
		public class ToCreateWithSpawn : ToCreate
		{

			public CharacterFactory.EntryHolder entries;

			public CharacterFactory.SpawnInfo   spawn;


			public override void behavior()
			{

				base.behavior();

				spawn.group = target;

				CharacterFactory.beginSpawn( ref entries, ref spawn );

			}

		}


		/// <summary>
		/// 既存のグループからキャラクターを生成させる。
		/// </summary>
		public class ToSpawn : TargetPublishableHandler<Proxy, ActivityGroup3>
		{

			public CharacterFactory.EntryHolder entries;

			public CharacterFactory.SpawnInfo   spawn;


			public override void behavior()
			{

				if( target == null ) return;

				spawn.group = target;

				CharacterFactory.beginSpawn( ref entries, ref spawn );

			}

		}



		// 条件 -------------------------

		/// <summary>
		/// グループが破棄された。
		/// </summary>
		public class OnDestroyed : Condition<ActivityGroup3>
		{
			protected override bool isSatisfied( ActivityGroup3 obj )
			{

				return obj.memberCount < 1 & !obj.spawns.isSpawning;

			}
		}


	}



	// グループ ========================================

	public static class SpawnDefines
	{

		// プロキシ --------------------

		public class Proxy : TargetProxy<ActivityGroup3>
		{
			protected override int triggerCountDefine { get { return 1; } }

			protected override void passTriggersToTarget( ActivityGroup3 obj )
			{
				obj.memberEvent.receiveTrigger( triggers[ 0 ] );
			}
		}



		// ハンドラ ----------------------

		/// <summary>
		/// グループ生成。
		/// </summary>
		public class ToCreate : TargetPublishableHandler<Proxy, ActivityGroup3>
		{

			public CharacterFactory.EntryUnit[] entries;	// 出現種類・性格・重みの定義

			public int targetMemberCount;	// 最終的に出現させる数

			public int maxStayCapacity;	 // 同時に存在できる最大定員

			public float intervalTime;		// 出現間隔


			public override void behavior()
			{



			}
		}



		// 条件 -------------------------

		/// <summary>
		/// グループが破棄された。
		/// </summary>
		public class OnDestroyed : Condition<ActivityGroup3>
		{
			protected override bool isSatisfied( ActivityGroup3 obj )
			{

				return true;

			}
		}


	}








}