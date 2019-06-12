using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace Abss.StructureObject
{
	
	public class _StructureBase : MonoBehaviour, IStructure
	{

		GameObject	near;
		
		
		Transform	tf;
		


		public void Build()
		{
			
			var content = this.GetComponent<IStructureContent>();
			var near_ = content.GetOrBuildNear();
			
			var rb = getOrCreateRigidBody_();
			initRigidbody_( rb );

			this.tf = this.transform;
			this.near = near_;

			return;


			void initRigidbody_( Rigidbody rb_ )
			{
				
				rb_.isKinematic = true;
				
				// 極端すぎるかも？　ゆれが少なすぎて違和感はある、でもまぁ見れる
				rb_.solverIterations = 1;//
				rb_.sleepThreshold = 1.0f; //
			}

			Rigidbody getOrCreateRigidBody_()
			{
				var rb_ = GetComponent<Rigidbody>();
				if( rb_ != null ) return rb_;
					
				rb_ = gameObject.AddComponent<Rigidbody>();
				rb_.mass = 100.0f;
				//var rb = rigidbody;
				//rb.inertiaTensor = rb.inertiaTensor;//
				//rb.inertiaTensorRotation = rb.inertiaTensorRotation;//
				return rb_;
			}
			
		}

		// 破壊したい時はこれを呼ぶ
		public void Destruct()
		{
			Destroy( this.gameObject );
		}

		/// <summary>
		/// 
		/// </summary>
		public void SwitchToNear()
		{
			//this.near.SetActive( true );
			this.near.GetComponent<Rigidbody>().detectCollisions = true;
			this.near.GetComponent<MeshRenderer>().enabled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void SwitchToFar()
		{
			//this.near.SetActive( false );
			this.near.GetComponent<Rigidbody>().detectCollisions = false;
			this.near.GetComponent<MeshRenderer>().enabled = false;
		}
		
		
	}
}

