using UnityEngine;
using System.Collections;
using UniRx;
using System.Collections.Generic;
using System.Linq;

public class StructureRenderer3 : MonoBehaviour
{


	[ HideInInspector ]
	public MeshRenderer	mr;//	{ private set; get; }



	// �\�����V�F�[�_�̔j��t���O�͔j�󂳂ꂽ���ɗ��̂ŁA�V�F�[�_�p�ɔ��]���K�v�B
	// �V�F�[�_�ł͂P�� float ���� 16bit �t���O�Ƃ��Ĉ����Ă���B
	// �萔�ɑ���ۂɂ́Afloat �ɂ� ushort ���Afloat4 �ɂ� uint*2 �����Z�b�g�ő���Ȃ���΂Ȃ�Ȃ��B

	
	Vector4[]	visiblePartFlags_ForShader;

	ISubject<Vector4[]> observable;
	
	
	public void initAllPartsVisibilityOn( _StructurePart3[] parts, MeshRenderer inmr )
	{

		mr = inmr;
		

		var flags = Enumerable

			//.Range( 0, ( ( parts.Length - 1 ) >> 4 >> 2 ) + 1 )
			// >> 2 �� Vector4 �̊e�v�f�� /4
			// >> 4 �� ��̗v�f�łP�U�̏�Ԃ�ێ����邽�߂� /16
			
			.Range( 0, ( ( 1024 - 1 ) >> 4 >> 2 ) + 1 )

			.Select( _ => new Vector4( (int)0xffff, (int)0xffff, (int)0xffff, (int)0xffff ) )
			
			.ToArray();
		
		
		mr.sharedMaterial.SetVectorArray( ShaderId.Vector, flags );
		//mr.material.SetVectorArray( ShaderId.Vector, flags );

	}

	public void setPartVisibilityOff( ref BitBoolArray flags, int partId )
	{
		
		if( visiblePartFlags_ForShader == null )
		{

			//visiblePartFlags_ForShader = mr.material.GetVectorArray( ShaderId.Vector );
			//visiblePartFlags_ForShader = mr.sharedMaterial.GetVectorArray( ShaderId.Vector );
			visiblePartFlags_ForShader = 
				
				Enumerable.Range( 0, ( ( flags.fieldLength - 1 ) >> 4 >> 2 ) + 1 )
				
				.Select( _ => new Vector4( (int)0xffff, (int)0xffff, (int)0xffff, (int)0xffff ) )

				.ToArray();


			observable = new Subject<Vector4[]>();


			Observable
				
				.ZipLatest( observable, Observable.EveryUpdate(), ( vcs, _ ) => vcs )//new { vcs, _ } )

				.Subscribe( vcs => mr.material.SetVectorArray( ShaderId.Vector, vcs ) )// Debug.Log( vcs._ ); } )
				
				.AddTo( gameObject );

		}

		visiblePartFlags_ForShader[ ( partId >> 4 >> 2 ) ] = flags.getAntiArrayBlockToVector4( partId );

		observable.OnNext( visiblePartFlags_ForShader );

	}
	

	/*
	public void setBoneMatrix( int id, ref Matrix4x4 mt )
	{
		mr.material.SetMatrix( "m" + id.ToString(), mt );
	}
	*/

	public void initPallet( StructurePallet3 pallet )
	{

		mr.sharedMaterial = pallet.getStructureMaterial( mr.sharedMaterial );

	}
	/*
	public void setPalletColor( int id, Color32 color )
	{

		//mr.material.SetColor( "_Pallet" + id.ToString(), color );
		mr.material.SetColor( "_Pallet", color );
	}
	*/


}










