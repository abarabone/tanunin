using UnityEngine;
using System.Collections;

public class Hit3EnemyCenter : _Hit3Character
{




	
	public override void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	{
		
		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{

				act.move.stopping( ref ds, act.speedRate );

				armor.applyDamage( ref ds, 0.0f, 30.0f );
				
				act.changeToDamageMode( attacker );
				
				act.connection.notifyAttack( act, attacker );
				
			}
		}

		//act.rb.AddForceAtPosition( force, hit.point, ForceMode.Impulse );

	}
	
	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, _Action3 attacker )
	{
		
		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{
				
				armor.applyDamage( ref ds, 0.5f, 30.0f );
				
				act.changeToBlowingMode( attacker, 0 );
				
				act.connection.notifyAttack( act, attacker );
				
			}
		}
		
		//act.rb.AddExplosionForce( pressure * 20.0f, center, radius * 10.0f, 1.0f, ForceMode.Impulse );//
		//act.rb.AddForceAtPosition( pressure * 20.0f * 0.75f * ( ( act.tfBody.position - center ).normalized + Vector3.up * 0.25f ), center, ForceMode.Impulse );

	}
	
	public override void eroded( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{

		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{

				act.move.stopping( ref ds, act.speedRate );

				armor.applyDamage( ref ds, 0.3f * GM.t.delta, 30.0f * GM.t.delta );
				
				act.connection.notifyAttack( act, attacker );
				
			}
		}

		//act.rb.AddForceAtPosition( force, hitpos, ForceMode.Force );

	}





	

}
