using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abss.StructureObject
{
	public interface IStructure
	{
		
		/// <summary>
		/// 
		/// </summary>
		void Build();
		
		/// <summary>
		/// 破壊したい時はこれを呼ぶ
		/// </summary>
		void Destruct();
	
		
		/// <summary>
		/// 
		/// </summary>
		void SwitchToNear();
	
		/// <summary>
		/// 
		/// </summary>
		void SwitchToFar();
		
	}
	
}

