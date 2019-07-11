using UnityEngine;
using System.Collections;




public class District3 : MonoBehaviour
{

	//public ShaderSettings	shaders;


	//public StructureTemplateHolder3	structureTemplateHolder;


	// �e�N�X�`���p�b�N
	// �@�E�G���A���������~�n
	// �r���h
	// �@�E�G���A���p�X�i�̃C���X�^���X���j�������E�~�n���n�`�i�������j


	// �e�N�X�`���p�b�N�̑O�Ƀp�[�c�����肵�Ă����K�v������B

	// �e�N�X�`���p�b�N�̓p�[�c���b�V���̃��x���ōs����i�R���o�C������O�ɍς܂��Ȃ��Ă͂Ȃ�Ȃ��j�B
	// �e�N�X�`���p�b�N���ɂ̓p�[�c�ƂȂ郁�b�V���̂t�u���W�����������Ă��܂��̂ŁA
	// �@���̑O�Ƀp�[�c�ƂȂ郁�b�V���͂ǂ����iDistrict3 �ȉ����j�ɃC���X�^���X�����Ă����Ȃ���΂Ȃ�Ȃ��B

	// �p�[�c���b�V������R���o�C�����č\�������b�V���ɂ���ꍇ�A�e�N�X�`���i�}�e���A���j�͂P�łȂ���΂Ȃ�Ȃ��B
	// �@�܂�A���̓_������R���o�C���̑O�Ƀp�[�c�̃e�N�X�`�����p�b�N���Ă����K�v������B

	// �p�X�́A���j�b�g�ƂȂ�v���n�u���e�N�X�`���p�b�N�̑O�ɐ��`�Ƃ��ăC���X�^���X�����Ă����Ȃ���΂Ȃ�Ȃ��B
	// �@�Z�O�����g���p�[�c�Ƃ��ăC���X�^���X������̂́A�e�N�X�`���p�b�N�̌�E�r���h�̑O���悢�B
	// �i���݂͐��`�쐬�� PathSegment3.Awake() �ł���Ă���悤�����A���������� District3 �ł�����ق������������j

	// ���߂���q�G�����L�[�ɃC���X�^���X������I�u�W�F�N�g�̃��b�V���́A�e�N�X�`���p�b�N�̑ΏۂɂȂ邪�A
	// �@�ŏ��̓v���n�u�ł���ꍇ�A�C���X�^���X������K�v������B���̎�i�͓��ꂵ���ق��������B
	// �@�@�E��x�����C���X�^���X�����鏈���𖾎��I�ɌĂԁB
	// �@�@�E�e�N�X�`���p�b�N�Ń��b�V�����擾���鎞�ɏ��񂾂��v���n�u����C���X�^���X������B
	// �@�̓�̂��������邪�A��҂̂ق�����������




	//public void build()
	void Awake()
	{


		IStructureBuilder areas = new AreasBuilder( this );

		IStructureBuilder plots = new PlotsBuilder( this );

		IStructureBuilder buildings = new BuildingsBuilder( this );



		areas.packTextures();

		buildings.packTextures();

		plots.packTextures();



		areas.build();

		buildings.build();

		plots.build();

		


		var spcss = GetComponentsInChildren<_StructurePartContents>();
		
		foreach( var s in spcss )
		{
			
			s.clean();
			
		}


		
		var fields	= GetComponentInChildren<TerrainFieldHolder3>();

		if( fields != null ) fields.split();


	}







	// ==================================================

	
	private interface IStructureBuilder
	{

		void packTextures();

		void build();

	}



	private class AreasBuilder : IStructureBuilder
	{

		private Area3[] areas;

		private _StructureInterArea3[] farBuildings;




		public AreasBuilder( District3 ditsrict )
		{
			areas = ditsrict.GetComponentsInChildren<Area3>();

			farBuildings = ditsrict.GetComponentsInChildren<_StructureInterArea3>();
		}




		public void packTextures()
		{

			if( farBuildings.Length == 0 ) return;


			var tpacker = new TexturePackerSub();//new TexturePacker3SubPallet();

			foreach( var fb in farBuildings )
			{

				tpacker.regist( fb.far );

			}

			tpacker.packTextures();
		
		}




		public void build()
		{

			foreach( var area in areas )
			{

				area.build();

			}

		}


	}





	private class BuildingsBuilder : _StructuresBuilder
	{

		public BuildingsBuilder( District3 ditsrict )
		{
			structures = ditsrict.GetComponentsInChildren<Building3>();
		}

	}




	private class PlotsBuilder : _StructuresBuilder
	{


		private PathSegment3[]	segments;



		public PlotsBuilder( District3 ditsrict )
		{
			structures = ditsrict.GetComponentsInChildren<Plot3>();

			segments = ditsrict.GetComponentsInChildren<PathSegment3>();
		}



		public override void build()
		{

			foreach( var s in segments )
			{

				s.build();

			}

			base.build();

		}


	}




	private abstract class _StructuresBuilder : IStructureBuilder
	{

		protected _Structure3[] structures;


		public void packTextures()
		{
			
			if( structures.Length == 0 ) return;



			var tpacker = new TexturePackerSubPallet();// new TexturePacker3SubPallet();

			foreach( var s in structures )
			{

				var partContents = s.near.GetComponent<_StructurePartContents>();
				//var partContents = s.GetComponentInChildren<_StructurePartContents>();

				tpacker.regist( partContents.getContentsObject() );

			}

			tpacker.packTextures();

		}



		public virtual void build()
		{

			foreach( var s in structures )
			{

				s.build();

			}

		}

	}




}
