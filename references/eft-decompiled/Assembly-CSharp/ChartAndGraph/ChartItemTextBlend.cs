using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

public class ChartItemTextBlend : ChartItemLerpEffect
{
	private Text text_0;

	private Shadow[] shadow_0;

	private Dictionary<Object, float> dictionary_0 = new Dictionary<Object, float>();

	private CanvasRenderer canvasRenderer_0;

	public override Quaternion Quaternion_0 => Quaternion.identity;

	public override Vector3 Vector3_0 => new Vector3(1f, 1f, 1f);

	public override Vector3 Vector3_1 => Vector3.zero;

	public override void Start()
	{
		base.Start();
		text_0 = GetComponent<Text>();
		shadow_0 = GetComponents<Shadow>();
		Shadow[] array = shadow_0;
		foreach (Shadow shadow in array)
		{
			dictionary_0.Add(shadow, shadow.effectColor.a);
		}
		ApplyLerp(0f);
	}

	public override float GetStartValue()
	{
		if (text_0 != null)
		{
			return text_0.color.a;
		}
		return 0f;
	}

	public CanvasRenderer method_3()
	{
		if (canvasRenderer_0 == null)
		{
			canvasRenderer_0 = GetComponent<CanvasRenderer>();
		}
		return canvasRenderer_0;
	}

	public override void ApplyLerp(float value)
	{
		for (int i = 0; i < shadow_0.Length; i++)
		{
			Shadow shadow = shadow_0[i];
			if (dictionary_0.TryGetValue(shadow, out var value2))
			{
				Color effectColor = shadow.effectColor;
				effectColor.a = Mathf.Lerp(0f, value2, value);
				shadow.effectColor = effectColor;
			}
		}
		if (!(text_0 != null))
		{
			return;
		}
		Color color = text_0.color;
		color.a = Mathf.Clamp(value, 0f, 1f);
		text_0.color = color;
		CanvasRenderer canvasRenderer = method_3();
		if (!(canvasRenderer != null))
		{
			return;
		}
		if (value <= 0f)
		{
			if (!canvasRenderer.cull)
			{
				canvasRenderer.cull = true;
			}
		}
		else if (canvasRenderer.cull)
		{
			canvasRenderer.cull = false;
		}
	}
}
