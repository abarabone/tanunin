using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelGeometry
{
	
	public class _StrucutureBase : MonoBehaviour, IStructure
	{


		public Transform	Tf;
	
	
		public GameObject	Near;
		
		public IStructurePart[] Parts;
		

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

			return Tf.findWithLayerInDirectChildren( UserLayer._bgDetail ).gameObject;

		}



		protected GameObject findPartContents()
		{

			var contents = Tf.getComponentInDirectChildren<_StructurePartContents>();
		
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
			
			this.Tf = transform;
			this.Near = findPartContents();

			initRigidbody_();

			return;


			void initRigidbody_()
			{
				var rb = getOrCreate_();
				
				rb.isKinematic = true;
				
				// 極端すぎるかも？　ゆれが少なすぎて違和感はある、でもまぁ見れる
				rb.solverIterations = 1;//
				rb.sleepThreshold = 1.0f; //
			}
			Rigidbody getOrCreate_()
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
		

		public void Destruct()
		{
			throw new System.NotImplementedException();
		}

		public void SwitchToNear()
		{
			throw new System.NotImplementedException();
		}

		public void SwitchToFar()
		{
			throw new System.NotImplementedException();
		}
		
	
		public virtual void destruct()
		// 破壊したい時はこれを呼ぶ
		{
			Destroy( gameObject );
		}


		public virtual void switchToNear()
		{
			//near.SetActive( true );

			Near.GetComponent<Rigidbody>().detectCollisions = true;
			Near.GetComponent<MeshRenderer>().enabled = true;
		}
	
		public virtual void switchToFar()
		{
			//near.SetActive( false );

			Near.GetComponent<Rigidbody>().detectCollisions = false;
			Near.GetComponent<MeshRenderer>().enabled = false;
		}
	}
}

