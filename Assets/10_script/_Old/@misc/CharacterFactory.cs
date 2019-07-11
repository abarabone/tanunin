using UnityEngine;
using System.Collections;
using System.Linq;



[CreateAssetMenu()]
public class CharacterFactory : ScriptableObject
{


	
	/// <summary>
	/// キャラクター生成定義
	/// </summary>
	public struct EntryUnit
	{

		public _CharacterClassDefinition	definition;

		public _Action3.CharacterInfo		ch;

		public float	weight;

		/// <summary>
		/// キャラクター生成定義を生成する。
		/// </summary>
		/// <param name="defCh">生成するキャラクターの定義</param>
		/// 
		/// <param name="ch">生成されるキャラクターの性格定義</param>
		/// 
		/// <param name="weight">生成確率や生成数にかかわる重み</param>
		/// 
		public EntryUnit( _CharacterClassDefinition defCh, _Action3.CharacterInfo ch, float weight )
		{
			this.definition	= defCh;
			this.ch			= ch;
			this.weight		= weight;
		}

	}



	/// <summary>
	/// キャラクター生成定義のホルダー。
	/// </summary>
	public struct EntryHolder
	{

		EntryUnit[]	 entries;   // 出現種類・性格・重みの定義


		/// <summary>
		/// キャラクター生成定義をセットする。
		/// </summary>
		public void setEntries( params EntryUnit[] ents )
		{

			entries = ents;

		}

		

		public float sumWeight()
		{
			return entries.Sum( x => x.weight );
		}


		public EntryUnit this[ int i ]
		{
			get { return entries[ i ]; }
		}


		public int length { get { return entries.Length; } }

	}



	/// <summary>
	/// キャラクター生成の情報定義。
	/// </summary>
	public struct SpawnInfo
	{

		public int targetMemberCount;	// 最終的に出現させる数

		public float intervalTime;		// 出現間隔


		public ActivityGroup3	group;

		public Transform		tfCenter;


		public EntryGetterDefines.GetEntryProc	getEntry;

		public SpawnDefines.SpawnProc			spawn;

	}





	/// <summary>
	/// キャラクター生成の状態変数。
	/// </summary>
	public struct SpawnState
	{
		public int		entryIndex;		// キャラクター定義のインデックス

		public int		entryCounter;	// そのキャラクターを何体生成したかのカウンター

		public int		spawned;		// 現在までに生成した数

		public float	totalWeight;	// 全定義の重みの合計
	}





	/// <summary>
	/// キャラクター生成を開始する。
	/// </summary>
	/// <param name="group">生まれた敵の属するグループ</param>
	/// 
	/// <param name="spawn">敵出現位置を決める関数</param>
	/// 
	/// <param name="getEntry">敵種類を決める関数</param>
	/// 
	static public void beginSpawn( ref EntryHolder entries, ref SpawnInfo spawnInfo )
	{

		spawnInfo.tfCenter = spawnInfo.tfCenter ?? spawnInfo.group.tf;

		spawnInfo.getEntry = spawnInfo.getEntry ?? EntryGetterDefines.getEntryByWeight;

		spawnInfo.spawn = spawnInfo.spawn ?? SpawnDefines.spawnOnGround;

		GM.startCoroutine( update( entries, spawnInfo ) );

	}


	static IEnumerator update( EntryHolder entries, SpawnInfo spawnInfo )
	{

		spawnInfo.group.spawns.notifyBegin();


		SpawnState	state = new SpawnState();

		state.totalWeight = entries.sumWeight();


		for( state.spawned = 0 ; state.spawned < spawnInfo.targetMemberCount ; state.spawned++ )
		{

			do
			{

				yield return new WaitForSeconds( spawnInfo.intervalTime );

				// グループ内に空きがなければ回り続ける

			}
			while( spawnInfo.group.memberCount >= spawnInfo.group.maxStayCapacity );


			var ent = spawnInfo.getEntry( ref state, ref spawnInfo, ref entries );

			var act = spawnInfo.spawn( ent.definition, ref state, ref spawnInfo );

			act.character = ent.ch;

			spawnInfo.group.enter( act );

		}


		spawnInfo.group.spawns.notifyEnd();

	}



	
	// 種類決定 -------------------


	static public class EntryGetterDefines
	{

		public delegate EntryUnit GetEntryProc( ref SpawnState state, ref SpawnInfo spawnInfo, ref EntryHolder entries );


		/// <summary>
		/// キャラクター定義エントリが、重み付きランダム値で選択される。
		/// </summary>
		static public EntryUnit getEntryByWeight( ref SpawnState state, ref SpawnInfo spawnInfo, ref EntryHolder entries )
		{
			var rvalue = Random.value * state.totalWeight;

			var limit = 0.0f;

			for( var i = entries.length ; i-- > 1 ; )
			{
				limit += entries[ i ].weight;

				if( rvalue < limit ) return entries[ i ];
			}

			return entries[ 0 ];
		}


		/// <summary>
		/// 重みを生成数と解釈して、定常的に生成する。
		/// </summary>
		static public EntryUnit getEntryInOrder( ref SpawnState state, ref SpawnInfo spawnInfo, ref EntryHolder entries )
		{

			var ent = entries[ state.entryIndex ];

			if( ++state.entryCounter > ent.weight )
			{
				if( ++state.entryIndex > entries.length ) state.entryIndex = 0;

				state.entryCounter = 0;
			}

			return ent;
		}



	}


	
	
	// 出現位置決定 ----------------

	
	static public class SpawnDefines
	{
		
		public delegate _Action3 SpawnProc( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo );


		
		/// <summary>
		/// グループテリトリーサークル内のランダムな地上位置に出現する。
		/// </summary>
		static public _Action3 spawnOnGround( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo )
		{

			var pos = spawnInfo.group.getRandomPointOnCircle();

			var ray = new Ray( new Vector3( pos.x, 1024.0f, pos.z ), Vector3.down );

			var hit = new RaycastHit();

			if( Physics.Raycast( ray, out hit, float.PositiveInfinity, UserLayer.groundForEnemy ) )
			{
				pos = hit.point;
			}


			var dirxz = Random.insideUnitCircle;

			var rot = Quaternion.LookRotation( new Vector3( dirxz.x, 0.0f, dirxz.y ), Vector3.up );


			return defCh.instantiate( pos, rot );
		}



		/// <summary>
		/// グループテリトリースフィア内のランダムな空間位置に出現する。
		/// </summary>
		static public _Action3 spawnInSphere( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo )
		{
			return null;
		}



		/// <summary>
		/// グループテリトリースフィア内のランダムな壁面上に出現する。
		/// 位置は tfCenter からのキャストによる。キャスト範囲に壁面がなければ、空間位置に出現する。
		/// </summary>
		static public _Action3 spqwnAroundOnWall( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo )
		{
			return null;
		}



		/// <summary>
		/// tfCenter 位置に出現する。
		/// </summary>
		static public _Action3 spawnCenterPoint( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo )
		{
			var pos = spawnInfo.tfCenter.position;

			var dirxz = Random.insideUnitCircle;

			var rot = Quaternion.LookRotation( new Vector3( dirxz.x, 0.0f, dirxz.y ), Vector3.up );

			return defCh.instantiate( pos, rot );
		}



		/// <summary>
		/// グループテリトリーに外接するＸＺ平面四角形を埋めるように、規則正しく出現する。
		/// tfCenter を先頭位置とし、近辺との距離を間隔とする。tfCenter の向いている方向に出現していく。
		/// </summary>
		static public _Action3 spawnSquare( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo )
		{
			return null;
		}



		/// <summary>
		/// tfCenter から渦を巻くように出現する。
		/// 最後のメンバーの出現位置が、グループテリトリーの半径と同じ距離になるように調整される。
		/// </summary>
		static public _Action3 spawnScrew( _CharacterClassDefinition defCh, ref SpawnState state, ref SpawnInfo spawnInfo )
		{
			return null;
		}


	}





	// キャラクター定義 -------------------


	public _CharacterClassDefinition[] characterDefinitions;


	public _CharacterClassDefinition GetCharacterDefinition( int chId )
	{

		return characterDefinitions[ chId ];

	}


}






