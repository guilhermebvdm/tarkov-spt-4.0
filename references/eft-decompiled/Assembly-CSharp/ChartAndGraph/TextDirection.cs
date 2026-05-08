using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

[ExecuteInEditMode]
public class TextDirection : MonoBehaviour
{
	public Material PointMaterial;

	public Material LineMaterial;

	public float Length = 20f;

	public float Gap = 5f;

	public float Thickness = 2f;

	public float PointSize = 10f;

	public CanvasLines Lines;

	public CanvasLines Point;

	public Text Text;

	private Transform transform_0;

	private Transform transform_1;

	private TextController textController_0;

	public void SetTextController(TextController control)
	{
		textController_0 = control;
	}

	public void SetRelativeTo(Transform from, Transform to)
	{
		transform_0 = to;
		transform_1 = from;
	}

	public void LateUpdate()
	{
		if (!(transform_1 == null) && !(transform_0 == null) && !(textController_0 == null) && !(textController_0.Camera == null))
		{
			Vector3 vector = (transform_0.position - transform_1.position).normalized * Length;
			vector = Quaternion.Inverse(textController_0.Camera.transform.rotation) * vector;
			method_0(vector);
		}
	}

	public void SetDirection(float angle)
	{
		method_0(GClass1664.smethod_10(angle, Length));
	}

	public void method_0(Vector3 dir)
	{
		float num = Mathf.Sign(dir.x);
		Vector3 vector = new Vector3(1f, 0f, 0f) * num * Length;
		Vector3 vector2 = new Vector3(1f, 0f, 0f) * num * Gap;
		if (LineMaterial != null)
		{
			List<CanvasLines.Class1065> list = new List<CanvasLines.Class1065>();
			list.Add(new CanvasLines.Class1065(new Vector3[3]
			{
				Vector3.zero,
				dir,
				dir + vector
			}));
			List<CanvasLines.Class1065> lines = list;
			Lines.Thickness = Thickness;
			Lines.Tiling = 1f;
			Lines.material = LineMaterial;
			Lines.method_2(lines);
		}
		if (PointMaterial != null)
		{
			List<CanvasLines.Class1065> list = new List<CanvasLines.Class1065>();
			list.Add(new CanvasLines.Class1065(new Vector3[1] { Vector3.zero }));
			List<CanvasLines.Class1065> lines2 = list;
			Point.MakePointRender(PointSize);
			Point.material = PointMaterial;
			Point.method_2(lines2);
		}
		Vector2 vector3 = new Vector2(0.5f, 0.5f);
		Vector2 pivot = new Vector2((num > 0f) ? 0f : 1f, 0.5f);
		Text.rectTransform.anchorMin = vector3;
		Text.rectTransform.anchorMax = vector3;
		Text.rectTransform.pivot = pivot;
		Text.alignment = ((num > 0f) ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
		Text.rectTransform.anchoredPosition = dir + vector + vector2;
	}
}
