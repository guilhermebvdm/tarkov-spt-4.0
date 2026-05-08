using System;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutGroup : LayoutGroup
{
	[SerializeField]
	private float _fDistance;

	[Range(0f, 360f)]
	[SerializeField]
	private float _minAngle;

	[Range(0f, 360f)]
	[SerializeField]
	private float _maxAngle;

	[Range(0f, 360f)]
	public float StartAngle;

	public override void OnEnable()
	{
		base.OnEnable();
		method_0();
	}

	public override void SetLayoutHorizontal()
	{
		method_0();
	}

	public override void SetLayoutVertical()
	{
		method_0();
	}

	public override void CalculateLayoutInputVertical()
	{
		method_0();
	}

	public override void CalculateLayoutInputHorizontal()
	{
		method_0();
	}

	public void method_0()
	{
		m_Tracker.Clear();
		if (GClass949.ActiveChildCount(base.transform) == 0)
		{
			return;
		}
		float num = (_maxAngle - _minAngle) / (float)(GClass949.ActiveChildCount(base.transform) - 1);
		float num2 = StartAngle;
		for (int i = 0; i < GClass949.ActiveChildCount(base.transform); i++)
		{
			RectTransform rectTransform = (RectTransform)base.transform.GetChild(i);
			if (rectTransform != null)
			{
				m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot);
				Vector3 vector = new Vector3(Mathf.Cos(num2 * (MathF.PI / 180f)), Mathf.Sin(num2 * (MathF.PI / 180f)), 0f);
				rectTransform.localPosition = vector * _fDistance;
				Vector2 vector2 = (rectTransform.pivot = new Vector2(0.5f, 0.5f));
				Vector2 anchorMin = (rectTransform.anchorMax = vector2);
				rectTransform.anchorMin = anchorMin;
				num2 += num;
			}
		}
	}
}
