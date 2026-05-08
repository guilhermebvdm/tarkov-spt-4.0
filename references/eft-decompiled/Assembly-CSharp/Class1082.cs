using System;
using System.Collections.Generic;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class Class1082 : GClass1669
{
	[NonSerialized]
	public VertexHelper VertexHelper_0;

	[NonSerialized]
	public List<UIVertex> List_3;

	[NonSerialized]
	public UIVertex[] Uivertex_0 = new UIVertex[4];

	[NonSerialized]
	public bool Bool_1;

	public Class1082(bool forText)
	{
		Bool_1 = forText;
	}

	public Class1082(VertexHelper wrapAround)
	{
		VertexHelper_0 = wrapAround;
	}

	public override BillboardText AddText(AnyChart chart, Text prefab, Transform parentTransform, int fontSize, float fontScale, string text, float x, float y, float z, float angle, object userData)
	{
		if (Bool_1)
		{
			return base.AddText(chart, prefab, parentTransform, fontSize, fontScale, text, x, y, z, angle, userData);
		}
		return null;
	}

	public override void AddQuad(UIVertex vLeftTop, UIVertex vRightTop, UIVertex vLeftBottom, UIVertex vRightBottom)
	{
		if (List_3 != null)
		{
			List_3.Add(vLeftTop);
			List_3.Add(vRightTop);
			List_3.Add(vRightBottom);
			List_3.Add(vLeftBottom);
		}
		else if (VertexHelper_0 != null)
		{
			Uivertex_0[0] = vLeftTop;
			Uivertex_0[1] = vRightTop;
			Uivertex_0[2] = vRightBottom;
			Uivertex_0[3] = vLeftBottom;
			VertexHelper_0.AddUIVertexQuad(Uivertex_0);
		}
	}

	public override void AddXYRect(Rect rect, int subMeshGroup, float depth)
	{
		Vector2[] array = method_0(rect);
		UIVertex vLeftTop = GClass1664.smethod_14(new Vector3(rect.xMin, rect.yMin, depth), array[0]);
		UIVertex vRightTop = GClass1664.smethod_14(new Vector3(rect.xMax, rect.yMin, depth), array[1]);
		UIVertex vLeftBottom = GClass1664.smethod_14(new Vector3(rect.xMin, rect.yMax, depth), array[2]);
		UIVertex vRightBottom = GClass1664.smethod_14(new Vector3(rect.xMax, rect.yMax, depth), array[3]);
		AddQuad(vLeftTop, vRightTop, vLeftBottom, vRightBottom);
	}

	public override void AddXZRect(Rect rect, int subMeshGroup, float yPosition)
	{
	}

	public override void AddYZRect(Rect rect, int subMeshGroup, float xPosition)
	{
	}
}
