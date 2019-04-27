using UnityEngine;
using System.Collections;

public abstract class _Hit3Character : _Hit3
{

	public _Action3	act { get; private set; }



	public _Armor3	armorDefinition;

	protected _ArmorUnit	armor;	// 活動可能耐久力であり、完全破壊とは別。



	public float	ownHitAllowDistance;



	public CharactorReactions	reactions;// 廃止予定





	/// <summary>
	/// 適当なボーン位置を返す。
	/// </summary>
	public override Transform getRandomBone()
	{
		var act = getAct();

		var i = UnityEngine.Random.Range( (int)0, act.smr.bones.Length - 1 );

		return act.smr.bones[ i ];
	}




	// 初期化・更新 ----------------------------

	new protected void Awake()
	{

		base.Awake();

		
		act = GetComponent<_Action3>();

		reactions.deepInit( GetComponent<AudioSource>() );//


		armor = armorDefinition.instantiate();

	}

	//protected void OnEnable()// これはどうなんだいいのか
	public override void init()
	{

		armor.init();

		reactions.init();//

	}

	
	public override void update()
	{
		
		if( !act.isDead )
		{
			
			var isDestroyedArmor = armor.update( act );
			
			
			if( isDestroyedArmor )
			{
				reactions.playDeath();//

				act.changeToDeadMode();
			}

		}

	}

	// --------------------



	public override bool isThroughOwnHit( _Action3 emissionOwnerAct, float emissionDistance )
	{
		return emissionOwnerAct != act | emissionDistance > ownHitAllowDistance * act.tf.localScale.z;//
	}

	public override _Action3 getAct()
	{
		return act;
	}


	


	
	
	[System.Serializable]
	public struct CharactorReactions
	{
		
		public AudioClip	deathScream;
		
		public AudioClip	damageScream;


		public ParticleSystem	deathEffect;


		
		public AudioSource	sound	{ get; private set; }
		
		
		public void deepInit( AudioSource s )
		{
			sound = s;
		}

		public void init()
		{
			if( deathEffect != null )
			{
				deathEffect.gameObject.SetActive( false );
			}
		}

		
		public void playDeath()
		{
			if( !sound ) return;

			sound.PlayOneShot( deathScream );

			if( deathEffect != null )
			{
				deathEffect.gameObject.SetActive( true );
			}
		}
		
		public void playDamage()
		{
			if( !sound ) return;

			sound.PlayOneShot( damageScream );
		}
		
	}




}
