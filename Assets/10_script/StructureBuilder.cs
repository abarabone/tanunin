using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Abss.StructureObject
{

	public class StructureBuilder : MonoBehaviour, IDisposable
	{
		
		public void Build( _StrucutureBase s )
		{

			
			//isReplicated = !Tf.getComponentInDirectChildren<StructurePartContentsHolder>();//


			//var contentsTop = s.Near.GetComponent<IStructureContent>();


			//var builder = contentsTop.Build(  );

			//Near = builder.near;

			//Parts = builder.parts;


			//nearRenderer = Near.GetComponent<StructureRenderer3>();

			//nearRenderer.initPallet( colorPallets );


			//var hitter = GetComponent<_StructureHit3>();

			//hitter.init( builder );

			//hitter.landings = hitter.landings ?? GM.defaultLandings;

			
			
			//Near.SetActive( true );//
			//switchToFar();//


		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}

}
