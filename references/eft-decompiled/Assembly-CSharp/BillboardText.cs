using System;
using System.Runtime.CompilerServices;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class BillboardText : MonoBehaviour
{
	private RectTransform rectTransform_0;

	[CompilerGenerated]
	private Text text_0;

	internal TextDirection textDirection_0;

	public RectTransform RectTransformOverride;

	[CompilerGenerated]
	private object object_0;

	[NonSerialized]
	public float Scale = 1f;

	public bool parentSet;

	public RectTransform parent;

	public bool Recycled;

	private CanvasRenderer[] canvasRenderer_0;

	public Text UIText
	{
		[CompilerGenerated]
		get
		{
			return text_0;
		}
		[CompilerGenerated]
		set
		{
			text_0 = value;
		}
	}

	public object UserData
	{
		[CompilerGenerated]
		get
		{
			return object_0;
		}
		[CompilerGenerated]
		set
		{
			object_0 = value;
		}
	}

	public RectTransform Rect
	{
		get
		{
			if (UIText == null)
			{
				return null;
			}
			if (RectTransformOverride != null)
			{
				return RectTransformOverride;
			}
			if (rectTransform_0 == null)
			{
				rectTransform_0 = UIText.GetComponent<RectTransform>();
			}
			return rectTransform_0;
		}
	}

	public void SetVisible(bool visible)
	{
		bool cull = !visible;
		RectTransform rect = Rect;
		if (!(rect == null))
		{
			if (canvasRenderer_0 == null)
			{
				canvasRenderer_0 = rect.GetComponentsInChildren<CanvasRenderer>();
			}
			for (int i = 0; i < canvasRenderer_0.Length; i++)
			{
				canvasRenderer_0[i].cull = cull;
			}
		}
	}
}
