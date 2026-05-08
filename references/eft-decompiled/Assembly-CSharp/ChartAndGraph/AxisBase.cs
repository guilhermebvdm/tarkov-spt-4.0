using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChartAndGraph;

public abstract class AxisBase : ChartSettingItemBase, ISerializationCallbackReceiver
{
	public class Class1063
	{
		public ChartDivisionInfo info;

		public float interp;

		public int fractionDigits;
	}

	[CompilerGenerated]
	public class Class1064
	{
		public double startValue;

		public double range;

		public double parentSize;

		public double method_0(double x)
		{
			return (x - startValue) / range * parentSize;
		}
	}

	[SerializeField]
	private bool SimpleView = true;

	[SerializeField]
	[Tooltip("the format of the axis labels. This can be either a number, time or date time. If the selected value is either DateTime or Time , user ChartDateUtillity to convert dates to double values that can be set to the graph")]
	private AxisFormat format;

	[SerializeField]
	[Tooltip("the depth of the axis reltive to the chart position")]
	private AutoFloat depth;

	[SerializeField]
	[Tooltip("The main divisions of the chart axis")]
	[FormerlySerializedAs("MainDivisions")]
	private ChartMainDivisionInfo mainDivisions = new ChartMainDivisionInfo();

	[SerializeField]
	[Tooltip("The sub divisions of each main division")]
	[FormerlySerializedAs("SubDivisions")]
	private ChartSubDivisionInfo subDivisions = new ChartSubDivisionInfo();

	private Dictionary<double, string> dictionary_0 = new Dictionary<double, string>();

	private List<double> list_0 = new List<double>();

	public AxisFormat Format
	{
		get
		{
			return format;
		}
		set
		{
			format = value;
			RaiseOnChanged();
		}
	}

	public AutoFloat Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
			RaiseOnChanged();
		}
	}

	public ChartDivisionInfo MainDivisions => mainDivisions;

	public ChartDivisionInfo SubDivisions => subDivisions;

	public AxisBase()
	{
		method_3();
	}

	public void method_3()
	{
		AddInnerItem(MainDivisions);
		AddInnerItem(SubDivisions);
	}

	public void ValidateProperties()
	{
		mainDivisions.ValidateProperites();
		subDivisions.ValidateProperites();
	}

	public void method_4(AnyChart parent, ChartOrientation orientation, float total, out float start, out float end)
	{
		start = 0f;
		end = total;
	}

	public void method_5(Interface7 mesh, float length, float offset)
	{
		if (length < 0f)
		{
			offset -= length;
		}
		mesh.Length = length;
		mesh.Offset = offset;
	}

	public void method_6(double scrollOffset, AnyChart parent, Transform parentTransform, ChartDivisionInfo info, Interface7 mesh, int group, ChartOrientation orientation, double gap, bool oppositeSide, double mainGap)
	{
		double parentSize = ((orientation == ChartOrientation.Vertical) ? ((IInternalUse)parent).InternalTotalHeight : ((IInternalUse)parent).InternalTotalWidth);
		method_7(parent, info, orientation, 0f, oppositeSide, out var startPosition, out var lengthDirection, out var advanceDirection);
		double num = GClass1664.smethod_5(parent, orientation, info);
		double num2 = GClass1664.smethod_6(parent, orientation, info);
		double num3 = ((orientation == ChartOrientation.Vertical) ? ((IInternalUse)parent).InternalTotalWidth : ((IInternalUse)parent).InternalTotalHeight);
		if (!info.MarkBackLength.Automatic)
		{
			num3 = info.MarkBackLength.Value;
		}
		double num4 = Math.Abs(num2);
		if (num3 != 0.0 && num > 0.0)
		{
			num4 += Math.Abs(num3) + Math.Abs(num);
		}
		GStruct159 gStruct = advanceDirection * (info.MarkThickness * 0.5f);
		bool flag = ((IInternalUse)parent).InternalHasValues(this);
		double num5 = ((IInternalUse)parent).InternalMaxValue(this);
		double num6 = ((IInternalUse)parent).InternalMinValue(this);
		double range = num5 - num6;
		float num7 = Depth.Value;
		if (Depth.Automatic)
		{
			num7 = (float)((double)((IInternalUse)parent).InternalTotalDepth - num);
		}
		double startValue = scrollOffset + num6;
		double num8 = scrollOffset + num5 + double.Epsilon;
		Func<double, double> func = (double x) => (x - startValue) / range * parentSize;
		double num9 = gap - (scrollOffset - Math.Floor(scrollOffset / gap - double.Epsilon) * gap);
		double num10 = -1.0;
		double num11 = 0.0;
		if (mainGap > 0.0)
		{
			num10 = mainGap - (scrollOffset - Math.Floor(scrollOffset / mainGap - double.Epsilon) * mainGap);
			num11 = scrollOffset + num6 + num10;
		}
		int num12 = 0;
		list_0.Clear();
		double num13 = startValue + num9;
		foreach (double key in dictionary_0.Keys)
		{
			if (key > num8 || key < num13)
			{
				list_0.Add(key);
			}
		}
		for (int num14 = 0; num14 < list_0.Count; num14++)
		{
			dictionary_0.Remove(list_0[num14]);
		}
		for (double num15 = num13; num15 <= num8; num15 += gap)
		{
			num12++;
			if (num12 > 3000)
			{
				break;
			}
			if (mainGap > 0.0)
			{
				if (Math.Abs(num15 - num11) < 1E-05)
				{
					num11 += mainGap;
					continue;
				}
				if (num15 > num11)
				{
					num11 += mainGap;
				}
			}
			double num16 = func(num15);
			GStruct159 gStruct2 = startPosition + advanceDirection * num16;
			GStruct159 gStruct3 = gStruct + num2 * lengthDirection;
			gStruct2 -= gStruct;
			float num17 = 0f;
			Rect rect = GClass1664.smethod_11(new Rect((float)gStruct2.x, (float)gStruct2.y, (float)gStruct3.x, (float)gStruct3.y));
			method_5(mesh, (float)((0.0 - num2) / num4), num17);
			num17 += Math.Abs(mesh.Length);
			mesh.AddXYRect(rect, group, num7);
			if (flag)
			{
				double num18 = Math.Round(num15 * 1000.0) / 1000.0;
				string value = "";
				int num19 = (int)Math.Round(num18);
				Dictionary<int, string> dictionary = ((orientation == ChartOrientation.Horizontal) ? parent.HorizontalValueToStringMap : parent.VerticalValueToStringMap);
				if (Math.Abs(num18 - (double)num19) < 0.001 && dictionary.TryGetValue(num19, out value))
				{
					value = info.TextPrefix + value + info.TextSuffix;
				}
				else if (!dictionary_0.TryGetValue(num18, out value))
				{
					if (format == AxisFormat.Number)
					{
						value = ChartAdvancedSettings.Instance.FormatFractionDigits(info.FractionDigits, num18);
					}
					else
					{
						DateTime dateTime = EFTDateTimeClass.Now.AddDays(num18);
						switch (format)
						{
						case AxisFormat.DateTime:
							value = Class1072.DateToDateTimeString(dateTime);
							break;
						default:
							value = Class1072.DateToTimeString(dateTime);
							break;
						case AxisFormat.Date:
							Debug.LogWarning(Class1072.DateToDateString(dateTime) + ", date: " + dateTime.ToString() + ", val: " + num18);
							value = Class1072.DateToDateString(dateTime);
							break;
						}
					}
					value = info.TextPrefix + value + info.TextSuffix;
					dictionary_0[num18] = value;
				}
				GStruct159 gStruct4 = new GStruct159(gStruct2.x, gStruct2.y);
				gStruct4 += lengthDirection * info.TextSeperation;
				Class1063 @class = new Class1063();
				@class.interp = (float)(num16 / parentSize);
				@class.info = info;
				@class.fractionDigits = info.FractionDigits;
				mesh.AddText(parent, info.TextPrefab, parentTransform, info.FontSize, info.FontSharpness, value, (float)gStruct4.x, (float)gStruct4.y, num7 + info.TextDepth, 0f, @class);
			}
			if (num > 0.0)
			{
				if (orientation == ChartOrientation.Horizontal)
				{
					method_5(mesh, (float)(num / num4), num17);
					rect = GClass1664.smethod_11(new Rect((float)gStruct2.x, num7, (float)gStruct3.x, (float)num));
					mesh.AddXZRect(rect, group, (float)gStruct2.y);
				}
				else
				{
					method_5(mesh, (float)((0.0 - num) / num4), num17);
					rect = GClass1664.smethod_11(new Rect((float)gStruct2.y, num7, (float)gStruct3.y, (float)num));
					mesh.AddYZRect(rect, group, (float)gStruct2.x);
				}
				num17 += Math.Abs(mesh.Length);
				if (num3 != 0.0)
				{
					method_5(mesh, (float)(num3 / num4), num17);
					num17 += Math.Abs(mesh.Length);
					GStruct159 gStruct5 = gStruct + num3 * lengthDirection;
					Rect rect2 = GClass1664.smethod_11(new Rect((float)gStruct2.x, (float)gStruct2.y, (float)gStruct5.x, (float)gStruct5.y));
					mesh.AddXYRect(rect2, group, (float)((double)num7 + num));
				}
			}
		}
	}

	public void method_7(AnyChart parent, ChartDivisionInfo info, ChartOrientation orientation, float scrollOffset, bool oppositeSide, out GStruct159 startPosition, out GStruct159 lengthDirection, out GStruct159 advanceDirection)
	{
		if (orientation == ChartOrientation.Horizontal)
		{
			advanceDirection = new GStruct159(1.0, 0.0);
			if (oppositeSide)
			{
				startPosition = new GStruct159(scrollOffset, ((IInternalUse)parent).InternalTotalHeight);
				lengthDirection = new GStruct159(0.0, -1.0);
			}
			else
			{
				startPosition = new GStruct159(0.0, 0.0);
				lengthDirection = new GStruct159(0.0, 1.0);
			}
		}
		else
		{
			advanceDirection = new GStruct159(0.0, 1.0);
			if (oppositeSide)
			{
				startPosition = new GStruct159(0.0, 0.0);
				lengthDirection = new GStruct159(1.0, 0.0);
			}
			else
			{
				startPosition = new GStruct159(((IInternalUse)parent).InternalTotalWidth, scrollOffset);
				lengthDirection = new GStruct159(-1.0, 0.0);
			}
		}
	}

	public void method_8(double scrollOffset, AnyChart parent, Transform parentTransform, Interface7 mesh, ChartOrientation orientation)
	{
		int total = SubDivisions.Total;
		if (total <= 1)
		{
			return;
		}
		double num = ((IInternalUse)parent).InternalMaxValue(this);
		double num2 = ((IInternalUse)parent).InternalMinValue(this);
		double range = num - num2;
		double? num3 = method_9(parent, range);
		if (num3.HasValue)
		{
			double gap = num3.Value / (double)total;
			mesh.Tile = GClass1664.smethod_16(SubDivisions.MaterialTiling);
			if ((SubDivisions.Alignment & ChartDivisionAligment.Opposite) == ChartDivisionAligment.Opposite)
			{
				method_6(scrollOffset, parent, parentTransform, SubDivisions, mesh, 0, orientation, gap, oppositeSide: false, num3.Value);
			}
			if ((SubDivisions.Alignment & ChartDivisionAligment.Standard) == ChartDivisionAligment.Standard)
			{
				method_6(scrollOffset, parent, parentTransform, SubDivisions, mesh, 0, orientation, gap, oppositeSide: true, num3.Value);
			}
		}
	}

	public double? method_9(AnyChart parent, double range)
	{
		double value = ((ChartMainDivisionInfo)MainDivisions).UnitsPerDivision;
		if (((ChartMainDivisionInfo)MainDivisions).Messure == ChartDivisionInfo.DivisionMessure.TotalDivisions)
		{
			int total = ((ChartMainDivisionInfo)MainDivisions).Total;
			if (total <= 0)
			{
				return null;
			}
			value = range / (double)total;
		}
		return value;
	}

	public void method_10(double scrollOffset, AnyChart parent, Transform parentTransform, Interface7 mesh, ChartOrientation orientation)
	{
		double num = ((IInternalUse)parent).InternalMaxValue(this);
		double num2 = ((IInternalUse)parent).InternalMinValue(this);
		double range = num - num2;
		double? num3 = method_9(parent, range);
		if (num3.HasValue)
		{
			mesh.Tile = GClass1664.smethod_16(MainDivisions.MaterialTiling);
			if ((MainDivisions.Alignment & ChartDivisionAligment.Opposite) == ChartDivisionAligment.Opposite)
			{
				method_6(scrollOffset, parent, parentTransform, MainDivisions, mesh, 0, orientation, num3.Value, oppositeSide: false, -1.0);
			}
			if ((MainDivisions.Alignment & ChartDivisionAligment.Standard) == ChartDivisionAligment.Standard)
			{
				method_6(scrollOffset, parent, parentTransform, MainDivisions, mesh, 0, orientation, num3.Value, oppositeSide: true, -1.0);
			}
		}
	}

	public void OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnBeforeSerialize
		this.OnBeforeSerialize();
	}

	public void OnAfterDeserialize()
	{
		method_3();
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnAfterDeserialize
		this.OnAfterDeserialize();
	}
}
