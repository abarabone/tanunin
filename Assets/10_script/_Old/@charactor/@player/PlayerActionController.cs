using UnityEngine;
using System.Collections;



public partial class PlayerAction3
{

	private class PlayerActionController : IActionController
	{


		PushTimeUnit    dashButton;

		LeftStickUnit   ls;



		public void walk( PlayerAction3 act )
		{

			if( GamePad._l3 )
			{

				act.state.changeTo( act.crawl );

			}
			else
			{

				if( GamePad._l1 && act.grounder.isGround )
				{

					//if( ls.sqrmag > 0.6f * 0.6f ) 
					if( ls.sqrmag > 0.1f * 0.1f )
					{
						if( act.checkOverBarrier() )
						{
							act.state.changeTo( act.overBarrier );
						}
						else
						{
							//move.setSpeedRate( 2.0f );

							act.state.changeTo( act.avoidance );
						}
					}
					/*else if( ls.sqrmag < 0.1f * 0.1f )
					{
						changeToJumpMode( 200.0f );
					}
					*/
				}
				else if( !dashButton.over( true ) && GamePad.l1_ && act.grounder.isGround )//
				{//

					act.changeToJumpMode( 200.0f );//


				}//
				else if( !act.grounder.isGround )
				{

					act.changeToFallMode();

				}

			}

			/*if( GamePad._l2 )
			{

				state.changeTo( wire );

			}*/

		}


	}

}



