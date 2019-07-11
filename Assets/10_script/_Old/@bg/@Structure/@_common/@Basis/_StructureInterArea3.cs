using UnityEngine;
using System.Collections;

public class _StructureInterArea3 : _Structure3//Replica3
{
	
	
	public Rigidbody	rb	{ get; private set; }

	
	public GameObject	far		{ get; private set; }
	
	public StructureBonedRenderer3	farRenderer	{ get; private set; }
	
	
//	public GameObject	envelope			{ get; private set; }

	public Collider[]	envelopeColliders	{ get; private set; }


	
	
	public short	partId		{ get; private set; }

	public short	entityId	{ get; set; }//	{ get; private set; }
	




	public override void deepInit()
	{
		
		base.deepInit();


		far = findOutlineObject();

		initFar( far );


		var envelope = tf.findWithLayerInDirectChildren( UserLayer._bgEnvelope ).gameObject;

		initEnvelope( envelope );


		rb = GetComponent<Rigidbody>();

	}

	void initFar( GameObject far )
	{

		var mc = far.GetComponent<MeshCollider>();

		mc.enabled = false;	// メッシュコライダは一瞬でも非キネマティックになるとエラーがでてしまう


		var rbFar = far.AddComponent<Rigidbody>();
		
		rbFar.isKinematic = true;


		mc.enabled = true;
		
	}

	void initEnvelope( GameObject env )
	{

		envelopeColliders = env.GetComponents<Collider>();

		setEnvelopeLayer( UserLayer._bgSleepEnvelope );

	}




	public override void build()
	{

		base.build();


		var hitter = GetComponent<_StructureHit3>();


		var farHitter = far.AddComponent<StructureHitFar>();

		farHitter.landings = hitter.landings;

	}


	public virtual void attatchToArea( StructureBonedRenderer3 sbr, int id )
	{

		farRenderer = sbr;

		partId	= (short)id;

		entityId = (short)id;


		farRenderer.setLocationVisible( partId, rb.position, rb.rotation );

	}
	

	protected void OnDestroy()
	{

		if( farRenderer && farRenderer.gameObject.activeInHierarchy )
		{

			farRenderer.requestRecalculateAllBounds( 30.0f );
			
			farRenderer.destroyChildStructure( entityId );

			farRenderer.setLocationInvisible( partId );

		}

	}













	
	
	protected void setEnvelopeLayer( int layer )
	{
		var goenv = envelopeColliders[0].gameObject;
		
		foreach( var env in envelopeColliders ) env.enabled = false;//goenv.SetActive( false );
		goenv.layer = layer;
		foreach( var env in envelopeColliders ) env.enabled = true;//goenv.SetActive( true );
	}















	public override void switchToNear()
	{

		far.SetActive( false );

		farRenderer.setLocationInvisible( partId );


		//near.SetActive( true );
		base.switchToNear();
	}
	
	
	public override void switchToFar()
	{

		//near.SetActive( false );
		base.switchToFar();

		
		farRenderer.setLocationVisible( partId, rb.position, rb.rotation );
		
		far.SetActive( true );
		
	}

	
}