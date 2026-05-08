using System;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;

public class GraphChartFeed : MonoBehaviour
{
	[SerializeField]
	private GraphChartBase _graphChart;

	public void Awake()
	{
		List<GClass1434> list = new List<GClass1434>();
		for (int i = 0; i < 100; i++)
		{
			GClass1434 item = new GClass1434
			{
				expired = UnityEngine.Random.Range(0, 20),
				sold = UnityEngine.Random.Range(20, 50),
				placed = UnityEngine.Random.Range(50, 100)
			};
			list.Add(item);
		}
		Show(list.ToArray());
	}

	public void Show(GClass1434[] entities)
	{
		_graphChart.DataSource.StartBatch();
		_graphChart.DataSource.ClearAndMakeBezierCurve("Placed");
		_graphChart.DataSource.ClearAndMakeBezierCurve("Sold");
		_graphChart.DataSource.ClearAndMakeBezierCurve("Expired");
		_graphChart.DataSource.SetCurveInitialPoint("Placed", 0.0, entities[0].placed);
		_graphChart.DataSource.SetCurveInitialPoint("Sold", 0.0, entities[0].sold);
		_graphChart.DataSource.SetCurveInitialPoint("Expired", 0.0, entities[0].expired);
		for (int i = 1; i < entities.Length; i++)
		{
			GClass1434 gClass = entities[i];
			gClass.date = i;
			Debug.Log("Expired: " + gClass.expired + ", sold: " + gClass.sold + ", placed: " + gClass.placed + ", date: " + gClass.date);
			_graphChart.DataSource.AddLinearCurveToCategory("Placed", new GStruct158(gClass.date, gClass.placed));
			_graphChart.DataSource.AddLinearCurveToCategory("Sold", new GStruct158(gClass.date, gClass.sold));
			_graphChart.DataSource.AddLinearCurveToCategory("Expired", new GStruct158(gClass.date, gClass.expired));
		}
		_graphChart.DataSource.EndBatch();
	}

	public DateTime method_0()
	{
		DateTime dateTime = new DateTime(1995, 1, 1);
		int days = (DateTime.Today - dateTime).Days;
		return dateTime.AddDays(UnityEngine.Random.Range(DateTime.Today.Day, days));
	}
}
