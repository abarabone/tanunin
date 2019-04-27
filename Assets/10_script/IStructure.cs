using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bg
{
	public interface IStructure
	{
		
		/// <summary>
		/// 生成時初期化
		/// </summary>
		void Init();

		/// <summary>
		/// 再初期化
		/// </summary>
		void Reset();
		

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

