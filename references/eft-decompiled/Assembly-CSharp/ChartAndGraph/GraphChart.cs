using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChartAndGraph;

public class GraphChart : GraphChartBase, GInterface162
{
	public class Class1075
	{
		public Class1082 mItemLabels;

		public CanvasLines mLines;

		public CanvasLines mDots;

		public CanvasLines mFill;

		public Dictionary<int, string> mCahced = new Dictionary<int, string>();
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1076
	{
		public static readonly Class1076 class1076_0 = new Class1076();

		public static Func<GStruct159, Vector4> func_0;

		public Vector4 method_0(GStruct159 x)
		{
			return x.ToVector3();
		}
	}

	[CompilerGenerated]
	public class Class1077
	{
		public string catName;

		public GraphChart graphChart_0;

		public void method_0(int idx, Vector2 pos)
		{
			graphChart_0.method_14(catName, idx, pos);
		}

		public void method_1(int idx, Vector2 pos)
		{
			graphChart_0.method_13(catName, idx, pos);
		}

		public void method_2()
		{
			graphChart_0.method_12(catName);
		}
	}

	private Vector2 vector2_0 = Vector2.zero;

	private HashSet<string> hashSet_0 = new HashSet<string>();

	private Dictionary<string, Dictionary<int, BillboardText>> dictionary_0 = new Dictionary<string, Dictionary<int, BillboardText>>();

	private HashSet<BillboardText> hashSet_1 = new HashSet<BillboardText>();

	private Dictionary<string, Class1075> dictionary_1 = new Dictionary<string, Class1075>();

	private List<GStruct159> list_0 = new List<GStruct159>();

	private List<GClass1665> list_1 = new List<GClass1665>();

	private List<Vector4> list_2 = new List<Vector4>();

	private List<int> list_3 = new List<int>();

	private GameObject gameObject_0;

	private GameObject gameObject_1;

	private Vector2? nullable_0;

	private GraphicRaycaster graphicRaycaster_0;

	private bool bool_0;

	private StringBuilder stringBuilder_0 = new StringBuilder();

	public UnityEvent MousePan;

	[SerializeField]
	private bool horizontalPanning;

	[SerializeField]
	private bool verticalPanning;

	public bool HorizontalPanning
	{
		get
		{
			return horizontalPanning;
		}
		set
		{
			horizontalPanning = value;
			Invalidate();
		}
	}

	public bool VerticalPanning
	{
		get
		{
			return verticalPanning;
		}
		set
		{
			verticalPanning = value;
			Invalidate();
		}
	}

	public GameObject method_6(Rect viewRect)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(GClass861.Load("Chart And Graph/RectMask") as GameObject);
		GClass1664.smethod_3(gameObject, hideHierarchy);
		gameObject.AddComponent<ChartItem>();
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(0f, 0f);
		component.pivot = new Vector2(0f, 1f);
		component.sizeDelta = viewRect.size;
		component.anchoredPosition = new Vector2(0f, viewRect.size.y);
		gameObject_1 = gameObject;
		Mask component2 = gameObject_1.GetComponent<Mask>();
		if (component2 != null)
		{
			component2.enabled = base.Scrollable;
		}
		return gameObject_1;
	}

	public CanvasLines method_7(GraphData.CategoryData data, GameObject rectMask)
	{
		GameObject obj = new GameObject("Lines", typeof(RectTransform));
		GClass1664.smethod_3(obj, hideHierarchy);
		obj.AddComponent<ChartItem>();
		RectTransform component = obj.GetComponent<RectTransform>();
		obj.AddComponent<CanvasRenderer>();
		CanvasLines canvasLines = obj.AddComponent<CanvasLines>();
		canvasLines.maskable = true;
		component.SetParent(rectMask.transform, worldPositionStays: false);
		component.localScale = new Vector3(1f, 1f, 1f);
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(0f, 0f);
		component.anchoredPosition = Vector3.zero;
		component.localRotation = Quaternion.identity;
		return canvasLines;
	}

	public override void Update()
	{
		base.Update();
		method_11();
		RectTransform component = GetComponent<RectTransform>();
		if (component != null && component.hasChanged && vector2_0 != component.rect.size)
		{
			Invalidate();
		}
	}

	public void method_8(string cat, int index, BillboardText text)
	{
		if (text.UIText != null)
		{
			text.UIText.maskable = false;
		}
		if (!dictionary_0.TryGetValue(cat, out var value))
		{
			value = new Dictionary<int, BillboardText>(GClass1664.DefaultIntComparer);
			dictionary_0.Add(cat, value);
		}
		value.Add(index, text);
	}

	public override void ClearChart()
	{
		if (gameObject_1 != null)
		{
			gameObject_1.GetComponent<Mask>().enabled = false;
		}
		base.ClearChart();
		dictionary_0.Clear();
		hashSet_1.Clear();
		dictionary_1.Clear();
	}

	public double method_9(double radius, double mag, double min, double max)
	{
		return (max - min) / mag * radius;
	}

	public override void GenerateRealtime()
	{
		if (bool_0)
		{
			return;
		}
		base.GenerateRealtime();
		double minValue = ((IInternalGraphData)Data).GetMinValue(0, false);
		double minValue2 = ((IInternalGraphData)Data).GetMinValue(1, false);
		double maxValue = ((IInternalGraphData)Data).GetMaxValue(0, false);
		double maxValue2 = ((IInternalGraphData)Data).GetMaxValue(1, false);
		double scrollOffset = GetScrollOffset(0);
		double scrollOffset2 = GetScrollOffset(1);
		double num = maxValue - minValue;
		double num2 = maxValue2 - minValue2;
		double x = minValue + scrollOffset + num;
		double y = minValue2 + scrollOffset2 + num2;
		GStruct159 min = new GStruct159(scrollOffset + minValue, scrollOffset2 + minValue2);
		GStruct159 max = new GStruct159(x, y);
		Rect rect = new Rect(0f, 0f, widthRatio, heightRatio);
		Transform parentTransform = base.transform;
		if (gameObject_0 != null)
		{
			parentTransform = gameObject_0.transform;
		}
		foreach (Dictionary<int, BillboardText> value3 in dictionary_0.Values)
		{
			value3.Clear();
		}
		foreach (GraphData.CategoryData category in ((IInternalGraphData)Data).Categories)
		{
			Class1075 value = null;
			if (!dictionary_1.TryGetValue(category.Name, out value))
			{
				continue;
			}
			list_1.Clear();
			list_0.Clear();
			list_0.AddRange(category.getPoints());
			Rect uv;
			int num3 = ClipPoints(list_0, list_1, out uv);
			TransformPoints(list_1, list_2, rect, min, max);
			list_3.Clear();
			int num4 = num3 + list_1.Count;
			foreach (int key2 in value.mCahced.Keys)
			{
				if (key2 < num3 || key2 > num4)
				{
					list_3.Add(key2);
				}
			}
			for (int i = 0; i < list_3.Count; i++)
			{
				value.mCahced.Remove(list_3[i]);
			}
			if (list_0.Count == 0)
			{
				continue;
			}
			if (mItemLabels != null && mItemLabels.isActiveAndEnabled && value.mItemLabels != null)
			{
				Rect rect2 = rect;
				rect2.xMin -= 1f;
				rect2.yMin -= 1f;
				rect2.xMax += 1f;
				rect2.yMax += 1f;
				Class1082 @class = value.mItemLabels;
				@class.Clear();
				for (int j = 0; j < list_2.Count; j++)
				{
					if ((double)list_2[j].w == 0.0)
					{
						continue;
					}
					Vector3 vector = (Vector3)list_2[j] + new Vector3(mItemLabels.Location.Breadth, mItemLabels.Seperation, mItemLabels.Location.Depth);
					if (mItemLabels.Alignment == ChartLabelAlignment.Base)
					{
						vector.y -= list_2[j].y;
					}
					if (rect2.Contains((Vector2)list_2[j]))
					{
						string value2 = null;
						int key = j + num3;
						if (!value.mCahced.TryGetValue(key, out value2))
						{
							GStruct159 gStruct = list_0[j + num3];
							string arg = StringFromAxisFormat(gStruct.x, mHorizontalAxis, mItemLabels.FractionDigits);
							string arg2 = StringFromAxisFormat(gStruct.y, mVerticalAxis, mItemLabels.FractionDigits);
							mItemLabels.TextFormat.Format(stringBuilder_0, $"{arg}:{arg2}", category.Name, "");
							value2 = stringBuilder_0.ToString();
							value.mCahced[key] = value2;
						}
						BillboardText text = @class.AddText(this, mItemLabels.TextPrefab, parentTransform, mItemLabels.FontSize, mItemLabels.FontSharpness, value2, vector.x, vector.y, vector.z, 0f, null);
						method_8(category.Name, j + num3, text);
					}
				}
				@class.DestoryRecycled();
				if (@class.TextObjects != null)
				{
					foreach (BillboardText textObject in @class.TextObjects)
					{
						((IInternalUse)this).InternalTextController.AddText(textObject);
					}
				}
			}
			if (value.mDots != null)
			{
				Rect r = rect;
				float num5 = (float)(category.PointSize * 0.5);
				r.xMin -= num5;
				r.yMin -= num5;
				r.xMax += num5;
				r.yMax += num5;
				value.mDots.SetViewRect(r, uv);
				value.mDots.method_1(list_2);
				value.mDots.refrenceIndex = num3;
			}
			if (value.mLines != null)
			{
				float num6 = 1f;
				if (category.LineTiling.EnableTiling && category.LineTiling.TileFactor > 0f)
				{
					float num7 = 0f;
					for (int k = 1; k < list_2.Count; k++)
					{
						num7 += (list_2[k - 1] - list_2[k]).magnitude;
					}
					num6 = num7 / category.LineTiling.TileFactor;
				}
				if (num6 <= 0.0001f)
				{
					num6 = 1f;
				}
				value.mLines.Tiling = num6;
				value.mLines.SetViewRect(rect, uv);
				value.mLines.method_1(list_2);
				value.mLines.refrenceIndex = num3;
			}
			if (value.mFill != null)
			{
				value.mFill.SetViewRect(rect, uv);
				value.mFill.method_1(list_2);
				value.mFill.refrenceIndex = num3;
			}
		}
	}

	public override void InternalGenerateChart()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		base.InternalGenerateChart();
		ClearChart();
		if (Data == null)
		{
			return;
		}
		GenerateAxis(force: true);
		double minValue = ((IInternalGraphData)Data).GetMinValue(0, false);
		double minValue2 = ((IInternalGraphData)Data).GetMinValue(1, false);
		double maxValue = ((IInternalGraphData)Data).GetMaxValue(0, false);
		double maxValue2 = ((IInternalGraphData)Data).GetMaxValue(1, false);
		double scrollOffset = GetScrollOffset(0);
		double scrollOffset2 = GetScrollOffset(1);
		double num = maxValue - minValue;
		double num2 = maxValue2 - minValue2;
		double x = minValue + scrollOffset + num;
		double y = minValue2 + scrollOffset2 + num2;
		GStruct159 min = new GStruct159(scrollOffset + minValue, scrollOffset2 + minValue2);
		GStruct159 max = new GStruct159(x, y);
		Rect rect = new Rect(0f, 0f, widthRatio, heightRatio);
		int num3 = 0;
		int num4 = ((IInternalGraphData)Data).TotalCategories + 1;
		bool flag = false;
		dictionary_0.Clear();
		hashSet_1.Clear();
		GameObject rectMask = method_6(rect);
		foreach (GraphData.CategoryData category in ((IInternalGraphData)Data).Categories)
		{
			list_1.Clear();
			GStruct159[] array = category.getPoints().ToArray();
			Rect uv;
			int num5 = ClipPoints(array, list_1, out uv);
			TransformPoints(list_1, list_2, rect, min, max);
			if (array.Length == 0 && GClass1664.Boolean_0)
			{
				flag = true;
				int num6 = num4 - 1 - num3;
				float num7 = (float)num6 / (float)num4;
				float num8 = ((float)num6 + 1f) / (float)num4;
				GStruct159 gStruct = interpolateInRect(rect, new GStruct159(0.0, num7, -1.0)).ToDoubleVector3();
				GStruct159 gStruct2 = interpolateInRect(rect, new GStruct159(0.5, num8, -1.0)).ToDoubleVector3();
				GStruct159 gStruct3 = interpolateInRect(rect, new GStruct159(1.0, num7, -1.0)).ToDoubleVector3();
				array = new GStruct159[3] { gStruct, gStruct2, gStruct3 };
				list_2.AddRange(((IEnumerable<GStruct159>)array).Select((Func<GStruct159, Vector4>)((GStruct159 gStruct4) => gStruct4.ToVector3())));
				num3++;
			}
			List<CanvasLines.Class1065> list = new List<CanvasLines.Class1065>();
			list.Add(new CanvasLines.Class1065(list_2));
			Class1075 @class = new Class1075();
			if (category.FillMaterial != null)
			{
				CanvasLines canvasLines = method_7(category, rectMask);
				canvasLines.material = category.FillMaterial;
				canvasLines.refrenceIndex = num5;
				canvasLines.method_2(list);
				canvasLines.SetViewRect(rect, uv);
				canvasLines.MakeFillRender(rect, category.StetchFill);
				@class.mFill = canvasLines;
			}
			if (category.LineMaterial != null)
			{
				CanvasLines canvasLines2 = method_7(category, rectMask);
				float num9 = 1f;
				if (category.LineTiling.EnableTiling && category.LineTiling.TileFactor > 0f)
				{
					float num10 = 0f;
					for (int num11 = 1; num11 < list_2.Count; num11++)
					{
						num10 += (list_2[num11 - 1] - list_2[num11]).magnitude;
					}
					num9 = num10 / category.LineTiling.TileFactor;
				}
				if (num9 <= 0.0001f)
				{
					num9 = 1f;
				}
				canvasLines2.SetViewRect(rect, uv);
				canvasLines2.Thickness = (float)category.LineThickness;
				canvasLines2.Tiling = num9;
				canvasLines2.refrenceIndex = num5;
				canvasLines2.material = category.LineMaterial;
				canvasLines2.SetHoverPrefab(category.LineHoverPrefab);
				canvasLines2.method_2(list);
				@class.mLines = canvasLines2;
			}
			CanvasLines canvasLines3 = (@class.mDots = method_7(category, rectMask));
			canvasLines3.material = category.PointMaterial;
			canvasLines3.method_2(list);
			Rect r = rect;
			float num12 = (float)category.PointSize * 0.5f;
			r.xMin -= num12;
			r.yMin -= num12;
			r.xMax += num12;
			r.yMax += num12;
			canvasLines3.SetViewRect(r, uv);
			canvasLines3.refrenceIndex = num5;
			canvasLines3.SetHoverPrefab(category.PointHoverPrefab);
			if (category.PointMaterial != null)
			{
				canvasLines3.MakePointRender((float)category.PointSize);
			}
			else
			{
				canvasLines3.MakePointRender(0f);
			}
			if (mItemLabels != null && mItemLabels.isActiveAndEnabled)
			{
				Class1082 class2 = new Class1082(forText: true);
				class2.RecycleText = true;
				@class.mItemLabels = class2;
				Rect rect2 = rect;
				rect2.xMin -= 1f;
				rect2.yMin -= 1f;
				rect2.xMax += 1f;
				rect2.yMax += 1f;
				for (int num13 = 0; num13 < list_2.Count; num13++)
				{
					if (list_2[num13].w == 0f)
					{
						continue;
					}
					Vector2 point = list_2[num13];
					if (rect2.Contains(point))
					{
						if (!flag)
						{
							point = Data.GetPoint(category.Name, num13 + num5).ToVector2();
						}
						string arg = StringFromAxisFormat(point.x, mHorizontalAxis, mItemLabels.FractionDigits);
						string arg2 = StringFromAxisFormat(point.y, mVerticalAxis, mItemLabels.FractionDigits);
						Vector3 vector = (Vector3)list_2[num13] + new Vector3(mItemLabels.Location.Breadth, mItemLabels.Seperation, mItemLabels.Location.Depth);
						if (mItemLabels.Alignment == ChartLabelAlignment.Base)
						{
							vector.y -= list_2[num13].y;
						}
						string text = mItemLabels.TextFormat.Format($"{arg}:{arg2}", category.Name, "");
						BillboardText billboardText = class2.AddText(this, mItemLabels.TextPrefab, base.transform, mItemLabels.FontSize, mItemLabels.FontSharpness, text, vector.x, vector.y, vector.z, 0f, null);
						base.TextController.AddText(billboardText);
						method_8(category.Name, num13 + num5, billboardText);
					}
				}
			}
			string catName = category.Name;
			canvasLines3.Hover += delegate(int idx, Vector2 pos)
			{
				method_14(catName, idx, pos);
			};
			canvasLines3.Click += delegate(int idx, Vector2 pos)
			{
				method_13(catName, idx, pos);
			};
			canvasLines3.Leave += delegate
			{
				method_12(catName);
			};
			dictionary_1[catName] = @class;
		}
		base.TextController.transform.SetAsLastSibling();
		method_15();
	}

	public void method_10(Vector2 delta)
	{
		bool flag = false;
		bool_0 = true;
		if (VerticalPanning)
		{
			float num = (float)((IInternalGraphData)Data).GetMinValue(1, false);
			float num2 = (float)((IInternalGraphData)Data).GetMaxValue(1, false) - num;
			base.VerticalScrolling -= delta.y / heightRatio * num2;
			if (Mathf.Abs(delta.y) > 1f)
			{
				flag = true;
			}
		}
		if (HorizontalPanning)
		{
			float num3 = (float)((IInternalGraphData)Data).GetMinValue(0, false);
			float num4 = (float)((IInternalGraphData)Data).GetMaxValue(0, false) - num3;
			base.HorizontalScrolling -= delta.x / widthRatio * num4;
			if (Mathf.Abs(delta.x) > 1f)
			{
				flag = true;
			}
		}
		bool_0 = false;
		if (flag)
		{
			GenerateRealtime();
			if (MousePan != null)
			{
				MousePan.Invoke();
			}
		}
	}

	public void method_11()
	{
		if (!verticalPanning && !horizontalPanning)
		{
			return;
		}
		graphicRaycaster_0 = GetComponentInParent<GraphicRaycaster>();
		if (!(graphicRaycaster_0 == null))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform as RectTransform, Input.mousePosition, graphicRaycaster_0.eventCamera, out var localPoint);
			bool flag = RectTransformUtility.RectangleContainsScreenPoint(base.transform as RectTransform, Input.mousePosition);
			if (Input.GetMouseButton(0) && flag && nullable_0.HasValue)
			{
				Vector2 delta = localPoint - nullable_0.Value;
				method_10(delta);
			}
			nullable_0 = localPoint;
		}
	}

	public void method_12(string category)
	{
		foreach (BillboardText item in hashSet_1)
		{
			if (item == null || item.UIText == null)
			{
				continue;
			}
			ChartItemEffect[] components = item.UIText.GetComponents<ChartItemEffect>();
			foreach (ChartItemEffect chartItemEffect in components)
			{
				if (chartItemEffect != null)
				{
					chartItemEffect.TriggerOut(deactivateOnEnd: false);
				}
			}
		}
		hashSet_1.Clear();
		OnItemLeave(new GClass1667(0, Vector3.zero, new GStruct158(0.0, 0.0), -1f, category, "", ""));
	}

	public void method_13(string category, int idx, Vector2 pos)
	{
		GStruct159 point = Data.GetPoint(category, idx);
		if (!dictionary_0.TryGetValue(category, out var value) || !value.TryGetValue(idx, out var value2))
		{
			return;
		}
		foreach (BillboardText item in hashSet_1)
		{
			if (item == null || item.UIText == null || item.UIText == value2.UIText)
			{
				continue;
			}
			ChartItemEffect[] components = item.UIText.GetComponents<ChartItemEffect>();
			foreach (ChartItemEffect chartItemEffect in components)
			{
				if (chartItemEffect != null)
				{
					chartItemEffect.TriggerOut(deactivateOnEnd: false);
				}
			}
		}
		hashSet_1.Clear();
		Text uIText = value2.UIText;
		if (uIText != null)
		{
			ChartItemEvents component = uIText.GetComponent<ChartItemEvents>();
			if (component != null)
			{
				component.OnMouseDown();
				hashSet_1.Add(value2);
			}
		}
		string xString = StringFromAxisFormat(point.x, mHorizontalAxis);
		string yString = StringFromAxisFormat(point.y, mVerticalAxis);
		OnItemSelected(new GClass1667(idx, pos, point.ToDoubleVector2(), (float)point.z, category, xString, yString));
	}

	public void method_14(string category, int idx, Vector2 pos)
	{
		GStruct159 point = Data.GetPoint(category, idx);
		if (!dictionary_0.TryGetValue(category, out var value) || !value.TryGetValue(idx, out var value2))
		{
			return;
		}
		foreach (BillboardText item in hashSet_1)
		{
			if (item == null || item.UIText == null || item.UIText == value2.UIText)
			{
				continue;
			}
			ChartItemEffect[] components = item.UIText.GetComponents<ChartItemEffect>();
			foreach (ChartItemEffect chartItemEffect in components)
			{
				if (chartItemEffect != null)
				{
					chartItemEffect.TriggerOut(deactivateOnEnd: false);
				}
			}
		}
		hashSet_1.Clear();
		Text uIText = value2.UIText;
		if (uIText != null)
		{
			ChartItemEvents component = uIText.GetComponent<ChartItemEvents>();
			if (component != null)
			{
				component.OnMouseEnter();
				hashSet_1.Add(value2);
			}
		}
		string xString = StringFromAxisFormat(point.x, mHorizontalAxis);
		string yString = StringFromAxisFormat(point.y, mVerticalAxis);
		OnItemHoverted(new GClass1667(idx, pos, point.ToDoubleVector2(), (float)point.z, category, xString, yString));
	}

	public override void OnItemHoverted(object userData)
	{
		base.OnItemHoverted(userData);
		GClass1667 gClass = userData as GClass1667;
		hashSet_0.Add(gClass.Category);
	}

	public override void OnItemSelected(object userData)
	{
		base.OnItemSelected(userData);
		GClass1667 gClass = userData as GClass1667;
		hashSet_0.Add(gClass.Category);
	}

	public override void OnItemLeave(object userData)
	{
		if (userData is GClass1667 { Category: var category })
		{
			hashSet_0.Remove(category);
			hashSet_0.RemoveWhere((string x) => !Data.HasCategory(x));
			if (hashSet_0.Count == 0 && NonHovered != null)
			{
				NonHovered.Invoke();
			}
		}
	}

	public void method_15()
	{
		RectTransform component = GetComponent<RectTransform>();
		GameObject gameObject = (gameObject_0 = new GameObject());
		GClass1664.smethod_3(gameObject, hideHierarchy);
		gameObject.AddComponent<ChartItem>();
		gameObject.transform.position = base.transform.position;
		while (base.gameObject.transform.childCount > 0)
		{
			base.transform.GetChild(0).SetParent(gameObject.transform, worldPositionStays: false);
		}
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		float val = component.rect.size.x / base.WidthRatio;
		float val2 = component.rect.size.y / base.HeightRatio;
		float num = Math.Min(val, val2);
		gameObject.transform.localScale = new Vector3(num, num, num);
		gameObject.transform.localPosition = new Vector3((0f - base.WidthRatio) * num * 0.5f, (0f - base.HeightRatio) * num * 0.5f, 0f);
		vector2_0 = component.rect.size;
	}

	[CompilerGenerated]
	public bool method_16(string x)
	{
		return !Data.HasCategory(x);
	}
}
