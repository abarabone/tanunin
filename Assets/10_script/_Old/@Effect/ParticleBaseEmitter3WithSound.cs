using UnityEngine;
using System.Collections;

public class ParticleBaseEmitter3WithSound : ParticlePoolingEmitter3WithSound
{

	// ワールドスペースＰＳを一個だけ置いて使いまわして撃ち出す、という使い方を想定している。

	// インスタンスがたくさん必要なわけでもないので、サウンドいらなくてもこれでいいでしょ。



	//protected override void deepInit()
	new protected void Awake()
	{

		base.Awake();

	}


	public override void init()
	{

		gameObject.SetActive( true );

	}

	void Update()
	{

		if( !ps.IsAlive() )
		{
			ps.Stop();

			gameObject.SetActive( false );
		}

	}




	
	public override void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var ei = (ParticleBaseEmitter3WithSound)getBaseInstance( pos, rot );

		ei.init();


		ei.sizing( sizeFactor, this );



		if( ei.ps.isStopped ) ei.ps.Play();

		var num = numCount > 0 ? numberOfEmit * numCount : numberOfEmit;

		ei.ps.Emit( num );


		playSound( pos );

	}


}
