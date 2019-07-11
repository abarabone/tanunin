using UnityEngine;
using System.Collections;

public class GamePadConfigKey : MonoBehaviour
{

	public GamePad.EnKeyUnits	deviceId;

	Vector4	currentPosture;




	public void setDeviceForAxis( string name, float sign )
	{
		var input = new GamePad.InputUnit();

		input.initForAxisDevice( name, sign, 0.3f );

		GamePad.inputs[ (int)deviceId ] = input;


		setInvisible();
	}

	public void setDeviceForKey( KeyCode keyCode, float acc )
	{
		var input = new GamePad.InputUnit();

		input.initForKeyDevice( keyCode, acc );

		GamePad.inputs[ (int)deviceId ] = input;


		setInvisible();
	}



	// 強調表示適用 -----------------------------

	public void setInvisible()
	{
		var configer = GetComponentInParent<GamePadConfig>();

		configer.currentSelected = null;

		setMaterial( configer.matNotSelected );
	}

	void setMaterial( Material mat )
	{
		if( deviceId >= GamePad.EnKeyUnits.upL )
		{
			transform.parent.GetChild(0).GetComponent<Renderer>().sharedMaterial = mat;

			var configer = GetComponentInParent<GamePadConfig>();

			GetComponent<Renderer>().enabled = configer.currentSelected == this;
		}
		else
		{

			GetComponent<Renderer>().sharedMaterial = mat;

		}
	}


	// メッセージ -------------------

	void Awake()
	{
		if( deviceId >= GamePad.EnKeyUnits.upL )
		{
			var qti = Quaternion.identity;

			currentPosture = new Vector4( qti.x, qti.y, qti.z, qti.w );

			GetComponent<Renderer>().enabled = false;
		}
		else
		{
			currentPosture = transform.localPosition;
		}
	}


	void OnMouseDown()
	{

		var configer = GetComponentInParent<GamePadConfig>();

		var preSelected = configer.currentSelected;

		configer.currentSelected = configer.currentSelected == this ? null : this;


		if( configer.currentSelected )
		{
			if( preSelected ) preSelected.setMaterial( configer.matNotSelected );

			setMaterial( configer.matSelected );
		}
		else
		{
			setMaterial( configer.matNotSelected );
		}

		configer.isBlockClickBackGround = true;
	}

	void Update()
	{

		var configer = GetComponentInParent<GamePadConfig>();


		if( deviceId >= GamePad.EnKeyUnits.upL )
		{
			turningStick( GamePad.inputs[ (int)deviceId ].getValue() );
		}
		else
		{
			sizingButton( configer.currentSelected == this );

			blendingStick();

			pushingButton( GamePad.inputs[ (int)deviceId ].isPushing() );
		}

	}


	// 子の回転を反映してスティック外見を傾ける -------------------------------

	void blendingStick()
	{
		var tfParent = transform.parent;

		if( tfParent.GetComponent<GamePadConfig>() ) return;

		var rot = Quaternion.identity;

		for( var i = 1 ; i < tfParent.childCount ; i++ )
		{
			var vc4 = tfParent.GetChild( i ).GetComponent<GamePadConfigKey>().currentPosture;

			rot *= new Quaternion( vc4.x, vc4.y, vc4.z, vc4.w );
		}

		transform.localRotation = rot;
	}


	// 自身の外見を変化させる -----------------------

	void pushingButton( bool isOn )
	{

		var tf = transform;


		Vector3	targetPostion;

		if( isOn )
		{
			targetPostion = (Vector3)currentPosture + tf.localRotation * new Vector3( 0.0f, 0.0f, -0.2f );
		}
		else
		{
			targetPostion = currentPosture;
		}


		tf.localPosition = Vector3.Lerp( tf.localPosition, targetPostion, GM.t.delta * 30.0f );
		
	}

	void turningStick( float value )
	{

		var tf = transform;

		var tfParent = transform.parent;


		Quaternion	qtTarget;

		if( value > 0.0f )
		{
			switch( name )
			{
				case "upL":
				case "upR":
					qtTarget = Quaternion.Euler( -30.0f * value, 0.0f, 0.0f ); break;
				case "leftL":
				case "leftR":
					qtTarget = Quaternion.Euler( 0.0f, 30.0f * value, 0.0f ); break;
				case "rightL":
				case "rightR":
					qtTarget = Quaternion.Euler( 0.0f, -30.0f * value, 0.0f ); break;
				case "downL":
				case "downR":
					qtTarget = Quaternion.Euler( 30.0f * value, 0.0f, 0.0f ); break;
				default:
					qtTarget = Quaternion.identity; break;
			}
		}
		else
		{

			qtTarget = Quaternion.identity;

		}


		var rot = new Quaternion( currentPosture.x, currentPosture.y, currentPosture.z, currentPosture.w );

		rot = Quaternion.Lerp( rot, qtTarget, GM.t.delta * 30.0f );

		currentPosture = new Vector4( rot.x, rot.y, rot.z, rot.w );

	}
	
	void sizingButton( bool isOn )
	{

		var tf = transform;


		Vector3	targetScale;

		if( isOn )
		{
			targetScale = new Vector3( 1.3f, 1.3f, 1.0f );
		}
		else
		{
			targetScale = new Vector3( 1.0f, 1.0f, 1.0f );
		}


		tf.localScale = Vector3.Lerp( tf.localScale, targetScale, GM.t.delta * 30.0f );

	}





}
