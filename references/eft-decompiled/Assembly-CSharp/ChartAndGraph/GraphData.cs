using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace ChartAndGraph;

[Serializable]
public class GraphData : IInternalGraphData
{
	[Serializable]
	public class CategoryData
	{
		public string Name;

		public bool IsBezierCurve;

		public int SegmentsPerCurve = 10;

		public List<GStruct159> mTmpCurveData = new List<GStruct159>();

		public List<GStruct159> Data = new List<GStruct159>();

		public bool Regenerate = true;

		public double? MaxX;

		public double? MaxY;

		public double? MinX;

		public double? MinY;

		public double? MaxRadius;

		public ChartItemEffect LineHoverPrefab;

		public ChartItemEffect PointHoverPrefab;

		public Material LineMaterial;

		public MaterialTiling LineTiling;

		public double LineThickness = 1.0;

		public Material FillMaterial;

		public bool StetchFill;

		public Material PointMaterial;

		public double PointSize = 5.0;

		public PathGenerator LinePrefab;

		public FillPathGenerator FillPrefab;

		public GameObject DotPrefab;

		public double Depth;

		public List<GStruct159> getPoints()
		{
			if (!IsBezierCurve)
			{
				return Data;
			}
			if (!Regenerate)
			{
				return mTmpCurveData;
			}
			Regenerate = false;
			mTmpCurveData.Clear();
			if (Data.Count <= 0)
			{
				return mTmpCurveData;
			}
			mTmpCurveData.Add(Data[0]);
			if (Data.Count < 4)
			{
				return mTmpCurveData;
			}
			int num = Data.Count - 1;
			for (int i = 0; i < num; i += 3)
			{
				AddInnerCurve(Data[i], Data[i + 1], Data[i + 2], Data[i + 3]);
				mTmpCurveData.Add(Data[i + 3]);
			}
			return mTmpCurveData;
		}

		public void AddInnerCurve(GStruct159 p1, GStruct159 c1, GStruct159 c2, GStruct159 p2)
		{
			for (int i = 0; i < SegmentsPerCurve; i++)
			{
				double num = (double)i / (double)SegmentsPerCurve;
				double num2 = 1.0 - num;
				GStruct159 gStruct = num2 * num2 * num2 * p1 + 3.0 * num2 * num2 * num * c1 + 3.0 * num * num * num2 * c2 + num * num * num * p2;
				mTmpCurveData.Add(new GStruct159(gStruct.x, gStruct.y, 0.0));
			}
		}
	}

	public class GClass1668 : IComparer<GStruct159>
	{
		public int Compare(GStruct159 x, GStruct159 y)
		{
			if (x.x < y.x)
			{
				return -1;
			}
			if (!(x.x > y.x))
			{
				return 0;
			}
			return 1;
		}
	}

	[Serializable]
	public class SerializedCategory
	{
		[GAttribute22]
		public double Depth;

		[GAttribute22]
		public PathGenerator LinePrefab;

		[GAttribute22]
		public FillPathGenerator FillPrefab;

		[GAttribute22]
		public GameObject DotPrefab;

		[HideInInspector]
		public GStruct159[] data;

		[HideInInspector]
		public double? MaxX;

		[HideInInspector]
		public double? MaxY;

		[HideInInspector]
		public double? MinX;

		[HideInInspector]
		public double? MinY;

		[HideInInspector]
		public double? MaxRadius;

		public string Name;

		public bool IsBezierCurve;

		public int SegmentsPerCurve = 10;

		public ChartItemEffect LineHoverPrefab;

		public ChartItemEffect PointHoverPrefab;

		public Material Material;

		public MaterialTiling LineTiling;

		public Material InnerFill;

		public double LineThickness = 1.0;

		public bool StetchFill;

		public Material PointMaterial;

		public double PointSize;
	}

	public class Class1078
	{
		public string category;

		public int from;

		public GStruct159 To;

		public GStruct159 current;

		public int index;

		public double startTime;

		public double totalTime;
	}

	[CompilerGenerated]
	public class Class1079
	{
		public string category;

		public bool method_0(Class1078 x)
		{
			return x.category == category;
		}
	}

	[CompilerGenerated]
	public class Class1080
	{
		public string category;

		public bool method_0(Class1078 x)
		{
			return x.category == category;
		}
	}

	[CompilerGenerated]
	private EventHandler DataChanged;

	[CompilerGenerated]
	private EventHandler RealtimeDataChanged;

	[NonSerialized]
	public bool SuspendEvents;

	[NonSerialized]
	public bool SliderUpdated;

	[NonSerialized]
	public List<GStruct159> MTmpDriv = new List<GStruct159>();

	[SerializeField]
	public double automaticVerticalViewGap;

	[SerializeField]
	public bool automaticVerticallView = true;

	[SerializeField]
	public double automaticcHorizontaViewGap;

	[SerializeField]
	public bool automaticHorizontalView = true;

	[SerializeField]
	public double horizontalViewSize = 100.0;

	[SerializeField]
	public double horizontalViewOrigin;

	[SerializeField]
	public double verticalViewSize = 100.0;

	[SerializeField]
	public double verticalViewOrigin;

	[NonSerialized]
	public List<Class1078> Sliders = new List<Class1078>();

	[NonSerialized]
	public GClass1668 Comparer = new GClass1668();

	[NonSerialized]
	public Dictionary<string, CategoryData> CategoryData = new Dictionary<string, CategoryData>();

	[SerializeField]
	public SerializedCategory[] mSerializedData = new SerializedCategory[0];

	public double AutomaticVerticallViewGap
	{
		get
		{
			return automaticVerticalViewGap;
		}
		set
		{
			automaticVerticalViewGap = value;
			RestoreDataValues();
			method_2();
		}
	}

	public bool AutomaticVerticallView
	{
		get
		{
			return automaticVerticallView;
		}
		set
		{
			automaticVerticallView = value;
			RestoreDataValues();
			method_2();
		}
	}

	public double AutomaticcHorizontaViewGap
	{
		get
		{
			return automaticcHorizontaViewGap;
		}
		set
		{
			automaticcHorizontaViewGap = value;
			RestoreDataValues();
			method_2();
		}
	}

	public bool AutomaticHorizontalView
	{
		get
		{
			return automaticHorizontalView;
		}
		set
		{
			automaticHorizontalView = value;
			RestoreDataValues();
			method_2();
		}
	}

	public double HorizontalViewSize
	{
		get
		{
			return horizontalViewSize;
		}
		set
		{
			horizontalViewSize = value;
			method_2();
		}
	}

	public double HorizontalViewOrigin
	{
		get
		{
			return horizontalViewOrigin;
		}
		set
		{
			horizontalViewOrigin = value;
			method_2();
		}
	}

	public double VerticalViewSize
	{
		get
		{
			return verticalViewSize;
		}
		set
		{
			verticalViewSize = value;
			method_2();
		}
	}

	public double VerticalViewOrigin
	{
		get
		{
			return verticalViewOrigin;
		}
		set
		{
			verticalViewOrigin = value;
			method_2();
		}
	}

	int IInternalGraphData.TotalCategories => this.CategoryData.Count;

	IEnumerable<CategoryData> IInternalGraphData.Categories => this.CategoryData.Values;

	public event EventHandler Event_0
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = DataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref DataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = DataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref DataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler Event_1
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = RealtimeDataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref RealtimeDataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = RealtimeDataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref RealtimeDataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler ChartAndGraph_002EIInternalGraphData_002EInternalRealTimeDataChanged
	{
		add
		{
			Event_1 += value;
		}
		remove
		{
			Event_1 -= value;
		}
	}

	public event EventHandler ChartAndGraph_002EIInternalGraphData_002EInternalDataChanged
	{
		add
		{
			Event_0 += value;
		}
		remove
		{
			Event_0 -= value;
		}
	}

	public void RestoreDataValues()
	{
		if (automaticHorizontalView)
		{
			method_0(0);
		}
		if (AutomaticVerticallView)
		{
			method_0(1);
		}
	}

	public void method_0(int axis)
	{
		if (axis == 0)
		{
			double maxValue = ((IInternalGraphData)this).GetMaxValue(0, true);
			horizontalViewSize = maxValue - (horizontalViewOrigin = ((IInternalGraphData)this).GetMinValue(0, true));
		}
		else
		{
			double maxValue2 = ((IInternalGraphData)this).GetMaxValue(1, true);
			verticalViewSize = maxValue2 - (verticalViewOrigin = ((IInternalGraphData)this).GetMinValue(1, true));
		}
	}

	public static void smethod_0(CategoryData data, GStruct159 point)
	{
		if (!data.MaxRadius.HasValue || data.MaxRadius.Value < point.z)
		{
			data.MaxRadius = point.z;
		}
		if (!data.MaxX.HasValue || data.MaxX.Value < point.x)
		{
			data.MaxX = point.x;
		}
		if (!data.MinX.HasValue || data.MinX.Value > point.x)
		{
			data.MinX = point.x;
		}
		if (!data.MaxY.HasValue || data.MaxY.Value < point.y)
		{
			data.MaxY = point.y;
		}
		if (!data.MinY.HasValue || data.MinY.Value > point.y)
		{
			data.MinY = point.y;
		}
	}

	public void Update()
	{
		SliderUpdated = false;
		Sliders.RemoveAll(delegate(Class1078 x)
		{
			if (!this.CategoryData.TryGetValue(x.category, out var value))
			{
				return true;
			}
			if (value.IsBezierCurve)
			{
				return false;
			}
			List<GStruct159> data = value.Data;
			if (x.from < data.Count && x.index < data.Count)
			{
				GStruct159 a = data[x.from];
				GStruct159 to = x.To;
				double num = Time.time;
				num -= x.startTime;
				num = ((!(x.totalTime <= 9.999999747378752E-05)) ? (num / x.totalTime) : 1.0);
				GStruct159 gStruct = (x.current = GStruct159.Lerp(a, to, num));
				data[x.index] = gStruct;
				SliderUpdated = true;
				if (num >= 1.0)
				{
					smethod_0(value, gStruct);
					return true;
				}
				return false;
			}
			return true;
		});
		if (SliderUpdated)
		{
			method_1();
		}
	}

	public void method_1()
	{
		if (!SuspendEvents && RealtimeDataChanged != null)
		{
			RealtimeDataChanged(this, EventArgs.Empty);
		}
	}

	public void method_2()
	{
		if (!SuspendEvents && DataChanged != null)
		{
			DataChanged(this, EventArgs.Empty);
		}
	}

	public void StartBatch()
	{
		SuspendEvents = true;
	}

	public void EndBatch()
	{
		SuspendEvents = false;
		method_2();
	}

	public void ClearAndMakeBezierCurve(string category)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		this.CategoryData[category].IsBezierCurve = true;
		ClearCategory(category);
	}

	public void ClearAndMakeLinear(string category)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		this.CategoryData[category].IsBezierCurve = false;
		ClearCategory(category);
	}

	public void RenameCategory(string prevName, string newName)
	{
		if (!(prevName == newName))
		{
			if (this.CategoryData.ContainsKey(newName))
			{
				throw new ArgumentException($"A category named {newName} already exists");
			}
			CategoryData categoryData = this.CategoryData[prevName];
			this.CategoryData.Remove(prevName);
			categoryData.Name = newName;
			this.CategoryData.Add(newName, categoryData);
			method_2();
		}
	}

	public void AddCategory(string category, Material lineMaterial, double lineThickness, MaterialTiling lineTiling, Material innerFill, bool strechFill, Material pointMaterial, double pointSize)
	{
		if (this.CategoryData.ContainsKey(category))
		{
			throw new ArgumentException($"A category named {category} already exists");
		}
		CategoryData categoryData = new CategoryData();
		this.CategoryData.Add(category, categoryData);
		categoryData.Name = category;
		categoryData.LineMaterial = lineMaterial;
		categoryData.LineHoverPrefab = null;
		categoryData.PointHoverPrefab = null;
		categoryData.FillMaterial = innerFill;
		categoryData.LineThickness = lineThickness;
		categoryData.LineTiling = lineTiling;
		categoryData.StetchFill = strechFill;
		categoryData.PointMaterial = pointMaterial;
		categoryData.PointSize = pointSize;
		method_2();
	}

	public void Set2DCategoryPrefabs(string category, ChartItemEffect lineHover, ChartItemEffect pointHover)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		categoryData.LineHoverPrefab = lineHover;
		categoryData.PointHoverPrefab = pointHover;
	}

	public void AddCategory3DGraph(string category, PathGenerator linePrefab, Material lineMaterial, double lineThickness, MaterialTiling lineTiling, FillPathGenerator fillPrefab, Material innerFill, bool strechFill, GameObject pointPrefab, Material pointMaterial, double pointSize, double depth, bool isCurve, int segmentsPerCurve)
	{
		if (this.CategoryData.ContainsKey(category))
		{
			throw new ArgumentException($"A category named {category} already exists");
		}
		if (depth < 0.0)
		{
			depth = 0.0;
		}
		CategoryData categoryData = new CategoryData();
		this.CategoryData.Add(category, categoryData);
		categoryData.Name = category;
		categoryData.LineMaterial = lineMaterial;
		categoryData.FillMaterial = innerFill;
		categoryData.LineThickness = lineThickness;
		categoryData.LineTiling = lineTiling;
		categoryData.StetchFill = strechFill;
		categoryData.PointMaterial = pointMaterial;
		categoryData.PointSize = pointSize;
		categoryData.LinePrefab = linePrefab;
		categoryData.FillPrefab = fillPrefab;
		categoryData.DotPrefab = pointPrefab;
		categoryData.Depth = depth;
		categoryData.IsBezierCurve = isCurve;
		categoryData.SegmentsPerCurve = segmentsPerCurve;
		method_2();
	}

	public void SetCategoryLine(string category, Material lineMaterial, double lineThickness, MaterialTiling lineTiling)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		categoryData.LineMaterial = lineMaterial;
		categoryData.LineThickness = lineThickness;
		categoryData.LineTiling = lineTiling;
		method_2();
	}

	public void Clear()
	{
		Sliders.Clear();
		this.CategoryData.Clear();
	}

	public bool HasCategory(string category)
	{
		return this.CategoryData.ContainsKey(category);
	}

	public bool RemoveCategory(string category)
	{
		Sliders.RemoveAll((Class1078 x) => x.category == category);
		return this.CategoryData.Remove(category);
	}

	public void SetCategoryPoint(string category, Material pointMaterial, double pointSize)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		categoryData.PointMaterial = pointMaterial;
		categoryData.PointSize = pointSize;
		method_2();
	}

	public void Set3DCategoryPrefabs(string category, PathGenerator linePrefab, FillPathGenerator fillPrefab, GameObject dotPrefab)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		categoryData.LinePrefab = linePrefab;
		categoryData.DotPrefab = dotPrefab;
		categoryData.FillPrefab = fillPrefab;
		method_2();
	}

	public void Set3DCategoryDepth(string category, double depth)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		if (depth < 0.0)
		{
			depth = 0.0;
		}
		this.CategoryData[category].Depth = depth;
		method_2();
	}

	public void SetCategoryFill(string category, Material fillMaterial, bool strechFill)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		categoryData.FillMaterial = fillMaterial;
		categoryData.StetchFill = strechFill;
		method_2();
	}

	public void ClearCategory(string category)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		Sliders.RemoveAll((Class1078 x) => x.category == category);
		this.CategoryData[category].MaxX = null;
		this.CategoryData[category].MaxY = null;
		this.CategoryData[category].MinX = null;
		this.CategoryData[category].MinY = null;
		this.CategoryData[category].MaxRadius = null;
		this.CategoryData[category].Data.Clear();
		this.CategoryData[category].Regenerate = true;
		method_2();
	}

	public void AddPointToCategory(string category, DateTime x, DateTime y, double pointSize = -1.0)
	{
		double num = Class1072.DateToValue(x);
		double num2 = Class1072.DateToValue(y);
		AddPointToCategory(category, num, num2, pointSize);
	}

	public GStruct159 GetPoint(string category, int index)
	{
		List<GStruct159> points = this.CategoryData[category].getPoints();
		if (points.Count == 0)
		{
			return GStruct159.zero;
		}
		if (index < 0)
		{
			return points[0];
		}
		if (index < points.Count)
		{
			return points[index];
		}
		return points[points.Count - 1];
	}

	public void AddPointToCategory(string category, DateTime x, double y, double pointSize = -1.0)
	{
		double num = Class1072.DateToValue(x);
		AddPointToCategory(category, num, y, pointSize);
	}

	public void AddPointToCategory(string category, double x, DateTime y, double pointSize = -1.0)
	{
		double num = Class1072.DateToValue(y);
		AddPointToCategory(category, x, num, pointSize);
	}

	public void AddPointToCategoryRealtime(string category, DateTime x, DateTime y, double slideTime = 0.0, double pointSize = -1.0)
	{
		double num = Class1072.DateToValue(x);
		double num2 = Class1072.DateToValue(y);
		AddPointToCategoryRealtime(category, num, num2, slideTime, pointSize);
	}

	public void AddPointToCategoryRealtime(string category, DateTime x, double y, double slideTime = 0.0, double pointSize = -1.0)
	{
		double num = Class1072.DateToValue(x);
		AddPointToCategoryRealtime(category, num, y, slideTime, pointSize);
	}

	public void AddPointToCategoryRealtime(string category, double x, DateTime y, double slideTime = 0.0, double pointSize = -1.0)
	{
		double num = Class1072.DateToValue(y);
		AddPointToCategoryRealtime(category, x, num, slideTime, pointSize);
	}

	public void AddPointToCategoryRealtime(string category, double x, double y, double slideTime = 0.0, double pointSize = -1.0)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		if (categoryData.IsBezierCurve)
		{
			Debug.LogWarning("Category is Bezier curve. use AddCurveToCategory instead ");
			return;
		}
		GStruct159 gStruct = new GStruct159(x, y, pointSize);
		List<GStruct159> data = categoryData.Data;
		if (data.Count > 0 && data[data.Count - 1].x > gStruct.x)
		{
			Debug.LogWarning("realtime points can only be added at the end of the graph");
			return;
		}
		if (!(slideTime <= 0.0) && data.Count != 0)
		{
			Class1078 @class = new Class1078
			{
				category = category,
				from = data.Count - 1,
				index = data.Count,
				startTime = Time.time,
				totalTime = slideTime,
				To = gStruct
			};
			Sliders.Add(@class);
			@class.current = data[data.Count - 1];
			data.Add(@class.current);
		}
		else
		{
			data.Add(gStruct);
			smethod_0(categoryData, gStruct);
		}
		method_1();
	}

	public void SetCurveInitialPoint(string category, DateTime x, double y, double pointSize = -1.0)
	{
		SetCurveInitialPoint(category, Class1072.DateToValue(x), y, pointSize);
	}

	public void SetCurveInitialPoint(string category, DateTime x, DateTime y, double pointSize = -1.0)
	{
		SetCurveInitialPoint(category, Class1072.DateToValue(x), Class1072.DateToValue(y), pointSize);
	}

	public void SetCurveInitialPoint(string category, double x, DateTime y, double pointSize = -1.0)
	{
		SetCurveInitialPoint(category, x, Class1072.DateToValue(y), pointSize);
	}

	public void SetCurveInitialPoint(string category, double x, double y, double pointSize = -1.0)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		if (!categoryData.IsBezierCurve)
		{
			Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
			return;
		}
		if (categoryData.Data.Count > 0)
		{
			Debug.LogWarning("Initial point already set for this category, call is ignored. Call ClearCategory to create a new curve");
			return;
		}
		categoryData.Regenerate = true;
		if (!categoryData.MaxRadius.HasValue || categoryData.MaxRadius.Value < pointSize)
		{
			categoryData.MaxRadius = pointSize;
		}
		if (!categoryData.MaxX.HasValue || categoryData.MaxX.Value < x)
		{
			categoryData.MaxX = x;
		}
		if (!categoryData.MinX.HasValue || categoryData.MinX.Value > x)
		{
			categoryData.MinX = x;
		}
		if (!categoryData.MaxY.HasValue || categoryData.MaxY.Value < y)
		{
			categoryData.MaxY = y;
		}
		if (!categoryData.MinY.HasValue || categoryData.MinY.Value > y)
		{
			categoryData.MinY = y;
		}
		GStruct159 item = new GStruct159(x, y, pointSize);
		categoryData.Data.Add(item);
		method_2();
	}

	public double method_3(double a, double b, double c)
	{
		return Math.Min(a, Math.Min(b, c));
	}

	public double method_4(double a, double b, double c)
	{
		return Math.Max(a, Math.Max(b, c));
	}

	public GStruct158 method_5(GStruct158 a, GStruct158 b, GStruct158 c)
	{
		return new GStruct158(method_4(a.x, b.x, c.x), method_4(a.y, b.y, c.y));
	}

	public GStruct158 method_6(GStruct158 a, GStruct158 b, GStruct158 c)
	{
		return new GStruct158(method_3(a.x, b.x, c.x), method_3(a.y, b.y, c.y));
	}

	public void MakeCurveCategorySmooth(string category)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		if (!categoryData.IsBezierCurve)
		{
			Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
			return;
		}
		List<GStruct159> data = categoryData.Data;
		categoryData.Regenerate = true;
		MTmpDriv.Clear();
		for (int i = 0; i < data.Count; i += 3)
		{
			GStruct159 gStruct = data[Mathf.Max(i - 3, 0)];
			GStruct159 gStruct2 = data[Mathf.Min(i + 3, data.Count - 1)] - gStruct;
			MTmpDriv.Add(gStruct2 * 0.25);
		}
		for (int j = 3; j < data.Count; j += 3)
		{
			int num = j / 3;
			GStruct159 value = data[j - 3] + MTmpDriv[num - 1];
			GStruct159 value2 = data[j] - MTmpDriv[num];
			data[j - 2] = value;
			data[j - 1] = value2;
		}
		method_2();
	}

	public void AddLinearCurveToCategory(string category, GStruct158 toPoint, double pointSize = -1.0)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		if (!categoryData.IsBezierCurve)
		{
			Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
			return;
		}
		if (categoryData.Data.Count == 0)
		{
			Debug.LogWarning("Initial not set for this category, call is ignored. Call SetCurveInitialPoint to create a new curve");
			return;
		}
		List<GStruct159> data = categoryData.Data;
		GStruct159 a = data[data.Count - 1];
		GStruct159 gStruct = GStruct159.Lerp(a, toPoint.ToDoubleVector3(), 0.3333333432674408);
		GStruct159 gStruct2 = GStruct159.Lerp(a, toPoint.ToDoubleVector3(), 0.6666666865348816);
		AddCurveToCategory(category, gStruct.ToDoubleVector2(), gStruct2.ToDoubleVector2(), toPoint, pointSize);
	}

	public void AddCurveToCategory(string category, GStruct158 controlPointA, GStruct158 controlPointB, GStruct158 toPoint, double pointSize = -1.0)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		if (!categoryData.IsBezierCurve)
		{
			Debug.LogWarning("Category is not Bezier curve. use AddPointToCategory instead ");
			return;
		}
		if (categoryData.Data.Count == 0)
		{
			Debug.LogWarning("Initial not set for this category, call is ignored. Call SetCurveInitialPoint to create a new curve");
			return;
		}
		List<GStruct159> data = categoryData.Data;
		if (data.Count > 0 && data[data.Count - 1].x > toPoint.x)
		{
			Debug.LogWarning("Curves must be added sequentialy according to the x axis. toPoint.x is smaller then the previous point x value");
			return;
		}
		categoryData.Regenerate = true;
		GStruct158 gStruct = method_6(controlPointA, controlPointB, toPoint);
		GStruct158 gStruct2 = method_5(controlPointA, controlPointB, toPoint);
		if (!categoryData.MaxRadius.HasValue || categoryData.MaxRadius.Value < pointSize)
		{
			categoryData.MaxRadius = pointSize;
		}
		if (!categoryData.MaxX.HasValue || categoryData.MaxX.Value < gStruct2.x)
		{
			categoryData.MaxX = gStruct2.x;
		}
		if (!categoryData.MinX.HasValue || categoryData.MinX.Value > gStruct.x)
		{
			categoryData.MinX = gStruct.x;
		}
		if (!categoryData.MaxY.HasValue || categoryData.MaxY.Value < gStruct2.y)
		{
			categoryData.MaxY = gStruct2.y;
		}
		if (!categoryData.MinY.HasValue || categoryData.MinY.Value > gStruct.y)
		{
			categoryData.MinY = gStruct.y;
		}
		data.Add(controlPointA.ToDoubleVector3());
		data.Add(controlPointB.ToDoubleVector3());
		data.Add(new GStruct159(toPoint.x, toPoint.y, pointSize));
		method_2();
	}

	public void AddPointToCategory(string category, double x, double y, double pointSize = -1.0)
	{
		if (!this.CategoryData.ContainsKey(category))
		{
			Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
			return;
		}
		CategoryData categoryData = this.CategoryData[category];
		if (categoryData.IsBezierCurve)
		{
			Debug.LogWarning("Category is Bezier curve. use AddCurveToCategory instead ");
			return;
		}
		GStruct159 item = new GStruct159(x, y, pointSize);
		List<GStruct159> data = categoryData.Data;
		if (!categoryData.MaxRadius.HasValue || categoryData.MaxRadius.Value < pointSize)
		{
			categoryData.MaxRadius = pointSize;
		}
		if (!categoryData.MaxX.HasValue || categoryData.MaxX.Value < item.x)
		{
			categoryData.MaxX = item.x;
		}
		if (!categoryData.MinX.HasValue || categoryData.MinX.Value > item.x)
		{
			categoryData.MinX = item.x;
		}
		if (!categoryData.MaxY.HasValue || categoryData.MaxY.Value < item.y)
		{
			categoryData.MaxY = item.y;
		}
		if (!categoryData.MinY.HasValue || categoryData.MinY.Value > item.y)
		{
			categoryData.MinY = item.y;
		}
		if (data.Count > 0 && data[data.Count - 1].x <= item.x)
		{
			data.Add(item);
			return;
		}
		int num = data.BinarySearch(item, Comparer);
		if (num < 0)
		{
			num = ~num;
		}
		data.Insert(num, item);
		method_2();
	}

	public double GetMaxValue(int axis, bool dataValue)
	{
		if (!dataValue)
		{
			if (axis == 0 && !automaticHorizontalView)
			{
				return HorizontalViewOrigin + Math.Max(0.0010000000474974513, horizontalViewSize);
			}
			if (axis == 1 && !AutomaticVerticallView)
			{
				return VerticalViewOrigin + Math.Max(0.0010000000474974513, verticalViewSize);
			}
		}
		double? num = null;
		double num2 = 0.0;
		foreach (CategoryData value in this.CategoryData.Values)
		{
			if (value.MaxRadius.HasValue && num2 < value.MaxRadius)
			{
				num2 = value.MaxRadius.Value;
			}
			if (axis == 0)
			{
				if (!num.HasValue || (value.MaxX.HasValue && num.Value < value.MaxX))
				{
					num = value.MaxX;
				}
			}
			else if (!num.HasValue || (value.MaxY.HasValue && num.Value < value.MaxY))
			{
				num = value.MaxY;
			}
		}
		foreach (Class1078 slider in Sliders)
		{
			if (axis == 0)
			{
				if (!num.HasValue || num.Value < slider.current.x)
				{
					num = slider.current.x;
				}
			}
			else if (!num.HasValue || num.Value < slider.current.y)
			{
				num = slider.current.y;
			}
		}
		if (!num.HasValue)
		{
			return 10.0;
		}
		double num3 = ((axis == 0) ? automaticcHorizontaViewGap : automaticVerticalViewGap);
		return num.Value + num2 + num3;
	}

	double IInternalGraphData.GetMaxValue(int axis, bool dataValue)
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetMaxValue
		return this.GetMaxValue(axis, dataValue);
	}

	public double GetMinValue(int axis, bool dataValue)
	{
		if (!dataValue)
		{
			if (axis == 0 && !automaticHorizontalView)
			{
				return horizontalViewOrigin;
			}
			if (axis == 1 && !AutomaticVerticallView)
			{
				return verticalViewOrigin;
			}
		}
		double? num = null;
		double num2 = 0.0;
		foreach (CategoryData value in this.CategoryData.Values)
		{
			if (value.MaxRadius.HasValue && num2 < value.MaxRadius)
			{
				num2 = value.MaxRadius.Value;
			}
			if (axis == 0)
			{
				if (!num.HasValue || (value.MinX.HasValue && num.Value > value.MinX))
				{
					num = value.MinX;
				}
			}
			else if (!num.HasValue || (value.MinY.HasValue && num.Value > value.MinY))
			{
				num = value.MinY;
			}
		}
		foreach (Class1078 slider in Sliders)
		{
			if (axis == 0)
			{
				if (!num.HasValue || num.Value > slider.current.x)
				{
					num = slider.current.x;
				}
			}
			else if (!num.HasValue || num.Value > slider.current.y)
			{
				num = slider.current.y;
			}
		}
		if (!num.HasValue)
		{
			return 0.0;
		}
		if (((IInternalGraphData)this).GetMaxValue(axis, dataValue) == num.Value)
		{
			if (num.Value == 0.0)
			{
				return -10.0;
			}
			return 0.0;
		}
		double num3 = ((axis == 0) ? automaticcHorizontaViewGap : automaticVerticalViewGap);
		return num.Value - num2 - num3;
	}

	double IInternalGraphData.GetMinValue(int axis, bool dataValue)
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetMinValue
		return this.GetMinValue(axis, dataValue);
	}

	public void OnAfterDeserialize()
	{
		if (mSerializedData == null)
		{
			return;
		}
		this.CategoryData.Clear();
		SuspendEvents = true;
		SerializedCategory[] array = mSerializedData;
		foreach (SerializedCategory serializedCategory in array)
		{
			if (serializedCategory.Depth < 0.0)
			{
				serializedCategory.Depth = 0.0;
			}
			string name = serializedCategory.Name;
			AddCategory3DGraph(name, serializedCategory.LinePrefab, serializedCategory.Material, serializedCategory.LineThickness, serializedCategory.LineTiling, serializedCategory.FillPrefab, serializedCategory.InnerFill, serializedCategory.StetchFill, serializedCategory.DotPrefab, serializedCategory.PointMaterial, serializedCategory.PointSize, serializedCategory.Depth, serializedCategory.IsBezierCurve, serializedCategory.SegmentsPerCurve);
			Set2DCategoryPrefabs(name, serializedCategory.LineHoverPrefab, serializedCategory.PointHoverPrefab);
			CategoryData categoryData = this.CategoryData[name];
			if (categoryData.Data == null)
			{
				categoryData.Data = new List<GStruct159>();
			}
			else
			{
				categoryData.Data.Clear();
			}
			if (serializedCategory.data != null)
			{
				categoryData.Data.AddRange(serializedCategory.data);
			}
			categoryData.MaxX = serializedCategory.MaxX;
			categoryData.MaxY = serializedCategory.MaxY;
			categoryData.MinX = serializedCategory.MinX;
			categoryData.MinY = serializedCategory.MinY;
			categoryData.MaxRadius = serializedCategory.MaxRadius;
		}
		SuspendEvents = false;
	}

	void IInternalGraphData.OnAfterDeserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAfterDeserialize
		this.OnAfterDeserialize();
	}

	public void OnBeforeSerialize()
	{
		List<SerializedCategory> list = new List<SerializedCategory>();
		foreach (KeyValuePair<string, CategoryData> categoryDatum in this.CategoryData)
		{
			SerializedCategory serializedCategory = new SerializedCategory
			{
				Name = categoryDatum.Key,
				MaxX = categoryDatum.Value.MaxX,
				MinX = categoryDatum.Value.MinX,
				MaxY = categoryDatum.Value.MaxY,
				MaxRadius = categoryDatum.Value.MaxRadius,
				MinY = categoryDatum.Value.MinY,
				LineThickness = categoryDatum.Value.LineThickness,
				StetchFill = categoryDatum.Value.StetchFill,
				Material = categoryDatum.Value.LineMaterial,
				LineHoverPrefab = categoryDatum.Value.LineHoverPrefab,
				PointHoverPrefab = categoryDatum.Value.PointHoverPrefab,
				LineTiling = categoryDatum.Value.LineTiling,
				InnerFill = categoryDatum.Value.FillMaterial,
				data = categoryDatum.Value.Data.ToArray(),
				PointSize = categoryDatum.Value.PointSize,
				IsBezierCurve = categoryDatum.Value.IsBezierCurve,
				SegmentsPerCurve = categoryDatum.Value.SegmentsPerCurve,
				PointMaterial = categoryDatum.Value.PointMaterial,
				LinePrefab = categoryDatum.Value.LinePrefab,
				Depth = categoryDatum.Value.Depth,
				DotPrefab = categoryDatum.Value.DotPrefab,
				FillPrefab = categoryDatum.Value.FillPrefab
			};
			if (serializedCategory.Depth < 0.0)
			{
				serializedCategory.Depth = 0.0;
			}
			list.Add(serializedCategory);
		}
		mSerializedData = list.ToArray();
	}

	void IInternalGraphData.OnBeforeSerialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnBeforeSerialize
		this.OnBeforeSerialize();
	}

	[CompilerGenerated]
	public bool method_7(Class1078 x)
	{
		if (!this.CategoryData.TryGetValue(x.category, out var value))
		{
			return true;
		}
		if (value.IsBezierCurve)
		{
			return false;
		}
		List<GStruct159> data = value.Data;
		if (x.from < data.Count && x.index < data.Count)
		{
			GStruct159 a = data[x.from];
			GStruct159 to = x.To;
			double num = Time.time;
			num -= x.startTime;
			num = ((!(x.totalTime <= 9.999999747378752E-05)) ? (num / x.totalTime) : 1.0);
			GStruct159 gStruct = (x.current = GStruct159.Lerp(a, to, num));
			data[x.index] = gStruct;
			SliderUpdated = true;
			if (num >= 1.0)
			{
				smethod_0(value, gStruct);
				return true;
			}
			return false;
		}
		return true;
	}
}
