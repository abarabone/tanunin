using UnityEngine;
using System.Collections;

public class Plot3 : _Structure3
{


	public override void deepInit()
	{

		base.deepInit();

		gameObject.AddComponent<PlotHit3>();

	}


	public override void build()
	{
/*
		var segs = GetComponentsInChildren<PathSegment3>();
		
		foreach( var s in segs )
		{
			
			s.build();
			
		}
*/

		base.build();

		buildEnvelope();

	}



	void buildEnvelope()
	{

		
		var tfEnvelope = tf.findWithLayerInDirectChildren( UserLayer._bgEnvelope );


		if( tfEnvelope == null )
		{

			tfEnvelope = new GameObject( "envelope" ).transform;

			var tfNear = near.transform;


			tfEnvelope.SetParent( tf, false );


			var col = tfEnvelope.gameObject.AddComponent<BoxCollider>();

			var bounds = near.GetComponent<MeshFilter>().sharedMesh.bounds;// すべて見えないパーツの時にはドローメッシュ取得エラーでるかも


			var mt = tf.worldToLocalMatrix * near.transform.localToWorldMatrix;


			col.enabled = false;

			col.center = mt.MultiplyPoint3x4( bounds.center );

			col.size = bounds.size;

			col.isTrigger = true;// 爆発物判定にも対トリガーヒットフラグがついたのでＯＫ

			col.enabled = true;

		}

		
		setLayer( tfEnvelope.gameObject, UserLayer._bgPlotEnvelope );
		//setLayer( tfEnvelope.gameObject, UserLayer._bgEnvelope );
		// ふつうのエンベロープだと、たとえキネマティックでも建物や敵と衝突してしまう

	}














}
