using UnityEngine;
using System.Collections;

public partial class PlayerAction3
{



	// アクション ==============================================================================

	public float    avo;
	
	// avoidance & rolling ------------------------------------------------------
	
	void avoidance( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( avoidancing );

			state.setPhysics( physHitCheckWall );
			
			
			motion.crossFade( msDash, 0.1f );
			
			changeToStand();
			
			physMats.change( figure.currentCollider, physMats.dash );



			//var dashForce = ( !isSmallDash ? 1000.0f : 600.0f ) * move.sqSpeedRate01;

			//rb.AddForce( stance.direction * dashForce + Vector3.up * 35.0f, ForceMode.Impulse );


			
			msNowWaponUpperStance = null;

			motionUpperBody.fadeOut( 0.1f );
			
			
			isMoveHeadInFps = false;

			isAvoiding = true;
			
			playerShoot.available -= ShootOpenFlag.shoot;


			move.setRepairPowerRate( 0.0f );

			output.set( 1.0f, 5.0f, 1.0f, 1.0f, 0.0f );
			
		};
		
		action.last = () =>
		{

			physMats.change( figure.currentCollider, physMats.stand );

			isMoveHeadInFps = true;

			isAvoiding = false;

			playerShoot.available += ShootOpenFlag.shoot;

			//if( GamePad.l1 ) rb.AddForce( stance.direction * -300.0f, ForceMode.Impulse );

		};
		
	}
	
	void avoidancing()
	{

		if( stance.direction.sqrMagnitude > 0.0f )
		{
			posture.rot = Quaternion.LookRotation( stance.direction, Vector3.up );
		}


		stance.setSpeed( 1.0f, move.speedRate01 );

		
		if( GamePad._l1 ) move.repairRate( 0.4f );  // 連打でストッピング回復


		if( wall.isTouchWall )
		{
			
			state.changeTo( walk );
			
		}
		else if( msDash.time > 0.3f )
		{
			
			state.changeTo( roll );
			
		}
		
	}
	

	void roll( ref ActionStateUnit.ActionHolder action )
	{
		
		action.first = () =>
		{
			
			state.shiftTo( rolling );

			state.setPhysics( physHitCheckWall );


			motion.crossFade( msRoll, 0.1f );
			
			changeToStand();

			physMats.change( figure.currentCollider, physMats.rolling );


			msNowWaponUpperStance = null;
			
			motionUpperBody.fadeOut( 0.1f );
			
			
			isMoveHeadInFps = false;

			isAvoiding = true;

			isSmallDash = false;


			playerShoot.available -= ShootOpenFlag.shoot;
			
			msRoll.speed = 1.7f;//2.0f;


			move.setRepairPowerRate( 0.0f );

			output.set( 1.0f, 3.0f, 1.0f, 1.0f, 0.0f );
			
			
		};
		
		action.last = () =>
		{

			physMats.change( figure.currentCollider, physMats.stand );

			isMoveHeadInFps = true;

			isAvoiding = false;


			playerShoot.available += ShootOpenFlag.shoot;
			
			//msRoll.speed = 0.0f;
			
			//msRoll.time = msRoll.length;
			
		};
		
	}
	
	void rolling()
	{

		// 進行方向を向いて、立ち姿は直立、移動は継続する
		lookAtDirectionWithZeroCheck();


		//		move.dec( -7.0f * move.speed, 0.0f );
		stance.setSpeed( 1.0f, move.speedRate01 );



		if( GamePad._l1 ) move.repairRate( 0.4f );	// 連打でストッピング回復



		if( msRoll.time >= msRoll.length * 0.5f )
		{
			
			rollingover();
			
		}
		else if( msRoll.time >= msRoll.length * 0.75f )//0.6f )
		{
			
			if( GamePad.r1 )	// このタイミングでなら、発砲できる
			{
				
				isMoveHeadInFps = true;
				
				playerShoot.available += ShootOpenFlag.shoot;
				
				
				msNowWaponUpperStance = msWaponUpperStance;

				motionUpperBody.fadeIn( msNowWaponUpperStance, 0.1f );
				
				
				state.shiftTo( rollingover );
				
			}
			
		}
		else if( GamePad._l3 )
		{
			// 途中に伏せるとキャンセルでき、かつ滑る。

			state.changeTo( crawl );
			
			rb.AddForce( stance.direction * 80.0f, ForceMode.Impulse );
			
		}
		
	}
	

	void rollingover()
	{

		if( msRoll.time >= msRoll.length )
		{

			state.changeTo( walk );

		}
		else if( wall.isTouchWall )
		{

			state.changeTo( walk );

		}
		else if( GamePad._l1 && grounder.isGround ) // ムリな連続回避はスタミナを減らすが、モーションをキャンセルできる。
		{

			var m = new MoveUtilityUnit( ref ls );


			// 進行方向を向いて、立ち姿は直立だが、移動だけ接地面に沿う
			lookAtDirectionStandOnGround( m.hdir );


			if( ls.sqrmag > 0.6f * 0.6f )
			{
				state.changeTo( avoidance );

				isSmallDash = true;
			}

		}
		else
		{

			return;

		}



		stance.setSpeed( 1.0f, move.speedRate01 );
		//if( grounder.isGround ) rb.velocity = Vector3.zero;


	}



	// sliding ------------------------------------------------------

	void slide( ref ActionStateUnit.ActionHolder action )
	{

		action.first = () =>
		{

			state.shiftTo( sliding );

			state.setPhysics( physHitCheckWall );


			motion.crossFade( msSitdown, 0.1f );

			changeToCrawl();



			//move._inc( 20.0f, 30.0f );//30.0f, 20.0f );// * stamina.get() );
			
			rb.AddForce( stance.direction * move.speedRate01 * 80.0f, ForceMode.Impulse );



			upperStanceToWapon();



			output.set( 1.0f, 5.0f, 1.0f, 1.0f, 0.0f );

		};

		action.last = () =>
		{
			


		};

	}

	
	void sliding()
	{



	}

	void sliping()
	{



	}

}





