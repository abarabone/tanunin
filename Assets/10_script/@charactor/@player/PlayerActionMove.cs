using UnityEngine;
using System.Collections;

public partial class PlayerAction3
{
	
	
	// アクション ==============================================================================
	

	
	// walk ------------------------------------------------------------------
	
	void walk( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( walking );
			
			state.setPhysics( physHitCheckWall );
			
			
			changeToStand();
			
			motion.crossFade( msStandStance, 0.1f );
			
			
			msNowWaponUpperStance = msWaponUpperStance;

			motionUpperBody.fadeIn( msNowWaponUpperStance, 0.1f );
			
			
			output.set( 1.0f, 1.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
		};
		
	}
	
	
	

	void walking()
	{
		
		if( ls.sqrmag > 0.0f )
		{
			
			var m = new MoveUtilityUnit( ref ls );


			//move.inc( 60.0f, m.mag );// 最大ダメージ 30 だと 0.5 秒のストッピングとなる
			//moveSpeed.set( m.mag );
			move.setRepairTime( moveThresholds.walk );
			stance.setSpeed( m.mag, move.speedRate01 );


			// 立ち姿は直立だが、移動だけ接地面に沿う
			lookAtForwardStandOnGround( m.hdir);
			
			
			if( move.isMovable )
			{
				//ragdoll.switchToAnimation();//
				motion.blend2D( msWalkSide, msWalkStance, ls.stick, 1.0f, move.speedRate01 * stance.moveRate * 0.5f, 0.1f );
				//motion.crossFade( msNowWalk, 0.1f );
			}
			
			
			output.loudness = 1.0f + m.mag;
			
		}
		else
		{

			if( stance.moveDistancePerSec > 1.2f )
			{
				motion.crossFade( msJump, 0.2f );
			}
			else
			{
				if( GamePad.l1 ) motion.crossFade( msKneesit, 0.3f ); else//
				motion.crossFade( msStandStance, 0.2f );
			}

			msJump.time = 0.0f;



			//moveSpeed.inc( 0.0f, 0.0f );
			//move.velocity = 0.0f;// 止まればダメージは回復する（が静止状態）
			move.setRepairTime( moveThresholds.stand );
			stance.setSpeedZero();


			// 立ち姿は直立だが、移動だけ接地面に沿う
			lookAtForwardStandOnGround();


			output.loudness = 1.0f;
			
		}
		
		
		if( GamePad._l3 )
		{
			
			state.changeTo( crawl );
			
		}
		else
		{
			
			if( GamePad._l1 && grounder.isGround )
			{
				
				//if( ls.sqrmag > 0.6f * 0.6f ) 
				if( ls.sqrmag > 0.1f * 0.1f )
				{
					if( checkOverBarrier() )
					{
						state.changeTo( overBarrier );
					}
					else
					{
						//move.setSpeedRate( 2.0f );

						state.changeTo( avoidance );
					}
				}
				/*else if( ls.sqrmag < 0.1f * 0.1f )
				{
					changeToJumpMode( 200.0f );
				}
				*/
			}
			else if( !dashButton.over( true ) && GamePad.l1_ && grounder.isGround )//
			{//
				
				changeToJumpMode( 200.0f );//
				

			}//
			else if( !grounder.isGround )
			{

				changeToFallMode();

			}
			
		}

		/*if( GamePad._l2 )
		{
			
			state.changeTo( wire );
			
		}*/
		
	}
	
	
	
	
	// crawl ------------------------------------------------------------------
	
	void crawl( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( crawling );
			
			state.setPhysics( physHitCheckWall );
			
			
			motion.crossFade( msCrawl, 0.2f );
			
			changeToCrawl();
			
			figure.changeCollider( EnStance.crawl );


			// 暫定ずらし　本来はアニメーションを調整すべき
			var newBody = tfBody.localPosition;
			newBody.z = -0.24f;
			tfBody.localPosition = newBody;
			
			
			msNowWaponUpperStance = msWaponCrawlUpperStance;
			
			//motionUpperBody.play( msNowWaponUpperStance );
			//motionUpperBody.crossFade( msNowWaponUpperStance, 0.1f );
			
			
			output.set( 1.0f, 1.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{

			figure.changeCollider( EnStance.stand );
			
			
			// 暫定ずらし　本来はアニメーションを調整すべき
			var newBody = tfBody.localPosition;
			newBody.z = 0.0f;
			tfBody.localPosition = newBody;


			Destroy( jtHang );//
			jtHang = null;//
		};
		
	}
	
	
	void crawling()
	{
		
		if( GamePad.r1 )
		{
			motionUpperBody.fadeIn( msNowWaponUpperStance, 0.1f );
		}
		else
		{
			motionUpperBody.fadeOut( 0.2f );
		}
		
		{
			if( !GamePad.r1 & ls.sqrmag > 0.0f )
			{
				
				var m = new MoveUtilityUnit( ref ls );


				////move.inc( 10.0f, m.mag * 0.33f );
				//moveSpeed.set( mag * (1.0f / 3.0f) );
				move.setRepairTime( moveThresholds.stand );
				stance.setSpeed( m.mag * 0.33f, move.speedRate01 );


				// 立ち姿・移動ともに接地面に沿う
				lookAtForwardFitOnGround( m.hdir );
				
				
				motion.blend1D( msCrawlMove, ls.stick.y != 0.0f ? ls.stick.y : ls.stick.x, 1.0f, move.speedRate01 * stance.moveRate * 0.5f, 0.1f );
				//motion.crossFade2D( msCrawlMove, msCrawlMove, ls.stick, moveSpeed * 0.5f, 0.1f );
				//motion.crossFade( msCrawlMove, 0.3f );
				
				
				output.loudness = 1.0f + m.mag * 0.33f;
				
			}
			else
			{
				
				motion.crossFade( msCrawl, 0.3f );


				//moveSpeed.inc( 0.0f, 0.0f );
				//move.velocity = 0.0f;
				move.setRepairTime( moveThresholds.walk );
				stance.setSpeedZero();


				// 立ち姿・移動ともに接地面に沿う
				lookAtForwardFitOnGround();


				output.loudness = 1.0f;
				
			}
			
		}

		if( GamePad._l3 )
		{

			state.changeTo( walk );

		}
		else if( GamePad._l1 && grounder.isGround )
		{
			/*
			if( ls.sqrmag > 0.6f * 0.6f )
			{
				state.changeTo( avoidance );
			}*/
			/*else if( ls.sqrmag < 0.1f * 0.1f )
			{
				changeToJumpMode( 150.0f );
			}
			*/

			// ひっつき仮
			jtHang = gameObject.AddComponent<FixedJoint>();
			jtHang.connectedBody = grounder.groundCollider.attachedRigidbody;

		}
		else if( GamePad.l1_ )
		{

			Destroy( jtHang );
			jtHang = null;// 伏せ終期化にもある

		}
		else if( rb.velocity.y < Physics.gravity.y * ( 1.0f / 60.0f ) * 60.0f )
		{

			changeToFallMode();

		}
		
	}
	Joint	jtHang;

	// run ---------------------------------------------------------
	float runSpeed = 2.6f;
	void run( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( running );

			state.setPhysics( physHitCheckWall );
			
			motion.crossFade( msWalkStance, 0.1f );
			
			changeToStand();
			
			/*
			if( stance.direction.sqrMagnitude == 0.0f )
			{
				stance.direction = rb.rotation * Vector3.forward;
			}
			*/
			move.setSpeedRate( Mathf.Min( move.speedRate, runSpeed ) );

			move.setRepairTime( 3.0f );
						

			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.2f );
			
			
			isRunable = false;
			
			//playerShoot.isShootable = false;
			playerShoot.available -= ShootOpenFlag.shoot;
			
			
			output.set( 1.0f, 2.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{
			
			isRunable = true;
			
			//playerShoot.isShootable = true;
			playerShoot.available += ShootOpenFlag.shoot;
			
			
			msWalkStance.speed = 1.0f;
			
		};
		
	}


	void running()
	{

		var stickRate = 0.0f;


		if( ls.sqrmag > 0.0f )
		{

			var m = new MoveUtilityUnit( ref ls );

			
			// 立ち姿は直立だが、移動だけ接地面に沿う
			lookAtDirectionStandOnGround( m.hdir );




			msWalkStance.speed = stance.moveDistancePerSec * ( 1.0f / 5.0f );//moveSpeed * ( 1.0f / 5.0f );

			stickRate = m.mag;




			if( stance.moveDistancePerSec > 0.4f )
			{
				motion.crossFade( msWalkStance, 0.2f );
			}
			else
			{
				motion.crossFade( msStandStance, 0.3f );
			}


		}
		else
		{

			if( stance.moveDistancePerSec > 1.2f )
			{
				motion.crossFade( msJump, 0.2f );
			}
			else
			{
				motion.crossFade( msStandStance, 0.3f );
			}

			msJump.time = 0.0f;

		}



		stance.setSpeed( stickRate, move.speedRate01 * runSpeed );



		if( !GamePad.l1 & rb.velocity.magnitude < 1.4f & ls.sqrmag < 0.1f * 0.1f )
		{
			
			state.changeTo( walk );
			
		}
		else if( GamePad._l3 )
		{
			
			state.changeTo( crawl );
			
			rb.AddForce( stance.direction * stance.speedRate * 80.0f, ForceMode.Impulse );
			
		}
		else if( grounder.isGround )
		{
			if( GamePad._l1 )
			{
				
				if( ls.sqrmag > 0.6f * 0.6f )
				{
					if( checkOverBarrier() )
					{
						state.changeTo( overBarrier );
					}
					else
					{
						state.changeTo( avoidance );
					}
				}
				/*else if( ls.sqrmag < 0.1f * 0.1f )
				{
					changeToJumpMode( 200.0f );
				}*/

			}
			/*else if( GamePad.l1_ & dashButton.over(true) & ls.sqrmag < 0.1f * 0.1f )
			{
				changeToJumpMode( 200.0f );
			}*/
		}
		else
		{
			
			changeToFallMode();
			
		}
		
		
		output.loudness = stance.speedRate;
		
	}


	void checkSlip()
	{



	}


}
