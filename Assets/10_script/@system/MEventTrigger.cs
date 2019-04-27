using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;






namespace MEvent
{


	/// <summary>
	/// イベント発信者にイベント条件を登録し、イベント発生時に条件チェックする。
	/// </summary>
	public class Trigger<Ttarget> : ConditionLinkBase<Ttarget> where Ttarget : IEventable
	{


		// イベント条件のセット／取得  - - - - - -

		
		/// <summary>
		/// 先頭の要素を返す。要素がなければ null が返る。
		/// </summary>
		protected Condition<Ttarget> linkTop
		{
			get { return this.next; }
		}


		/// <summary>
		/// 最後の要素を返す。要素がなければ null が返る。
		/// ループで取得するので、返った値はキャッシュするべき。
		/// </summary>
		protected Condition<Ttarget> linkLast
		{
			get
			{
				for( var c = linkTop ; c != null ; c = c.next ) if( c.next == null ) return c;

				return null;
			}
		}


		/// <summary>
		/// 先頭に条件をセットする。
		/// </summary>
		/// <param name="co">セットするイベント条件</param>
		protected void setTop( Condition<Ttarget> co )
		{
			this.setNext( co );
		}



		// 登録 - - - - - -

		
		/// <summary>
		/// 新しい条件を追加登録する。
		/// </summary>
		/// <param name="co">登録するイベント条件</param>
		public void regist( ConditionLinkBase<Ttarget> co )
		{
			this.setTop( co.linkToChain( this.next ) );
		}


		/// <summary>
		/// 他のトリガーの持つ条件を、自分の先頭にすべて挿入する形で登録する。
		/// </summary>
		/// <param name="other">挿入するトリガー</param>
		public void regist( Trigger<Ttarget> other )
		{
			if( other.linkTop == null ) return;

			other.linkLast.setNext( this.linkTop ); // 相手の条件の最後に、自分の条件を繋げる。

			this.setNext( other.linkTop );			// 自分の条件の先頭に、相手の条件を入れる。
		}



		// チェック - - - - - -

		
		/// <summary>
		/// 登録してある条件をすべてチェックし、役目を終えたものはチェインから外す。
		/// </summary>
		public void check( Ttarget obj )
		{

			ConditionLinkBase<Ttarget> prev = this;

			for( var c = linkTop ; c != null ; c = c.next )
			{

				c.check( obj );


				if( c.isComplited )
				{
					prev.linkToChain( c.linkToChain( null ) );
				}


				prev = c;
			}

		}

	}





	/// <summary>
	/// タイマーに特化させたトリガー。
	/// </summary>
	public class TimerTrigger : Trigger<IEventable>
	{
		public void setTimer()
		{
			for( var c = linkTop ; c != null ; c = c.next )
			{

				var co = (Timer.OnOver)c;

				GM.startCoroutine( co.setTimer() );

			}
		}
	}





	// トリガーホルダー ==========================================================


	/// <summary>
	/// イベント生成したいオブジェクトにトリガーを所持させる。null チェックなどするためのクッション。
	/// </summary>
	public struct TriggerHolder<Ttarget> where Ttarget : IEventable
	{


		Trigger<Ttarget> trigger;



		/// <summary>
		/// トリガーをまるごと受け取る。すでにトリガーがある場合は、既存条件の先頭に挿入される。
		/// </summary>
		/// <param name="t">所持させたいトリガー</param>
		public void receiveTrigger( Trigger<Ttarget> otherTrigger )
		{
			if( otherTrigger == null ) return;

			if( this.trigger != null )
			{
				this.trigger.regist( otherTrigger );
			}
			else
			{
				this.trigger = otherTrigger;
			}
		}





		/// <summary>
		/// トリガーの条件をチェックする。
		/// </summary>
		/// <param name="obj">条件の対象となる IEventable オブジェクト</param>
		public void check( Ttarget obj )
		{
			if( trigger != null )
			{
				trigger.check( obj );
			}
		}




		/// <summary>
		/// トリガーを破棄する。
		/// </summary>
		public void destruct()
		{

			trigger = null;

		}


	}







	// イベントターゲットプロキシ ==================================================





	/// <summary>
	/// イベントターゲットはこれを継承する。
	/// </summary>
	public interface IEventable
	{ }






	/// <summary>
	/// 条件をターゲットオブジェクトに関連付ける際に、まだオブジェクトが存在していない場合の一時登録場所。
	/// イベントクリエイション時に、条件登録済みトリガーを保持する。
	/// ハンドラでオブジェクトが生成された時に、トリガーを引き渡す代理人。
	/// </summary>
	public abstract class TargetProxy<Ttarget> where Ttarget : IEventable
	{

		TimerTrigger	timerTrigger;

		protected Trigger<Ttarget>[] triggers { private set; get; }



		/// <summary>
		/// イベントターゲットのトリガー数。実装で具体的に設定する。
		/// </summary>
		protected abstract int triggerCountDefine { get; }





		// 条件の登録 ---------


		/// <summary>
		/// トリガーをイベントターゲットへ登録する。
		/// 領域確保は登録時にチェックし行う。
		/// </summary>
		/// <param name="i">トリガー位置</param>
		/// <param name="co">登録したい条件</param>
		public void registCondition( int i, Condition<Ttarget> co )
		{
			if( i > triggerCountDefine ) return;

			if( triggers == null ) triggers = new Trigger<Ttarget>[ triggerCountDefine ];

			if( triggers[ i ] == null ) triggers[ i ] = new Trigger<Ttarget>();

			triggers[ i ].regist( co );
		}



		/// <summary>
		/// タイマー条件をイベントターゲットへ登録する。
		/// 領域確保は登録時にチェックし行う。
		/// </summary>
		/// <param name="co">登録したいタイマー条件</param>
		/// 
		public void registCondition( Timer.OnOver co )
		{
			if( timerTrigger == null ) timerTrigger = new TimerTrigger();

			timerTrigger.regist( co );
		}





		// イベントターゲットへの引き渡し -----------

		/// <summary>
		/// イベントターゲットへトリガーを引き渡す処理を実装する。
		/// どの位置のトリガをターゲットのどのメンバへ渡すかは各ターゲットの実装のメンバ名次第となる。
		/// </summary>
		/// <param name="obj">イベントターゲット</param>
		/// 
		protected abstract void passTriggersToTarget( Ttarget obj );



		/// <summary>
		/// イベントターゲットへのトリガー引き渡しを行う。
		/// ハンドラ実装でイベントターゲットを生成した時などに呼ぶ。
		/// </summary>
		/// <param name="obj">イベントターゲット</param>
		/// 
		public void passTriggersTo( Ttarget obj )
		{
			if( timerTrigger != null ) timerTrigger.setTimer();

			if( triggers != null ) passTriggersToTarget( obj );
		}

	}




}