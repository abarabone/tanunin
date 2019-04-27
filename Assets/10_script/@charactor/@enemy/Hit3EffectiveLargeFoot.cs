using UnityEngine;
using System.Collections;

public class Hit3EffectiveLargeFoot : Hit3EffectiveLarge
{



	const float	fieldHitDistanceLimit = 5.0f;




	protected FieldHitUnit[]	hits;


	protected struct FieldHitUnit
	{
		public Collider	c;

		public Vector3	preHitPos;

		public float	limitTime;
	}



	
	new protected void Awake()
	{

		base.Awake();


		var cs = GetComponentsInChildren<Collider>();

		hits = new FieldHitUnit[ cs.Length ];

		for( var i = 0 ; i < cs.Length ; i++ )
		{

			hits[ i ].c = cs[ i ];

		}

	}
	




	// 衝突 -------------------------------------------

	new protected void OnCollisionEnter( Collision c )
	{

		
		var isTerrain = c.collider is TerrainCollider;

		if( isTerrain )
		{
			// 足（接地部位の総称としての）と地形との衝突時は、
			// 　前回ヒット位置からのずれが大きくない場合は新規衝突とみなさない。
			// 　もちろん、足踏みなど同じ位置にくる場合もあるのだが、現状はアイデアがなく対処できていない。


			if( c.contacts.Length == 0 ) return;


			var i = findChildCollider( c.contacts[0].thisCollider );

			if( hits[ i ].limitTime > Time.time ) return;


			var pos = c.contacts[ 0 ].point;

			if( ( pos - hits[i].preHitPos ).sqrMagnitude < fieldHitDistanceLimit * fieldHitDistanceLimit ) return;


			hits[ i ].preHitPos = pos;

			hits[ i ].limitTime = Time.time + 1.0f;

		}


		base.OnCollisionEnter( c );

	}



	int findChildCollider( Collider thisc )
	{

		for( var i = 0 ; i < hits.Length ; i++ )
		{

			if( hits[ i ].c == thisc ) return i;

		}

		return -1;
	}


}
