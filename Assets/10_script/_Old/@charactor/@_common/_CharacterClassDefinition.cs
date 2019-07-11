using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class _CharacterClassDefinition : ScriptableObject
{

	public _Action3Enemy	template;

	public Material			material;


	public float	sizeScale;	// 雛形の何倍のボディサイズか


	public float	groundHookReachRate;
	// 足を延ばして足場に引っ付ける距離（胴体の中心から）を figure.bodyRadius への倍率で設定


	public float	dext;		// 最大旋回性能（度／秒）
	public float	quickness;	// 基本速度倍率、アニメーションの速度もだいたいこれで。
	public float	reach;		// 攻撃に転ずる間合いの基本値 これは武器ごとでもいいか？

	public float attackTiming;	// 0.00f ～ 0.07f の範囲で遅延可能、回避のしやすいタイミングで！（使用時 quickness 等の補正が入る）



	public _Armor3	armorDefinition;

	public float	durability;





	public _Action3 instantiate( Vector3 pos, Quaternion rot )
	{

		var baseInstance = getBaseInstance( true );


		var act = (_Action3Enemy)baseInstance.instantiateWithPoolingGameObject( pos, rot, true );

		act.def = this;

		act.init();
		
		
		return act;
		
	}


	public _Action3Enemy getBaseInstance( bool isCreateWithTexturePack )
	{

		var isFirst = template.isUnused;

		if( isFirst )
		{

			var baseInstance = (_Action3Enemy)template.getBaseInstance();


			baseInstance.combineMesh( isCreateWithTexturePack );

			//tfmeshbody.GetChild(0).GetComponent<Renderer>().sharedMaterial = material;


		//	addphys(baseInstance);// 剛体付加テスト

			baseInstance.init();

			baseInstance.deadize();

			baseInstance.gameObject.SetActive( false );

		}

		return (_Action3Enemy)template.getBaseInstance();
	}
	void addphys( _Action3Enemy act )// テスト
	{
		// 剛体を各コライダに作ると高速化せず、逆に多少遅くなるみたい。
		var cols = act.tfBody.GetComponentsInChildren<Collider>()
			.Where( x => x.gameObject.layer == UserLayer._enemyDetail )
			.Where( x => x.gameObject.GetComponent<Rigidbody>() == null );
		foreach( var col in cols )
		{
			var rb = col.gameObject.AddComponent<Rigidbody>();
			rb.isKinematic = true;
		}
	}

	
	public abstract void init( _Action3Enemy act );

}
// メッシュ単一化とてくすちゃパックもあるし、ひな形はホルダーでつくったほうがいいかもね
// （ミッションスタートの時に全部のキャラのひな形を作った方がいい）

// クラス定義からぱらめーたを書き込んで、その後アクティブにする、がいいかも

