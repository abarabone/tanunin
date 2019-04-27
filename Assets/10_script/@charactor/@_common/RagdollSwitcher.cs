using UnityEngine;
using System.Collections;
using System.Text;


/// <summary>
/// アニメーションボーン階層をラグドールボーン階層に挿げ替える。
/// ラグドールへの切り替え時には、キネマティックオンとコライダーオフを行う。
/// アニメーションへの復帰時には、逆の操作をしつつラグドール位置/回転を復帰用アニメーションクリップに反映する。
/// </summary>
public class RagdollSwitcher : MonoBehaviour
{

	public Rigidbody[]		rbs			{ get; private set; }

	public Collider[]		colliders	{ get; private set; }

	public AnimationState	msClipPose	{ get; private set; }


	public Transform	tfBase	{ get; private set; }

	public _Action3	 act		{ get; private set; }



	public bool isRagdollMode
	{
		get { return !rbs[ 0 ].isKinematic; }
	}

	public bool isLowerVelocity( float limit )
	{
		return rbs[ 0 ].velocity.sqrMagnitude < limit * limit;
	}



	void FixedUpdate()
	{

		act.rb.MovePosition( rbs[ 0 ].position - act.tfBody.localPosition );


		if( !act.isDead & GamePad._l1 )
		{
			var rbPart = rbs[ Random.Range( 0, rbs.Length - 1 ) ] ?? rbs[ 0 ];

			//rbPart.AddRelativeTorque( Random.onUnitSphere * 5.0f, ForceMode.VelocityChange );// ForceMode.Impulse );
			rbPart.AddForceAtPosition( Random.onUnitSphere * 2.0f, rbPart.worldCenterOfMass + Random.onUnitSphere * 2.0f, ForceMode.VelocityChange );
		}

	}

	

	/// <summary>
	/// 
	/// </summary>
	/// <param name="act"></param>
	/// <param name="tfNewBoneBase"></param>
	public void deepInit( SkinnedMeshRender smr, Transform tfNewBoneBase )
	{

		act = smr.GetComponentInParent<_Action3>();

		replaceBone( smr, act, tfNewBoneBase );


		deepInit( smr );

	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="act"></param>
	public void deepInit( SkinnedMeshRender smr )
	{

		act = smr.GetComponentInParent<_Action3>();


		tfBase = smr.bones[ 0 ];
		
		rbs = tfBase.GetComponentsInChildren<Rigidbody>();

		colliders = tfBase.GetComponentsInChildren<Collider>();


		msClipPose = RagdollCliper.createState( act.tfBody.GetComponent<Animation>() );


		setMaxMoveForDepenetration( rbs );

		switchMode( false );

	}



	public void switchToRagdoll()
	{
		if( isRagdollMode ) return;


		switchMode( true );


		tfBase.SetParent( null );

		rbs[ 0 ].velocity = act.rb.velocity;
		rbs[ 0 ].angularVelocity = act.rb.angularVelocity;

		act.rb.isKinematic = true;
		act.rb.detectCollisions = false;
		act.tfBody.GetComponent<Animation>().enabled = false;

	}

	public void switchToAnimation()
	{
		if( !isRagdollMode ) return;


		switchMode( false );


		act.rb.MovePosition( rbs[ 0 ].position - act.tfBody.localPosition );

		act.rb.MoveRotation( Quaternion.LookRotation( act.rb.rotation * Vector3.forward, Vector3.up ) );


		tfBase.SetParent( act.tfBody, true );

		RagdollCliper.clipPose( tfBase, msClipPose.clip );

		act.playMotion( msClipPose );

		act.rb.isKinematic = false;
		act.rb.detectCollisions = true;
		act.tfBody.GetComponent<Animation>().enabled = true;
		
	}

	void switchMode( bool isToRagdoll )
	{
		foreach( var rb in rbs )
		{
			rb.isKinematic = !isToRagdoll;
		}

		foreach( var c in colliders )
		{
			c.enabled = isToRagdoll;
		}

		this.enabled = isToRagdoll;
	}



	/// <summary>
	/// アニメーション／メッシュ用ボーン階層を、渡されたルートボーンから作成し置き換える。
	/// ボーンは元ボーンと同名のものが取得され、SkinnedMeshRender にセットされる。
	/// </summary>
	/// <param name="tfSkinMeshBones"></param>
	/// <param name="act"></param>
	/// <param name="tfNewBoneBase"></param>
	void replaceBone( SkinnedMeshRender smr, _Action3 act, Transform tfNewBoneBase )
	{

		var tfPreBones = smr.bones;

		var tfRagdollBones = new Transform[ tfPreBones.Length ];


		tfRagdollBones[ 0 ] = tfNewBoneBase;
		

		var tfSrcBase = tfPreBones[ 0 ];

		var tfDstBase = tfRagdollBones[ 0 ];

		tfDstBase.name = tfSrcBase.name;

		
		for( var i = 1 ; i < tfPreBones.Length ; i++ )
		{

			var tfSrc = tfPreBones[ i ];

			var tfDst = tfDstBase.Find( getPathName( tfSrc, tfSrcBase ) );

			if( tfDst != null )
			{

				tfRagdollBones[ i ] = tfDst;
				
			}

		}


		// 観測対象中心位置を置き換える。

		var tfObservedCenter = tfDstBase.Find( getPathName( act.tfObservedCenter, tfSrcBase ) );

		if( tfObservedCenter ) act.tfObservedCenter = tfObservedCenter;


		// プレイヤー用にショット位置を置き換える。

		var pshoot = act.getComponentInDirectChildren<_PlayerShoot3>();

		if( pshoot )
		{
			var tfHand = tfDstBase.Find( getPathName( pshoot.tfHand, tfSrcBase ) );

			if( tfHand ) pshoot.tfHand = tfHand;
		}



		tfPreBones[ 0 ].SetParent( null );

		GameObject.Destroy( tfPreBones[0].gameObject );

		tfNewBoneBase.SetParent( act.tfBody );

		smr.bones = tfRagdollBones;

	}

	string getPathName( Transform tfPart, Transform tfRoot )
	{

		var name = tfPart.name;

		if( tfPart != tfRoot )
		{
			for( var tf = tfPart.parent ; tf != tfRoot ; tf = tf.parent )
			{

				name = tf.name + "/" + name;

			}
		}

		return name;

	}



	void setMaxMoveForDepenetration( Rigidbody[] rbs )
	{
		foreach( var rb in rbs )
		{
			rb.maxDepenetrationVelocity = 0.1f;
			// めりこみを解消しようとする反発力の限界を決めてしまう。

			var jt = rb.GetComponent<Joint>();
			if( jt ) jt.enablePreprocessing = false;
			// よくわからんけど安定するらしい
		}
	}




}




static public class RagdollCliper
{

	static System.Type		type = typeof(Transform);

	static StringBuilder	path = new StringBuilder( 256 );

	static AnimationCurve	curve = new AnimationCurve();


	static public AnimationState createState( Animation anim )
	{

		var clip = new AnimationClip();

		clip.wrapMode = WrapMode.Loop;

		clip.legacy = true;


		anim.AddClip( clip, "ragdoll" );


		var ms = anim[ "ragdoll" ];

		ms.blendMode = AnimationBlendMode.Blend;

		ms.speed = 0.0f;

		return ms;

	}

	static public void clipPose( Transform tfBase, AnimationClip clip )
	{

		var key1 = new Keyframe();
		var key2 = new Keyframe( 1.0f, 0.0f );

		var rot = tfBase.localPosition;

		// ルートのポジション
		key1.value = key2.value = rot.x; clip.SetCurve( tfBase.name, type, "localPosition.x", new AnimationCurve( key1, key2 ) );
		key1.value = key2.value = rot.y; clip.SetCurve( tfBase.name, type, "localPosition.y", new AnimationCurve( key1, key2 ) );
		key1.value = key2.value = rot.z; clip.SetCurve( tfBase.name, type, "localPosition.z", new AnimationCurve( key1, key2 ) );

		setCurve( tfBase, clip );

	}



	static void setCurve( Transform tfBone, AnimationClip clip )
	{

		var lp = path.Length;


		var bonepath = path.Append( tfBone.name ).ToString();//Debug.Log(bonepath);

		var key1 = new Keyframe();
		var key2 = new Keyframe( 1.0f, 0.0f );

		var rot = tfBone.localRotation;

		var c1 = new AnimationCurve();
		var c2 = new AnimationCurve();
		var c3 = new AnimationCurve();
		var c4 = new AnimationCurve();

		// ルート・子の回転
		key1.value = key2.value = rot.x; c1.AddKey( key1 ); c1.AddKey( key2 );
		key1.value = key2.value = rot.y; c2.AddKey( key1 ); c2.AddKey( key2 );
		key1.value = key2.value = rot.z; c3.AddKey( key1 ); c3.AddKey( key2 );
		key1.value = key2.value = rot.w; c4.AddKey( key1 ); c4.AddKey( key2 );
		clip.SetCurve( bonepath, type, "localRotation.x", c1 );
		clip.SetCurve( bonepath, type, "localRotation.y", c2 );
		clip.SetCurve( bonepath, type, "localRotation.z", c3 );
		clip.SetCurve( bonepath, type, "localRotation.w", c4 );


		if( tfBone.childCount > 0 )
		{
			path.Append( "/" );

			for( int i = 0 ; i < tfBone.childCount ; i++ )
			{
				setCurve( tfBone.GetChild( i ), clip );
			}
		}


		path.Length = lp;

	}




}



