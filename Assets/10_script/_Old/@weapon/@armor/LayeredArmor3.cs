using UnityEngine;
using System.Collections;

public class LayeredArmor3 : _Armor3
{


	public float	armorDurability;

	public float	bodyDurability;


	//public float	damageTimeRate;

	//public float	armorRecoveryTimeRate;

	//public float	bodyRecoveryTimeRate;

	public Material		outerCircleMaterial;
	
	public Material		innerCircleMaterial;
	




	
	public override _ArmorUnit instantiate()
	{

		var armor = new LayeredArmorUnit( this );

		return armor;

	}



	public class LayeredArmorUnit : _ArmorUnit
	{

		LayeredArmor3	definition;


		DurabilityUnit	outer;

		DurabilityUnit	inner;

		DamageSourceInfo	dsi;


		public LayeredArmorUnit( LayeredArmor3 def )
		{

			definition = def;

		}

		public override void init()
		{

			outer = new DurabilityUnit( definition.armorDurability );

			inner = new DurabilityUnit( definition.bodyDurability );

		}



		public override void applyDamage( ref _Bullet3.DamageSourceUnit ds, float takeTime, float recoveryTime )
		{

			var vaildDamage = ds.damage * ( 1.0f - ds.moveStoppingRate );

			var recoverableRate = 1.0f - ds.heavyRate;


			dsi = new DamageSourceInfo( ref ds );


			if( !outer.isDestroyed )
			{
				var hardDamage = vaildDamage;// Debug.Log(hardDamage);
				
				outer.applyDamage( hardDamage, takeTime, recoverableRate, recoveryTime );
			}
			else// if( !inner.isDestroyed )
			{
				var softDamage = vaildDamage * ds.fragmentationRate;// Debug.Log( softDamage );
				
				inner.applyDamage( softDamage, takeTime, recoverableRate, recoveryTime );
			}

		}

		public override bool update( _Action3 act )
		{

			var isArmorDestroyed = outer.update();
			
			if( isArmorDestroyed )
			{
				
				var overDamage = outer.desroy();

				overDamage.damage *= dsi.fragmentationRate;
				
				inner.applyOverDamage( ref overDamage );
				
			}


			var isBodyDestroyed = inner.update();

			if( isBodyDestroyed )
			{

				inner.desroy();

			}


			return isBodyDestroyed;

		}


		
		public override void showGui()
		{
			
			var pos = new Vector3( -1.42f, -0.7f, 1.0f );

			
			if( outer.durability > 0.0f )
			{
				
				var skinSizeDr = outer.normalizedDurability * 0.5f + 0.2f;
				
				var skinSizeRc = outer.normalizedRecoverable * 0.4f + 0.2f;
				
				var foreSkinColor = Gui.foreSafeColor;
				
				var backSkinColor = outer.durability < outer.recoverable ? Gui.backDamageColor : Gui.backSafeColor;
				
				
				var meshSkin = outer.normalizedRecoverable > inner.normalizedDurability & outer.normalizedDurability > 0.1f ? Gui.meshHarfRound : Gui.meshFullRound;
				
				
				Gui.draw( pos, skinSizeDr, meshSkin, definition.outerCircleMaterial, Gui.idTintColor, foreSkinColor, 1.0f );
				
				Gui.draw( pos, skinSizeRc, meshSkin, definition.innerCircleMaterial, Gui.idColor, backSkinColor, 1.1f );
				
			}
			
			
			
			
			var underSizeDr = inner.normalizedDurability * 0.5f + 0.2f;
			
			var underSizeRc = inner.normalizedRecoverable * 0.4f + 0.2f;
			
			var foreUnderColor = inner.normalizedDurability > 0.2f ? Gui.backDangerColor : Color.Lerp( Gui.dangerColorOn, Gui.dangerColorOff, Time.time % 1.0f );
			
			var backUnderColor = Color.black;
			
			
			Gui.draw( pos, underSizeDr, Gui.meshFullRound, definition.outerCircleMaterial, Gui.idTintColor, foreUnderColor, 1.5f );
			
			Gui.draw( pos, underSizeRc, Gui.meshFullRound, definition.innerCircleMaterial, Gui.idColor, backUnderColor, 1.6f );
			
			
			Gui.draw( pos, 0.2f, Gui.meshFullRound, definition.innerCircleMaterial, Gui.idColor, Color.yellow, 0.8f );
			
		}

	}

}
