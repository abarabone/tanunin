using UnityEngine;
using System.Collections;

using MEvent;


//public class SystemManager : MonoBehaviour
public class GM : MonoBehaviour
{



	public ShaderSettings			_shaders;
	static public ShaderSettings	shaders { get { return entity._shaders; } }




	public GameObject			_largeSmokePrefab;
	static public GameObject	largeSmokePrefab	{ get{ return entity._largeSmokePrefab; } }
	
	public GameObject			_largeSmokeOnDestructPrefab;
	static public GameObject	largeSmokeOnDestructPrefab	{ get{ return entity._largeSmokeOnDestructPrefab; } }

	public GameObject			_roadBreakPrefab;
	static public GameObject	roadBreakPrefab	{ get{ return entity._roadBreakPrefab; } }
	
	public CutCable3		_cutcablePrefab;
	static public CutCable3	cutcablePrefab	{ get{ return entity._cutcablePrefab; } }


	public _Emitter3	_groundDustCloud;
	static public _Emitter3 groundDustCloud { get { return entity._groundDustCloud; } }
	

	public LandingList			_defaultLandings;
	static public LandingList	defaultLandings	{ get{ return entity._defaultLandings; } }




	public CharacterFactory _characterFactory;
	static public CharacterFactory characterFactory { get { return entity._characterFactory; } }





	public GroupMaster			_groups;
	static public GroupMaster	groups	{ get{ return entity._groups; } }
	
	public EnemyMaster			_enemys;
	static public EnemyMaster	enemys	{ get{ return entity._enemys; } }
	
	public MusicMaster			_music;
	static public MusicMaster	music	{ get{ return entity._music; } }

	public SoundEffectMaster	_se;
	static public SoundEffectMaster	se	{ get{ return entity._se; } }


	static public TimeMaster	t;

	static public CameraMaster	cameras;


	static public GM	entity;
	



	static public BulletPool3			bulletPool	{ get; protected set; }
	//static public StringParticlePool	stringPool	{ get; protected set; }





	static public void startCoroutine( IEnumerator proc )
	{
		entity.StartCoroutine( proc );
	}
	
	
	void Awake()
	{
		
		entity = this;
		
		var tf = transform;
		

		bulletPool	= tf.GetComponentInChildren<BulletPool3>();
		//stringPool	= tf.GetComponentInChildren<StringParticlePool>();

		
		enemys.init();
		
		groups.init();
		
		music.init();
		
		se.init();
		
		cameras.init();

		
		Shader.WarmupAllShaders();


		Application.targetFrameRate = 60;

	}
	
	void Start()
	{
		
		
		if( !Application.isEditor )
		{

			Resources.UnloadUnusedAssets();

			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			System.GC.Collect();

		}

		ev();

		GM.gotoMissionClear();
	}


	IEnumerator sss()
	{
		
		var www = new WWW("http://translate.google.com/translate_tts?tl=ja&q=このやろう");
		
		yield return www;
		
		GetComponent<AudioSource>().PlayOneShot( www.GetAudioClip( false, true, AudioType.OGGVORBIS ) );
		
	}
	

	void ev()
	{
		
		var ch = new _Action3.CharacterInfo();
		ch.aggressive = _Action3.CharacterInfo.EnAggressive.middle;
		ch.obedience = _Action3.CharacterInfo.EnObedience.middle;
		ch.persistence = _Action3.CharacterInfo.EnPersistence.middle;

		var e0 = new ActivityGroupDefines.ToCreateWithSpawn();
		e0.waitTime = 0.0f;
		e0.pos = Vector3.zero + Vector3.up * 10.0f;
		e0.rot = Quaternion.identity;
		e0.spawn.intervalTime = 0.1f;
		e0.spawn.targetMemberCount = 500;
		e0.territory = 1.0f;
		e0.maxStayCapacity = 200;
		//e0.entries.setEntries( new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 0 ), ch, 40.0f ) );
		e0.entries.setEntries(
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 0 ), ch, 50.0f ),
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 2 ), ch, 30.0f ),
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 1 ), ch, 5.0f )
		//	new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 3 ), ch, 5.0f ),
		//	new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 5 ), ch, 1.0f )
		);

		e0.execute();


		return;
		
		/*
		var ch = new _Action3.CharacterInfo();
		ch.aggressive = _Action3.CharacterInfo.EnAggressive.middle;
		ch.obedience = _Action3.CharacterInfo.EnObedience.middle;
		ch.persistence = _Action3.CharacterInfo.EnPersistence.middle;

		var e0 = new ActivityGroupDefines.ToCreateWithSpawn();
		e0.waitTime = 0.0f;
		e0.pos = Vector3.zero;
		e0.rot = Quaternion.identity;
		e0.spawn.intervalTime = 0.1f;
		e0.spawn.targetMemberCount = 50;
		e0.territory = 1.0f;
		e0.maxStayCapacity = 30;
		e0.entries.setEntries(
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 2 ), ch, 40.0f ),
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 1 ), ch, 10.0f ),
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 2 ), ch, 30.0f )
		);

		e0.execute();


		return;


		var e1 = new ActivityGroupDefines.ToCreateWithSpawn();
		e1.waitTime = 0.0f;
		e1.pos = new Vector3( 100.0f, 0.0f, 200.0f );
		e1.rot = Quaternion.identity;
		e1.spawn.intervalTime = 3.0f;
		e1.spawn.targetMemberCount = 50;
		e1.territory = 50.0f;
		e1.maxStayCapacity = 20;
		e1.entries.setEntries(
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 1 ), ch, 40.0f ),
			new CharacterFactory.EntryUnit( characterFactory.GetCharacterDefinition( 2 ), ch, 100.0f )
		);

		e1.execute();
		*/

	}




	void Update()
	{

		if( Input.GetKeyDown( KeyCode.Escape ) )
		{
			Application.Quit();
		}

		
		music.checkPlayLoop();


	//	playerCamera.setViewPlanes();



		Random.seed = Time.frameCount;//


		se.updateListnerPosition();

		t.update();


		

	}

	void FixedUpdate()
	{
		

		t.fixedUpdate();

	}

	
	
	public static void gotoMissionClear()
	{
		entity.StartCoroutine( entity.viewMissionClear() );
	}
	
	
	IEnumerator viewMissionClear()
	{
		
		yield return new WaitForSeconds( 3.0f );

		if( !MEventMaster.hasEvents && enemys.count < 1 & groups.count < 1 )
		{
			
			//Application.ExternalCall( "sp", "この局面を生き残るなんて、。あなたは一体、？" );
			
			
			var text = entity.transform.Find( "IFCamera/mission clear" ).GetComponent<GUIText>();
			
			
			yield return new WaitForSeconds( 3.0f );
			
			float limit = Time.time + 8.0f;
			
			
			text.fontSize = 500;
			
			text.gameObject.SetActive( true );
			
			for(;;)
			{
				yield return 0;
				
				text.fontSize += (100 - text.fontSize) >> 4;
				
				if( Time.time > limit ) break;
			}
			
			for(;;)
			{
				yield return 0;
				
				text.fontSize -= text.fontSize >> 3;
				
				if( text.fontSize < 10 ) break;
			}
			
			text.gameObject.SetActive( false );
			
		//	yield return new WaitForSeconds( 3.0f );
			
		//	Application.Quit();
			
		}
	}

}








[ System.Serializable ]
public class GroupMaster
{

	public ActivityGroup3	template;
	
	public int				count	{ get; private set; }


	public void init()
	{}

	
	public void increase()
	{
		count++;
	}
	
	public void decrease()
	{
		count--;
	}


	public ActivityGroup3 create( Vector3 pos, Quaternion rot )
	{
		return template.instantiate( pos, rot );
	}

}



[ System.Serializable ]
public class EnemyMaster
{
	
	public int		count	{ get; private set; }
	
	public GUIText	view	{ get; private set; }

	
	public void init()
	{
		count = 0;
		
		view = GM.entity.transform.Find( "IFCamera/enemy counter" ).GetComponent<GUIText>();
		
		view.text = count.ToString();
	}
	
	
	public void increase()
	{
		count++;
		
		view.text = count.ToString();
	}
	
	public void decrease()
	{
		count--;
		
		view.text = count.ToString();
	}
	
}



[ System.Serializable ]
public class MusicMaster
{
	
	public string[]		urls;
	
	public AudioClip[]	musics	{ get; private set; }
	
	AudioSource	sound;
	
//	bool	triedLoadAll;
	
	
	public void init()
	{
		sound = GM.entity.gameObject.GetComponent<AudioSource>();

		musics = new AudioClip[ urls.Length ];
		
		loadAll();//
	}
	
	
	public void loadAll()
	{
		
	//	triedLoadAll = true;
		
		GM.startCoroutine( _loadAll() );
		
	}
	IEnumerator _loadAll()
	{
		for( var i = 0; i < urls.Length; i++ )
		{
			var www = new WWW( urls[i] );
			
			yield return www;
			
			musics[i] = www.GetAudioClip( false, true );
		}
	}
	
	public void play( int id, float delay )
	{
		
		if( musics[id] )
		{
			_play( id, delay );
		}
		else
		{
			GM.startCoroutine( _loadAndPlay(id,delay) );
		}
	}
	
	IEnumerator _loadAndPlay( int id, float delay )
	{
		var www = new WWW( urls[id] );
		
		yield return www;
		
		musics[id] = www.GetAudioClip( false, true, AudioType.OGGVORBIS );
		
		_play( id, delay );
	}
	
	void _play( int id, float delay )
	{
		sound.clip = musics[id];
		
		sound.PlayDelayed( delay );
	}
	
	public void checkPlayLoop()
	{
		if( sound.clip != null && !sound.isPlaying && sound.clip.isReadyToPlay )
		{
			sound.PlayDelayed( 0.0f );
		}
	}
	
}




public struct CameraMaster
{

	public Camera	player	{ get; private set; }

	public Camera	iface	{ get; private set; }

	Plane[]	viewPlanes;	// 使ってないな


	public void init()
	{

		player = Camera.main;
		player.eventMask = 0;

		iface =	GM.entity.transform.Find( "IFCamera" ).GetComponent<Camera>();
		iface.eventMask = 0;

	}

	public void setViewPlanes()// 使ってないな
	{

		viewPlanes = GeometryUtility.CalculateFrustumPlanes( player );

	}

	public bool isVisible( Bounds bounds )// 使ってないな
	{

		return GeometryUtility.TestPlanesAABB( viewPlanes, bounds );

	}

}



[ System.Serializable ]
public class SoundEffectMaster
{

	public SoundSourcer3	soundSourcerTemplate;
	public SoundSourcer3	source { get { return soundSourcerTemplate; } }

	public int	maxSoundEffectPolyphony;

	public float	farDistance;
	public float	sqrFarDistance	{ get; private set; }

	Vector3		listnerPosition;
	Rigidbody	rbPlayer;
	
	public void init()
	{

		sqrFarDistance = farDistance * farDistance;

		//rbPlayer = Camera.main.GetComponentInParent<PlayerAction3>().GetComponent<Rigidbody>();//
		rbPlayer = GameObject.Find( "players/player" ).GetComponent<Rigidbody>();
	}

	public void updateListnerPosition()
	{
		listnerPosition = rbPlayer.position;
	}

	public bool isNear( Vector3 pos )
	{
		return ( pos - listnerPosition ).sqrMagnitude < sqrFarDistance;
	}


}


public struct TimeMaster
{

	public float	delta	{ get; private set; }
	public float	deltaR	{ get; private set; }

	public void update()
	{
		delta	= Time.deltaTime;
		deltaR	= 1.0f / delta;
	}
	
	public void fixedUpdate()
	{
		delta	= Time.fixedDeltaTime;
		deltaR	= 1.0f / delta;
	}

}





	
	[ System.Serializable ]
	public struct ShaderSettings
	{
		public Shader	area;
		public Shader	structure;
		public Shader	structureTransparent;
		public Shader	structureWithBone;
	}
	
