using UnityEngine;
using System.Collections;

public class BulletMultiFireUnit3 : BulletFireUnit3
{
	

	public int	numberOfBullets;



	protected override void fire( _Shoot3 shoot, _Weapon3 wapon )
	{

		var pos = wapon.tfMuzzle.position;
		
		var rot = wapon.tfMuzzle.rotation;

		var tfMuzzle = wapon.tf.childCount > 0 ? wapon.tf.GetChild(0) : wapon.tfMuzzle;


		for( var i =0; i < numberOfBullets; i++ )
		{

			Random.seed = i;

			var noiseAngle = Random.insideUnitCircle * lowAccuracy;
			
			var rotNoize = Quaternion.Euler( -emitAngle + noiseAngle.x, noiseAngle.y, 0.0f );
			
			
			bullet.emit( pos, rot * rotNoize, barralFactor, 1.0f, 0, shoot.act, tfMuzzle );

		}


		sound.Play();
		
		if( flash != null ) flash.Emit( 1 );
		
	}



}
