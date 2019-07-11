using UnityEngine;
using System.Collections;

public abstract class _PoolingObject3<T> : MonoBehaviour where T:_PoolingObject3<T>
{


	public Transform	tf	{ get; private set; }



	
	
	
	protected void Awake()
	{
		//if( isUsing )
		{
		//	deepInit();
			tf = transform;//
		}
	}
	
	protected void OnDestroy()
	{
		//if( isUsing )
		{
		//	deepFinal();
		}
	}
	
	/*
	protected virtual void deepInit()
	{//Debug.Log(this+"di");
		tf = transform;
	}
	
	protected virtual void deepFinal()
	{}//Debug.Log(this+"df");}
	*/
	
	public virtual void init()
	{}//Debug.Log(this+"i");}
	
	public virtual void final()
	{}//Debug.Log(this+"f");}





	// 課題
	// プーリング方式とベース方式で別のクラスにしたほうがよくない？　一緒のほうが都合いい点あるんだっけ？
	// 　→継承先でプールとベースに分岐とか自由にできるメリットはある
	// init() は OnEnable() ではなくて Start() と instantiateWithPoolingGameObject() 内にしたほうがよくない？　isActive もいらなくなるし。
	// 　ああ、でもプーリングしない場合は OnEnable() と OnDisable() が必要なのか？　いやそもそも、active かどうかで init()/final() を実行するのはよくないかも。
	// 　release() みたいなのを呼ぶようにして、そこから final() を呼んで destroy() にするか releaseSelfToPool() するかを自動で決定したほうがいいのかも？
	// 　もしくは、OnDestroy() 内でプールインスタンスじゃない場合のみ final() を呼ぶようにしてもいい？
	// あとプレハブから呼ぶ関数に clean() を付けるべきだな。

	// あーでも
	// active/deactive がオブジェクトのオン／オフなわけだから、その切り替えの時に init()/final() を呼ぶのはいいんじゃないかな
	// プレハブからインスタンス作った場合に、いきなり active だと事前に細工ができないのは問題
	// 　なので、プレハブを deactive にしちゃってそれをインスタンス化するよ、って決めちゃっていいと思う
	// 　またはもともとの active/deactive を保存しといてオフにしてすぐもどす、とか


	// 現状
	// OnEnable() と OnDisable() で init()/final() 呼ぶのやめました。
	// むしろ使う側は init() と final() を呼びなさいという方針にしました。
	// final() の中で破棄とかリリースとかすべきで、そうなるとやっぱぷーリングとベースを分けるべきなのかなぁと。



	protected _PoolingObject3<T>	poolingLink;


	public bool isUnused		{ get { return poolingLink == null; } }
	public bool isUsing			{ get { return poolingLink != null; } }



	// 足りない場合はインスタンスを作り、以後はプーリングして使いまわし続ける -------------------------------------------

	public _PoolingObject3<T> instantiateWithPoolingGameObject( Vector3 pos, Quaternion rot )
	// プレハブから呼ばれる。holder は自分自身のこと。
	{
		
		_PoolingObject3<T>	instance;


		var holder = this;

		if( holder.isUsing )
		{
			
			instance = holder.poolingLink;
			
			holder.poolingLink = instance.poolingLink;
			

			instance.tf.position = pos;
			
			instance.tf.rotation = rot;

		}
		else
		{

			instance = (_PoolingObject3<T>)Instantiate( holder, pos, rot );

		}
		
		
		instance.poolingLink = holder;

		return instance;
		
	}

	public _PoolingObject3<T> instantiateWithPoolingGameObject( Vector3 pos, Quaternion rot, bool isActive )
	{

		var instance = instantiateWithPoolingGameObject( pos, rot );

		instance.gameObject.SetActive( isActive );

		return instance;

	}

	
	protected void releaseSelfToPoolOrDestroy()
	// インスタンスから呼ばれる。holder はリンク先になる。
	// ベースインスタンスから呼ぶと、破棄となる。
	// テンプレートから呼んではいけない。
	{

		var instance = this;

		if( instance.isUsing )
		{

			gameObject.SetActive( false );


			var holder = this.poolingLink;

			instance.poolingLink = holder.poolingLink;

			holder.poolingLink = instance;

		}
		else
		{

			Destroy( gameObject );

		}

	}



	
	
	// ベースインスタンスを一個だけつくり、以後はそれを渡し続ける -------------------------------------------
	
	public _PoolingObject3<T> getBaseInstance( Vector3 pos, Quaternion rot )
	{
		
		var template = this;
		

		if( template.isUsing )
		{
			
			var tf = template.poolingLink.tf;
			
			tf.position = pos;
			
			tf.rotation = rot;

		}
		else
		{

			var baseInstance = (_PoolingObject3<T>)Instantiate( template, pos, rot );

			template.poolingLink = baseInstance;

		}
		
		
		return (_PoolingObject3<T>)template.poolingLink;
		
	}
	
	public _PoolingObject3<T> getBaseInstance()
	{
		
		var template = this;
		
		
		if( template.isUsing )
		{



		}
		else
		{

			var baseInstance = Instantiate<_PoolingObject3<T>>( template );

			template.poolingLink = baseInstance;

		}

		
		return (_PoolingObject3<T>)template.poolingLink;
		
	}


}
