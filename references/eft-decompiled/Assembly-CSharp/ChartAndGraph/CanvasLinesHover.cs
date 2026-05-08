using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

public class CanvasLinesHover : MaskableGraphic
{
	private float float_0;

	private UIVertex[] uivertex_0 = new UIVertex[4];

	public void Init(float thickness)
	{
		float_0 = thickness * 0.5f;
		SetAllDirty();
		Rebuild(CanvasUpdate.PreRender);
	}

	public IEnumerable<UIVertex> method_0()
	{
		UIVertex uIVertex = new UIVertex
		{
			position = new Vector3(0f - float_0, 0f - float_0),
			uv0 = new Vector2(0f, 0f)
		};
		yield return uIVertex;
		uIVertex.position = new Vector3(0f - float_0, float_0);
		uIVertex.uv0 = new Vector2(0f, 1f);
		yield return uIVertex;
		uIVertex.position = new Vector3(float_0, float_0);
		uIVertex.uv0 = new Vector2(1f, 1f);
		yield return uIVertex;
		uIVertex.position = new Vector3(float_0, 0f - float_0);
		uIVertex.uv0 = new Vector2(1f, 0f);
		yield return uIVertex;
	}

	public override void OnPopulateMesh(VertexHelper vh)
	{
		base.OnPopulateMesh(vh);
		vh.Clear();
		int num = 0;
		foreach (UIVertex item in method_0())
		{
			uivertex_0[num++] = item;
			if (num == 4)
			{
				UIVertex uIVertex = uivertex_0[2];
				uivertex_0[2] = uivertex_0[3];
				uivertex_0[3] = uIVertex;
				num = 0;
				vh.AddUIVertexQuad(uivertex_0);
			}
		}
	}

	public override void OnPopulateMesh(Mesh m)
	{
		GClass1670 gClass = new GClass1670(1);
		int num = 0;
		foreach (UIVertex item in method_0())
		{
			uivertex_0[num++] = item;
			if (num == 4)
			{
				num = 0;
				gClass.AddQuad(uivertex_0[0], uivertex_0[1], uivertex_0[2], uivertex_0[3]);
			}
		}
		gClass.ApplyToMesh(m);
	}
}
