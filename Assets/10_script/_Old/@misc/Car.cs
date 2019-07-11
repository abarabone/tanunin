using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Car : MonoBehaviour
{

	WheelCollider	whFrontL;
	WheelCollider	whFrontR;
	WheelCollider	whRearL;
	WheelCollider	whRearR;
	
	Transform	tfFrontL;
	Transform	tfFrontR;
	Transform	tfRearL;
	Transform	tfRearR;

	Transform	tfDoorL;
	Transform	tfDoorR;

	
	public float	maxMotorTorque;
	
	public float	brakeToruque;

	public float	maxSteeringAngle;

	public float	steeringRatio;	// 1.0f なら１回転で最大角度




	public float	steering;

	float	motor;

	bool	isDriving;

	float	steeringPerHandling;



	Vector2	preHandlePosition;


	public PlayerAction3	player;

	Rigidbody	rb;





	void Awake()
	{

		var tf = transform;

		tfFrontL = tf.Find( "body/fwheel_L_" );
		tfFrontR = tf.Find( "body/fwheel_R_" );
		tfRearL = tf.Find( "body/rwheel_L_" );
		tfRearR = tf.Find( "body/rwheel_R_" );
		
		whFrontL = tf.Find( "wheel colliders/fwheel_L_" ).GetComponent<WheelCollider>();
		whFrontR = tf.Find( "wheel colliders/fwheel_R_" ).GetComponent<WheelCollider>();
		whRearL = tf.Find( "wheel colliders/rwheel_L_" ).GetComponent<WheelCollider>();
		whRearR = tf.Find( "wheel colliders/rwheel_R_" ).GetComponent<WheelCollider>();

		tfDoorL = tf.Find( "body/door_L_" );
		tfDoorR = tf.Find( "body/door_R_" );

		rb = GetComponent<Rigidbody>();
		
		rb.centerOfMass += Vector3.down * 0.5f;

		//rb = tf.FindChild( "body" ).GetComponent<Rigidbody>();


		steeringPerHandling = maxSteeringAngle / steeringRatio * ( 1.0f / 360.0f );

	}

	void OnEnable()
	{

		preHandlePosition = Vector2.zero;

		isDriving = false;

		steering = 0.0f;

		motor = 0.0f;

	}


	
	void Update()
	{

		if( GamePad._select )
		{
			isDriving = !isDriving;
			
			player.enabled = !isDriving;
			player.cam.enabled = !isDriving;
			//if( isDriving ) player.gameObject.AddComponent<FixedJoint>().connectedBody = rb;
			//else Destroy( player.GetComponent<FixedJoint>() );

			tfDoorL.localRotation = !isDriving ? Quaternion.Euler( 0.0f, +70.0f, 0.0f ) : Quaternion.identity;
			tfDoorR.localRotation = !isDriving ? Quaternion.Euler( 0.0f, -70.0f, 0.0f ) : Quaternion.identity;
		}


		applyWheelVisual( whFrontL, tfFrontL );
		applyWheelVisual( whFrontR, tfFrontR );
		applyWheelVisual( whRearL, tfRearL );
		applyWheelVisual( whRearR, tfRearR );

		if( isDriving ) player.rb.rotation = rb.rotation;

		if( !isDriving ) return;

		var angle = getDeltaSteeringAngle( GamePad.rs );

		steering += angle * steeringPerHandling;

		if( -maxSteeringAngle > steering ) steering = -maxSteeringAngle;
		else
		if(	maxSteeringAngle < steering ) steering =	maxSteeringAngle;

	}

	void FixedUpdate()
	{

		if( !isDriving ) return;


		var motor = maxMotorTorque * GamePad.ls.y;
		

		whFrontL.steerAngle = steering;
		whFrontR.steerAngle = steering;

		whFrontL.motorTorque = motor;
		whFrontR.motorTorque = motor;

		var brake = GamePad.l1 ? brakeToruque : 0.0f;

		whFrontL.brakeTorque = brake;
		whFrontR.brakeTorque = brake;
		whRearL.brakeTorque = brake;
		whRearR.brakeTorque = brake;


		if( preHandlePosition == Vector2.zero )
		{

			var v = 10.0f * rb.velocity.magnitude;

			if( steering > 1.0f | steering < -1.0f )
			{
				steering += ( steering > 0.0f ? -v : v ) *GM.t.delta;
			}
			else
			{
				steering = 0.0f;
			}

			Debug.Log( steering );

		}

	}


	void applyWheelVisual( WheelCollider wh, Transform tf )
	{
		
		Vector3		pos;
		Quaternion	rot;

		wh.GetWorldPose( out pos, out rot );

		tf.position = pos;
		tf.rotation = rot;

	}


	float getDeltaSteeringAngle( Vector2 stick )
	{

		if( stick.sqrMagnitude < 0.2f * 0.2f )
		{
			preHandlePosition = Vector2.zero;
			
			return 0.0f;
		}


		var hpos = stick.normalized;


		if( preHandlePosition == Vector2.zero )
		{
			preHandlePosition = hpos;

			return 0.0f;
		}


		var cross = preHandlePosition.x * hpos.y - preHandlePosition.y * hpos.x;

		var angle = -Mathf.Asin( cross ) * Mathf.Rad2Deg;


		preHandlePosition = hpos;


		var absAngle = angle > 0.0f ? angle : -angle;

		return absAngle > 45.0f ? 0.0f : angle;

	}

}

