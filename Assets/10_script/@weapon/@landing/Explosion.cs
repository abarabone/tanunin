using UnityEngine;
using System.Collections;
using System.Linq;


public class Explosion : _Bullet3
{

	public float	radius;
	
	public float	impulsiveForce;
	
	public float	boringFactor;
	
	public float	damage;//hardness;

	public float	fragmentation;

	public float	downDamage;

	public float	noiseVolume;// なんだっけこれ？雑音の大きさか？？



	ParticleSystem	ps;


	//protected override void deepInit()
	protected void Awake()
	{

		base.Awake();

		ps = GetComponent<ParticleSystem>();

	}
	
	void Update()
	{
		
		if( !ps.IsAlive() )
		{
			ps.Stop();

			final();
		}

	}



	struct HitterAndColliderPair
	{
		public _Hit3    h;
		public Collider c;
	}

	public override void emit( Vector3 pos, Quaternion rot, float rangeFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var sizedRadius = radius * sizeFactor;


		var eo = (Explosion)instantiateWithPoolingGameObject( pos, rot, true );//Instantiate( gameObject, pos, rot ) as GameObject;

		eo.ps.startSize = sizedRadius * 0.8f;

		eo.ps.startSpeed = sizedRadius * 0.8f;

		eo.ps.Play();


		var colliders = Physics.OverlapSphere( pos, sizedRadius, UserLayer.explosionHittable, QueryTriggerInteraction.Collide );

		var cgroups = colliders.Select( col => new HitterAndColliderPair { h=getHitter(col), c=col } ).GroupBy( x => x.h.getParentHitter(), x => x );
		
		foreach( var hittersByParent in cgroups )
		{
			
			//foreach( var c in colliders )
			foreach( var hit in hittersByParent )
			{

				var hitter = hit.h;//getHitter( c );

				if( hitter != null )
				{

					if( hitter.isThroughOwnHit( act, float.PositiveInfinity ) )
					{

						var ds = new DamageSourceUnit( damage, 0.6f, fragmentation, downDamage, 0.0f );

						hitter.blasted( ref ds, impulsiveForce, boringFactor, pos, sizedRadius, hit.c, act );

					}
				}
				else
				{

					//var rb = c.attachedRigidbody;

					////rb.AddForceAtPosition( impulsiveForce * 20.0f * 0.75f * ( ( act.tfBody.position - pos ).normalized + Vector3.up * 0.25f ), pos, ForceMode.Impulse );
					////rb.AddForceAtPosition( impulsiveForce * 0.05f * ( ( rb.position - pos ).normalized + Vector3.up * 0.4f ), pos, ForceMode.Impulse );
					//rb.AddForceAtPosition( impulsiveForce * ( ( rb.position - pos ).normalized * 0.6f + Vector3.up * 0.4f ), pos, ForceMode.Impulse );

				}

			}

			if( hittersByParent.Key != null )
			{
				var ds = new DamageSourceUnit( damage, 0.6f, fragmentation, downDamage, 0.0f );

				hittersByParent.Key.blasted( ref ds, impulsiveForce, boringFactor, pos, sizedRadius, null, act );
			}

		}

		

	}

}
