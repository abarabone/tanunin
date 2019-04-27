using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;




// Trigger -> Condition -> Handler




namespace MEvent
{




	public static class MEventMaster
	{

		/// <summary>
		/// イベントハンドラがまだ存在しているか。
		/// </summary>
		static public bool hasEvents { get { return eventCounter > 0; } }

		static int eventCounter; // キャラクタ発生コルーチンの現存数


		/// <summary>
		/// イベントハンドラ生成通知
		/// </summary>
		static public void notifyCreate()
		{
			eventCounter++;
		}

		/// <summary>
		/// イベントハンドラ破棄通知
		/// </summary>
		static public void notifyDestruct()
		{
			eventCounter--;
		}

	}


















	// 条件・ハンドラの基底 ===============================================================



	// ハンドラの基底 -------------------------

	/// <summary>
	/// イベントが発火された時に実行される内容。
	/// </summary>
	public abstract class Handler
	{



		/// <summary>
		/// 条件を追加する。
		/// </summary>
		/// <param name="co">追加するイベント条件</param>
		/// 
		public Handler attach<Ttarget>( Condition<Ttarget> co ) where Ttarget : IEventable
		{
			co.attachToEvent( this );

			return this;
		}



		/// <summary>
		/// Aggregator を追加する。
		/// </summary>
		/// <param name="ag">追加するイベント条件</param>
		/// 
		public Handler attatch( Aggregator ag )
		{
			ag.attachToEvent( this );

			return this;
		}



		// ハンドラを実行する。条件が成立した場合に呼び出される。

		public abstract void execute();

	}




	/// <summary>
	/// マスターへの生成／破棄通知の付いたハンドラ。
	/// 実行前タイマー機能も搭載する。
	/// </summary>
	public abstract class NotificationHandler : Handler
	{


		/// <summary>
		/// イベントハンドラが実行されるまでの猶予時間。
		/// </summary>
		public float	waitTime;





		public NotificationHandler()
		{
			MEventMaster.notifyCreate();
		}



		public override void execute()
		{
			if( waitTime > 0.0f )
			{
				GM.startCoroutine( _executeOnTimer() );
			}
			else
			{
				behavior();

				MEventMaster.notifyDestruct();
			}
		}



		IEnumerator _executeOnTimer()
		{//Debug.Log( "wait time " + t + " " + mevent );

			yield return new WaitForSeconds( waitTime );
			
			behavior();

			MEventMaster.notifyDestruct();

		}



		// ハンドラ実装。execute() によって実行されるふるまい。

		public abstract void behavior();

	}







	// 条件の基底 -------------------------------


	/// <summary>
	/// イベントハンドラにセットできるもの。
	/// </summary>
	public interface ICondition
	{
		void attachToEvent( Handler ev );
	}



	/// <summary>
	/// イベント条件機能の基底クラス。
	/// </summary>
	public abstract class Condition<Ttarget> : ConditionLinkBase<Ttarget>, ICondition where Ttarget : IEventable
	{

		protected Handler	e;

		public void attachToEvent( Handler ev )
		{
			e = ev;
		}



		public bool isComplited { get { return e == null; } }

		public void check( Ttarget obj )
		{
			if( isComplited ) return;	// すでに役目を終えていた場合は何もしない。

			if( isSatisfied( obj ) )
			{
				e.execute();

				e = null;
			}
		}


		/// <summary>
		/// 条件判定。
		/// </summary>
		/// <param name="obj">条件判定に使用するオブジェクト</param>
		/// <returns>条件が成立したなら True</returns>
		protected abstract bool isSatisfied( Ttarget obj );

	}


	
	/// <summary>
	/// イベント条件にチェインリンク機能を持たせるためのクラス。
	/// すべてのイベント条件の基底クラスとなる。
	/// </summary>
	public class ConditionLinkBase<Ttarget> where Ttarget : IEventable
	{

		public Condition<Ttarget> next { get; private set; }


		public Condition<Ttarget> linkToChain( Condition<Ttarget> newNext )
		{
			var prev = next;

			next = newNext;

			return prev;
		}

		public void setNext( Condition<Ttarget> co )
		{
			next = co;
		}

	}





	// 条件のハブ ----------------------

	
	/// <summary>
	/// 条件とイベントの間に挟んで、複数条件を可能にする。
	/// </summary>
	public class Aggregator : Handler, ICondition
	{

		Handler	e;


		public void attachToEvent( Handler ev )
		{
			e = ev;
		}


		public override void execute()
		{
			if( count-- == 0 )
			{
				e.execute();

				e = null;
			}
		}




		int count;
		

		/// <summary>
		/// 条件がいくつ成立したら発火するかの閾値を設定する。
		/// </summary>
		/// <param name="threshold">閾値</param>
		/// 
		public void setThreshold( int threshold )
		{
			count = threshold;
		}


	}






	




	// 各種条件・ハンドラ継承用基底 ===============================================================

	
	/// <summary>
	/// ハンドラにプロキシ発行（生成）のインターフェースを持たせる。
	/// </summary>
	/// <typeparam name="Tproxy">イベントターゲットプロキシ</typeparam>
	/// <typeparam name="Ttarget">イベントターゲット</typeparam>
	/// 
	public abstract class TargetPublishableHandler<Tproxy, Ttarget>
		: NotificationHandler where Ttarget : IEventable, new() where Tproxy : TargetProxy<Ttarget>, new()
	{

		protected Tproxy	proxy;


		/// <summary>
		/// イベントターゲットプロキシ発行（生成）。
		/// </summary>
		/// <returns>イベントターゲットプロキシ</returns>
		public Tproxy publish()
		{
			return proxy = new Tproxy();
		}





		/// <summary>
		/// 生成されたイベントターゲットを取得する。生成されていない場合は null が返るので注意。
		/// セットはハンドラ実装で生成した時に行う。
		/// </summary>
		public Ttarget	target { get; protected set; }
		


		
	}






	


}


