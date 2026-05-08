using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace ChartAndGraph;

[ExecuteInEditMode]
public abstract class GraphChartBase : AxisChart, ISerializationCallbackReceiver
{
	public class GClass1667
	{
		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		[NonSerialized]
		[CompilerGenerated]
		public int Int_0;

		[NonSerialized]
		[CompilerGenerated]
		public string String_0;

		[NonSerialized]
		[CompilerGenerated]
		public string String_1;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_0;

		[NonSerialized]
		[CompilerGenerated]
		public GStruct158 Gstruct158_0;

		[NonSerialized]
		[CompilerGenerated]
		public string String_2;

		public float Magnitude
		{
			[CompilerGenerated]
			get
			{
				return Float_0;
			}
			[CompilerGenerated]
			set
			{
				Float_0 = value;
			}
		}

		public int Index
		{
			[CompilerGenerated]
			get
			{
				return Int_0;
			}
			[CompilerGenerated]
			set
			{
				Int_0 = value;
			}
		}

		public string XString
		{
			[CompilerGenerated]
			get
			{
				return String_0;
			}
			[CompilerGenerated]
			set
			{
				String_0 = value;
			}
		}

		public string YString
		{
			[CompilerGenerated]
			get
			{
				return String_1;
			}
			[CompilerGenerated]
			set
			{
				String_1 = value;
			}
		}

		public Vector3 Position
		{
			[CompilerGenerated]
			get
			{
				return Vector3_0;
			}
			[CompilerGenerated]
			set
			{
				Vector3_0 = value;
			}
		}

		public GStruct158 Value
		{
			[CompilerGenerated]
			get
			{
				return Gstruct158_0;
			}
			[CompilerGenerated]
			set
			{
				Gstruct158_0 = value;
			}
		}

		public string Category
		{
			[CompilerGenerated]
			get
			{
				return String_2;
			}
			[CompilerGenerated]
			set
			{
				String_2 = value;
			}
		}

		public GClass1667(int index, Vector3 position, GStruct158 value, float magnitude, string category, string xString, string yString)
		{
			Position = position;
			Value = value;
			Category = category;
			XString = xString;
			YString = yString;
			Index = index;
			Magnitude = magnitude;
		}
	}

	[Serializable]
	public class GraphEvent : UnityEvent<GClass1667>
	{
	}

	[SerializeField]
	protected float heightRatio = 300f;

	[SerializeField]
	protected float widthRatio = 600f;

	public GraphEvent PointClicked = new GraphEvent();

	public GraphEvent PointHovered = new GraphEvent();

	public UnityEvent NonHovered = new UnityEvent();

	[SerializeField]
	private bool scrollable = true;

	[HideInInspector]
	[SerializeField]
	private bool autoScrollHorizontally;

	[HideInInspector]
	[SerializeField]
	private double horizontalScrolling;

	[HideInInspector]
	[SerializeField]
	private bool autoScrollVertically;

	[HideInInspector]
	[SerializeField]
	private double verticalScrolling;

	[HideInInspector]
	[SerializeField]
	protected GraphData Data = new GraphData();

	public float HeightRatio
	{
		get
		{
			return heightRatio;
		}
		set
		{
			heightRatio = value;
			Invalidate();
		}
	}

	public float WidthRatio
	{
		get
		{
			return widthRatio;
		}
		set
		{
			widthRatio = value;
			Invalidate();
		}
	}

	public bool Scrollable
	{
		get
		{
			return scrollable;
		}
		set
		{
			scrollable = value;
			Invalidate();
		}
	}

	public bool AutoScrollHorizontally
	{
		get
		{
			return autoScrollHorizontally;
		}
		set
		{
			autoScrollHorizontally = value;
			GenerateRealtime();
		}
	}

	public double HorizontalScrolling
	{
		get
		{
			return horizontalScrolling;
		}
		set
		{
			horizontalScrolling = value;
			GenerateRealtime();
		}
	}

	public bool AutoScrollVertically
	{
		get
		{
			return autoScrollVertically;
		}
		set
		{
			autoScrollVertically = value;
			GenerateRealtime();
		}
	}

	public double VerticalScrolling
	{
		get
		{
			return verticalScrolling;
		}
		set
		{
			verticalScrolling = value;
			GenerateRealtime();
		}
	}

	public GraphData DataSource => Data;

	public override bool SupportsCategoryLabels => false;

	public override bool SupportsGroupLables => false;

	public override bool SupportsItemLabels => true;

	public override float TotalHeightLink => heightRatio;

	public override float TotalWidthLink => widthRatio;

	public override float TotalDepthLink => 0f;

	public override double GetScrollOffset(int axis)
	{
		if (!scrollable)
		{
			return 0.0;
		}
		if ((autoScrollHorizontally && axis == 0) || (autoScrollVertically && axis == 1))
		{
			double maxValue = ((IInternalGraphData)Data).GetMaxValue(axis, false);
			return ((IInternalGraphData)Data).GetMaxValue(axis, true) - maxValue;
		}
		return axis switch
		{
			1 => verticalScrolling, 
			0 => horizontalScrolling, 
			_ => base.GetScrollOffset(axis), 
		};
	}

	public void method_3()
	{
		((IInternalGraphData)Data).InternalDataChanged -= Data_InternalDataChanged;
		((IInternalGraphData)Data).InternalRealTimeDataChanged -= Data_InternalRealTimeDataChanged;
		((IInternalGraphData)Data).InternalDataChanged += Data_InternalDataChanged;
		((IInternalGraphData)Data).InternalRealTimeDataChanged += Data_InternalRealTimeDataChanged;
	}

	public void Data_InternalRealTimeDataChanged(object sender, EventArgs e)
	{
		GenerateRealtime();
	}

	public void Data_InternalDataChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	public override void Start()
	{
		base.Start();
		if (!GClass1664.Boolean_0)
		{
			method_3();
		}
		Invalidate();
	}

	public override void OnValidate()
	{
		base.OnValidate();
		if (!GClass1664.Boolean_0)
		{
			method_3();
		}
		Data.RestoreDataValues();
		Invalidate();
	}

	public override void OnLabelSettingChanged()
	{
		base.OnLabelSettingChanged();
		Invalidate();
	}

	public override void OnAxisValuesChanged()
	{
		base.OnAxisValuesChanged();
		Invalidate();
	}

	public override void OnLabelSettingsSet()
	{
		base.OnLabelSettingsSet();
		Invalidate();
	}

	public GClass1665 interpolateInRect(Rect rect, GStruct159 point)
	{
		double x = (double)rect.x + (double)rect.width * point.x;
		double y = (double)rect.y + (double)rect.height * point.y;
		return new GClass1665(x, y, point.z, 0.0);
	}

	public GClass1665 TransformPoint(Rect viewRect, Vector3 point, GStruct158 min, GStruct158 range)
	{
		return interpolateInRect(viewRect, new GStruct159(((double)point.x - min.x) / range.x, ((double)point.y - min.y) / range.y));
	}

	public override void Update()
	{
		base.Update();
		((IInternalGraphData)Data).Update();
	}

	public void method_4(GStruct159 point, ref double minX, ref double minY, ref double maxX, ref double maxY)
	{
		minX = Math.Min(minX, point.x);
		maxX = Math.Max(maxX, point.x);
		minY = Math.Min(minY, point.y);
		maxY = Math.Max(maxY, point.y);
	}

	public Rect method_5(Rect completeRect, Rect lineRect)
	{
		if (!(completeRect.width < 0.0001f) && completeRect.height >= 0.0001f)
		{
			if (!float.IsInfinity(lineRect.xMax) && !float.IsInfinity(lineRect.xMin) && !float.IsInfinity(lineRect.yMin) && !float.IsInfinity(lineRect.yMax))
			{
				float x = (lineRect.xMin - completeRect.xMin) / completeRect.width;
				float y = (lineRect.yMin - completeRect.yMin) / completeRect.height;
				float width = lineRect.width / completeRect.width;
				float height = lineRect.height / completeRect.height;
				return new Rect(x, y, width, height);
			}
			return default(Rect);
		}
		return default(Rect);
	}

	public int ClipPoints(IList<GStruct159> points, List<GClass1665> res, out Rect uv)
	{
		double minValue = ((IInternalGraphData)Data).GetMinValue(0, false);
		double minValue2 = ((IInternalGraphData)Data).GetMinValue(1, false);
		double maxValue = ((IInternalGraphData)Data).GetMaxValue(0, false);
		double maxValue2 = ((IInternalGraphData)Data).GetMaxValue(1, false);
		double num = GetScrollOffset(0) + minValue;
		double num2 = GetScrollOffset(1) + minValue2;
		double num3 = maxValue - minValue;
		double num4 = maxValue2 - minValue2;
		double num5 = num + num3;
		bool flag = false;
		bool flag2 = false;
		double maxX = double.MinValue;
		double minX = double.MaxValue;
		double maxY = double.MinValue;
		double minY = double.MaxValue;
		minValue = double.MaxValue;
		minValue2 = double.MaxValue;
		maxValue = double.MinValue;
		maxValue2 = double.MinValue;
		int result = 0;
		for (int i = 0; i < points.Count; i++)
		{
			bool flag3 = flag;
			bool flag4 = flag2;
			flag = false;
			flag2 = false;
			GStruct159 point = points[i];
			method_4(points[i], ref minValue, ref minValue2, ref maxValue, ref maxValue2);
			if (!(point.x < num) && point.x <= num5)
			{
				flag2 = true;
				if (flag3)
				{
					result = i - 1;
					method_4(points[i - 1], ref minX, ref minY, ref maxX, ref maxY);
					res.Add(points[i - 1].ToDoubleVector4());
				}
				method_4(point, ref minX, ref minY, ref maxX, ref maxY);
				res.Add(point.ToDoubleVector4());
				continue;
			}
			flag = true;
			if (flag4)
			{
				res.Add(point.ToDoubleVector4());
			}
			if (flag3 && point.x > num5 && points[i - 1].x < num)
			{
				method_4(points[i - 1], ref minX, ref minY, ref maxX, ref maxY);
				method_4(point, ref minX, ref minY, ref maxX, ref maxY);
				res.Add(points[i - 1].ToDoubleVector4());
				res.Add(point.ToDoubleVector4());
			}
		}
		for (int j = 0; j < res.Count; j++)
		{
			GClass1665 gClass = res[j];
			gClass.w = gClass.z;
			gClass.z = 0.0;
			res[j] = gClass;
		}
		uv = method_5(new Rect((float)minValue, (float)minValue2, (float)(maxValue - minValue), (float)(maxValue2 - minValue2)), new Rect((float)minX, (float)num2, (float)(maxX - minX), (float)num4));
		return result;
	}

	public void TransformPoints(IList<GStruct159> points, Rect viewRect, GStruct159 min, GStruct159 max)
	{
		GStruct159 gStruct = max - min;
		if (!(gStruct.x <= 9.999999747378752E-05) && gStruct.y >= 9.999999747378752E-05)
		{
			double num = Math.Min((double)viewRect.width / gStruct.x, (double)viewRect.height / gStruct.y);
			for (int i = 0; i < points.Count; i++)
			{
				GStruct159 gStruct2 = points[i];
				GClass1665 gClass = interpolateInRect(viewRect, new GStruct159((gStruct2.x - min.x) / gStruct.x, (gStruct2.y - min.y) / gStruct.y));
				gClass.z = gStruct2.z * num;
				points[i] = gClass.ToDoubleVector3();
			}
		}
	}

	public void TransformPoints(IList<GClass1665> points, List<Vector4> output, Rect viewRect, GStruct159 min, GStruct159 max)
	{
		output.Clear();
		GStruct159 gStruct = max - min;
		if (!(gStruct.x <= 9.999999747378752E-05) && gStruct.y >= 9.999999747378752E-05)
		{
			double num = Math.Min((double)viewRect.width / gStruct.x, (double)viewRect.height / gStruct.y);
			for (int i = 0; i < points.Count; i++)
			{
				GClass1665 gClass = points[i];
				GClass1665 gClass2 = interpolateInRect(viewRect, new GStruct159((gClass.x - min.x) / gStruct.x, (gClass.y - min.y) / gStruct.y));
				gClass2.z = 0.0;
				gClass2.w = gClass.w * num;
				output.Add(gClass2.ToVector4());
			}
		}
	}

	public override void ValidateProperties()
	{
		base.ValidateProperties();
		if (heightRatio < 0f)
		{
			heightRatio = 0f;
		}
		if (widthRatio < 0f)
		{
			widthRatio = 0f;
		}
	}

	public virtual void GenerateRealtime()
	{
		GenerateAxis(force: false);
	}

	public override bool HasValues(AxisBase axis)
	{
		return true;
	}

	public override double MaxValue(AxisBase axis)
	{
		if (axis == null)
		{
			return 0.0;
		}
		if (axis == mHorizontalAxis)
		{
			return ((IInternalGraphData)Data).GetMaxValue(0, false);
		}
		if (axis == mVerticalAxis)
		{
			return ((IInternalGraphData)Data).GetMaxValue(1, false);
		}
		return 0.0;
	}

	public override double MinValue(AxisBase axis)
	{
		if (axis == null)
		{
			return 0.0;
		}
		if (axis == mHorizontalAxis)
		{
			return ((IInternalGraphData)Data).GetMinValue(0, false);
		}
		if (axis == mVerticalAxis)
		{
			return ((IInternalGraphData)Data).GetMinValue(1, false);
		}
		return 0.0;
	}

	public void OnBeforeSerialize()
	{
		if (Data != null)
		{
			((IInternalGraphData)Data).OnBeforeSerialize();
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnBeforeSerialize
		this.OnBeforeSerialize();
	}

	public void OnAfterDeserialize()
	{
		if (Data != null)
		{
			((IInternalGraphData)Data).OnAfterDeserialize();
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAfterDeserialize
		this.OnAfterDeserialize();
	}

	public override void OnItemHoverted(object userData)
	{
		base.OnItemHoverted(userData);
		GClass1667 arg = userData as GClass1667;
		if (PointHovered != null)
		{
			PointHovered.Invoke(arg);
		}
	}

	public string StringFromAxisFormat(double val, AxisBase axis)
	{
		if (axis == null)
		{
			return ChartAdvancedSettings.Instance.FormatFractionDigits(2, val);
		}
		return StringFromAxisFormat(val, axis, axis.MainDivisions.FractionDigits);
	}

	public string StringFromAxisFormat(double val, AxisBase axis, int fractionDigits)
	{
		if (axis == null)
		{
			return ChartAdvancedSettings.Instance.FormatFractionDigits(fractionDigits, val);
		}
		string text = "";
		if (axis.Format == AxisFormat.Number)
		{
			return ChartAdvancedSettings.Instance.FormatFractionDigits(fractionDigits, val);
		}
		DateTime dateTime = Class1072.ValueToDate(val);
		if (axis.Format == AxisFormat.DateTime)
		{
			return Class1072.DateToDateTimeString(dateTime);
		}
		if (axis.Format == AxisFormat.Date)
		{
			return Class1072.DateToDateString(dateTime);
		}
		return Class1072.DateToTimeString(dateTime);
	}

	public override void OnItemSelected(object userData)
	{
		base.OnItemSelected(userData);
		GClass1667 arg = userData as GClass1667;
		if (PointClicked != null)
		{
			PointClicked.Invoke(arg);
		}
	}

	public GraphChartBase()
	{
	}
}
