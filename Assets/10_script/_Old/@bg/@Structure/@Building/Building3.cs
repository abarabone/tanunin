using UnityEngine;
using System.Collections;

public class Building3 : _StructureInterArea3//_StructureSwitchOfDetail3
{
	


	public BuildingBinder3	binder	{ get; set; }


	bool	wasDestructed;
	
	Vector3	prepos;


	AudioSource	sound;



	public override void deepInit()
	{

		base.deepInit();
		

		sound = GetComponent<AudioSource>();

	}

	public override void build()
	{

		var hitter = gameObject.AddComponent<BuildingHit3>();


		base.build();

	}

	
	public override void attatchToArea( StructureBonedRenderer3 sbr, int id )
	{
		
		base.attatchToArea( sbr, id );


		this.enabled = false;

		prepos = rb.worldCenterOfMass;

	}
	





	public override void destruct()
	{
		if( wasDestructed == false )
		{

			var tfSmoke = ((GameObject)Instantiate( GM.largeSmokeOnDestructPrefab )).transform;
			
			tfSmoke.parent = tf;
			
			tfSmoke.position = rb.worldCenterOfMass;
			
			
			if( binder != null )
			{
				binder.bindOff();
			}
			else
			{
				startToMove();
			}
			
			
			setEnvelopeLayer( 0 );//UserLayer._bgSleepEnvelope );
			//rb.detectCollisions = false;


			Destroy( gameObject, 5.0f );


			wasDestructed = true;

		}
	}
	
	
	public void startToMove()
	{
		
		if( envelopeColliders[0].gameObject.layer == UserLayer._bgSleepEnvelope )//rb.isKinematic )
		{

			tf.parent = null;//

			setEnvelopeLayer( UserLayer._bgEnvelope );
			//setEnvelopeLayer( UserLayer._bgPlotEnvelope );

			rb.isKinematic = false;

		}
		
		rb.WakeUp();

		if( !this.enabled ) this.enabled = true;

	}




	
	void OnCollisionEnter( Collision col )
	// ビル等の積み重なってる構造物で、キネマティック状態の最初に必ず呼ばれるかと思いきや、呼ばれないみたい。
	// つまり、毎フレーム衝突判定はしてないっぽい。また OnStay() であっても、スリープ状態には呼ばれない。
	{
		//Debug.Log(col.collider.name);
		//return;
		
		var otherLayerFlag	= 1 << col.collider.gameObject.layer;
		
		var layerMask		= UserLayer.bgField | UserLayer.bgEnvelope;
		
		if( ( otherLayerFlag & layerMask ) != 0 & rb.velocity.sqrMagnitude > 0.5f * 0.5f )
		{
			
			var otherRigidBody = col.rigidbody;//.collider.attachedRigidbody;
			
			var other = ( otherRigidBody )? otherRigidBody.gameObject: col.collider.gameObject;

			var	otherSound = other.GetComponent<AudioSource>();
			
			if( !otherSound || otherSound && !otherSound.isPlaying )
			// 相手側が音声を出してない時だけ衝突音を出す
			{
				
				sound.PlayDelayed( 0.0f );

				foreach( var cont in col.contacts )
				{

					Destroy( Instantiate( GM.largeSmokePrefab, cont.point, Quaternion.identity ), 1.0f );

				}

			}

			startToMove();
			
		}
		else if( ( otherLayerFlag & UserLayer.enemyEnvelopeMove ) != 0 )
		{
			//Debug.Log( rb.velocity.magnitude +" "+ rb.velocity.sqrMagnitude * rb.mass );
			if( rb.velocity.sqrMagnitude > 5.0f * 5.0f )
			{

				var damage = rb.velocity.magnitude * rb.mass * 2.0f;//col.relativeVelocity.magnitude * rb.mass * 2.0f;

		//		Debug.Log( "damage / " + damage );

				if( damage > 500.0f )
				{
					//var hp = col.rigidbody.GetComponent<_HitProcessBase>();

					//hp.shot( damage, 0.0f, Vector3.zero, col.contacts[0].point, 0, 0 );
				}

			}

		}//if( rb.velocity.sqrMagnitude > 5.0f * 5.0f ) Debug.Log( "fall / " + rb.velocity.magnitude );
		
	}



	void OnCollisionExit( Collision col )
	{
		
		var otherLayerFlag	= 1 << col.collider.gameObject.layer;
		
		var layerMask		= UserLayer.bgField | UserLayer.bgEnvelope;
		
		if( ( otherLayerFlag & layerMask ) != 0 )
		{
			
			if( rb.IsSleeping() ) startToMove();
			
		}
		
	}

	


	void LateUpdate()
	{

		if( rb.IsSleeping() )
		{

			enabled = false;


			var nowpos = rb.worldCenterOfMass;

			if( farRenderer.isMoveToNear( prepos, nowpos ) )//, 0.5f ) )
			{

				farRenderer.requestRecalculateAllBounds( 30.0f );//Debug.Log("move to near");

			}

			prepos = nowpos;

		}
		//else Debug.Log( "not sleep "+GetInstanceID() );


		if( farRenderer.isOutOfBounds( envelopeColliders ) )
		{

			farRenderer.encapsulateBounds( envelopeColliders );//Debug.Log("out of bounds");

		}


		if( farRenderer.isVisible() )
		// 剛体がスリープになるまで、毎フレーム（視錐台にある時のみ）姿勢情報をシェーダにセットし続ける。
		{
			
			farRenderer.setLocation( partId, rb.position, rb.rotation, far.activeInHierarchy );
			
		}


		if( rb.position.y < -300.0f )
			// 延々と落下するのを防止する
		{
			
			Destroy( gameObject );
			
		}

	}





}
