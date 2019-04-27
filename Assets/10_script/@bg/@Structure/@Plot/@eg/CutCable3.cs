using UnityEngine;
using System.Collections;

public class CutCable3 : MonoBehaviour
{

	public CutCable3		cutcableLink	{ get; set; }


	public Rigidbody[]	rbs;

	public LineRenderer	line;

	public Rigidbody	rb;



	float	refreshTimer;


	void OnEnable()
	{
		refreshTimer = Time.time + Random.value * 1.0f;

		//foreach( var irb in rbs )
		//{
		//	irb.maxAngularVelocity = 0.5f;
		//}
	}


	void Update()
	{

		affectPositionsToLine();


		var rbOutSide = rbs[ rbs.Length - 1 ];

		effectForce( rbOutSide );

		//saveForce( rbOutSide );


		hitSpark( rbOutSide );
		
	}


	void affectPositionsToLine()
	{
		line.SetPosition( 0, rb.position );


		for( var i = 0 ; i < rbs.Length ; i++ )
		{

			line.SetPosition( i + 1, rbs[ i ].position );

		}
	}

	void effectForce( Rigidbody rbOutSide )
	{
		if( rbOutSide.velocity.sqrMagnitude < 1.0f * 1.0f )
		{
			//Debug.Log( "cutcable " + rboutside.velocity.ToString() );

			rbOutSide.AddRelativeForce( Random.insideUnitSphere * 300.0f );
		}
	}

	void saveForce( Rigidbody rbOutSide )
	{
		if( ( rbOutSide.position - rb.position ).sqrMagnitude > 15.0f * 15.0f )
		{
			//Debug.Log( "cutcable over" );

			rbOutSide.position = rb.position + Random.insideUnitSphere * 8.0f;

			foreach( var irb in rbs )
			{
				irb.velocity = Vector3.zero;

				irb.angularVelocity = Vector3.zero;
			}
		}
	}

	void hitSpark( Rigidbody rbOutSide )
	{

		if( refreshTimer > Time.time ) return;


		var cs = Physics.OverlapSphere( rbOutSide.position, 3.0f, UserLayer.player | UserLayer.enemyEnvelope ); 

		foreach( var c in cs )
		{

			var rb = c.attachedRigidbody;

			if( rb != null )
			{

				var act = rb.GetComponent<_Action3>();


				act.changeToDamageMode( null );


				var ds = new _Bullet3.DamageSourceUnit( 0.0f, 1.0f, 1.0f );

				act.move.stopping( ref ds, act.speedRate );//キャリアーはキャラクタじゃないので例外出る

			}

		}


		refreshTimer = Time.time + 1.0f;

	}

}
