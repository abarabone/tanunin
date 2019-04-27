using UnityEngine;
using System.Collections;

public class SkinningArmor3 : _Armor3
{
	
	public float	skinDurability;
	
	public float	underDurability;


	//public float	damageTimeRate;
	
	//public float	skinRecoveryTimeRate;
	
	//public float	underRecoveryTimeRate;



	public float	skinRepairCost;

	public float	skinRepairTime;

	public float	underRepairTime;

	public float	underCostRecoveryMinutes;


	public AudioClip	skinPurgeSound;
	
	public AudioClip	underPurgeSound;

	public AudioClip	refreshCompleteSound;


	public GameObject	purgeEffect;



	public Material		skinCircleMaterial;

	public Material		underCircleMaterial;


	
	public override _ArmorUnit instantiate()
	{
		
		var armor = new SkinningArmorUnit( this );
		
		return armor;
		
	}
	
	

	public class SkinningArmorUnit : _ArmorUnit
	{
		
		SkinningArmor3	definition;

		AudioSource		sound;

		
		DurabilityUnit	skin;
		
		DurabilityUnit	under;


		enMode	mode;

		enum enMode { ready, repairingSkin, repairingUnder }




		public SkinningArmorUnit( SkinningArmor3 def )
		{
			
			definition = def;

		}

		public override void init()
		{

			skin = new DurabilityUnit( definition.skinDurability );
			
			under = new DurabilityUnit( definition.underDurability );
			
		}
		

		
		public override void applyDamage( ref _Bullet3.DamageSourceUnit ds, float takeTime, float recoveryTime )
		{

			var vaildDamage = ds.damage * ( 1.0f - ds.moveStoppingRate );
			
			var recoverableRate = 1.0f - ds.heavyRate;


			if( mode == enMode.ready )
			{
				var hardDamage = vaildDamage;

				skin.applyDamage( hardDamage, takeTime, recoverableRate, recoveryTime );
			}
			else if( mode == enMode.repairingSkin )
			{
				var softDamage = vaildDamage * ds.fragmentationRate;
				
				under.applyDamage( softDamage, takeTime, recoverableRate, recoveryTime );
			}
			else// if( mode == enMode.repairingUnder )
			{
				under.applyDamage( under.durability * 1.5f, 0.1f );
			}
			
		}


		public override bool update( _Action3 act )
		{

			var isDestroySkin = skin.update();

			if( isDestroySkin )
			{

				if( mode != enMode.repairingUnder )
				{
					skin.desroy();
				}
				else
				{
					skin.reset( definition.skinDurability );

					isDestroySkin = false;
				}

			}


			var isDestroyUnder = under.update();

			if( isDestroyUnder )
			{

				under.desroy();

			}



			purgeAndRefresh( act );



			return isDestroySkin | isDestroyUnder;
			
		}


		void purgeAndRefresh( _Action3 act )
		{

			if( GamePad._r3 )
			{

				if( mode == enMode.ready & under.durability >= definition.skinRepairCost )
				{

					GM.se.source.play( act.rb.position, definition.skinPurgeSound );


					mode = enMode.repairingSkin;

					skin.desroy();

					skin.repair( definition.skinDurability, definition.skinRepairTime );

					under.applyDamage( definition.skinRepairCost, 0.3f, 1.0f, 60.0f * definition.underCostRecoveryMinutes );

				}
				else if( mode == enMode.repairingSkin )
				{

					GM.se.source.play( act.rb.position, definition.underPurgeSound );


					mode = enMode.repairingUnder;

					skin.reset( skin.recoverable );

					skin.applyDamage( skin.durability * 1.1f, 0.3f );

					under.desroy();

					under.repair( definition.underDurability, definition.underRepairTime );

				}

			}
			else if(
				mode == enMode.repairingSkin & skin.durability >= definition.skinDurability ||
				mode == enMode.repairingUnder & under.durability >= definition.underDurability
			)
			{

				GM.se.source.play( act.rb.position, definition.refreshCompleteSound );

				mode = enMode.ready;

			}

		}





		
		public override void showGui()
		{
			
			var pos = new Vector3( -1.42f, -0.7f, 1.0f );

			
			if( mode != enMode.repairingUnder || skin.isNowDamaging() )
			{
				
				var skinSizeDr = skin.normalizedDurability * 0.5f + 0.2f;
				
				var skinSizeRc = skin.normalizedRecoverable * 0.4f + 0.2f;
				
				var foreSkinColor = mode == enMode.ready ? Gui.foreSafeColor : Color.Lerp( Gui.dangerColorOn, Gui.dangerColorOff, Time.time % 1.0f );
				
				var backSkinColor = mode == enMode.repairingSkin ? Gui.backDamageColor : ( skin.durability < skin.recoverable ? Gui.backDamageColor : Gui.backSafeColor );
				
				
				var meshSkin = skin.normalizedRecoverable > under.normalizedDurability & skin.normalizedDurability > 0.1f ? Gui.meshHarfRound : Gui.meshFullRound;
				
				
				Gui.draw( pos, skinSizeDr, meshSkin, definition.skinCircleMaterial, Gui.idTintColor, foreSkinColor, 1.0f );
				
				Gui.draw( pos, skinSizeRc, meshSkin, definition.underCircleMaterial, Gui.idColor, backSkinColor, 1.1f );
				
			}
			
			
			
			
			var underSizeDr = under.normalizedDurability * 0.5f + 0.2f;
			
			var underSizeRc = under.normalizedRecoverable * 0.4f + 0.2f;
			
			var foreUnderColor = mode != enMode.repairingUnder ? Gui.backDangerColor : Color.Lerp( Gui.dangerColorOn, Gui.dangerColorOff, Time.time % 1.0f );
			
			var backUnderColor = mode == enMode.repairingUnder ? foreUnderColor : Color.black;//( under.durability < under.recoverable ? Gui.dangerColorOff : Color.black );//Gui.backDangerColor );
			
			
			Gui.draw( pos, underSizeDr, Gui.meshFullRound, definition.skinCircleMaterial, Gui.idTintColor, foreUnderColor, 1.5f );
			
			Gui.draw( pos, underSizeRc, Gui.meshFullRound, definition.underCircleMaterial, Gui.idColor, backUnderColor, 1.6f );
			
			
			Gui.draw( pos, 0.2f, Gui.meshFullRound, definition.underCircleMaterial, Gui.idColor, Color.yellow, 0.8f );
			
		}

	}

}
