using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;






namespace MEvent
{


	/// <summary>
	/// �C�x���g���M�҂ɃC�x���g������o�^���A�C�x���g�������ɏ����`�F�b�N����B
	/// </summary>
	public class Trigger<Ttarget> : ConditionLinkBase<Ttarget> where Ttarget : IEventable
	{


		// �C�x���g�����̃Z�b�g�^�擾  - - - - - -

		
		/// <summary>
		/// �擪�̗v�f��Ԃ��B�v�f���Ȃ���� null ���Ԃ�B
		/// </summary>
		protected Condition<Ttarget> linkTop
		{
			get { return this.next; }
		}


		/// <summary>
		/// �Ō�̗v�f��Ԃ��B�v�f���Ȃ���� null ���Ԃ�B
		/// ���[�v�Ŏ擾����̂ŁA�Ԃ����l�̓L���b�V������ׂ��B
		/// </summary>
		protected Condition<Ttarget> linkLast
		{
			get
			{
				for( var c = linkTop ; c != null ; c = c.next ) if( c.next == null ) return c;

				return null;
			}
		}


		/// <summary>
		/// �擪�ɏ������Z�b�g����B
		/// </summary>
		/// <param name="co">�Z�b�g����C�x���g����</param>
		protected void setTop( Condition<Ttarget> co )
		{
			this.setNext( co );
		}



		// �o�^ - - - - - -

		
		/// <summary>
		/// �V����������ǉ��o�^����B
		/// </summary>
		/// <param name="co">�o�^����C�x���g����</param>
		public void regist( ConditionLinkBase<Ttarget> co )
		{
			this.setTop( co.linkToChain( this.next ) );
		}


		/// <summary>
		/// ���̃g���K�[�̎��������A�����̐擪�ɂ��ׂđ}������`�œo�^����B
		/// </summary>
		/// <param name="other">�}������g���K�[</param>
		public void regist( Trigger<Ttarget> other )
		{
			if( other.linkTop == null ) return;

			other.linkLast.setNext( this.linkTop ); // ����̏����̍Ō�ɁA�����̏������q����B

			this.setNext( other.linkTop );			// �����̏����̐擪�ɁA����̏���������B
		}



		// �`�F�b�N - - - - - -

		
		/// <summary>
		/// �o�^���Ă�����������ׂă`�F�b�N���A��ڂ��I�������̂̓`�F�C������O���B
		/// </summary>
		public void check( Ttarget obj )
		{

			ConditionLinkBase<Ttarget> prev = this;

			for( var c = linkTop ; c != null ; c = c.next )
			{

				c.check( obj );


				if( c.isComplited )
				{
					prev.linkToChain( c.linkToChain( null ) );
				}


				prev = c;
			}

		}

	}





	/// <summary>
	/// �^�C�}�[�ɓ����������g���K�[�B
	/// </summary>
	public class TimerTrigger : Trigger<IEventable>
	{
		public void setTimer()
		{
			for( var c = linkTop ; c != null ; c = c.next )
			{

				var co = (Timer.OnOver)c;

				GM.startCoroutine( co.setTimer() );

			}
		}
	}





	// �g���K�[�z���_�[ ==========================================================


	/// <summary>
	/// �C�x���g�����������I�u�W�F�N�g�Ƀg���K�[������������Bnull �`�F�b�N�Ȃǂ��邽�߂̃N�b�V�����B
	/// </summary>
	public struct TriggerHolder<Ttarget> where Ttarget : IEventable
	{


		Trigger<Ttarget> trigger;



		/// <summary>
		/// �g���K�[���܂邲�Ǝ󂯎��B���łɃg���K�[������ꍇ�́A���������̐擪�ɑ}�������B
		/// </summary>
		/// <param name="t">�������������g���K�[</param>
		public void receiveTrigger( Trigger<Ttarget> otherTrigger )
		{
			if( otherTrigger == null ) return;

			if( this.trigger != null )
			{
				this.trigger.regist( otherTrigger );
			}
			else
			{
				this.trigger = otherTrigger;
			}
		}





		/// <summary>
		/// �g���K�[�̏������`�F�b�N����B
		/// </summary>
		/// <param name="obj">�����̑ΏۂƂȂ� IEventable �I�u�W�F�N�g</param>
		public void check( Ttarget obj )
		{
			if( trigger != null )
			{
				trigger.check( obj );
			}
		}




		/// <summary>
		/// �g���K�[��j������B
		/// </summary>
		public void destruct()
		{

			trigger = null;

		}


	}







	// �C�x���g�^�[�Q�b�g�v���L�V ==================================================





	/// <summary>
	/// �C�x���g�^�[�Q�b�g�͂�����p������B
	/// </summary>
	public interface IEventable
	{ }






	/// <summary>
	/// �������^�[�Q�b�g�I�u�W�F�N�g�Ɋ֘A�t����ۂɁA�܂��I�u�W�F�N�g�����݂��Ă��Ȃ��ꍇ�̈ꎞ�o�^�ꏊ�B
	/// �C�x���g�N���G�C�V�������ɁA�����o�^�ς݃g���K�[��ێ�����B
	/// �n���h���ŃI�u�W�F�N�g���������ꂽ���ɁA�g���K�[�������n���㗝�l�B
	/// </summary>
	public abstract class TargetProxy<Ttarget> where Ttarget : IEventable
	{

		TimerTrigger	timerTrigger;

		protected Trigger<Ttarget>[] triggers { private set; get; }



		/// <summary>
		/// �C�x���g�^�[�Q�b�g�̃g���K�[���B�����ŋ�̓I�ɐݒ肷��B
		/// </summary>
		protected abstract int triggerCountDefine { get; }





		// �����̓o�^ ---------


		/// <summary>
		/// �g���K�[���C�x���g�^�[�Q�b�g�֓o�^����B
		/// �̈�m�ۂ͓o�^���Ƀ`�F�b�N���s���B
		/// </summary>
		/// <param name="i">�g���K�[�ʒu</param>
		/// <param name="co">�o�^����������</param>
		public void registCondition( int i, Condition<Ttarget> co )
		{
			if( i > triggerCountDefine ) return;

			if( triggers == null ) triggers = new Trigger<Ttarget>[ triggerCountDefine ];

			if( triggers[ i ] == null ) triggers[ i ] = new Trigger<Ttarget>();

			triggers[ i ].regist( co );
		}



		/// <summary>
		/// �^�C�}�[�������C�x���g�^�[�Q�b�g�֓o�^����B
		/// �̈�m�ۂ͓o�^���Ƀ`�F�b�N���s���B
		/// </summary>
		/// <param name="co">�o�^�������^�C�}�[����</param>
		/// 
		public void registCondition( Timer.OnOver co )
		{
			if( timerTrigger == null ) timerTrigger = new TimerTrigger();

			timerTrigger.regist( co );
		}





		// �C�x���g�^�[�Q�b�g�ւ̈����n�� -----------

		/// <summary>
		/// �C�x���g�^�[�Q�b�g�փg���K�[�������n����������������B
		/// �ǂ̈ʒu�̃g���K���^�[�Q�b�g�̂ǂ̃����o�֓n�����͊e�^�[�Q�b�g�̎����̃����o������ƂȂ�B
		/// </summary>
		/// <param name="obj">�C�x���g�^�[�Q�b�g</param>
		/// 
		protected abstract void passTriggersToTarget( Ttarget obj );



		/// <summary>
		/// �C�x���g�^�[�Q�b�g�ւ̃g���K�[�����n�����s���B
		/// �n���h�������ŃC�x���g�^�[�Q�b�g�𐶐��������ȂǂɌĂԁB
		/// </summary>
		/// <param name="obj">�C�x���g�^�[�Q�b�g</param>
		/// 
		public void passTriggersTo( Ttarget obj )
		{
			if( timerTrigger != null ) timerTrigger.setTimer();

			if( triggers != null ) passTriggersToTarget( obj );
		}

	}




}