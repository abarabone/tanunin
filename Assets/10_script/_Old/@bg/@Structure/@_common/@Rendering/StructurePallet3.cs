using UnityEngine;
using System.Collections;

public class StructurePallet3 : ScriptableObject
{
	/*
	public Color32	color0	= new Color32( 255, 255, 255, 255 );
	public Color32	color1	= new Color32( 255, 255, 255, 255 );
	public Color32	color2	= new Color32( 255, 255, 255, 255 );
	public Color32	color3	= new Color32( 255, 255, 255, 255 );
	*/
	public Color  color0  = new Color32( 255, 255, 255, 255 );
	public Color  color1  = new Color32( 255, 255, 255, 255 );
	public Color  color2  = new Color32( 255, 255, 255, 255 );
	public Color  color3  = new Color32( 255, 255, 255, 255 );

	MaterialUnit	structureMaterial;

	MaterialUnit	partMaterial;



	public void setPropertyBlock( MaterialPropertyBlock mpb )
	{
		/*
		mpb.SetColor( "_Pallet0", Color.white );
		mpb.SetColor( "_Pallet1", color0 );
		mpb.SetColor( "_Pallet2", color1 );
		mpb.SetColor( "_Pallet3", color2 );
		mpb.SetColor( "_Pallet4", color3 );
		*/
		mpb.SetVectorArray( "_Pallet", new Vector4[] { Color.white, color0, color1, color2, color3 } );
		
	}
	public void setProperty( Material mat )
	{
		mat.SetColorArray( "_Pallet", new Color[] { Color.white, color0, color1, color2, color3 } );
	}


	public Material getStructureMaterial( Material srcMat )
	{

		return structureMaterial.get( srcMat, this );

	}

	public Material getPartMaterial( Material srcMat )
	{
		
		return partMaterial.get( srcMat, this );
		
	}



	struct MaterialUnit
	{

		Material	material;


		public Material get( Material srcMat, StructurePallet3 pallet )
		{
			
			if( material == null )
			{
				
				var mat = new Material( srcMat.shader );
				
				mat.CopyPropertiesFromMaterial( srcMat );

				/*
				mat.SetColor( "_Pallet0", Color.white );
				mat.SetColor( "_Pallet1", pallet.color0 );
				mat.SetColor( "_Pallet2", pallet.color1 );
				mat.SetColor( "_Pallet3", pallet.color2 );
				mat.SetColor( "_Pallet4", pallet.color3 );
				*/
				mat.SetColorArray( "_Pallet", new Color[] { Color.white, pallet.color0, pallet.color1, pallet.color2, pallet.color3 } );
				
				material = mat;
				
			}
			
			
			return material;
			
		}

	}
		


}
