using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{



	public Transform tf { get; protected set; }


	// tf.position の同期元

	Rigidbody	rbAttatched { get { return location.isFps ? rbAttatchedFps : rbAttatchedTps; } } 

	Rigidbody	rbAttatchedTps;

	Rigidbody	rbAttatchedFps;


	Camera	cam;

	float	standardFov;

	float	fovRatio;



	public float	aimMaxSpeed;

	float	aimAngleLimit;


	[SerializeField]
	AimSpeedUnit	aim;




	CameraPositionUnit	location;




	public float verticalAngle { get; protected set; }

	public float horizontalAngle { get; protected set; }

	public bool isFpsView { get { return location.isFps; } }


	public Quaternion rotVertical { get; private set; }

	public Quaternion rotHorizontal { get; private set; }


	public float aimSpeedEaseOutFactor { get; set; }

	public float	aimSpeedLimit;




	// 外部公開 ----------------

	public void init( Rigidbody rbAttatchTps, Rigidbody rbAttatchFps, float vAngle=0.0f, float hAngle=0.0f )
	{

		rbAttatchedTps = rbAttatchTps;

		rbAttatchedFps = rbAttatchFps;


		tf.position = rbAttatched.position;

		verticalAngle = vAngle;

		horizontalAngle = hAngle;


		cam.fieldOfView = standardFov;

	}


	public void changeStance( PlayerAction3.EnStance stance, float fittingSpeedRate )
	{

		location.changeStance( stance, fittingSpeedRate );

	}

	public void changeFpsMode( bool isFpsMode, float fittingSpeedRate )
	{

		location.changeFpsMode( isFpsMode, fittingSpeedRate );

	}


	public void zooming( float ratioR )
	{
		
		cam.fieldOfView = standardFov * ratioR;

		fovRatio = ratioR;
		
	}

	public void aimMode( bool isInertive )
	{

		aim.isInertive = isInertive;

	}




	// イベント -------------------------

	void Awake()
	{

		tf = transform;


		cam = GetComponentInChildren<Camera>();

		standardFov = cam.fieldOfView;

		fovRatio = 1.0f;
		
		aimAngleLimit = 90.0f;


		location.setup( tf );


		rotHorizontal = tf.rotation;

		verticalAngle = tf.rotation.eulerAngles.x;

		horizontalAngle = tf.rotation.eulerAngles.y;
		
	}

	
	void Update()
	{

		var d = aim.getSpeed( GamePad.rs, fovRatio ) * GM.t.delta;

		var h = horizontalAngle + d.x;

		var hrot = Quaternion.AngleAxis( h, Vector3.up );

		var v = Mathf.Clamp( verticalAngle + d.y, -aimAngleLimit, aimAngleLimit );

		var vrot = Quaternion.AngleAxis( v, Vector3.left );
		


		// 回転の保存

		verticalAngle = v;

		horizontalAngle = h;

		rotHorizontal = hrot;

		rotVertical = vrot;


		// 回転・位置の反映・同期

		location.rotRollCenter = vrot;

		tf.rotation = hrot;

		tf.position = rbAttatched.position;


		// カメラ位置補正
		
		location.adjust( this );



		Radar.setCenter( tf );

	}













	[System.Serializable]
	struct AimSpeedUnit
	{

		public float	maxSpeed;

		public float	trque;

		public float	inertia;


		float	acc;

		Vector2 speed;

		public bool isInertive { private get; set; }


		public void setup( float trque, float inartia )
		{
			acc = trque / inartia;
		}
		

		public Vector2 getSpeed( Vector2 lv, float fovRatio )
		{

			if( isInertive )
			{
				return speed = Vector2.MoveTowards( speed, lv * maxSpeed * fovRatio, acc * fovRatio * GM.t.delta );
			}
			else
			{
				return speed = maxSpeed * lv * fovRatio;
			}

		}

	}



	public struct CameraPositionUnit
	{

		Transform	tfCamera;

		Transform	tfRollCenter;

		Transform	tfMuzzle;
		


		Vector3 rollCenterStandLocalPosition;

		Vector3 rollCenterCrawlLocalPosition;

		Vector3 rollCenterTpsLocalPosition { get { return isNowCrawl ? rollCenterCrawlLocalPosition : rollCenterStandLocalPosition; } }

		Vector3 rollCenterLocalPosition { get { return isFps ? Vector3.zero : rollCenterTpsLocalPosition; } }


		Vector3	standLocalPosition;

		Vector3	crawlLocalPosition;

		Vector3 tpsLocalPosition { get { return isNowCrawl ? crawlLocalPosition : standLocalPosition; } }

		Vector3 localPosition { get { return isFps ? fpsLocalPosition : tpsLocalPosition; } }

		Vector3	fpsLocalPosition;


		bool isNowCrawl;

		public bool isFps { get; private set; }


		Vector3 cameraLocalPosition;

		public float	fittingRate;


		public Quaternion rotRollCenter { set{ tfRollCenter.localRotation = value; } }

		public Vector3 muzzlePosition { get { return tfMuzzle.position; } }


		const float	cameraRadius = 0.05f;


		public void setup( Transform tfCameraBase )
		{

			tfRollCenter = tfCameraBase.Find( "roll center" );
			rollCenterStandLocalPosition = tfRollCenter.localPosition;

			tfCamera = tfRollCenter.Find( "Camera" );
			standLocalPosition = tfCamera.localPosition;
			
			var tfFps = tfRollCenter.Find( "fps" );
			fpsLocalPosition = tfFps.localPosition;
			GameObject.Destroy( tfFps.gameObject );


			var tfRollCenterCrawl = tfCameraBase.Find( "roll center crawl" );
			rollCenterCrawlLocalPosition = tfRollCenterCrawl.localPosition;
			GameObject.Destroy( tfRollCenterCrawl.gameObject );

			var tfCrawl = tfRollCenterCrawl.Find( "crawl" );
			crawlLocalPosition = tfCrawl.localPosition;
			GameObject.Destroy( tfCrawl.gameObject );


			tfMuzzle = tfRollCenter.Find( "muzzle" );


			fittingRate = 10.0f;

		}
		
		
		public void changeStance( PlayerAction3.EnStance stance, float fittingSpeedRate )
		{

			isNowCrawl = stance == PlayerAction3.EnStance.crawl;

			if( !isFps )
			{
				changeTo();

				fittingRate = fittingSpeedRate;
			}

		}

		public void changeFpsMode( bool isFpsMode, float fittingSpeedRate )
		{

			isFps = isFpsMode;

			changeTo();

			fittingRate = fittingSpeedRate;

		}

		void changeTo()
		{

			tfCamera.parent = null;

			tfRollCenter.localPosition = rollCenterLocalPosition;

			tfCamera.parent = tfRollCenter;


			cameraLocalPosition = localPosition;


			var newMuzzle = tfMuzzle.localPosition;

			newMuzzle.y = cameraLocalPosition.y;

			tfMuzzle.localPosition = newMuzzle;

		}


		// カメラ位置調整 ------------------

		public void adjust( PlayerCamera cam )
		{

			var local = localPosition;

			local = adjustForOverHeadBarrier( cam, local );

			local = adjustForOcclusion( cam, local );

			cameraLocalPosition = local;


			fitting();
			//Debug.Log( location.fittingRate );

		}


		/// <summary>
		/// キャラとカメラの間を壁が遮る場合の調整
		/// </summary>
		Vector3 adjustForOcclusion( PlayerCamera cam, Vector3 localPos )
		{

			if( isFps ) return adjustForOcclusionInFps( cam, localPos );


			var rotDirection = cam.rotHorizontal * cam.rotVertical;
			
			var rollCenter = tfRollCenter.position;


			var start = rollCenter + rotDirection * ( Vector3.up * localPos.y );

			var end = rollCenter + rotDirection * localPos;

			var xofs = tfRollCenter.right * 0.2f;


			var hitR = new RaycastHit();
			var hitL = new RaycastHit();

			var res =
			Physics.Linecast( start + xofs, end + xofs, out hitR, UserLayer.eyeOcculusionForPlayer ) &&
			Physics.Linecast( start - xofs, end - xofs, out hitL, UserLayer.eyeOcculusionForPlayer );

			if( res )
			{
				
				var dist = ( hitR.distance + hitL.distance ) * 0.5f - cameraRadius;

				localPos = setZ( localPos, -dist );

				fittingRate = 30.0f;

			}
			else
			{
				
				if( fittingRate == 30.0f ) fittingRate = 0.5f;

			}


			return localPos;
			
		}

		/// <summary>
		/// 基本的にＴＰＳと同じルーチンでよいが、レイが一つで済むのと即時フィッティングできるので別にしている。
		/// </summary>
		Vector3 adjustForOcclusionInFps( PlayerCamera cam, Vector3 localPos )
		{
			
			var rotDirection = cam.rotHorizontal * cam.rotVertical;
			
			var rollCenter = tfRollCenter.position;


			var start = rollCenter + rotDirection * ( Vector3.up * localPos.y );

			var end = rollCenter + rotDirection * localPos;


			var hit = new RaycastHit();

			var res = Physics.Linecast( start, end, out hit, UserLayer.eyeOcculusionForPlayer );
			
			if( res )
			{
				
				localPos = setZ( localPos, hit.distance );// ここはＴＰＳと違う

				tfCamera.localPosition = setZ( tfCamera.localPosition, localPos.z );

			}


			return localPos;

		}


		/// <summary>
		/// カメラが上の障害物に押し戻される場合の調整
		/// </summary>
		Vector3 adjustForOverHeadBarrier( PlayerCamera cam, Vector3 localPos )
		{

			var rotDirection = cam.rotHorizontal * cam.rotVertical;

			var up = Vector3.up;

			var start = tfRollCenter.position;
			

			var hit = new RaycastHit();

			var res = Physics.SphereCast( start, cameraRadius, rotDirection * up, out hit, localPos.y, UserLayer.eyeOcculusionForPlayer );

			if( res )
			{
				
				localPos = setY( localPos, hit.distance - cameraRadius );

				tfCamera.localPosition = setY( tfCamera.localPosition, localPos.y );

			}

			
			tfMuzzle.localPosition = setY( tfMuzzle.localPosition, localPos.y );

			return localPos;

		}


		void fitting()
		{
			var pos = tfCamera.localPosition;

			if( pos != cameraLocalPosition )
			{
				tfCamera.localPosition = Vector3.Lerp( pos, cameraLocalPosition, GM.t.delta * fittingRate );

				if( ( tfCamera.localPosition - cameraLocalPosition ).sqrMagnitude <= 0.01f * 0.01f )
				{
					tfCamera.localPosition = cameraLocalPosition;
				}
			}
			//else Debug.Log("asigned");
		}





		Vector3 setZ( Vector3 vc, float newZ )
		{
			return new Vector3( vc.x, vc.y, newZ );
		}

		Vector3 setY( Vector3 vc, float newY )
		{
			return new Vector3( vc.x, newY, vc.z );
		}

	}






	struct HeadSwicther
	{

		SkinnedMeshRender	smr;

		Transform	tfOriginalHead;

		Transform	tfDummyHead;


		public void setup()
		{



		}

		/*
		void initFpsModeParams()
		{

			//tfHead = tf.parent.FindChild( "body/base/chest/head" );
			tfHead = tf.parent.FindChild( "body/ragdoll base/chest/head" );

			fpsCameraPosition -= tfHead.position - tf.parent.GetComponent<Rigidbody>().position;

		}
		*/
	}


}
