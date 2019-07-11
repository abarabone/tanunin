using UnityEngine;
using System.Collections;

public class PlayerRagdoll : MonoBehaviour
{

	PlayerAction3		act;

	SkinnedMeshRender	smr;

	public Rigidbody	rbBase	{ get { return rbRagdollParts[0]; } }


	//public Transform	tfObservedCenter;


	Transform	tfBodyBase;

	Transform	tfRagdollBase;
	
	Transform	tfBodyHand;
	Transform	tfRagdollHand;

	Transform	tfBodyHeadCenter;
	Transform	tfRagdollHeadCenter;



	Transform[]	tfBodyParts;

	Transform[]	tfRagdollParts;


	Rigidbody[]	rbRagdollParts;



	AnimationState	msRagdoll;



	void FixedUpdate()
	{

		act.rb.MovePosition( tfRagdollBase.position - act.tfBody.localPosition );

		if( GamePad._l1 )
		{
			var rbPart = rbRagdollParts[ Random.Range( 1, rbRagdollParts.Length - 1 ) ] ?? rbRagdollParts[0];

			//rbPart.AddTorque( Random.onUnitSphere * 500.0f, ForceMode.Impulse );
			rbPart.AddForceAtPosition( Random.onUnitSphere * 3.0f, Random.onUnitSphere, ForceMode.VelocityChange );
		}

	}


	public bool isLowerVelocity( float limit )
	{
		return rbRagdollParts[0].velocity.sqrMagnitude < limit * limit;
	}

	public bool isRagdollMode
	{
		get { return smr.bones == tfRagdollParts; }
	}


	
	
	public void switchToRagdoll()
	{
		if( !isRagdollMode )
		{

			act.tfObservedCenter = tfRagdollHeadCenter;
			
			var tfWapon = act.playerShoot.weapons.current.tf;
			
			tfWapon.parent = tfRagdollHand;
			
			tfWapon.localPosition = Vector3.zero;
			
			tfWapon.localRotation = Quaternion.identity;


			
			//tfRagdollBase.gameObject.SetActive( true );// ここじゃだめ

			for( var i = 1; i < tfBodyParts.Length; i++ )
			{
				
				var tfBodyPart = tfBodyParts[i];
				
				var tfRagdollPart = tfRagdollParts[i];
				
				var rbRagdollPart = rbRagdollParts[i];
				/*
				if( rbRagdollPart != null )
				{
					//rbRagdollPart.position = tfBodyPart.position;
					
					//rbRagdollPart.rotation = tfBodyPart.rotation;

					rbRagdollPart.velocity = act.rb.velocity;//Vector3.zero;//
					
					rbRagdollPart.angularVelocity = act.rb.angularVelocity;//Vector3.zero;//
				}
				else*/ 
				if( tfRagdollPart != null )
				{
					tfRagdollPart.localPosition = tfBodyPart.localPosition;
					
					tfRagdollPart.localRotation = tfBodyPart.localRotation;
				}
				else
				{
					Debug.Log( tfBodyPart.name );
				}
				
			}
			
			tfRagdollBase.position = tfBodyBase.position;
			
			tfRagdollBase.rotation = tfBodyBase.rotation;

			rbRagdollParts[0].velocity = act.rb.velocity;
			
			rbRagdollParts[0].angularVelocity = act.rb.angularVelocity;

			
			act.activateCollider( false );
			
			act.rb.isKinematic = true;


			tfBodyBase.gameObject.SetActive( false );
			
			tfRagdollBase.gameObject.SetActive( true );// ここじゃないとだめっぽい

			smr.switchBones( tfRagdollParts );


			//switchObservedCenter();

		}
	}
	
	public void switchToAnimation()
	{
		if( isRagdollMode )
		{

			act.tfObservedCenter = tfBodyHeadCenter;
			
			var tfWapon = act.playerShoot.weapons.current.tf;
			
			tfWapon.parent = tfBodyHand;
			
			tfWapon.localPosition = Vector3.zero;
			
			tfWapon.localRotation = Quaternion.identity;
			
			
			
			tfRagdollBase.gameObject.SetActive( false );
			
			tfBodyBase.gameObject.SetActive( true );
			
			act.activateCollider( true );
			
			act.rb.isKinematic = false;
			
			smr.switchBones( tfBodyParts );
			
			
			act.rb.MovePosition( tfRagdollBase.position - act.tfBody.localPosition );

			act.rb.MoveRotation( Quaternion.LookRotation( act.rb.rotation * Vector3.forward, Vector3.up ) );


			RagdollCliper.clipPose( tfRagdollBase, msRagdoll.clip );

			act.playMotion( msRagdoll );

			/*
			for( var i = 1; i < tfBodyParts.Length; i++ )
			{
				
				var tfBodyPart = tfBodyParts[i];
				
				var tfRagdollPart = tfRagdollParts[i];

				tfBodyPart.localPosition = tfRagdollPart.localPosition;

				tfBodyPart.localRotation = tfRagdollPart.localRotation;
				
			}

			tfBodyBase.position = tfRagdollBase.position;

			tfBodyBase.rotation = tfRagdollBase.rotation;


			//act.playMotion( ragMotion.getNowPosing() );

			//switchObservedCenter();
			*/

		}
	}


/*
	void switchObservedCenter()
	{Debug.Log( this.tfObservedCenter.name );
		
		var pre = act.tfObservedCenter;

		act.tfObservedCenter = this.tfObservedCenter;

		this.tfObservedCenter = pre;

	}
*/


	public void setup( PlayerAction3 action )
	{

		act = action;


		smr = act.GetComponentInChildren<SkinnedMeshRender>();

		setupRigidbodys( smr.bones );

		tfRagdollBase.parent = null;

		tfRagdollBase.gameObject.SetActive( false );


		tfRagdollBase.name = "base";//

		msRagdoll = RagdollCliper.createState( act.tfBody.GetComponent<Animation>() );//
		msRagdoll.blendMode = AnimationBlendMode.Blend;
		msRagdoll.speed = 0.0f;

	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="tfSrcs">スキンメッシュのボーン</param>
	void setupRigidbodys( Transform[] tfSrcs )
	{

		tfBodyBase = tfSrcs[0];

		tfRagdollBase = this.transform;


		tfBodyParts = tfSrcs;

		tfRagdollParts = new Transform[ tfSrcs.Length ];

		rbRagdollParts = new Rigidbody[ tfSrcs.Length ];


		tfRagdollParts[0] = tfRagdollBase;
		
		rbRagdollParts[0] = tfRagdollBase.GetComponent<Rigidbody>();


		for( var i = 1; i < tfBodyParts.Length; i++ )
		{

			var tfSrc = tfBodyParts[i];

			var tfDst = tfRagdollBase.Find( getPathName(tfSrc,tfBodyBase) );

			if( tfDst != null )
			{

				tfRagdollParts[i] = tfDst;

				rbRagdollParts[ i ] = tfDst.GetComponent<Rigidbody>();// == null ? null : tfDst.GetComponent<Rigidbody>();

				if( tfDst.name == "head" )
				{
					tfBodyHeadCenter = tfSrc.GetChild(0);

					tfRagdollHeadCenter = tfDst.GetChild(0);
				}
				else if( tfDst.name == "wapon" )
				{
					tfBodyHand = tfSrc;

					tfRagdollHand = tfDst;
				}

			}

		}

		setMaxMoveForDepenetration();
	}

	string getPathName( Transform tfPart, Transform tfRoot )
	{

		var name = tfPart.name;

		if( tfPart != tfRoot )
		{
			for( var tf = tfPart.parent; tf != tfRoot; tf = tf.parent )
			{
				
				name = tf.name + "/" + name;
				
			}
		}

		return name;

	}



	protected void setMaxMoveForDepenetration()
	{
		//var rbCrotchLowers = tfBody.GetComponentsInChildren<Rigidbody>();
		foreach( var rbbone in rbRagdollParts )// rbCrotchLowers )// rbBones )
		{
			if( rbbone != null )
			rbbone.maxDepenetrationVelocity = 0.1f;
			// めりこみを解消しようとする反発力の限界を決めてしまう。

		}
		var js = tfBodyBase.GetComponentsInChildren<Joint>();
		foreach( var j in js )// rbBones )
		{
			j.enablePreprocessing = false;
			// よくわからんけど安定するらしい
		}
	}


}
