using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;






public class GamePadConfig : MonoBehaviour
{


	const	int	axisDeviceLength	= 10;



	public GamePadConfigKey currentSelected { get; set; }

	//GamePadConfigKey preSelected;


	public bool isBlockClickBackGround;


	public Material	matSelected;

	public Material	matNotSelected;

	

	void Update()
	{

		chagePadAngle();

		poll();

	}



	// キー割り当て関係 ---------------------------

	public void setInputsForGamePad()
	{
		GamePad.setInputsForGamePad();
	}

	public void setInputsForKeyAndMouse()
	{
		GamePad.setInputsForKeyAndMouse();
	}

	public void bindAtKeyCode( int keyCode )
	{
		if( currentSelected == null ) return;

		bindAtKey( (KeyCode)keyCode );
	}
	public void bindAtMouseAxisX( float sign )
	{
		if( currentSelected == null ) return;

		bindAtAxis( "mx", sign * GamePad.mouseMoveRate );
	}
	public void bindAtMouseAxisY( float sign )
	{
		if( currentSelected == null ) return;

		bindAtAxis( "my", sign * GamePad.mouseMoveRate );
	}
	public void bindAtMouseAxisW( float sign )
	{
		if( currentSelected == null ) return;

		bindAtAxis( "mw", sign );
	}


	void poll()
	{

		if( currentSelected == null ) return;


		for( var i = 0 ; i < axisDeviceLength ; i++ )
		{
			var name = i.ToString();

			var value = Input.GetAxisRaw( name );

			var sign = value > 0.0f ? 1.0f : -1.0f;

			if( value * sign > 0.5f )
			{
				bindAtAxis( name, sign );

				return;
			}

		}

		for( var i = 0 ; i < keyCodes.Length ; i++ )
		{
			if( Input.GetKey( keyCodes[ i ] ) )
			{
				bindAtKey( keyCodes[ i ] );
				
				return;
			}
		}

	}

	void bindAtAxis( string name, float sign )
	{
		keyMakeFreeByAxisName( name, sign );

		currentSelected.setDeviceForAxis( name, sign );

		GamePad.wasKeyChanged = true;
	}

	void bindAtKey( KeyCode keyCode )
	{
		keyMakeFreeByKeyCode( keyCode );

		currentSelected.setDeviceForKey( keyCode, 3.0f );

		GamePad.wasKeyChanged = true;
	}

	void keyMakeFreeByAxisName( string name, float sign )
	{
		for( var i = 0 ; i < GamePad.inputs.Length ; i++ )
		{
			if( GamePad.inputs[ i ].isSameDevice( name, sign ) )
			{
				GamePad.inputs[ i ] = new GamePad.InputUnit();

				break;
			}
		}
	}

	void keyMakeFreeByKeyCode( KeyCode code )
	{
		for( var i = 0 ; i < GamePad.inputs.Length ; i++ )
		{
			if( GamePad.inputs[ i ].isSameDevice( code ) )
			{
				GamePad.inputs[ i ] = new GamePad.InputUnit();

				break;
			}
		}
	}

	public void freeSelect()
	{
		if( !isBlockClickBackGround )
		{
			if( currentSelected ) currentSelected.setInvisible();
		}

		isBlockClickBackGround = false;
	}

	// コントローラー表示関係 ----------------------------------

	void chagePadAngle()
	{

		var cam = GetComponentInParent<Camera>();

		var tf = transform;

		if( cam.ScreenToViewportPoint( Input.mousePosition ).y > 0.7f )
		{
			tf.localRotation = Quaternion.Lerp( tf.localRotation, Quaternion.Euler( 90.0f, 180.0f, 0.0f ), GM.t.delta * 20.0f );
			tf.localPosition = Vector3.Lerp( tf.localPosition, new Vector3( 0.0f, 3.0f, 12.0f ), GM.t.delta * 20.0f );
		}
		else
		{
			tf.localRotation = Quaternion.Lerp( tf.localRotation, Quaternion.Euler( 0.0f, 180.0f, 0.0f ), GM.t.delta * 20.0f );
			tf.localPosition = Vector3.Lerp( tf.localPosition, new Vector3( 0.0f, 0.15f, 10.5f ), GM.t.delta * 20.0f );
		}

	}




	KeyCode[]	keyCodes =
	{
		KeyCode.Backspace,
		KeyCode.Tab,
		//KeyCode.Clear,
		KeyCode.Return,
		//KeyCode.Pause,
		//KeyCode.Escape,
		KeyCode.Space,
		//KeyCode.Exclaim,
		//KeyCode.DoubleQuote,
		//KeyCode.Hash,
		//KeyCode.Dollar,
		//KeyCode.Ampersand,
		//KeyCode.Quote,
		//KeyCode.LeftParen,
		//KeyCode.RightParen,
		//KeyCode.Asterisk,
		//KeyCode.Plus,
		KeyCode.Comma,
		KeyCode.Minus,
		KeyCode.Period,
		KeyCode.Slash,
		KeyCode.Alpha0,
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Alpha9,
		KeyCode.Colon,
		KeyCode.Semicolon,
		//KeyCode.Less,
		//KeyCode.Equals,
		//KeyCode.Greater,
		//KeyCode.Question,
		//KeyCode.At,
		KeyCode.LeftBracket,
		KeyCode.Backslash,
		KeyCode.RightBracket,
		//KeyCode.Caret,
		//KeyCode.Underscore,
		//KeyCode.BackQuote,
		KeyCode.A,
		KeyCode.B,
		KeyCode.C,
		KeyCode.D,
		KeyCode.E,
		KeyCode.F,
		KeyCode.G,
		KeyCode.H,
		KeyCode.I,
		KeyCode.J,
		KeyCode.K,
		KeyCode.L,
		KeyCode.M,
		KeyCode.N,
		KeyCode.O,
		KeyCode.P,
		KeyCode.Q,
		KeyCode.R,
		KeyCode.S,
		KeyCode.T,
		KeyCode.U,
		KeyCode.V,
		KeyCode.W,
		KeyCode.X,
		KeyCode.Y,
		KeyCode.Z,
		KeyCode.Delete,
		KeyCode.Keypad0,
		KeyCode.Keypad1,
		KeyCode.Keypad2,
		KeyCode.Keypad3,
		KeyCode.Keypad4,
		KeyCode.Keypad5,
		KeyCode.Keypad6,
		KeyCode.Keypad7,
		KeyCode.Keypad8,
		KeyCode.Keypad9,
		KeyCode.KeypadPeriod,
		KeyCode.KeypadDivide,
		KeyCode.KeypadMultiply,
		KeyCode.KeypadMinus,
		KeyCode.KeypadPlus,
		KeyCode.KeypadEnter,
		KeyCode.KeypadEquals,
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.RightArrow,
		KeyCode.LeftArrow,
		KeyCode.Insert,
		KeyCode.Home,
		KeyCode.End,
		KeyCode.PageUp,
		KeyCode.PageDown,
		KeyCode.F1,
		KeyCode.F2,
		KeyCode.F3,
		KeyCode.F4,
		KeyCode.F5,
		KeyCode.F6,
		KeyCode.F7,
		KeyCode.F8,
		KeyCode.F9,
		KeyCode.F10,
		KeyCode.F11,
		KeyCode.F12,
		KeyCode.F13,
		KeyCode.F14,
		KeyCode.F15,
		KeyCode.Numlock,
		KeyCode.CapsLock,
		//KeyCode.ScrollLock,
		KeyCode.RightShift,
		KeyCode.LeftShift,
		KeyCode.RightControl,
		KeyCode.LeftControl,
		KeyCode.RightAlt,
		KeyCode.LeftAlt,
		KeyCode.RightCommand,
		KeyCode.RightApple,
		KeyCode.LeftCommand,
		KeyCode.LeftApple,
		//KeyCode.LeftWindows,
		//KeyCode.RightWindows,
		//KeyCode.AltGr,
		//KeyCode.Help,
		//KeyCode.Print,
		//KeyCode.SysReq,
		//KeyCode.Break,
		KeyCode.Menu,
		//KeyCode.Mouse0,
		KeyCode.Mouse1,
		KeyCode.Mouse2,
		KeyCode.Mouse3,
		KeyCode.Mouse4,
		KeyCode.Mouse5,
		KeyCode.Mouse6,
		KeyCode.JoystickButton0,
		KeyCode.JoystickButton1,
		KeyCode.JoystickButton2,
		KeyCode.JoystickButton3,
		KeyCode.JoystickButton4,
		KeyCode.JoystickButton5,
		KeyCode.JoystickButton6,
		KeyCode.JoystickButton7,
		KeyCode.JoystickButton8,
		KeyCode.JoystickButton9,
		KeyCode.JoystickButton10,
		KeyCode.JoystickButton11,
		KeyCode.JoystickButton12,
		KeyCode.JoystickButton13,
		KeyCode.JoystickButton14,
		KeyCode.JoystickButton15,
		KeyCode.JoystickButton16,
		KeyCode.JoystickButton17,
		KeyCode.JoystickButton18,
		KeyCode.JoystickButton19
	};

}


