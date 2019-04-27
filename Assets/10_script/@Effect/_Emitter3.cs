using UnityEngine;
using System.Collections;

public abstract class _Emitter3 : _PoolingObject3<_Emitter3>
{






	// 継承先で上書きしない限りはプール型の final()

	public override void final()
	{
		releaseSelfToPoolOrDestroy();

		base.final();
	}




	public abstract void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null );
	// 放出する。これを継承する。戻り値で発射物体を返したいところだが、扱うオブジェクトがそれぞれ異なるのでやめとく。


	public void emit( Vector3 pos, Vector3 normal, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{
		
		var rot = Quaternion.LookRotation( normal );

		emit( pos, rot, rangeFactor, sizeFactor, numCount, act, tfMuzzle );
		
	}



	// オプションの値はすべて「省略ならデフォルトを採用する」との意志のもと運用する。
	// rangeFactor と sizeFactor は省略すると 1.0f となり、普通に掛け算すればよい。
	// numCount は 0 となるので、その時はデフォルト値を採用するロジックが必要となる。



	public void emit( Vector3 pos, Quaternion rot, _Action3 act )
	{
		emit( pos, rot, 1.0f, 1.0f, 0, act );
	}

	public void emit( Vector3 pos, Quaternion rot )
	{
		emit( pos, rot, 1.0f, 1.0f, 0, null );
	}

	public void emit( Vector3 pos, Quaternion rot, float rangeFactor )
	{
		emit( pos, rot, rangeFactor, 1.0f, 0, null );
	}

	public void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor )
	{
		emit( pos, rot, rangeFactor, sizeFactor, 0, null );
	}

	public void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount )
	{
		emit( pos, rot, rangeFactor, sizeFactor, numCount, null );
	}


	public void emit( Vector3 pos, Vector3 normal, _Action3 act )
	{
		emit( pos, normal, 1.0f, 1.0f, 0, act );
	}

	public void emit( Vector3 pos, Vector3 normal )
	{
		emit( pos, normal, 1.0f, 1.0f, 0, null );
	}

	public void emit( Vector3 pos, Vector3 normal, float rangeFactor )
	{
		emit( pos, normal, rangeFactor, 1.0f, 0, null );
	}

	public void emit( Vector3 pos, Vector3 normal, float rangeFactor, float sizeFactor )
	{
		emit( pos, normal, rangeFactor, sizeFactor, 0, null );
	}

	public void emit( Vector3 pos, Vector3 normal, float rangeFactor, float sizeFactor, int numCount )
	{
		emit( pos, normal, rangeFactor, sizeFactor, numCount, null );
	}






}
