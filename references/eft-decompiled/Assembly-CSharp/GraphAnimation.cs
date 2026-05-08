using System;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;

public class GraphAnimation : MonoBehaviour
{
	public class Class90
	{
		public float maxX;

		public float minX;

		public float maxY;

		public float minY;

		public float totalTime = 3f;

		public float next;

		public string category;

		public List<Vector2> points;

		public int index;

		public void Update(GraphChartBase graphChart)
		{
			if (!(graphChart == null) && points != null && points.Count != 0 && index < points.Count)
			{
				next -= Time.deltaTime;
				if (next <= 0f)
				{
					next = totalTime / (float)points.Count;
					Vector2 vector = points[index];
					graphChart.DataSource.AddPointToCategoryRealtime(category, vector.x, vector.y, next);
					index++;
				}
			}
		}
	}

	private GraphChartBase graphChartBase_0;

	public float AnimationTime = 3f;

	public bool ModifyRange = true;

	private Dictionary<string, Class90> dictionary_0 = new Dictionary<string, Class90>();

	public void Start()
	{
		graphChartBase_0 = GetComponent<GraphChartBase>();
	}

	public bool method_0(float val)
	{
		if (float.IsNaN(val))
		{
			return false;
		}
		if (float.IsInfinity(val))
		{
			return false;
		}
		return true;
	}

	public void Animate(string category, List<Vector2> points, float totalTime)
	{
		if (graphChartBase_0 == null || points == null || points.Count == 0)
		{
			return;
		}
		Class90 @class = new Class90();
		@class.maxX = float.MinValue;
		@class.maxY = float.MinValue;
		@class.minX = float.MaxValue;
		@class.minY = float.MaxValue;
		for (int i = 0; i < points.Count; i++)
		{
			@class.maxX = Mathf.Max(points[i].x, @class.maxX);
			@class.maxY = Mathf.Max(points[i].y, @class.maxY);
			@class.minX = Mathf.Min(points[i].x, @class.minX);
			@class.minY = Mathf.Min(points[i].y, @class.minY);
		}
		if (ModifyRange)
		{
			float num = @class.maxX;
			float num2 = @class.maxY;
			float num3 = @class.minX;
			float num4 = @class.minY;
			foreach (Class90 value in dictionary_0.Values)
			{
				num = Mathf.Max(num, value.maxX);
				num2 = Mathf.Max(num2, value.maxY);
				num3 = Mathf.Min(num3, value.minX);
				num4 = Mathf.Min(num4, value.minY);
			}
			GraphData dataSource = graphChartBase_0.DataSource;
			num = (float)Math.Max(((IInternalGraphData)dataSource).GetMaxValue(0, true), num);
			num3 = (float)Math.Min(((IInternalGraphData)dataSource).GetMinValue(0, true), num3);
			num2 = (float)Math.Max(((IInternalGraphData)dataSource).GetMaxValue(1, true), num2);
			num4 = (float)Math.Min(((IInternalGraphData)dataSource).GetMinValue(1, true), num4);
			if (method_0(num) && method_0(num2) && method_0(num3) && method_0(num4))
			{
				graphChartBase_0.DataSource.StartBatch();
				graphChartBase_0.DataSource.AutomaticHorizontalView = false;
				graphChartBase_0.DataSource.AutomaticVerticallView = false;
				graphChartBase_0.DataSource.HorizontalViewSize = num - num3;
				graphChartBase_0.DataSource.HorizontalViewOrigin = num3;
				graphChartBase_0.DataSource.VerticalViewSize = num2 - num4;
				graphChartBase_0.DataSource.VerticalViewOrigin = num4;
				graphChartBase_0.DataSource.EndBatch();
			}
		}
		@class.points = points;
		@class.index = 0;
		@class.next = 0f;
		@class.totalTime = totalTime;
		@class.category = category;
		graphChartBase_0.DataSource.ClearCategory(category);
		dictionary_0[category] = @class;
	}

	public void Update()
	{
		if (graphChartBase_0 == null)
		{
			return;
		}
		foreach (Class90 value in dictionary_0.Values)
		{
			value.Update(graphChartBase_0);
		}
	}
}
