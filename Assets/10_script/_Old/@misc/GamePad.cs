using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;





public class GamePad : MonoBehaviour
{
	
	static public Vector2	ls { get; private set; }
	static public Vector2	rs { get; private set; }
	static public Vector2	ps { get; private set; }


	static public float	mouseMoveRate = 2.0f;

	static public float keyMoveAccelerate = 4.0f;



	static public bool	wasKeyChanged;

	bool	isEnablePolling;


	static public InputUnit[]	inputs	= new InputUnit[ (int)EnKeyUnits.length ];

	static bool isMouseAllocated;// カーソルのロックのための、マウスが操作にバインドされているかのフラグ。


	
	public enum EnKeyUnits	// 扱えるキーの種類。ＰＳ２コントローラ互換。
	{
		upD,
		downD,
		leftD,
		rightD,

		maru,
		sankaku,
		shikaku,
		batsu,

		select,
		start,

		l1,
		l2,
		l3,

		r1,
		r2,
		r3,
		
		upL,
		downL,
		leftL,
		rightL,

		upR,
		downR,
		leftR,
		rightR,

		length
	}



	// ---------------------------------------------

	void Awake()
	{

		if( PlayerPrefs.HasKey( "0:type" ) )
		{
			load();
		}
		else
		{
			initForGamePadStyle();
		}

		isEnablePolling = true;
	}


	void OnDestroy()
	{
		if( wasKeyChanged )
		{
			save();
		}
	}


	void Update()
	{

		if( Input.GetKeyDown( KeyCode.Return ) )//|| GamePad.requestMenuOpen )
		{

			var parseIF = transform.Find( "UIcameraPers" ).gameObject;
			parseIF.SetActive( !parseIF.activeSelf );

			var orthoIF = transform.Find( "IFCamera" ).gameObject;
			orthoIF.SetActive( !parseIF.activeSelf );


			isEnablePolling = !parseIF.activeSelf;


			checkMouseAllocated();


			if( isEnablePolling )
			{
				System.GC.Collect();

				Cursor.lockState = isMouseAllocated ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !isMouseAllocated;
			}
			else
			{
				clearAllStates();

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}

		}

		if( isEnablePolling )
		{

			pollAll();

		}

	}


	void OnApplicationFocus( bool isGotFocus )
	{
		if( isGotFocus )
		{
			Cursor.lockState = isMouseAllocated ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !isMouseAllocated;
		}
	}


	static void initForGamePadStyle()// ゲームパッドスタイルに初期化する。
	{
		inputs[ (int)EnKeyUnits.upD ].initForAxisDevice( "5", 1.0f, 0.3f );
		inputs[ (int)EnKeyUnits.downD ].initForAxisDevice( "5", -1.0f, 0.3f );
		inputs[ (int)EnKeyUnits.rightD ].initForAxisDevice( "4", 1.0f, 0.3f );
		inputs[ (int)EnKeyUnits.leftD ].initForAxisDevice( "4", -1.0f, 0.3f );

		inputs[ (int)EnKeyUnits.maru ].initForKeyDevice( KeyCode.Joystick1Button1 );
		inputs[ (int)EnKeyUnits.sankaku ].initForKeyDevice( KeyCode.Joystick1Button0 );
		inputs[ (int)EnKeyUnits.shikaku ].initForKeyDevice( KeyCode.Joystick1Button3 );
		inputs[ (int)EnKeyUnits.batsu ].initForKeyDevice( KeyCode.Joystick1Button2 );

		inputs[ (int)EnKeyUnits.select ].initForKeyDevice( KeyCode.Joystick1Button9 );
		inputs[ (int)EnKeyUnits.start ].initForKeyDevice( KeyCode.Joystick1Button8 );

		inputs[ (int)EnKeyUnits.l1 ].initForKeyDevice( KeyCode.Joystick1Button6 );
		inputs[ (int)EnKeyUnits.l2 ].initForKeyDevice( KeyCode.Joystick1Button4 );
		inputs[ (int)EnKeyUnits.l3 ].initForKeyDevice( KeyCode.Joystick1Button10 );
		inputs[ (int)EnKeyUnits.r1 ].initForKeyDevice( KeyCode.Joystick1Button7 );
		inputs[ (int)EnKeyUnits.r2 ].initForKeyDevice( KeyCode.Joystick1Button5 );
		inputs[ (int)EnKeyUnits.r3 ].initForKeyDevice( KeyCode.Joystick1Button11 );

		inputs[ (int)EnKeyUnits.upL ].initForAxisDevice( "1", 1.0f, 0.3f, true );
		inputs[ (int)EnKeyUnits.downL ].initForAxisDevice( "1", -1.0f, 0.3f, true );
		inputs[ (int)EnKeyUnits.rightL ].initForAxisDevice( "0", 1.0f, 0.3f );
		inputs[ (int)EnKeyUnits.leftL ].initForAxisDevice( "0", -1.0f, 0.3f );

		inputs[ (int)EnKeyUnits.upR ].initForAxisDevice( "3", 1.0f, 0.3f, true );
		inputs[ (int)EnKeyUnits.downR ].initForAxisDevice( "3", -1.0f, 0.3f, true );
		inputs[ (int)EnKeyUnits.rightR ].initForAxisDevice( "2", 1.0f, 0.3f );
		inputs[ (int)EnKeyUnits.leftR ].initForAxisDevice( "2", -1.0f, 0.3f );
	}

	static void initForMouseStyle()// キーボードマウススタイルに初期化する。
	{
		inputs[ (int)EnKeyUnits.upD ].initForKeyDevice( KeyCode.UpArrow, keyMoveAccelerate );
		inputs[ (int)EnKeyUnits.downD ].initForKeyDevice( KeyCode.DownArrow, keyMoveAccelerate );
		inputs[ (int)EnKeyUnits.rightD ].initForKeyDevice( KeyCode.RightArrow, keyMoveAccelerate );
		inputs[ (int)EnKeyUnits.leftD ].initForKeyDevice( KeyCode.LeftArrow, keyMoveAccelerate );

		inputs[ (int)EnKeyUnits.maru ].initForKeyDevice( KeyCode.Alpha1 );
		inputs[ (int)EnKeyUnits.sankaku ].initForKeyDevice( KeyCode.Alpha2 );
		inputs[ (int)EnKeyUnits.shikaku ].initForKeyDevice( KeyCode.Alpha3 );
		inputs[ (int)EnKeyUnits.batsu ].initForKeyDevice( KeyCode.Alpha4 );

		inputs[ (int)EnKeyUnits.select ].initForKeyDevice( KeyCode.F2 );
		inputs[ (int)EnKeyUnits.start ].initForKeyDevice( KeyCode.F1 );

		inputs[ (int)EnKeyUnits.l1 ].initForKeyDevice( KeyCode.Space );
		inputs[ (int)EnKeyUnits.l2 ].initForAxisDevice( "mw", -1.0f * mouseMoveRate, 0.3f );
		inputs[ (int)EnKeyUnits.l3 ].initForKeyDevice( KeyCode.LeftShift );
		inputs[ (int)EnKeyUnits.r1 ].initForKeyDevice( KeyCode.Mouse0 );
		inputs[ (int)EnKeyUnits.r2 ].initForKeyDevice( KeyCode.Mouse1 );
		inputs[ (int)EnKeyUnits.r3 ].initForKeyDevice( KeyCode.Mouse3 );

		inputs[ (int)EnKeyUnits.upL ].initForKeyDevice( KeyCode.W, keyMoveAccelerate );
		inputs[ (int)EnKeyUnits.downL ].initForKeyDevice( KeyCode.S, keyMoveAccelerate );
		inputs[ (int)EnKeyUnits.rightL ].initForKeyDevice( KeyCode.D, keyMoveAccelerate );
		inputs[ (int)EnKeyUnits.leftL ].initForKeyDevice( KeyCode.L, keyMoveAccelerate );

		inputs[ (int)EnKeyUnits.upR ].initForAxisDevice( "my", -1.0f * mouseMoveRate, 0.3f, true );
		inputs[ (int)EnKeyUnits.downR ].initForAxisDevice( "my", 1.0f * mouseMoveRate, 0.3f, true );
		inputs[ (int)EnKeyUnits.rightR ].initForAxisDevice( "mx", 1.0f * mouseMoveRate, 0.3f );
		inputs[ (int)EnKeyUnits.leftR ].initForAxisDevice( "mx", -1.0f * mouseMoveRate, 0.3f );
	}

	static void load()
	{
		for( var i = EnKeyUnits.upD ; i < (EnKeyUnits)inputs.Length ; i++ )
		{
			inputs[ (int)i ].load( i );
		}

		wasKeyChanged = false;
	}

	static void save()
	{
		for( var i = EnKeyUnits.upD ; i < (EnKeyUnits)inputs.Length ; i++ )
		{
			inputs[ (int)i ].save( i );
		}

		wasKeyChanged = false;
	}

	static void pollAll()
	{
		for( var i = 0 ; i < (int)EnKeyUnits.length ; i++ ) inputs[ i ].poll();

		var px = inputs[ (int)EnKeyUnits.rightD ].state.value - inputs[ (int)EnKeyUnits.leftD ].state.value;
		var py = inputs[ (int)EnKeyUnits.upD ].state.value - inputs[ (int)EnKeyUnits.downD ].state.value;
		ps = new Vector2( px, py );

		var lx = inputs[ (int)EnKeyUnits.rightL ].state.value - inputs[ (int)EnKeyUnits.leftL ].state.value;
		var ly = inputs[ (int)EnKeyUnits.upL ].state.value - inputs[ (int)EnKeyUnits.downL ].state.value;
		ls = new Vector2( lx, ly );

		var rx = inputs[ (int)EnKeyUnits.rightR ].state.value - inputs[ (int)EnKeyUnits.leftR ].state.value;
		var ry = inputs[ (int)EnKeyUnits.upR ].state.value - inputs[ (int)EnKeyUnits.downR ].state.value;
		rs = new Vector2( rx, ry );
	}

	static void clearAllStates()
	{
		for( var i = 0 ; i < (int)EnKeyUnits.length ; i++ ) inputs[ i ].clearState();
		ps = Vector2.zero;
		ls = Vector2.zero;
		rs = Vector2.zero;
	}

	static void checkMouseAllocated()
	{
		isMouseAllocated = false;

		for( var i = 0 ; i < (int)EnKeyUnits.length ; i++ ) isMouseAllocated |= inputs[ i ].isMouseDevice();
	}



	static public bool up { get { return inputs[ (int)EnKeyUnits.upD ].state.push; } }
	static public bool up_ { get { return inputs[ (int)EnKeyUnits.upD ].state.up; } }
	static public bool _up { get { return inputs[ (int)EnKeyUnits.upD ].state.down; } }

	static public bool down { get { return inputs[ (int)EnKeyUnits.downD ].state.push; } }
	static public bool down_ { get { return inputs[ (int)EnKeyUnits.downD ].state.up; } }
	static public bool _down { get { return inputs[ (int)EnKeyUnits.downD ].state.down; } }

	static public bool left { get { return inputs[ (int)EnKeyUnits.leftD ].state.push; } }
	static public bool left_ { get { return inputs[ (int)EnKeyUnits.leftD ].state.up; } }
	static public bool _left { get { return inputs[ (int)EnKeyUnits.leftD ].state.down; } }

	static public bool right { get { return inputs[ (int)EnKeyUnits.rightD ].state.push; } }
	static public bool right_ { get { return inputs[ (int)EnKeyUnits.rightD ].state.up; } }
	static public bool _right { get { return inputs[ (int)EnKeyUnits.rightD ].state.down; } }

	static public bool sankaku { get { return inputs[ (int)EnKeyUnits.sankaku ].state.push; } }
	static public bool sankaku_ { get { return inputs[ (int)EnKeyUnits.sankaku ].state.up; } }
	static public bool _sankaku { get { return inputs[ (int)EnKeyUnits.sankaku ].state.down; } }

	static public bool batsu { get { return inputs[ (int)EnKeyUnits.batsu ].state.push; } }
	static public bool batsu_ { get { return inputs[ (int)EnKeyUnits.batsu ].state.up; } }
	static public bool _batsu { get { return inputs[ (int)EnKeyUnits.batsu ].state.down; } }

	static public bool shikaku { get { return inputs[ (int)EnKeyUnits.shikaku ].state.push; } }
	static public bool shikaku_ { get { return inputs[ (int)EnKeyUnits.shikaku ].state.up; } }
	static public bool _shikaku { get { return inputs[ (int)EnKeyUnits.shikaku ].state.down; } }

	static public bool maru { get { return inputs[ (int)EnKeyUnits.maru ].state.push; } }
	static public bool maru_ { get { return inputs[ (int)EnKeyUnits.maru ].state.up; } }
	static public bool _maru { get { return inputs[ (int)EnKeyUnits.maru ].state.down; } }

	static public bool select { get { return inputs[ (int)EnKeyUnits.select ].state.push; } }
	static public bool select_ { get { return inputs[ (int)EnKeyUnits.select ].state.up; } }
	static public bool _select { get { return inputs[ (int)EnKeyUnits.select ].state.down; } }

	static public bool start { get { return inputs[ (int)EnKeyUnits.start ].state.push; } }
	static public bool start_ { get { return inputs[ (int)EnKeyUnits.start ].state.up; } }
	static public bool _start { get { return inputs[ (int)EnKeyUnits.start ].state.down; } }

	static public bool l1 { get { return inputs[ (int)EnKeyUnits.l1 ].state.push; } }
	static public bool l1_ { get { return inputs[ (int)EnKeyUnits.l1 ].state.up; } }
	static public bool _l1 { get { return inputs[ (int)EnKeyUnits.l1 ].state.down; } }

	static public bool l2 { get { return inputs[ (int)EnKeyUnits.l2 ].state.push; } }
	static public bool l2_ { get { return inputs[ (int)EnKeyUnits.l2 ].state.up; } }
	static public bool _l2 { get { return inputs[ (int)EnKeyUnits.l2 ].state.down; } }

	static public bool l3 { get { return inputs[ (int)EnKeyUnits.l3 ].state.push; } }
	static public bool l3_ { get { return inputs[ (int)EnKeyUnits.l3 ].state.up; } }
	static public bool _l3 { get { return inputs[ (int)EnKeyUnits.l3 ].state.down; } }

	static public bool r1 { get { return inputs[ (int)EnKeyUnits.r1 ].state.push; } }
	static public bool r1_ { get { return inputs[ (int)EnKeyUnits.r1 ].state.up; } }
	static public bool _r1 { get { return inputs[ (int)EnKeyUnits.r1 ].state.down; } }

	static public bool r2 { get { return inputs[ (int)EnKeyUnits.r2 ].state.push; } }
	static public bool r2_ { get { return inputs[ (int)EnKeyUnits.r2 ].state.up; } }
	static public bool _r2 { get { return inputs[ (int)EnKeyUnits.r2 ].state.down; } }

	static public bool r3 { get { return inputs[ (int)EnKeyUnits.r3 ].state.push; } }
	static public bool r3_ { get { return inputs[ (int)EnKeyUnits.r3 ].state.up; } }
	static public bool _r3 { get { return inputs[ (int)EnKeyUnits.r3 ].state.down; } }

	static public bool pu { get { return inputs[ (int)EnKeyUnits.upD ].state.push; } }
	static public bool pu_ { get { return inputs[ (int)EnKeyUnits.upD ].state.up; } }
	static public bool _pu { get { return inputs[ (int)EnKeyUnits.upD ].state.down; } }

	static public bool pd { get { return inputs[ (int)EnKeyUnits.downD ].state.push; } }
	static public bool pd_ { get { return inputs[ (int)EnKeyUnits.downD ].state.up; } }
	static public bool _pd { get { return inputs[ (int)EnKeyUnits.downD ].state.down; } }

	static public bool pl { get { return inputs[ (int)EnKeyUnits.leftD ].state.push; } }
	static public bool pl_ { get { return inputs[ (int)EnKeyUnits.leftD ].state.up; } }
	static public bool _pl { get { return inputs[ (int)EnKeyUnits.leftD ].state.down; } }

	static public bool pr { get { return inputs[ (int)EnKeyUnits.rightD ].state.push; } }
	static public bool pr_ { get { return inputs[ (int)EnKeyUnits.rightD ].state.up; } }
	static public bool _pr { get { return inputs[ (int)EnKeyUnits.rightD ].state.down; } }

	static public bool lu { get { return inputs[ (int)EnKeyUnits.upL ].state.push; } }
	static public bool lu_ { get { return inputs[ (int)EnKeyUnits.upL ].state.up; } }
	static public bool _lu { get { return inputs[ (int)EnKeyUnits.upL ].state.down; } }

	static public bool ld { get { return inputs[ (int)EnKeyUnits.downL ].state.push; } }
	static public bool ld_ { get { return inputs[ (int)EnKeyUnits.downL ].state.up; } }
	static public bool _ld { get { return inputs[ (int)EnKeyUnits.downL ].state.down; } }

	static public bool ll { get { return inputs[ (int)EnKeyUnits.leftL ].state.push; } }
	static public bool ll_ { get { return inputs[ (int)EnKeyUnits.leftL ].state.up; } }
	static public bool _ll { get { return inputs[ (int)EnKeyUnits.leftL ].state.down; } }

	static public bool lr { get { return inputs[ (int)EnKeyUnits.rightL ].state.push; } }
	static public bool lr_ { get { return inputs[ (int)EnKeyUnits.rightL ].state.up; } }
	static public bool _lr { get { return inputs[ (int)EnKeyUnits.rightL ].state.down; } }

	static public bool u { get { return inputs[ (int)EnKeyUnits.upR ].state.push; } }
	static public bool ru_ { get { return inputs[ (int)EnKeyUnits.upR ].state.up; } }
	static public bool _ru { get { return inputs[ (int)EnKeyUnits.upR ].state.down; } }

	static public bool rd { get { return inputs[ (int)EnKeyUnits.downR ].state.push; } }
	static public bool rd_ { get { return inputs[ (int)EnKeyUnits.downR ].state.up; } }
	static public bool _rd { get { return inputs[ (int)EnKeyUnits.downR ].state.down; } }

	static public bool rl { get { return inputs[ (int)EnKeyUnits.leftR ].state.push; } }
	static public bool rl_ { get { return inputs[ (int)EnKeyUnits.leftR ].state.up; } }
	static public bool _rl { get { return inputs[ (int)EnKeyUnits.leftR ].state.down; } }

	static public bool rr { get { return inputs[ (int)EnKeyUnits.rightR ].state.push; } }
	static public bool rr_ { get { return inputs[ (int)EnKeyUnits.rightR ].state.up; } }
	static public bool _rr { get { return inputs[ (int)EnKeyUnits.rightR ].state.down; } }


	static public void setInputsForGamePad()
	{
		initForGamePadStyle();

		wasKeyChanged = true;
	}

	static public void setInputsForKeyAndMouse()
	{
		initForMouseStyle();

		wasKeyChanged = true;
	}



	//[StructLayout( LayoutKind.Explicit )]
	public struct InputUnit
	{
		
		//[FieldOffset( 0 )]
		public EnDeviceType	type;

		//[FieldOffset( 4 )]
		public InputValues	state;


		//[FieldOffset( 16 )]
		AxisDevice		deviceAxis;

		//[FieldOffset( 16 )]
		KeyDevice		deviceKey;


		public bool isSameDevice( string name, float sign )
		{
			if( type != EnDeviceType.axis ) return false;
			
			return deviceAxis.isSame( name, sign );
		}

		public bool isSameDevice( KeyCode code )
		{
			if( type != EnDeviceType.key ) return false;

			return deviceKey.isSame( code );
		}
		
		public void initForAxisDevice( string deviceName, float invertSign, float trigerLimit, bool isNeedInvert = false )
		{
			type = EnDeviceType.axis;

			deviceAxis.init( deviceName, invertSign * ( isNeedInvert ? -1.0f : 1.0f ), trigerLimit );
		}

		public void initForKeyDevice( KeyCode deviceCode, float acc = 0.0f )
		{
			type = EnDeviceType.key;

			deviceKey.init( deviceCode, acc );
		}

		public void poll()
		{
			switch( type )
			{
				case EnDeviceType.axis:

					state = deviceAxis.poll( state );

					break;

				case EnDeviceType.key:

					state = deviceKey.poll( state );

					break;
			}
		}
		
		public void load( EnKeyUnits keyId )
		{
			var id = ( (int)keyId ).ToString();

			var typeId = id + ":type";

			if( !PlayerPrefs.HasKey( typeId ) ) return;

			type = (EnDeviceType)PlayerPrefs.GetInt( typeId );

			switch( type )
			{
				case EnDeviceType.axis: deviceAxis.load( id );	break;
				case EnDeviceType.key:	deviceKey.load( id );	break;
			}
		}

		public void save( EnKeyUnits keyId )
		{
			var id = ( (int)keyId ).ToString();

			PlayerPrefs.SetInt( id + ":type", (int)type );

			switch( type )
			{
				case EnDeviceType.axis:	deviceAxis.save( id );	break;
				case EnDeviceType.key:	deviceKey.save( id );	break;
			}
		}

		public bool isPushing()
		{
			switch( type )
			{
				case EnDeviceType.axis: return deviceAxis.isPushing();
				case EnDeviceType.key:	return deviceKey.isPushing();
			}
			return false;
		}
		public float getValue()
		{
			switch( type )
			{
				case EnDeviceType.axis: return deviceAxis.getValue();
				case EnDeviceType.key:	return deviceKey.getValue();
			}
			return 0.0f;
		}

		public void clearState()
		{
			state = new InputValues();
		}

		public bool isMouseDevice()
		{
			switch( type )
			{
				case EnDeviceType.axis: return deviceAxis.isMouseDevice();
				case EnDeviceType.key: return deviceKey.isMouseDevice();
			}
			return false;
		}
	}



	public enum EnDeviceType
	{
		none,
		axis,
		key
	}

	public struct InputValues
	{
		public float	value;

		public bool	push;
		public bool	down;
		public bool	up;
	}


	struct AxisDevice
	{

		string	name;


		float	sign;

		float	limit;


		public void init( string axisName, float invertSign, float trigerLimit )
		{
			name = axisName;

			sign = invertSign;

			limit = trigerLimit;
		}

		public InputValues poll( InputValues prev )
		{

			var current = new InputValues();


			var rawValue = Input.GetAxisRaw( name ) * sign;

			current.value = rawValue > 0.0f ? rawValue : 0.0f;

			current.push = rawValue > limit;
			current.down = ( prev.push ^ current.push ) & current.push;
			current.up = ( prev.push ^ current.push ) & !current.push;


			return current;
		}

		public bool isPushing()
		{
			return Input.GetAxisRaw( name ) * sign > limit;
		}
		public float getValue()
		{
			var rawValue = Input.GetAxisRaw( name ) * sign;

			return rawValue > 0.0f ? rawValue : 0.0f;
		}

		public void load( string keyName )
		{
			name = PlayerPrefs.GetString( keyName + ":name" );
			sign = PlayerPrefs.GetFloat( keyName + ":sign" );
			limit = PlayerPrefs.GetFloat( keyName + ":limit" );
		}
		public void save( string keyName )
		{
			PlayerPrefs.DeleteKey( keyName + ":code" );
			PlayerPrefs.DeleteKey( keyName + ":acc" );

			PlayerPrefs.SetString( keyName + ":name", name );
			PlayerPrefs.SetFloat( keyName + ":sign", sign );
			PlayerPrefs.SetFloat( keyName + ":limit", limit );
		}

		public bool isSame( string otherName, float otherSign )
		{
			return name == otherName & sign == otherSign;
		}

		public bool isMouseDevice()
		{
			return name[ 0 ] == 'm';
		}
	}

	struct KeyDevice
	{

		KeyCode	code;

		float	acceleration;


		public void init( KeyCode keyCode, float acc )
		{
			code = keyCode;

			acceleration = acc;
		}

		public InputValues poll( InputValues prev )
		{
			var current = new InputValues();


			current.push = Input.GetKey( code );

			current.down = ( prev.push ^ current.push ) & current.push;
			current.up = ( prev.push ^ current.push ) & !current.push;


			if( acceleration != 0.0f & current.push )
			{
				var value = prev.value + acceleration * GM.t.delta;

				current.value = value > 1.0f ? 1.0f : value;
			}


			return current;
		}

		public bool isPushing()
		{
			return Input.GetKey( code );
		}
		public float getValue()
		{
			return Input.GetKey( code ) ? 1.0f : 0.0f;
		}

		public void load( string keyName )
		{
			code = (KeyCode)PlayerPrefs.GetInt( keyName + ":code" );
			acceleration = PlayerPrefs.GetFloat( keyName + ":acc" );
		}
		public void save( string keyName )
		{
			PlayerPrefs.DeleteKey( keyName + ":name" );
			PlayerPrefs.DeleteKey( keyName + ":sign" );
			PlayerPrefs.DeleteKey( keyName + ":limit" );

			PlayerPrefs.SetInt( keyName + ":code", (int)code );
			PlayerPrefs.SetFloat( keyName + ":acc", acceleration );
		}

		public bool isSame( KeyCode otherCode )
		{
			return code == otherCode;
		}

		public bool isMouseDevice()
		{
			return code == KeyCode.Mouse0 | code == KeyCode.Mouse1 | code == KeyCode.Mouse2 | code == KeyCode.Mouse3 | code == KeyCode.Mouse4 | code == KeyCode.Mouse5 | code == KeyCode.Mouse6;
		}
	}

}
