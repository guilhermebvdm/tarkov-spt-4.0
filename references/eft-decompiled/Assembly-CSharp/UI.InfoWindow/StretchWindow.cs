using System.Collections.Generic;
using EFT.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.InfoWindow;

public class StretchWindow : UIElement
{
	[SerializeField]
	private Vector2 _maxSize;

	[SerializeField]
	private LayoutElement _stretchableObject;

	[SerializeField]
	private RectTransform _rootTransform;

	[SerializeField]
	private Dictionary<StretchArea.EStretchAreaType, StretchArea> _stretchAreas;

	private ECursorType? nullable_0;

	private ECursorType? nullable_1;

	public void OnValidate()
	{
		if (base.transform.GetSiblingIndex() != 0)
		{
			base.transform.SetSiblingIndex(0);
		}
	}

	public void Start()
	{
		foreach (var (areaType, stretchArea2) in _stretchAreas)
		{
			stretchArea2.Init(areaType, _rootTransform, _stretchableObject, _maxSize);
			UI.AddDisposable(stretchArea2.OnCursorChange.Subscribe(method_1));
			UI.AddDisposable(stretchArea2.OnForcedCursorChange.Subscribe(method_0));
		}
	}

	public void OnDestroy()
	{
		UI.Dispose();
	}

	public void method_0(ECursorType? cursor)
	{
		nullable_0 = cursor;
		method_2();
	}

	public void method_1(ECursorType? cursor)
	{
		nullable_1 = cursor;
		method_2();
	}

	public void method_2()
	{
		GClass3746.SetCursor(nullable_0 ?? nullable_1.GetValueOrDefault());
	}
}
