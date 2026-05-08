using System;
using System.Collections.Generic;
using EFT.UI;
using UnityEngine;

public class TabSizeController : UIElement
{
	[SerializeField]
	private float _gap;

	private List<TabElement> list_0 = new List<TabElement>();

	public void UpdateLayout()
	{
		float num = 0f;
		foreach (TabElement item in list_0)
		{
			num = Math.Max(num, item.Width);
		}
		foreach (TabElement item2 in list_0)
		{
			item2.SetWidth(num, _gap);
		}
	}

	public void Awake()
	{
		foreach (Transform item in base.transform)
		{
			TabElement component = item.gameObject.GetComponent<TabElement>();
			if (!(component == null))
			{
				list_0.Add(component);
				UI.AddDisposable(component.OnLayoutUpdatedEvent.Subscribe(UpdateLayout));
			}
		}
	}

	public void OnDestroy()
	{
		Dispose();
	}
}
