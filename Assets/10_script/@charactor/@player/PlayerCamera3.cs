using UnityEngine;
using System.Collections;

public class PlayerCamera3 : MonoBehaviour
{


	public Transform	tf	{ get; protected set; }


	PlayerAction3	act;


	Camera		cam;


	float	standardFov;
	
	float	fovRatio;



	public float	aimSpeed;


	float	aimAngleLimit;


	
	public Vector3	standCameraPosition	= new Vector3( 0.0f, 1.43f, -2.6f );

	public Vector3	crawlCameraPosition	= new Vector3( 0.0f, 0.9f, -2.0f );//new Vector3( 0.0f, 0.8f, -2.0f );

	public Vector3	fpsCameraPosition	= new Vector3( 0.0f, 1.25f, 0.2f );//new Vector3( 0.0f, 1.0f, 0.33f );
	
	Vector3	standardCameraLocalPostion;



	public float	standRearDollyRatioMax = 1.0f;//1.2f;

	public float	crawlRearDollyRatioMax = 1.5f;//1.2f;

	float	rearDollyRatioMax;


	public float	standRollCenterHeight	= 0.6f;//0.7f;
	
	public float	crawlRollCenterHeight	= 0.2f;//0.3f;
	
	float	rollCenterHeight;





	public float		verticalAngle		{ get; protected set; }
	
	public Quaternion	rotMoveDirection	{ get; set; }

	public bool		isFpsView		{ get; protected set; }

	bool	isHeadScaleZero;


	public float	aimSpeedEaseOutFactor	{ get; set; }

	public float	aimSpeedLimit;



		
	public void changeToStand()
	{
		
		rollCenterHeight = standRollCenterHeight;
		
		standardCameraLocalPostion = standCameraPosition;

		rearDollyRatioMax = standRearDollyRatioMax;

		aimAngleLimit = 90.0f;

	}
	
	public void changeToCrawl()
	{
		
		rollCenterHeight = crawlRollCenterHeight;
		
		standardCameraLocalPostion = crawlCameraPosition;

		rearDollyRatioMax = crawlRearDollyRatioMax;

		aimAngleLimit = 90.0f;//60.0f;

	}
	
	public void switchFpsMode( bool isFps )
	{

		isFpsView = isFps;

		setHeadVisivility( !isFps );

	}

	void setHeadVisivility( bool isVisible )
	{
		if( isHeadScaleZero == isVisible )
		{
			tfHead.localScale = isVisible ? Vector3.one : Vector3.zero;

			isHeadScaleZero = !isVisible;
		}
	}




	Transform tfHead;

	void initFpsModeParams()
	{

		//tfHead = tf.parent.FindChild( "body/base/chest/head" );
		tfHead = tf.parent.Find( "body/ragdoll base/chest/head" );

		fpsCameraPosition -= tfHead.position - tf.parent.GetComponent<Rigidbody>().position;

	}
	

	public void zoomOn( float ratioR )
	{

		var fov = standardFov * ratioR;

		cam.fieldOfView = fov;
		
		fovRatio = ratioR;


		switchFpsMode( true );

	}

	public void zoomOff()
	{
		
		cam.fieldOfView = standardFov;
		
		fovRatio = 1.0f;

		
		switchFpsMode( false );

	}




	public void deepInitFromAction( PlayerAction3 action )
	{


		act = action;//GetComponentInParent<PlayerAction3>();


		tf = transform;
		
		cam = GetComponent<Camera>();


		standardFov = cam.fieldOfView;
		
		fovRatio = 1.0f;


		initFpsModeParams();

	}



	public void initFromAction()
	{

		rotMoveDirection = act.GetComponent<Rigidbody>().rotation;//act.rb.rotation;
		
		verticalAngle = 0.0f;


		rollCenterHeight = standRollCenterHeight;

		standardCameraLocalPostion = standCameraPosition;


		cam.fieldOfView = standardFov;


		isFpsView = false;

	}


	public void updateFromAction()
	{


		//rotMoveDirection = act.rb.rotation;


		standardCamera();


	}








	
	public void standardCamera()
	{

		var rv = GamePad.rs.y;
		var rh = GamePad.rs.x;
		//var rv = Input.GetAxisRaw( "3" );
		//var rh = Input.GetAxisRaw( "2" );

		//var aimRatio = aimSpeed * GM.t.delta * fovRatio;
		var aimRatio = aimSpeed * fovRatio * Time.deltaTime;


		
		var h = Quaternion.AngleAxis( rh * aimRatio, Vector3.up );

		var v = Mathf.Clamp( verticalAngle + rv * aimRatio, -aimAngleLimit, aimAngleLimit );

		var rot = act.rb.rotation * Quaternion.AngleAxis( v, Vector3.left );



		
		Vector3	idealCameraPosition;

		var	isOcculuded = false;
		var isFarToNearMove = false;


		if( isFpsView )
		{

			idealCameraPosition = adjustCameraPositionFps();

		}
		else
		{

			TpsDat	tps;

			idealCameraPosition = adjustCameraPositionTps( out tps, rot );

			var isOcculudedVirticalStance = adfustCameraUpPositionToOcclusionTps( ref tps, v );

			if( isOcculudedVirticalStance )
			{
				//Debug.Log("hit");
				idealCameraPosition = adjustCameraPositionFps();

				setHeadVisivility( false );

			}
			else
			{

				isOcculuded = adjustCameraDistanceToOcclusion( ref tps, ref idealCameraPosition );

				tf.RotateAround( tps.center, tps.left, v - verticalAngle );
				
				tfHead.localScale = Vector3.one;


				if( isOcculuded )
				{
					isFarToNearMove = (tf.position - tps.center).sqrMagnitude > (idealCameraPosition - tps.center).sqrMagnitude;
				}

				setHeadVisivility( true );

			}

		}



		verticalAngle = v;

		rotMoveDirection *= h;
		
		tf.rotation = rot;

		if( isOcculuded && isFarToNearMove )
		{
			tf.position = idealCameraPosition;
		}
		else
		{
			var speed = tf.InverseTransformVector( idealCameraPosition - tf.position ).z > 0.0f ? 30.0f : 5.0f;//

			tf.position += ( idealCameraPosition - tf.position ) * ( 0.5f * GM.t.delta * speed );//30.0f );
		}

	}


	Vector3 adjustCameraPositionFps()
	{
		if( act.isMoveHeadInFps )
		{

			var localHeadPos = act.tfBody.InverseTransformPoint( tfHead.position );
			
			localHeadPos.x = 0.0f;// = new Vector3( 0.0f, localHeadPos.y, localHeadPos.z );
			
			//var worldCameraPos = tf.rotation * fpsCameraPosition;


			return act.tf.TransformPoint( localHeadPos + fpsCameraPosition );// + worldCameraPos;

		}
		else
		{

			return act.rb.position + new Vector3( 0.0f, rollCenterHeight, 0.0f );

		}
	}

	struct TpsDat
	{
		public Vector3	center;
		public Vector3	up;
		public Vector3	forward;
		public Vector3	left;
		public Vector3	upofs;
		public Vector3	forwardofs;
	}

	Vector3 adjustCameraPositionTps( out TpsDat tps, Quaternion rot )
	{

		var zratio = verticalAngle > 0.0f ? verticalAngle * ( 1.0f / 90.0f ) * rearDollyRatioMax : 0.0f;

		tps.center = act.rb.position + new Vector3( 0.0f, rollCenterHeight, 0.0f );


		tps.forward = rot * Vector3.forward;

		tps.up = rot * Vector3.up;

		tps.left = rot * Vector3.left;


		tps.forwardofs = tps.forward * ( standardCameraLocalPostion.z + zratio );
		
		tps.upofs = tps.up * ( standardCameraLocalPostion.y - rollCenterHeight );
		

		return tps.center + tps.forwardofs + tps.upofs;

	}


	bool adfustCameraUpPositionToOcclusionTps( ref TpsDat tps, float vangle )
	{

		//if( Mathf.Abs( vangle ) > 45.0f )
		{

			var start = tps.center;//tps.center + tps.forwardofs;//
			
			var end = tps.center + tps.upofs;//start + tps.upofs;//


			var res = Physics.Linecast( start, end, UserLayer.eyeOcculusionForPlayer | UserLayer.bgDetailInvisible );


			return res;

		}
		/*else
		{

			var start = tps.center + tps.forwardofs;
			
			var end = start + tps.upofs;
			
			
			var res = Physics.Linecast( start, end, UserLayer.eyeOcculusionForPlayer );
			
			
			return res;

			return false;

		}*/
	}

	bool adjustCameraDistanceToOcclusion( ref TpsDat tps, ref Vector3 cameraPosition )
	{
		
		var start = tps.center + tps.upofs;
		
		var end = cameraPosition;
		
		var xofs = tps.left * 0.2f;
		
		
		var hitR = new RaycastHit();
		var hitL = new RaycastHit();
		
		var res =
		Physics.Linecast( start - xofs, end - xofs, out hitR, UserLayer.eyeOcculusionForPlayer ) &&
		Physics.Linecast( start + xofs, end + xofs, out hitL, UserLayer.eyeOcculusionForPlayer );
		
		
		if( res )
		{

			var dist = Mathf.Min( hitR.distance, hitL.distance );
				
			cameraPosition = start - tps.forward * dist;//( end - start ).normalized * dist;

		}


		return res;
		
	}


	public Vector3 calculateAdjustedMuzzleDistance()
	{

		if( isFpsView )
		{
			
			
			return tf.position;

		}
		else
		{

			var center = act.rb.position + new Vector3( 0.0f, rollCenterHeight, 0.0f );

			var up = tf.up;
			
			return center + up * Vector3.Dot( tf.position - center, up );// + tf.forward * 0.2f;

		}

	}



}
