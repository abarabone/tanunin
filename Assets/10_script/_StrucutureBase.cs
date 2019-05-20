using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abss.StructureObject
{
	
	public class _StrucutureBase : MonoBehaviour, IStructure
	{

		[SerializeField]
		Transform tfContentsTop;


		Transform	tf;
	
		
		GameObject	near;
		
		//IStructurePart[] parts;
		

		//public StructureRenderer3 nearRenderer { get; protected set; }

		//public StructurePallet3	colorPallets;


		//public bool isReplicated { get; private set; }//






		
		void Awake()
		{
			this.Init();
			this.Reset();
		}
		





		protected GameObject findOutlineObject()
		{

			// near が作成されていない前提で far を探している

			return tf.findWithLayerInDirectChildren( UserLayer._bgDetail ).gameObject;

		}



		protected GameObject findPartContents()
		{

			var contents = tf.getComponentInDirectChildren<_StructurePartContents>();
		
			return contents ? contents.gameObject : new GameObject( "near stab" );//tf.GetComponentInChildren<_StructurePartContents3>().gameObject;

		}





	
	
	
	
	
		protected void setLayer( GameObject go, int layer )
		{
			//var cs = go.GetComponents<Collider>();

			//foreach( var c in cs ) c.enabled = false;
			go.SetActive( false );

			go.layer = layer;

			//foreach( var c in cs ) c.enabled = true;
			go.SetActive( true );
		}









	








		public void Init()
		{
			
			this.tf = transform;
			this.near = findPartContents();
			
			initRigidbody_( getOrCreateRigidBody_() );

			return;


			void initRigidbody_( Rigidbody rb )
			{
				
				rb.isKinematic = true;
				
				// 極端すぎるかも？　ゆれが少なすぎて違和感はある、でもまぁ見れる
				rb.solverIterations = 1;//
				rb.sleepThreshold = 1.0f; //
			}
			Rigidbody getOrCreateRigidBody_()
			{
				var rb = GetComponent<Rigidbody>();
				if( rb != null ) return rb;
					
				rb = gameObject.AddComponent<Rigidbody>();
				rb.mass = 100.0f;
				//var rb = rigidbody;
				//rb.inertiaTensor = rb.inertiaTensor;//
				//rb.inertiaTensorRotation = rb.inertiaTensorRotation;//
				return rb;
			}
			
		}

		public void Reset()
		{
			throw new System.NotImplementedException();
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
			//near.SetActive( true );
			near.GetComponent<Rigidbody>().detectCollisions = true;
			near.GetComponent<MeshRenderer>().enabled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void SwitchToFar()
		{
			//near.SetActive( false );
			near.GetComponent<Rigidbody>().detectCollisions = false;
			near.GetComponent<MeshRenderer>().enabled = false;
		}
		
		
	}
}

