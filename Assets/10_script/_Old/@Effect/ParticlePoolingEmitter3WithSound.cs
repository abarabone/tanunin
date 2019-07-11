using UnityEngine;
using System.Collections;

public class ParticlePoolingEmitter3WithSound : ParticlePoolingEmitter3
{



	public int		numberOfEmit;


	public AudioClip	emitSound;

	public float	range;

	public float	volume;



	public override void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		base.emit( pos, rot, rangeFactor, sizeFactor, numberOfEmit * numCount, act );


		playSound( pos );

	}



	public void playSound( Vector3 pos )
	{

		if( emitSound != null )
		{
			GM.se.source.play( pos, emitSound, range, volume );
		}

	}

}
