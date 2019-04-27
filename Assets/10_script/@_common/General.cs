using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

using ul = UserLayer;





public class UserLayer
{
	
	static public readonly int _player			= LayerMask.NameToLayer( "player" );
	static public readonly int _itemObject		= LayerMask.NameToLayer( "itemObject" );
	
	static public readonly int _enemyDetail			= LayerMask.NameToLayer( "enemyDetail" );
	static public readonly int _enemyEnvelope		= LayerMask.NameToLayer( "enemyEnvelope" );
	static public readonly int _enemyEnvelopeMove	= LayerMask.NameToLayer( "enemyEnvelopeMove" );
	static public readonly int _enemyEnvelopeDead	= LayerMask.NameToLayer( "enemyEnvelopeDead" );
	
	static public readonly int _bgPlotEnvelope	= LayerMask.NameToLayer( "bgPlotEnvelope" );
	static public readonly int _bgEnvelope		= LayerMask.NameToLayer( "bgEnvelope" );
	static public readonly int _bgSleepEnvelope	= LayerMask.NameToLayer( "bgSleepEnvelope" );
	static public readonly int _bgField			= LayerMask.NameToLayer( "bgField" );
	
	static public readonly int _bgDetail			= LayerMask.NameToLayer( "bgDetail" );
	static public readonly int _bgDetailOcclusion	= LayerMask.NameToLayer( "bgDetailOcclusion" );
	static public readonly int _bgDetailInvisible	= LayerMask.NameToLayer( "bgDetailInvisible" );
	static public readonly int _bgDetailFence		= LayerMask.NameToLayer( "bgDetailFence" );
	
	static public readonly int _outerWall	= LayerMask.NameToLayer( "outerWall" );

	static public readonly int _ragdollDetail		= LayerMask.NameToLayer( "ragdollDetail" );
	static public readonly int _enemyRagdoll		= LayerMask.NameToLayer( "enemyRagdoll" );
	static public readonly int _enemyRagdollLarge	= LayerMask.NameToLayer( "enemyRagdollLarge" );

	static public readonly int _userInterface	= LayerMask.NameToLayer( "interface" );


	static public readonly int player	= 1 << _player;

	static public readonly int itemObject		= 1 << _itemObject;
	
	static public readonly int enemyDetail			= 1 << _enemyDetail;
	static public readonly int enemyEnvelope		= 1 << _enemyEnvelope;
	static public readonly int enemyEnvelopeMove	= 1 << _enemyEnvelopeMove;
	static public readonly int enemyEnvelopeDead	= 1 << _enemyEnvelopeDead;

	static public readonly int bgSleepEnvelope	= 1 << _bgSleepEnvelope;
	static public readonly int bgEnvelope		= 1 << _bgEnvelope;
	static public readonly int bgPlotEnvelope	= 1 << _bgPlotEnvelope;

	static public readonly int bgField		= 1 << _bgField;
	
	static public readonly int outerWall	= 1 << _outerWall;
	
	static public readonly int bgDetail				= 1 << _bgDetail;
	static public readonly int bgDetailOcclusion	= 1 << _bgDetailOcclusion;
	static public readonly int bgDetailInvisible	= 1 << _bgDetailInvisible;
	static public readonly int bgDetailFence		= 1 << _bgDetailFence;

	static public readonly int ragdollDetail		= 1 << _ragdollDetail;
	static public readonly int enemyRagdoll			= 1 << _enemyRagdoll;
	static public readonly int enemyRagdollLarge	= 1 << _enemyRagdollLarge;

	static public readonly int userInterface		= 1 << _userInterface;

	
	static public readonly int players			= player;
	static public readonly int charMoves		= players | enemyEnvelopeMove;
	
	static public readonly int eyeOcculusionForPlayer	= bgDetail | enemyDetail | bgField;

	static public readonly int groundForPlayer		= /*player | */bgDetail | bgField | enemyDetail | bgDetailInvisible | bgDetailFence | enemyRagdollLarge | itemObject;
	static public readonly int groundForEnemy		= bgSleepEnvelope | bgEnvelope | bgField | enemyEnvelopeDead | enemyRagdollLarge;// | enemyEnvelopeMove;
	static public readonly int groundForEnemyLarge	= bgSleepEnvelope | bgEnvelope | bgField;

	static public readonly int bulletHittableCh	= player | enemyDetail | itemObject | enemyRagdoll | enemyRagdollLarge;//アイテムは暫定
	static public readonly int bulletHittableBg	= bgDetail | bgField | bgDetailInvisible;
	static public readonly int bulletHittable	= bulletHittableCh | bulletHittableBg;

	static public readonly int explosionHittable	= player | enemyEnvelope | enemyEnvelopeDead | bgSleepEnvelope | bgEnvelope | bgField | bgPlotEnvelope | enemyRagdoll | enemyRagdollLarge;

	static public readonly int pressHittable	= bgField | bgEnvelope | bgPlotEnvelope | players | enemyEnvelopeDead | enemyRagdollLarge;
	static public readonly int solasHittable	= bgField | bgPlotEnvelope | players | enemyEnvelopeDead | enemyRagdollLarge;

	static public readonly int fireHittableCh	= player | enemyEnvelope | bgDetailOcclusion | bgDetailFence | enemyRagdoll | enemyRagdollLarge;
	static public readonly int fireHittableBg	= bgDetail | bgField | bgDetailInvisible;
	static public readonly int fireHittable		= fireHittableBg | fireHittableCh;

	static public readonly int homingable		= player | enemyEnvelope;

	static public readonly int acidHittableCh	= player | enemyEnvelope | bgDetailOcclusion | bgDetailFence | enemyRagdoll | enemyRagdollLarge;
	static public readonly int acidHittableBg	= bgDetail | bgField | bgDetailInvisible;

	static public readonly int boundHittableBg	= bgDetail | bgField | bgDetailInvisible;
	
	static public readonly int sensorEyeOcculusion	= bgDetail | bgDetailOcclusion | bgField;

}


public enum enActType
{
	players,
	npcSoldier,
	npcThird,
	enemy
}


public enum enHate
{
	
}





public enum enTeamShift
{
	
	player	= 0,
	
	enemy	= 8,

}

public enum enTeam : uint
{
	
	maskPlayer	= 0xffu,	// 最大８チーム
	
	maskEnemy	= ~0xffu,	// 最大２４チーム

	all		= ~0u,

	none	= 0u

}



public struct TeamLayer
{
	
	public enTeam	flags;


	public TeamLayer setPlayerTeam( int num )
	{
		flags = (enTeam)( 1 << ( (int)enTeamShift.player + num ) );

		return this;
	}

	public TeamLayer setEnemyTeam( int num )
	{
		flags = (enTeam)( 1 << ( (int)enTeamShift.enemy + num ) );

		return this;
	}

	public TeamLayer( uint _flags )
	{
		flags = (enTeam)_flags;
	}
	
	public TeamLayer( enTeam flag )
	{
		flags = (enTeam)flag;
	}
	
	public bool isMate( uint otherTeamFlags )
	{
		return ( flags & (enTeam)otherTeamFlags ) != 0;
	}

	public bool isMate( TeamLayer otherTeam )
	{
		return ( flags & (enTeam)otherTeam.flags ) != 0;
	}

	public bool isPlayerTeam
	{
		get { return ( (uint)flags & (uint)enTeam.maskPlayer ) != 0; }
	}

	public bool isEnemyTeam
	{
		get { return ( (uint)flags & (uint)enTeam.maskEnemy ) != 0; }
	}


	static public TeamLayer playerTeam( int num )
	{
		return new TeamLayer( (enTeam)( 1 << ( (int)enTeamShift.player + num ) ) );
	}

	static public TeamLayer enemyTeam( int num )
	{
		return new TeamLayer( (enTeam)( 1 << ( (int)enTeamShift.enemy + num ) ) );
	}

}




// 拡張メソッド ---------------------------------

static public class TransformExtensions
{
	static public Transform findWithLayerInDirectChildren( this Transform tf, int layer )
	{

		for( var i = tf.childCount; i-- > 0; )
		{
			var tfChild = tf.GetChild(i);
			
			if( tfChild.gameObject.layer == layer )
			{
				return tfChild;
			}
		}

		return null;
	}

	static public Transform findWithLayer( this Transform tf, int layer )
	{

		for( var i = tf.childCount ; i-- > 0 ; )
		{
			var tfChild = tf.GetChild( i );

			if( tfChild.gameObject.layer == layer )
			{
				return tfChild;
			}

			var tfRes = findWithLayer( tfChild, layer );

			if( tfRes != null ) return tfRes;
		}

		return null;
	}

	static public Transform findInParents( this Transform tfStart, string name )
	{

		for( var tf = tfStart.parent ; tf ; tf = tf.parent )
		{

			if( tf.name == name ) return tf;

		}

		return null;
	}

	static public void setChildrenLayer( this Transform tfThis, int layer )
	{
		for( var i = 0 ; i < tfThis.childCount ; i++ )
		{
			var tfChild = tfThis.GetChild( i );

			if( tfChild.childCount > 0 )
			{
				setChildrenLayer( tfChild, layer );
			}
		}

		tfThis.gameObject.layer = layer;
	}
	
}

static public class ComponentExtensions
{
	static public T getComponentInDirectChildren<T>( this Component component ) where T:Component
	{
		
		var tf	= component.transform;

		for( var i = tf.childCount; i-- > 0; )
		{
			var c = tf.GetChild(i).GetComponent<T>();
			
			if( c != null ) return c;
		}
		
		return null;
		
	}

	static public T[] getComponentsInDirectChildren<T>( this Component component ) where T:Component
	{

		var length	= 0;
		
		var tf	= component.transform;
		
		for( var i = tf.childCount; i-- > 0; )
		{
			if( tf.GetChild(i).GetComponent<T>() != null ) length++;
		}
		
		var children = new T[ length ];

		for( var i = tf.childCount; i-- > 0; )
		{
			var c = tf.GetChild(i).GetComponent<T>();

			if( c != null ) children[ --length ] = c;
		}

		return children;

	}

}

static public class GameObjectExtensions
{
	static public T CopyComponent<T>( this GameObject destination, T original ) where T : Component
	{

		System.Type type = original.GetType();

		Component copy = destination.AddComponent(type);

		System.Reflection.FieldInfo[] fields = type.GetFields();


		foreach( System.Reflection.FieldInfo field in fields )
		{
			field.SetValue( copy, field.GetValue( original ) );
		}


		return copy as T;
	}

}

/*
static public class TransformTraverserExtensions
{

	static public IEnumerator<Transform> GetEnumerator( this Transform tfThis )
	{
		Debug.Log(tfThis);
		return new TransformEnumerator( tfThis );
	}

	static public IEnumerable<Transform> enumerate( this Transform tfParent )
	{
		return tfParent.enumerateBottomUp();
	}

	static public IEnumerable<Transform> enumerateBottomUp( this Transform tfParent )
	{
		Debug.Log( "1" );
		for( var i = 0; i < tfParent.childCount; i++ )
		{
			Debug.Log( "2" );
			var tfChild = tfParent.GetChild( i );

			tfChild.enumerateBottomUp();
		}

		Debug.Log( "3" );
		yield return tfParent;
	}

	static public IEnumerable<Transform> enumerateTopDown( this Transform tfParent )
	{
		yield return tfParent;

		for( var i = 0 ; i < tfParent.childCount ; i++ )
		{
			var tfChild = tfParent.GetChild( i );

			tfChild.enumerateTopDown();
		}
	}

}
*/
/*
public struct TransformEnumerator : IEnumerator<Transform>
{

	Transform	tfStart;

	Transform	tfCurrent;


	public TransformEnumerator( Transform tfTop )
	{
		tfStart = tfTop;

		tfCurrent = tfTop;

		while( tfCurrent.childCount > 0 )
		{
			tfCurrent = tfCurrent.GetChild( 0 );
		}
	}


	public Transform Current
	{
		get
		{
			return tfCurrent;
		}
	}

	object IEnumerator.Current
	{
		get { return this.Current; }
	}

	public void Dispose()
	{
		throw new NotImplementedException();
	}

	public bool MoveNext()
	{
		
		if( tfCurrent.parent == null || tfCurrent == tfStart )
		{
			return false;
		}


		var i = tfCurrent.GetSiblingIndex() + 1;

		if( i >= tfCurrent.parent.childCount )
		{
			tfCurrent = tfCurrent.parent;

			return true;
		}


		if( tfCurrent.childCount == 0 )
		{
			return true;
		}


		do
		{
			tfCurrent = tfCurrent.GetChild( 0 );
		}
		while( tfCurrent.childCount > 0 );


		return true;

	}

	public void Reset()
	{
		throw new NotImplementedException();
	}
}
*/


// ----------------------------



[ System.Serializable]
//public class BitBoolArray
public struct BitBoolArray
{
	
	public uint[]	array;
	
	public int		fieldLength { get; private set; }
	
	
	int calculateIntSize( int len )
	{
		return ((len - 1) >> 5) + 1;
	}
	/*	
	public BitBoolArray( int len )
	{
		
		fieldLength = len;
		
		array = new int[ calculateIntSize(len) ];
		
	}
*/	
	
	public void init( int len )
	{
		
		fieldLength = len;
		
		array = new uint[ (calculateIntSize(len) + 1) & ~1 ];// 構造物シェーダのためだけに必ず偶数にしてる
		
	}
	
	public void reinit( int len )
	{

		var intSize = ( calculateIntSize( len ) + 1 ) & ~1;
		
		if( intSize > array.Length )
		{
			array = new uint[ intSize ];
		}
		else
		{
			System.Array.Clear( array, 0, array.Length );//calculateIntSize(fieldLength) );
		}
		
		fieldLength = len;
		
	}
	
	
	int toIndex( int i )
	{
		return i >> 5;
	}
	uint toMask( int i )
	{
		return 1u << ( i & 0x1F );
	}
	
	
	public bool this[ int i ]
	{
		get
		{
			return ( array[ toIndex(i) ] & toMask(i) ) != 0;
		}
		set
		{
			uint mask = toMask(i);
			
			if( value )
			{
				array[	toIndex(i) ] |= mask;
			}
			else
			{
				array[	toIndex(i) ] &= ~mask;
			}
		}
	}


	public uint getArrayBlock( int i )
	{
		return array[ toIndex(i) ];
	}

	public ushort getArrayBlockInShort( int i )
	{
		return (ushort)( array[ toIndex(i) ] >> ((i >> 4) & 0x1) * 16 );
	}

	public Vector4 getArrayBlockShortToVector4( int i )
	{
		var p = array[ toIndex( i ) ];

		var p0 = (float)(( p >>	0 ) & 0xf);
		var p1 = (float)(( p >>	4 ) & 0xf);
		var p2 = (float)(( p >>	8 ) & 0xf);
		var p3 = (float)(( p >> 12 ) & 0xf);

		return new Vector4( p0, p1, p2, p3 );
	}
	public Vector4 getAntiArrayBlockToVector4( int i )
	{
		var ph = ~array[ ( i >> 5 ) & ~1 ];

		var p0 = (float)( ( ph >>  0 ) & 0xffff );
		var p1 = (float)( ( ph >> 16 ) & 0xffff );


		var pl = ~array[ ( i >> 5 ) | 1 ];

		var p2 = (float)( ( pl >>  0 ) & 0xffff );
		var p3 = (float)( ( pl >> 16 ) & 0xffff );

		return new Vector4( p0, p1, p2, p3 );
	}

}


[System.Serializable]
public class BitFloatArray
	//public struct BitFloatArray
{
	
	public uint[]	array;// 		{ get; private set; }
	
	public int		fieldLength;// { get; private set; }
	
	int calculateIntSize( int len )
	{
		return ((len - 1) >> 5) + 1;
	}
	/*	
	public BitFloatArray( BitFloatArray src )
	{
		array	= src.array;
		fieldLength	= src.fieldLength;
	}
	
	public BitFloatArray( int len )
	{
		
		fieldLength = len;
		
		array = new int[ calculateIntSize(len) ];
		
	}
*/	
	public void init( int len, bool allOne = false )
	{
		
		fieldLength = len;
		
		array = new uint[ calculateIntSize(len) ];
		
		if( allOne )
		{
			for( var i = array.Length; i-- > 0; ) array[i] = 0xffffffff;
		}
		
	}
	
	public void reinit( int len )
	{
		
		var intSize = calculateIntSize( len );
		
		if( intSize > array.Length )
		{
			array = new uint[ intSize ];
		}
		else
		{
			System.Array.Clear( array, 0, calculateIntSize(fieldLength) );
		}
		
		fieldLength = len;
		
	}
	
	
	
	int toIndex( int i )
	{
		return i >> 5;
	}
	uint toMask( int i )
	{
		return 1u << ( i & 0x1F );
	}
	
	
	public float this[ int i ]
	{
		get
		{
			if( (array[ toIndex(i) ] & toMask(i)) != 0 )
			{
				return 1.0f;
			}
			else
			{
				return 0.0f;
			}
		}
		set
		{
			uint mask = toMask(i);
			
			if( value != 0.0f )
			{
				array[ toIndex(i) ] |= mask;
			}
			else
			{
				array[ toIndex(i) ] &= ~mask;
			}
		}
	}
	
	public bool isTrue( int i )
	{
		return ( array[ toIndex(i) ] & toMask(i) ) != 0;
	}
	
}






public struct BitFlags
{

	public int	flags;


	public BitFlags( int f )
	{
		flags = f;
	}

	public int set( int f )
	{
		return flags = f;
	}

	public int on( int f )
	{
		return flags |= f;
	}

	public int off( int f )
	{
		return flags &= ~f;
	}

	public bool isOn( int f )
	{
		return (flags & f) != 0;
	}

	public bool isOff( int f )
	{
		return (flags & f) == 0;
	}

	static public BitFlags operator+( BitFlags of, int f )
	{
		return new BitFlags( of.flags | f );
	}

	static public BitFlags operator-( BitFlags of, int f )
	{
		return new BitFlags( of.flags & ~f );
	}

	static public BitFlags operator<<( BitFlags of, int f )
	{
		return new BitFlags( f );
	}

	
	public int onn( int bitnum )
	{
		return flags |= ( 1 << bitnum );
	}

	public int offn( int bitnum )
	{
		return flags &= ~( 1 << bitnum );
	}

}






[StructLayout(LayoutKind.Explicit)]
public struct ifloat
{
	[FieldOffset(0)] public float	f;
	[FieldOffset(0)] public int		i;
	
	public ifloat( float _f ){ i = 0; f = _f; }
	public ifloat( int _i ){ f = 0.0f; i = _i; }
}

static public class MathOpt
// エディタ上でほんの若干速い、最適化でインライン化されればわりといいかも
{

	//[ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
	static public float Sqrt( float f )
	{
		
		var x = new ifloat();// f );
		
		x.f = f;
		
		float xHalf = 0.5f * x.f;
		
		x.i = 0x5F3759DF - ( x.i >> 1 );
		
		
		x.f *= ( 1.5f - ( xHalf * x.f * x.f ) );
		//xRes *= ( 1.5f - ( xHalf * xRes * xRes ) );//コメントアウトを外すと精度が上がる
		
		return x.f * f;
		
	}

	static public float invSqrt( float f )
	{
		
		var x = new ifloat();// f );
		
		x.f = f;
		
		float xHalf = 0.5f * x.f;
		
		x.i = 0x5F3759DF - ( x.i >> 1 );
		
		
		x.f *= ( 1.5f - ( xHalf * x.f * x.f ) );
		//xRes *= ( 1.5f - ( xHalf * xRes * xRes ) );//コメントアウトを外すと精度が上がる
		
		return x.f;
		
	}
	
}





public class MeshUtility
{

	static public bool isReverse( ref Matrix4x4 mt )
	{

		//var up = Vector3.Cross( mt.GetColumn(0), mt.GetColumn(2) );

		//return Vector3.Dot( up, mt.GetColumn(1) ) > 0.0f;


		var up = Vector3.Cross( mt.GetRow(0), mt.GetRow(2) );
		
		return Vector3.Dot( up, mt.GetRow(1) ) > 0.0f;

	}

}






/// <summary>
/// 共通で使用するマトリクス領域を保持する。
/// </summary>
public static class MatrixPool
{
	
	static public List<Matrix4x4>   entries = new List<Matrix4x4>( 64 );

	static public List<Matrix4x4> GetList()
	{
		entries.Clear();
		
		return entries;
	}
}


public static class ShaderId
{
	static public int Matrix { get; private set; }

	static public int Position { get; private set; }

	static public int Color { get; private set; }

	static public int ClassSize { get; private set; }

	static public int Vector { get; private set; }

	static public int Rotation { get; private set; }

	static public int Transrate { get; private set; }


	static ShaderId()
	{
		Matrix		= Shader.PropertyToID( "m" );
		Position	= Shader.PropertyToID( "p" );
		Color		= Shader.PropertyToID( "c" );
		ClassSize	= Shader.PropertyToID( "s" );
		Vector		= Shader.PropertyToID( "v" );
		Rotation	= Shader.PropertyToID( "r" );
		Transrate	= Shader.PropertyToID( "t" );
	}
}

