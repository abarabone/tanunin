using UnityEngine;
using System.Collections;

public partial class PlayerAction3
{
	
	
	
	
	// アクション ==============================================================================
	

	
	
	
	// blow -------------------------------------------------------
	
	void blow( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			
			state.setPhysics( physFree );
			
			figure.changeCollider( EnStance.crawl );
			
			
			switch( downLevel )
			{
				case 0:
					
					motion.crossFade( msJitabata, 0.1f );
					
					state.shiftTo( blowing );
					
					break;
					
				default:
					
					ragdoll.switchToRagdoll();
					
					state.shiftTo( faintedBlowining );
					
					break;
			}
			
			changeToStand();
			
			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.2f );
			
			
			move.setRepairPowerRate( 0.0f );

			move.setSpeedRate( 0.0f );

			stance.direction = rb.rotation * Vector3.forward;
			
			
			isRunable = false;
			
			playerShoot.available <<= ShootOpenFlag.notable;
			
			
			rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			
			
			output.set( 0.5f, 2.0f, 0.5f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			figure.changeCollider( EnStance.stand );
			
			
			isRunable = true;
			
			playerShoot.available <<= ShootOpenFlag.playerReady;
			
			
			rb.constraints = RigidbodyConstraints.FreezeRotation;
			
			//ragdoll.switchToCharacter();
			
		};
		
	}
	
	
	void faintedBlowining()
	{
		
		if( GamePad._l1 && --downLevel <= 0 )
		{
			
			state.shiftTo( blowing );
			
			ragdoll.switchToAnimation();

			motion.crossFade( msJitabata, 0.1f );

			stance.direction = rb.rotation * Vector3.forward;

			downLevel = 0;

		}
		
		if( grounder.isGround )
		{
			state.changeTo( down );
		}
		
	}
	
	
	void blowing()
	{

		// 進行方向を向いて、立ち姿は直立、移動は継続する
		lookAtDirection();


		if( grounder.isGround )
		{
			state.changeTo( down );
		}
		
	}
	
	
	
	
	
	
	// down -------------------------------------------------------
	
	void down( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{

			state.setPhysics( physFree );

			figure.changeCollider( EnStance.crawl );


			switch( downLevel )
			{
				case 0:
					
					motion.crossFade( msDown, 0.3f );
					
					state.shiftTo( downing );
					
					break;

				case (int)EnDamageLevel.faint:
					
					ragdoll.switchToRagdoll();

					state.shiftTo( lightFainting );

					break;

				default:
					
					ragdoll.switchToRagdoll();
					
					state.shiftTo( faintining );
					
					break;
			}

			changeToCrawl();
			//changeToStand();
			
			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.2f );


			move.setRepairPowerRate( 0.0f );

			move.setSpeedRate( 0.0f );

			stance.direction = rb.rotation * Vector3.forward;
			
			
			isRunable = false;
			
			playerShoot.available <<= ShootOpenFlag.notable;
			
			
			output.set( 0.5f, 2.0f, 0.5f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{

			figure.changeCollider( EnStance.stand );


			isRunable = true;
			
			playerShoot.available <<= ShootOpenFlag.playerReady;
			
			//ragdoll.switchToCharacter();
			
		};
		
	}


	// Ｌ１ですぐ復帰できる気絶。よろけ。
	void lightFainting()
	{

		if( GamePad._l1 )
		{
			
			state.shiftTo( downing );
			
			ragdoll.switchToAnimation();

			motion.crossFade( msYoroke, 0.2f );

            state.changeTo( walk );

			stance.direction = rb.rotation * Vector3.forward;
			
			downLevel = 0;

		}

	}


	void faintining()
	{
		
		if( GamePad._l1 && --downLevel <= 0 )
		{

			state.shiftTo( downing );

			ragdoll.switchToAnimation();

			motion.crossFade( msDown, 0.3f );

			stance.direction = rb.rotation * Vector3.forward;

			downLevel = 0;

		}
		
	}
	
	void downing()
	{

		posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );


		if( msDown.time > 0.2f & ls.sqrmag > 0.0f )
		{
			
			motion.crossFade( msStandup, 0.2f );
			
			state.shiftTo( standingup );
			
			msStandup.speed = 2.0f;//stamina.get();
			
		}
		
	}
	
	void standingup()
	{

		posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );

		
		if( msStandup.time > msStandup.length * 0.4f )
		{
			
			motion.crossFade( msStandStance, 0.2f );
			
			state.changeTo( walk );
			
		}
		
	}
	
	
	// dead -------------------------------------------------------
	
	void dead( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( deading );
			
			state.setPhysics( physFree );

			figure.changeCollider( EnStance.crawl );


			changeToCrawl();
			
			motion.crossFade( msDead, 0.5f );
			
			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.3f );



			move.setRepairPowerRate( 0.0f );

			move.setSpeedRate( 0.0f );

			stance.direction = rb.rotation * Vector3.forward;
			
			
			isRunable = false;
			
			playerShoot.available <<= ShootOpenFlag.notable;
			
			
			output.set( 0.5f, 0.0f, 0.5f, 1.0f, 0.0f );


			deadize();

		};
		
		action.last = () =>
		{

			figure.changeCollider( EnStance.stand );


			isRunable = true;
			
			playerShoot.available <<= ShootOpenFlag.playerReady;
			
			ragdoll.switchToAnimation();
			
		};
		
	}
	
	void deading()
	{

		posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );


		if( ragdoll.isRagdollMode && ragdoll.isLowerVelocity( 1.0f ) && grounder.isGround )
		{
			ragdoll.switchToAnimation();
		}
		
	}
	

}
