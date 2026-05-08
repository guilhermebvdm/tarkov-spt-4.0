using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

public class WorldSpaceGraphChart : GraphChartBase
{
	[Serializable]
	[CompilerGenerated]
	public class Class1081
	{
		public static readonly Class1081 class1081_0 = new Class1081();

		public static Func<GStruct159, Vector3> func_0;

		public Vector3 method_0(GStruct159 x)
		{
			return x.ToVector3();
		}
	}

	[SerializeField]
	private float _textIdleDistance = 20f;

	[SerializeField]
	[Tooltip("If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered to the main camera")]
	private Camera _textCamera;

	private readonly Dictionary<string, List<BillboardText>> dictionary_0 = new Dictionary<string, List<BillboardText>>();

	private float float_0;

	private GameObject gameObject_0;

	private readonly HashSet<BillboardText> hashSet_0 = new HashSet<BillboardText>();

	public Camera TextCamera
	{
		get
		{
			return _textCamera;
		}
		set
		{
			_textCamera = value;
			OnPropertyUpdated();
		}
	}

	public override Camera TextCameraLink => TextCamera;

	public float TextIdleDistance
	{
		get
		{
			return _textIdleDistance;
		}
		set
		{
			_textIdleDistance = value;
			OnPropertyUpdated();
		}
	}

	public override float TextIdleDistanceLink => TextIdleDistance;

	public override float TotalDepthLink => float_0;

	public override void OnPropertyUpdated()
	{
		base.OnPropertyUpdated();
		Invalidate();
	}

	public GameObject method_6(GraphData.CategoryData data)
	{
		GameObject dotPrefab = data.DotPrefab;
		if (dotPrefab == null)
		{
			if (gameObject_0 == null)
			{
				gameObject_0 = (GameObject)GClass861.Load("Chart And Graph/SelectHandle");
			}
			dotPrefab = gameObject_0;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(dotPrefab);
		GClass1664.smethod_3(gameObject, hideHierarchy);
		if (gameObject.GetComponent<ChartItem>() == null)
		{
			gameObject.AddComponent<ChartItem>();
		}
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		return gameObject;
	}

	public FillPathGenerator method_7(GraphData.CategoryData data)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(data.FillPrefab.gameObject);
		GClass1664.smethod_3(gameObject, hideHierarchy);
		FillPathGenerator component = gameObject.GetComponent<FillPathGenerator>();
		if (gameObject.GetComponent<ChartItem>() == null)
		{
			gameObject.AddComponent<ChartItem>();
		}
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		return component;
	}

	public PathGenerator method_8(GraphData.CategoryData data)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(data.LinePrefab.gameObject);
		GClass1664.smethod_3(gameObject, hideHierarchy);
		PathGenerator component = gameObject.GetComponent<PathGenerator>();
		if (gameObject.GetComponent<ChartItem>() == null)
		{
			gameObject.AddComponent<ChartItem>();
		}
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		return component;
	}

	public override void OnNonHoverted()
	{
		base.OnNonHoverted();
		foreach (BillboardText item in hashSet_0)
		{
			if (!(item.UIText == null))
			{
				ChartItemEffect[] components = item.UIText.GetComponents<ChartItemEffect>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].TriggerOut(deactivateOnEnd: false);
				}
			}
		}
		hashSet_0.Clear();
		if (NonHovered != null)
		{
			NonHovered.Invoke();
		}
	}

	public override void OnItemHoverted(object userData)
	{
		base.OnItemHoverted(userData);
		if (!(userData is GClass1667 gClass))
		{
			return;
		}
		foreach (BillboardText item in hashSet_0)
		{
			if (!(item.UIText == null))
			{
				ChartItemEffect[] components = item.UIText.GetComponents<ChartItemEffect>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].TriggerOut(deactivateOnEnd: false);
				}
			}
		}
		hashSet_0.Clear();
		if (!dictionary_0.TryGetValue(gClass.Category, out var value) || gClass.Index >= value.Count)
		{
			return;
		}
		BillboardText billboardText = value[gClass.Index];
		hashSet_0.Add(billboardText);
		Text uIText = billboardText.UIText;
		if (uIText != null)
		{
			ChartItemEffect[] components = uIText.GetComponents<ChartItemEffect>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].TriggerIn(deactivateOnEnd: false);
			}
		}
	}

	public void method_9(string cat, BillboardText text)
	{
		if (!dictionary_0.TryGetValue(cat, out var value))
		{
			value = new List<BillboardText>();
			dictionary_0.Add(cat, value);
		}
		value.Add(text);
	}

	public override void ClearChart()
	{
		base.ClearChart();
		dictionary_0.Clear();
		hashSet_0.Clear();
	}

	public override void GenerateRealtime()
	{
		base.GenerateRealtime();
		Debug.Log("realtime graph updates are not yet supported for 3d graphs");
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
		double x = (float)((IInternalGraphData)Data).GetMinValue(0, false);
		double y = (float)((IInternalGraphData)Data).GetMinValue(1, false);
		double x2 = (float)((IInternalGraphData)Data).GetMaxValue(0, false);
		double y2 = (float)((IInternalGraphData)Data).GetMaxValue(1, false);
		GStruct159 min = new GStruct159(x, y);
		GStruct159 max = new GStruct159(x2, y2);
		Rect rect = new Rect(0f, 0f, widthRatio, heightRatio);
		int num = 0;
		int num2 = ((IInternalGraphData)Data).TotalCategories + 1;
		double num3 = 0.0;
		double num4 = 0.0;
		bool flag = false;
		dictionary_0.Clear();
		hashSet_0.Clear();
		foreach (GraphData.CategoryData category in ((IInternalGraphData)Data).Categories)
		{
			num4 = Math.Max(category.LineThickness, num4);
			GStruct159[] array = category.getPoints().ToArray();
			TransformPoints(array, rect, min, max);
			if (array.Length == 0 && GClass1664.Boolean_0)
			{
				flag = true;
				int num5 = num2 - 1 - num;
				float num6 = (float)num5 / (float)num2;
				float num7 = ((float)num5 + 1f) / (float)num2;
				GStruct159 gStruct = interpolateInRect(rect, new GStruct159(0.0, num6, -1.0)).ToDoubleVector3();
				GStruct159 gStruct2 = interpolateInRect(rect, new GStruct159(0.5, num7, -1.0)).ToDoubleVector3();
				GStruct159 gStruct3 = interpolateInRect(rect, new GStruct159(1.0, num6, -1.0)).ToDoubleVector3();
				array = new GStruct159[3] { gStruct, gStruct2, gStruct3 };
				num++;
			}
			if (category.Depth > 0.0)
			{
				num3 = Math.Max(num3, category.Depth);
			}
			for (int i = 0; i < array.Length; i++)
			{
				GStruct159 gStruct4 = array[i];
				if (!flag)
				{
					gStruct4 = Data.GetPoint(category.Name, i);
				}
				string text = StringFromAxisFormat(gStruct4.x, mHorizontalAxis);
				string text2 = StringFromAxisFormat(gStruct4.y, mVerticalAxis);
				GClass1667 userData = new GClass1667(i, (array[i] + new GStruct159(0.0, 0.0, category.Depth)).ToVector3(), gStruct4.ToDoubleVector2(), (float)gStruct4.z, category.Name, text, text2);
				GameObject gameObject = method_6(category);
				ChartItemEvents[] componentsInChildren = gameObject.GetComponentsInChildren<ChartItemEvents>();
				foreach (ChartItemEvents chartItemEvents in componentsInChildren)
				{
					if (!(chartItemEvents == null))
					{
						((Interface6)chartItemEvents).Parent = this;
						((Interface6)chartItemEvents).UserData = userData;
					}
				}
				double num8 = array[i].z * category.PointSize;
				if (num8 < 0.0)
				{
					num8 = category.PointSize;
				}
				gameObject.transform.localScale = new GStruct159(num8, num8, num8).ToVector3();
				if (category.PointMaterial != null)
				{
					Renderer component = gameObject.GetComponent<Renderer>();
					if (component != null)
					{
						component.material = category.PointMaterial;
					}
					ChartMaterialController component2 = gameObject.GetComponent<ChartMaterialController>();
					if (component2 != null && component2.ChartDynamicMaterial_0 != null)
					{
						Color hover = component2.ChartDynamicMaterial_0.Hover;
						Color selected = component2.ChartDynamicMaterial_0.Selected;
						component2.ChartDynamicMaterial_0 = new ChartDynamicMaterial(category.PointMaterial, hover, selected);
					}
				}
				GStruct159 gStruct5 = array[i];
				gStruct5.z = category.Depth;
				gameObject.transform.localPosition = gStruct5.ToVector3();
				if (mItemLabels != null && mItemLabels.isActiveAndEnabled)
				{
					Vector3 vector = (array[i] + new GStruct159(mItemLabels.Location.Breadth, mItemLabels.Seperation, (double)mItemLabels.Location.Depth + category.Depth)).ToVector3();
					if (mItemLabels.Alignment == ChartLabelAlignment.Base)
					{
						vector.y -= (float)array[i].y;
					}
					string text3 = mItemLabels.TextFormat.Format($"{text}:{text2}", category.Name, "");
					BillboardText billboardText = GClass1664.smethod_23(null, mItemLabels.TextPrefab, base.transform, text3, vector.x, vector.y, vector.z, 0f, null, hideHierarchy, mItemLabels.FontSize, mItemLabels.FontSharpness);
					base.TextController.AddText(billboardText);
					method_9(category.Name, billboardText);
				}
			}
			for (int k = 0; k < array.Length; k++)
			{
				array[k].z = 0.0;
			}
			Vector3[] path = array.Select((GStruct159 gStruct6) => gStruct6.ToVector3()).ToArray();
			if (category.LinePrefab != null)
			{
				PathGenerator pathGenerator = method_8(category);
				pathGenerator.Generator(path, (float)category.LineThickness, closed: false);
				Vector3 localPosition = pathGenerator.transform.localPosition;
				localPosition.z = (float)category.Depth;
				pathGenerator.transform.localPosition = localPosition;
				if (category.LineMaterial != null)
				{
					Renderer component3 = pathGenerator.GetComponent<Renderer>();
					if (component3 != null)
					{
						component3.material = category.LineMaterial;
					}
					ChartMaterialController component4 = pathGenerator.GetComponent<ChartMaterialController>();
					if (component4 != null && component4.ChartDynamicMaterial_0 != null)
					{
						Color hover2 = component4.ChartDynamicMaterial_0.Hover;
						Color selected2 = component4.ChartDynamicMaterial_0.Selected;
						component4.ChartDynamicMaterial_0 = new ChartDynamicMaterial(category.LineMaterial, hover2, selected2);
					}
				}
			}
			float_0 = (float)(num3 + num4 * 2.0);
			if (!(category.FillPrefab != null))
			{
				continue;
			}
			FillPathGenerator fillPathGenerator = method_7(category);
			Vector3 localPosition2 = fillPathGenerator.transform.localPosition;
			localPosition2.z = (float)category.Depth;
			fillPathGenerator.transform.localPosition = localPosition2;
			if (!(category.LinePrefab == null) && category.LinePrefab is SmoothPathGenerator)
			{
				SmoothPathGenerator smoothPathGenerator = (SmoothPathGenerator)category.LinePrefab;
				fillPathGenerator.SetLineSmoothing(hasParent: true, smoothPathGenerator.JointSmoothing, smoothPathGenerator.JointSize);
			}
			else
			{
				fillPathGenerator.SetLineSmoothing(hasParent: false, 0, 0f);
			}
			fillPathGenerator.SetGraphBounds(rect.yMin, rect.yMax);
			fillPathGenerator.SetStrechFill(category.StetchFill);
			fillPathGenerator.Generator(path, (float)category.LineThickness * 1.01f, closed: false);
			if (category.FillMaterial != null)
			{
				Renderer component5 = fillPathGenerator.GetComponent<Renderer>();
				if (component5 != null)
				{
					component5.material = category.FillMaterial;
				}
				ChartMaterialController component6 = fillPathGenerator.GetComponent<ChartMaterialController>();
				if (component6 != null && component6.ChartDynamicMaterial_0 != null)
				{
					Color hover3 = component6.ChartDynamicMaterial_0.Hover;
					Color selected3 = component6.ChartDynamicMaterial_0.Selected;
					component6.ChartDynamicMaterial_0 = new ChartDynamicMaterial(category.FillMaterial, hover3, selected3);
				}
			}
		}
		GenerateAxis(force: true);
	}
}
