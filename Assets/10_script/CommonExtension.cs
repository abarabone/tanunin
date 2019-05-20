using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Runtime.InteropServices;

namespace Abss.Common.Extension
{
	public static class LinqExtension
	{ 

		public static IEnumerable<Tresult> Zip<T1,T2,Tresult>
			( in this (IEnumerable<T1> e1, IEnumerable<T2> e2) src, Func<T1,T2,Tresult> resultSelector )
		{
			return Enumerable.Zip( src.e1, src.e2, resultSelector );
		}
		public static IEnumerable<(T1, T2)> Zip<T1,T2>( in this (IEnumerable<T1> e1, IEnumerable<T2> e2) src )
		{
			return Enumerable.Zip(src.e1, src.e2, (x, y)=>(x, y) );
		}

		public static IEnumerable<Tresult> Zip<T1,T2,T3,Tresult>
			( this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3) src, Func<T1,T2,T3,Tresult> resultSelector )
		{
			//return src.e1.Zip( src.e2, (x,y)=>(x,y) ).Zip( src.e3, (xy,z)=>resultSelector(xy.x, xy.y, z) );
			var etor1 = src.e1.GetEnumerator();
			var etor2 = src.e2.GetEnumerator();
			var etor3 = src.e3.GetEnumerator();
			while( etor1.MoveNext() && etor2.MoveNext() && etor3.MoveNext() )
			{
				yield return resultSelector( etor1.Current, etor2.Current, etor3.Current );
			}
		}
		public static IEnumerable<(T1,T2,T3)> Zip<T1,T2,T3>( in this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3) src )
		{
			return src.Zip( (x,y,z)=>(x,y,z) );
		}

	}

	public static class ConversionExtension
	{
		// As はイメージそのままで型表現だけ変更、To は変換されるんだけど、名前似てるからバグを生みそう…。
		// → 考えたら To は new Vector4() でええやんけ

		public static float AsFloat( this int value )
		{
			return new FloatInt { IntValue = value }.FloatValue;
		}
		public static int AsInt( this float value )
		{
			return new FloatInt { FloatValue = value }.IntValue;
		}
		public static Vector4 AsVector4( in this (int x, int y, int z, int w) v )
		{
			return new Vector4( v.x.AsFloat(), v.y.AsFloat(), v.z.AsFloat(), v.w.AsFloat() );
		}
		//public static Vector4 ToVector4( in this (int x, int y, int z, int w) v )
		//{
		//	return new Vector4( v.x, v.y, v.z, v.w );
		//}
		//public static Vector4 ToVector4( in this (float x, float y, float z, float w) v )
		//{
		//	return new Vector4( v.x, v.y, v.z, v.w );
		//}

		public static Vector4 ToVector4( this int[] value )
		{
			return new Vector4( value[0], value[1], value[2], value[3] );
		}
		public static Vector4 ToVector4( this int[] value, int offset )
		{
			return new Vector4( value[offset+0], value[offset+1], value[offset+2], value[offset+3] );
		}
		public static Vector4 ToVector4( this float[] value )
		{
			return new Vector4( value[0], value[1], value[2], value[3] );
		}
		public static Vector4 ToVector4( this float[] value, int offset )
		{
			return new Vector4( value[offset+0], value[offset+1], value[offset+2], value[offset+3] );
		}

	
		[StructLayout(LayoutKind.Explicit)]
		struct FloatInt
		{
			[FieldOffset(0)] public float FloatValue;
			[FieldOffset(0)] public int IntValue;
		}
	}
}
