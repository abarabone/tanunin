using UnityEngine;
using System.Collections;




public class District3 : MonoBehaviour
{

	//public ShaderSettings	shaders;


	//public StructureTemplateHolder3	structureTemplateHolder;


	// テクスチャパック
	// 　・エリア→建物→敷地
	// ビルド
	// 　・エリア→パス（のインスタンス化）→建物・敷地→地形（分割等）


	// テクスチャパックの前にパーツを決定しておく必要がある。

	// テクスチャパックはパーツメッシュのレベルで行われる（コンバインする前に済ませなくてはならない）。
	// テクスチャパック時にはパーツとなるメッシュのＵＶ座標も書き換えてしまうので、
	// 　その前にパーツとなるメッシュはどこか（District3 以下等）にインスタンス化しておかなければならない。

	// パーツメッシュからコンバインして構造物メッシュにする場合、テクスチャ（マテリアル）は１つでなければならない。
	// 　つまり、この点からもコンバインの前にパーツのテクスチャをパックしておく必要がある。

	// パスは、ユニットとなるプレハブをテクスチャパックの前に雛形としてインスタンス化しておかなければならない。
	// 　セグメントをパーツとしてインスタンス化するのは、テクスチャパックの後・ビルドの前がよい。
	// （現在は雛形作成を PathSegment3.Awake() でやっているようだが、明示化して District3 でやったほうがいいかも）

	// 初めからヒエラルキーにインスタンスがあるオブジェクトのメッシュは、テクスチャパックの対象になるが、
	// 　最初はプレハブである場合、インスタンス化する必要がある。その手段は統一したほうがいい。
	// 　　・一度だけインスタンス化する処理を明示的に呼ぶ。
	// 　　・テクスチャパックでメッシュを取得する時に初回だけプレハブからインスタンス化する。
	// 　の二つのやり方があるが、後者のほうがいいかな




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
