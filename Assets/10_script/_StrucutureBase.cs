using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelGeometry
{
	
	public class _StrucutureBase : MonoBehaviour, IStructure
	{


		public Transform	tf		{ get; protected set; }
	
	
		public GameObject	near	{ get; protected set; }

		public _StructurePart3[] parts { get; protected set; }


		public StructureRenderer3 nearRenderer { get; protected set; }

		public StructurePallet3	colorPallets;


		public bool isReplicated { get; private set; }//







		void Awake()
		{

			deepInit();

		}




		public virtual void deepInit()
		{

			tf = transform;

			near = findPartContents();


			var rb = GetComponent<Rigidbody>();

			if( rb == null )
			{

				rb = gameObject.AddComponent<Rigidbody>();

				rb.mass = 100.0f;


				//var rb = rigidbody;

				//rb.inertiaTensor = rb.inertiaTensor;//

				//rb.inertiaTensorRotation = rb.inertiaTensorRotation;//

			}
		
			rb.isKinematic = true;


			// 極端すぎるかも？　ゆれが少なすぎて違和感はある、でもまぁ見れる
			rb.solverIterations = 1;//
			rb.sleepThreshold = 1.0f; //
		
		}



		public virtual void build()
		{

			if( near != null )
			{
			
				isReplicated = !tf.getComponentInDirectChildren<StructurePartContentsHolder>();//


				var contentsTop = near.GetComponent<_StructurePartContents>();


				var builder = contentsTop.build( tf );

				near = builder.near;

				parts = builder.parts;


				nearRenderer = near.GetComponent<StructureRenderer3>();

				nearRenderer.initPallet( colorPallets );


				var hitter = GetComponent<_StructureHit3>();

				hitter.init( builder );

				hitter.landings = hitter.landings ?? GM.defaultLandings;

			
			
				near.SetActive( true );//
				switchToFar();//
			}

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









	

		public virtual void switchToNear()
		{
			//near.SetActive( true );

			near.GetComponent<Rigidbody>().detectCollisions = true;
			near.GetComponent<MeshRenderer>().enabled = true;
		}
	
		public virtual void switchToFar()
		{
			//near.SetActive( false );

			near.GetComponent<Rigidbody>().detectCollisions = false;
			near.GetComponent<MeshRenderer>().enabled = false;
		}





	
		public virtual void destruct()
		// 破壊したい時はこれを呼ぶ
		{
			Destroy( gameObject );
		}

		public void Init()
		{
			throw new System.NotImplementedException();
		}

		public void Reset()
		{
			throw new System.NotImplementedException();
		}

		public void Build()
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
	}
}

