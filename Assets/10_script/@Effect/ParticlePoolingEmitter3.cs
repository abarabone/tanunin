using UnityEngine;
using System.Collections;

public class ParticlePoolingEmitter3 : _Emitter3
{

	// ライフサイクルがあったり、ローカルスペースだったり、撃ち出すごとにインスタンスが必要な場合に使用する。




	protected ParticleSystem	ps;



	//protected override void deepInit()
	protected void Awake()
	{

		base.Awake();
		

		ps = GetComponent<ParticleSystem>();

	}


	void Update()
	{

		if( !ps.IsAlive() )
		{
			ps.Stop();

			final();
		}

	}



	public override void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var ei = (ParticlePoolingEmitter3)instantiateWithPoolingGameObject( pos, rot, true );

		ei.init();

		
		// 指定された半径を反映して再生する。
		// 実際の大きさは、パーティクルで決められたサイズとの掛け合わせとなる。

		ei.sizing( sizeFactor, this );


		// ps は停止した状態でプールに戻されるので、単純に Play() でよい。

		ei.ps.Play();


		// 撃ち出す個数が指定されているなら放出する。

		if( numCount > 0 ) ei.ps.Emit( numCount );

	}

	protected void sizing( float size, ParticlePoolingEmitter3 emitter )
	{
		if( size > 0.0f & size != 1.0f )
		{

			var pps = emitter.GetComponent<ParticleSystem>();

			ps.startSize = pps.startSize * size;

			ps.startSpeed = pps.startSpeed * size;

			tf.localScale = Vector3.one * size;

		}
	}

}
