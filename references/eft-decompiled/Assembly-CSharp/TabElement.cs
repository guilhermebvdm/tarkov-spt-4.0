using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Diz.Binding;
using EFT.UI;
using UnityEngine;

public class TabElement : UIElement
{
	[SerializeField]
	private List<RectTransform> _sizeDefiners;

	[SerializeField]
	private float _widthAdjustment;

	public int OrderNumber;

	[CompilerGenerated]
	private float float_0;

	private BindableEvent bindableEvent_0;

	private RectTransform rectTransform_1;

	public float Width
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public BindableEvent OnLayoutUpdatedEvent => bindableEvent_0 ?? (bindableEvent_0 = new BindableEvent());

	public void SetWidth(float width, float gap)
	{
		Vector3 localPosition = rectTransform_1.localPosition;
		rectTransform_1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		rectTransform_1.localPosition = new Vector3((width + gap) * (float)OrderNumber, localPosition.y, localPosition.z);
	}

	public float method_0()
	{
		float num = 0f;
		foreach (RectTransform sizeDefiner in _sizeDefiners)
		{
			if (sizeDefiner != null && sizeDefiner.gameObject.activeInHierarchy)
			{
				num = Math.Max(num, sizeDefiner.rect.width);
			}
		}
		return num + _widthAdjustment;
	}

	public void Update()
	{
		float num = method_0();
		if (num != Width)
		{
			Width = num;
			OnLayoutUpdatedEvent.Invoke();
		}
	}

	public void Awake()
	{
		rectTransform_1 = base.gameObject.GetComponent<RectTransform>();
	}

	public void OnDestroy()
	{
		Dispose();
	}
}
