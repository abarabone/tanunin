using UnityEngine;
using System.Collections;

public class PlayerHit3 : _Hit3Character
{

	public new PlayerAction3	act { get { return (PlayerAction3)base.act; } }

	Rigidbody	rbHit	{ get { return act.ragdoll.isRagdollMode ? act.ragdoll.rbs[ 0 ] : act.rb; } }

	
	public AudioClip	deathBoice2;// あそび



	// 初期化・更新 ----------------------------

	public override void update()
	{

		base.update();
		

		armor.showGui();
		
	}

	// --------------------


	


	public override void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	{
		
		rbHit.AddForceAtPosition( force, hit.point, ForceMode.Impulse );


		var act = this.act;

		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{
				
				act.move.stopping( ref ds, act.speedRate );
				
				armor.applyDamage( ref ds, 0.2f, 1.0f );

				act.changeToDamageMode( attacker );

			}
		}

	}
	
	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, _Action3 attacker )
	{

		var act = this.act;


		var line = act.tfBody.position - center;

		var sqrmag = line.sqrMagnitude;

		var magR = MathOpt.invSqrt( sqrmag );

		var mag = magR * sqrmag;


		//if( action.ragdoll.isRagdollMode ) action.ragdoll.rbBase.AddForceAtPosition( line * ( magR * pressure * 0.6f ) + Vector3.up * 0.4f, center, ForceMode.Impulse );
		//else act.rb.AddForceAtPosition( line * ( magR * pressure * 0.6f ) + Vector3.up * 0.4f, center, ForceMode.Impulse );
		//act.rb.AddExplosionForce( pressure, center, radius, 0.8f, ForceMode.Impulse );//
		//act.rb.AddForceAtPosition( pressure * ( act.tfBody.position - center ).normalized, center, ForceMode.Impulse );

		//if( act.ragdoll.isRagdollMode	) foreach( var i in act.ragdoll.rbs ) i.AddForceAtPosition( ( line + Vector3.up ) * ( magR * pressure ), center, ForceMode.Impulse );
		//else
		rbHit.AddForceAtPosition( (line + Vector3.up) * ( magR * pressure ), center, ForceMode.Impulse );
		//rbHit.AddExplosionForce( pressure, center, radius, 0.8f, ForceMode.Impulse );//

		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{
				
				//var level = ( act.move.speed > 1.5f ) | ( mag / radius < 0.4f ) ? (int)ds.moveStoppingDamage + 1 : 0;
				var level = act.isAvoiding ? (int)(ds.moveStoppingDamage * 0.1f + 1) : 0;

				act.changeToBlowingMode( attacker, level );


				ds.moveStoppingDamage = 0.0f;

				//armor.applyDamage( ref ds, 1.5f, 1.0f );

			}
			else
			{

				act.ragdoll.switchToRagdoll();

			}

		}

	}

	public override void eroded( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{

		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{

				act.move.stopping( ref ds, act.speedRate );

				armor.applyDamage( ref ds, 0.5f, 5.0f * GM.t.delta );
				
			}
		}

		act.rb.AddForceAtPosition( force, hitpos, ForceMode.Force );

	}


	public override void fired( ref _Bullet3.DamageSourceUnit ds, Vector3 force, Vector3 hitpos, Collider collider, _Action3 attacker )
	{

		if( !attacker || attacker.targetTeam.isMate( act.attachedTeam ) )
		{
			if( !act.isDead )
			{

				act.move.stopping( ref ds, act.speedRate );

				armor.applyDamage( ref ds, 2.0f, 5.0f * GM.t.delta );

			}
		}

		act.rb.AddForceAtPosition( force, hitpos, ForceMode.Force );

	}

}
