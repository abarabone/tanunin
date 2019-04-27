using UnityEngine;
using System.Collections;

public sealed class SoundSourcer3 : _PoolingObject3<SoundSourcer3>
{
	

	AudioSource	sound;



	static int	instanceLength;






	//protected override void deepInit()
	protected void Awake()
	{

		base.Awake();

		sound = GetComponent<AudioSource>();

	}

	
	void Update()
	{

		if( !sound.isPlaying )
		{

			final();

		}

	}









	

	public void play( Vector3 pos, AudioClip clip, float range = 200.0f, float volume = 0.5f )
	{

		if( GM.se.isNear( pos ) )
		{
			var se = instantiateWithPoolingGameObject( pos );
			
			if( se != null )
			{
				
				se.sound.clip = clip;
				
				se.sound.volume = volume;

				se.sound.maxDistance = range;
				
				se.sound.PlayDelayed( 0.0f );
				
			}
		}

	}









	SoundSourcer3 instantiateWithPoolingGameObject( Vector3 pos )
	{

		if( instanceLength < GM.se.maxSoundEffectPolyphony )
		{

			var instance = (SoundSourcer3)base.instantiateWithPoolingGameObject( pos, Quaternion.identity, true );

			instance.init();


			instanceLength++;

			return instance;

		}
		else
		{

			return null;

		}

	}
	
	public override void final()
	{
		
		releaseSelfToPoolOrDestroy();

		instanceLength--;

	}













}
