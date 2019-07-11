using UnityEngine;
using System.Collections;

public partial class PlayerAction3
{
	
	
	// アクション ==============================================================================
	
	
	
	// jump & fall -------------------------------------------------------
	
	void fall( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( falling );
			
			state.setPhysics( physHitCheckWall );
			
			
			changeToStand();
			
			
			msNowWaponUpperStance = msWaponUpperStance;

			motionUpperBody.fadeIn( msNowWaponUpperStance, 0.1f );
			
			
			isRunable = false;
			
			output.set( 1.0f, 1.5f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			isRunable = true;
			
		};
		
	}
	
	void falling()
	{
		
		//if( rb.velocity.y > 0.0f )
		if( rb.velocity.y >= Physics.gravity.y * ( 1.0f / 60.0f ) )
		{
			motion.crossFade( msJump, 0.2f );
		}
		else
		{
			motion.crossFade( msFall, 0.2f );
		}
		
		
		if( ls.sqrmag > 0.0f )
		{
			
			var m = new MoveUtilityUnit( ref ls );
			
			
			//if( !move.isOver ) move.setSpeedRate( 0.0f );
			

			// 立ち姿・移動ともに常に直立
			lookAtForwardUpright( m.hdir );
			
		}
		else
		{

			//move.setSpeedRate( 0.0f );

		}


		if( grounder.isGround )
		{
			
			state.changeTo( walk );
			
		}

		if( GamePad._l3 )
		{
			
			state.changeTo( crawl );
			
		}

		/*if( GamePad._l2 )
		{

			state.changeTo( wire );

		}*/
		
	}
	
	
	// running jump & fall -------------------------------------------------------
	
	void runJumpAndFall( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( runJumping );
			
			state.setPhysics( physHitCheckWall );
			
			
			changeToStand();
			
			if( stance.direction.sqrMagnitude == 0.0f )
			{
				stance.direction = rb.rotation * Vector3.forward;
			}
			
			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.2f );
			
			
			isRunable = false;
			
			playerShoot.available -= ShootOpenFlag.shoot;
			
			output.set( 1.0f, 3.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			isRunable = true;
			
			playerShoot.available += ShootOpenFlag.shoot;
			
		};
		
	}
	
	void runJumping()
	{
		
		if( rb.velocity.y >= Physics.gravity.y * ( 1.0f / 60.0f ) )
		{
			motion.crossFade( msJump, 0.2f );
		}
		else
		{
			motion.crossFade( msFall, 0.2f );
		}
		
		
		if( ls.sqrmag > 0.0f )
		{
			
			var m = new MoveUtilityUnit( ref ls );


			// 立ち姿・移動ともに常に直立
			lookAtDirectionUpright( m.hdir );
			
		}

				
		if( /*msJump.time > 0.5f &&*/ grounder.isGround )
		{
			
			state.changeTo( run );
			
		}
		else if( GamePad._l1 && msJump.time < 0.2f )
		{
			
			rb.AddForce( stance.direction * 50.0f + Vector3.down * 200.0f, ForceMode.Impulse );
			
			state.changeTo( avoidance );
			
		}
		
	}

	
	// overBarrier -------------------------------------------------------
	
	void overBarrier( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			motion.crossFade( msBarrierOver, 0.1f );

			//move.velocity = 0.0f;
			
			
			state.shiftTo( gettingOverBarrier );

			state.setPhysics( physFree );// physHitCheckWall );


			//changeToStand();
			changeToCrawl();
			
			figure.changeCollider( EnStance.crawl );
			
			
			if( stance.direction.sqrMagnitude == 0.0f )
			{
				stance.direction = rb.rotation * Vector3.forward;
			}
			
			
			if( ls.sqrmag > 0.0f )
			{
					
				var m = new MoveUtilityUnit( ref ls );


				// 立ち姿・移動ともに常に直立
				lookAtForwardUpright( m.hdir );

			}


			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.1f );
			
			
			isRunable = false;
			
			playerShoot.available -= ShootOpenFlag.shoot;
			
			
			rb.isKinematic = true;
			
			
			output.set( 1.0f, 1.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			figure.changeCollider( EnStance.stand );
			
			
			isRunable = true;
			
			playerShoot.available += ShootOpenFlag.shoot;
			
			
			rb.isKinematic = false;
			
		};
		
	}

	void gettingOverBarrier()
	{

		posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );


		var pos = rb.position;
		
		//var y = pos.y + ( barrierHeight + 1.0f - pos.y ) * ( 0.5f * GM.t.delta * 6.0f );
		var y = pos.y + ( barrier.height - pos.y + 1.0f ) * ( 0.5f * GM.t.delta * 6.0f );


		if( y > barrier.height )
		{
			
			var landpos = new Vector3( pos.x, barrier.height, pos.z );
			
			pos = landpos + stance.direction * figure.moveRadius;

			//state.shiftTo( followingOverBarrier );
			state.changeTo( land );

		}
		else
		{
			
			pos.y = y;

		}


		
		rb.position = pos;
		//rb.MovePosition( pos );// これだとめり込んで反発によりすごい進んでしまう
		
	}

	
	// land -------------------------------------------------------
	
	void land( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			motion.crossFade( msLanding, 0.2f );
			//motion.crossFade( msKneesit, 0.2f );

			figure.changeCollider( EnStance.crawl );
			
			
			state.shiftTo( landing );

			state.setPhysics( physFree );// physHitCheckWall );
			
			
			//changeToStand();
			changeToCrawl();
			
			if( stance.direction.sqrMagnitude == 0.0f )
			{
				stance.direction = rb.rotation * Vector3.forward;
			}
			
			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.2f );
			
			
			isRunable = false;
			
			playerShoot.available -= ShootOpenFlag.shoot;
			
			output.set( 1.0f, 3.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			figure.changeCollider( EnStance.stand );
			
			
			isRunable = true;
			
			playerShoot.available += ShootOpenFlag.shoot;
			
		};
		
	}
	
	void landing()
	{
		/*
		if( ls.sqrmag > 0.0f )
		{
			var m = new MoveUtilityUnit( ref ls );
			
			move.inc( 30.0f, m.mag );
		}
		*/

		// 進行方向を向いて、立ち姿は直立、移動は継続する
		lookAtDirection();


		if( msLanding.time > 0.2f & ls.sqrmag > 0.1f || msLanding.time > msLanding.length )
		//if( msKneesit.time > 0.2f & ls.sqrmag > 0.1f || msKneesit.time > msLanding.length )
		{
			
			state.changeTo( walk );
			
		}
		
	}


	// wire -------------------------------------------------------

	void wire( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( wiring );// shootingWire );
			
			state.setPhysics( physFree );// physHitCheckWall );
			
			
			changeToStand();
			
			
			msNowWaponUpperStance = msWaponUpperStance;

			motionUpperBody.fadeIn( msNowWaponUpperStance, 0.1f );
			
			
			isRunable = true;
			
			output.set( 1.0f, 1.5f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			isRunable = true;
			
		};
		
	}

	void shootingWire()
	{

		/*if( !isWireShot )
		{

			var pos = playerShoot.tfMuzzle.position;
			
			var rot = playerShoot.tfMuzzle.rotation;
			
			
			wireTemplate.emit( pos, rot, 1.0f, this );
			
			hitter.reactions.sound.PlayOneShot( wireTemplate.emitSound );


			isWireShot = true;

		}*/


		if( ls.sqrmag > 0.0f )
		{

			var m = new MoveUtilityUnit( ref ls );


			//moveSpeed.inc( 60.0f * GM.t.delta, m.mag );// 最大ダメージ 30 だと 0.5 秒のストッピングとなる
			//moveSpeed.set( m.mag );


			// 立ち姿・移動ともに常に直立
			lookAtForwardUpright( m.hdir );


			//rb.AddForce( stance.direction * a, ForceMode.Force );

		}


		//state.changeTo( walk );

	}

	void wiring()
	{

		//wireHook.shootHook();

		state.changeTo( walk );

	}


}
