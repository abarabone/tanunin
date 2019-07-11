using UnityEngine;
using System.Collections;

public abstract class _Bullet3 : _Emitter3
{



	public _Action3	owner	{ get; protected set; }







	// �ÓI ----------------------------------


	static protected RaycastHitWork  hits;

	static _Bullet3()
	{
		hits = new RaycastHitWork( 64 );
	}




	/// <summary>
	/// �R���C�_��S������ _Hit3 ���擾����BattachedRigidbody -> �e GameObject �̏��ԂŒT���̒��ӁB
	/// </summary>
	static public _Hit3 getHitter( Collider c )
	{

		if( c.attachedRigidbody != null )
		// Rigidbody �����Ă���Ȃ炻�� hitter ��D�悷��B
		// �{���Ȃ�R���C�_�� hitter ���܂�����ق������R�����A���x�I�ɗL���Ȃق���I������B
		{
			return c.attachedRigidbody.GetComponentInParent<_Hit3>();
			//return c.attachedRigidbody.GetComponent<_Hit3>() ?? c.GetComponent<_Hit3>();
		}
		else
		{
			return c.GetComponent<_Hit3>();
		}

	}


	/// <summary>
	/// factor �̂������� rate �Œ������郆�[�e�B���e�B�B
	/// </summary>
	static protected float effectFactor( float factor, float rate )
	{
		return 1.0f + ( factor - 1.0f ) * rate;
	}



	/// <summary>
	/// �I�E���q�b�g�����O���āA���ߋ����ł��q�b�g����𐬗����������ꍇ�Ɏg�p����B
	/// �q�b�g���Ă���΁A��ԋߋ����̂h�c��Ԃ��B�q�b�g���Ă��Ȃ��E�I�E���q�b�g�̂݁A�̏ꍇ�� -1 ��Ԃ��B
	/// </summary>
	static public int getOtherHitIdForOwnHittable( int hitLength, RaycastHit[] hits, _Action3 owner )
	{

		for( var i = 0 ; i < hitLength ; i++ )
		{

			var hitter = getHitter( hits[ i ].collider );

			if( hitter == null || hitter.getAct() != owner ) return i;

		}

		return -1;

	}











	// �I�u�W�F�N�g ---------------------------------------------


	public struct DamageSourceUnit
	// hitter �ȂǂɃ_���[�W��n�����Ɉꎞ�I�Ɏg�p����B
	{
		
		public float	damage;				// �З́B
		
		public float	heavyRate;			// �񕜕s�\�Ȋ����B

		public float	fragmentationRate;	// �����ϋv�͂ւ̈З͔{���B��e�͒f�􂵂č����Ȃ�A�d�e�͊ђʂ��ĒႭ�Ȃ�B
		
		public float	moveStoppingDamage;	// �ړ��ւ̃_���[�W�B���s�� 1.0f �ŁA�ً}����Ȃǂ͏����l�� 20.0f �Ƃ�����B

		public float	moveStoppingRate;	// �ړ��_���[�W���K�p���ꂽ�ꍇ�́A�З͂��獷��������銄���B

		//public _Hit3.enMaterial	penetratableArmorClass;	// �ђʉ\�ȃA�[�}�[�N���X�B


		public DamageSourceUnit( float dmg )
		{
			damage = dmg;
			heavyRate = 1.0f;
			fragmentationRate = 1.0f;
			moveStoppingDamage = 0.0f;
			moveStoppingRate = 0.0f;
		}
		
		public DamageSourceUnit( float dmg, float fmr )
		{
			damage = dmg;
			heavyRate = 1.0f;
			fragmentationRate = fmr;
			moveStoppingDamage = 0.0f;
			moveStoppingRate = 0.0f;
		}

		public DamageSourceUnit( float dmg, float hvr, float fmr, float msd, float msr )
		{
			damage = dmg;
			heavyRate = hvr;
			fragmentationRate = fmr;
			moveStoppingDamage = msd;
			moveStoppingRate = msr;
		}

		public DamageSourceUnit( float dmg, float msd, float msr )
		{
			damage = dmg;
			heavyRate = 1.0f;
			fragmentationRate = 1.0f;
			moveStoppingDamage = msd;
			moveStoppingRate = msr;
		}

	}







	/// <summary>
	/// RaycastHit �̃��[�N�BNonAlloc �^�C�v�̃L���X�g�ƕ����Ďg�p����B
	/// </summary>
	protected struct RaycastHitWork
	{

		public RaycastHit[] array { get; private set; }	// �����蔻��p�Ɏg�p���Ă���


		public RaycastHitWork( int length )
		{

			array = new RaycastHit[ length ];  

        }
		

		/// <summary>
		/// ���ߋ����ł��q�b�g����𐬗����������e�ۂŎg�p����
		/// �i�I�E���q�b�g�����疳���ł��悢�ꍇ�͕��ʂ̃L���X�g�łn�j�j�B
		/// this.array ����A�I�E���q�b�g���Ă��Ȃ���ԋߋ����̂h�c��Ԃ��B
		/// �q�b�g���Ă��Ȃ��E�I�E���q�b�g�̂݁A�̏ꍇ�� -1 ��Ԃ��B
		/// </summary>
		public int getOtherHitIdForOwnHittable( int hitLength, _Action3 owner )
		{

			return _Bullet3.getOtherHitIdForOwnHittable( hitLength, array, owner );

		}

	}










}
