using UnityEngine;
using System.Collections;

#if false
public class WireBullet : _Bullet3, LineParticlePool.ILineParticle
{

	public StringParticlePool	poolTemplate;

	StringParticlePool	pool { get { return (StringParticlePool)poolTemplate.getBaseInstance(); } }



	public float	bulletSpeed;
	
	public Color	bulletColor;
	
	public float	bulletSize;

	public float	distance;

	
	public float	stiffness;

	public float	damping;




	enum enWireState
	{
		fly,
		fall,
		hitLarge,
		hitSmall,
		receive,
		carrySelf,
		carryItem,
		hangDawn
	}



	public float getSize()
	{
		return bulletSize;
	}




	// 生成 -----------------------------------------

	public override void emit( Vector3 pos, Quaternion rot, float barrelFactor, float sizeFactor, int numCount, _Action3 act, Transform tfMuzzle = null )
	{

		var s = pool.instantiate( pos );

		s.init( this, rot, act );

		s.barrelFactor = barrelFactor;

	}


	
	// 更新処理 -------------------------------------------------
	
	public bool update( LineParticlePool.LineUnit line )
	{
/*
		var td = GM.t.delta;

		var p = pool;



		var posfirst = p.getPosition( line, 0 );
		
		var poslast = p.getPosition( line, p.numberOfSegmnts + 1 );



		switch( (enWireState)line.state )
		{

			case enWireState.fly:
			{
				line.time += td;

				fly( p, line, posfirst );


				if( GamePad._l2 )
				{
					line.state = (int)enWireState.receive;

					line.time = 0.0f;
				}

				return false;
			}

			case enWireState.hitLarge:
			{
				p.moveOnFixedBothEnds( line, posfirst, line.owner.tfBody.position, stiffness, damping );

				if( GamePad._l2 )
				{
					line.state = (int)enWireState.carrySelf;

					var dir = ( posfirst - line.owner.tfBody.position ).normalized;

					line.owner.rb.AddForce( dir * 500.0f, ForceMode.Impulse );

					line.time = 0.0f;
				}

				return false;
			}

			case enWireState.receive:
			{

				line.time += td;

				//var dir = ( line.owner.tfBody.position - posfirst ).normalized;

				//posfirst += dir *( bulletSpeed * td );

				p.pullToLast( line, line.owner.tfBody.position, stiffness, damping );

				return ( posfirst - line.owner.tfBody.position ).sqrMagnitude < 0.5f * 0.5f;

			}

			case enWireState.carrySelf:
			{
				if( GamePad.l2 )
				{
					var dir = ( posfirst - line.owner.tfBody.position ).normalized;
					
					line.owner.rb.AddForce( dir * 800.0f, ForceMode.Force );
					
					p.shrink( line, posfirst, line.owner.tfBody.position );
					
					//if( GamePad._l2 ) line.state = (int)enWireState.carry;
				}
				else if( GamePad.l2_ )
				{

					line.state = (int)enWireState.hangDawn;

					var joint = ((PlayerAction3)line.owner).wireJoint;

					joint.anchor = Vector3.up * ( posfirst - line.owner.tfBody.position ).magnitude;

					joint.gameObject.SetActive( true );

					//line.owner.rb.velocity *= 0.1f;//Vector3.zero;

				}

				return GamePad._l1;
			}

			case enWireState.hangDawn:
			{
				
				//p.moveOnFixedBothEnds( line, posfirst, line.owner.tfBody.position, stiffness, damping );
				p.shrink( line, posfirst, line.owner.tfBody.position );

				if( GamePad._l2 )
				{

					line.state = (int)enWireState.fly;
					
					var joint = ((PlayerAction3)line.owner).wireJoint;

					joint.gameObject.SetActive( false );

					return true;

				}

				break;

			}
			
			default: break;

		}
		*/


		return false;

	}


	/*
	void fly( StringParticlePool p, LineParticlePool.LineUnit line, Vector3 posfirst )
	{

		var dir = line.rotation * Vector3.forward;
		
		var d = bulletSpeed * GM.t.delta;
		
		var vg = Physics.gravity * ( GM.t.delta * line.time );
		
		
		var pos = posfirst + dir * d + vg;
		
		
		hitcheck( line, ref pos, ref posfirst, dir, bulletSpeed * line.time, UserLayer.bulletHittable );
		

		p.moveOnFixedBothEnds( line, pos, line.owner.tfVisualMuzzle.position, stiffness, damping );


	}

	void hitcheck( LineParticlePool.LineUnit line, ref Vector3 posfirst, ref Vector3 poslast, Vector3 dir, float ownHitDist, int hitmask )
	{

		RaycastHit	hit;
		
		var res = Physics.Linecast( poslast, posfirst, out hit, hitmask );

		if( res )
		{
			
			var n = hit.normal;
			
			posfirst = hit.point;// + n * bulletSize;


			line.state = (int)enWireState.hitLarge;
			
			var joint = ((PlayerAction3)line.owner).wireJoint;

			joint.connectedBody = hit.rigidbody;
			
			joint.connectedAnchor = hit.rigidbody.transform.InverseTransformPoint( posfirst );
			

			
			var landing = GM.defaultLandings.fragmentation;
			
			landing.emit( hit.point, n );


			var	hitter = getHitter( hit.collider );

			if( hitter )
			{
				if( hitter.isThroughOwnHit( line.owner, ownHitDist ) )
				{

					
					var ds = new DamageSourceUnit( 0.0f, 0.0f, 0.0f, 1.0f, 1.0f ); 
					
					hitter.shot( ref ds, dir, ref hit, line.owner );

				}	
			}
			
		}

	}
*/
	


}
#endif