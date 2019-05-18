using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
}
