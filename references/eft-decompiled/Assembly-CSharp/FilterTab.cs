using EFT.UI;
using UnityEngine;
using UnityEngine.UI;

public class FilterTab : Tab
{
	[SerializeField]
	private Image _filterIcon;

	private RectTransform rectTransform_0;

	public override void OnAwake()
	{
		rectTransform_0 = _filterIcon.GetComponent<RectTransform>();
		base.OnAwake();
	}

	public void SetIcon(Sprite icon)
	{
		if (!(icon == null))
		{
			_filterIcon.sprite = icon;
			if (rectTransform_0 == null)
			{
				rectTransform_0 = _filterIcon.GetComponent<RectTransform>();
			}
			rectTransform_0.sizeDelta = new Vector2(_filterIcon.preferredWidth, _filterIcon.preferredHeight);
		}
	}

	public override void vmethod_0(bool active)
	{
		base.vmethod_0(active);
		GClass3839.ChangeImageAlpha(_filterIcon, active ? 1f : 0.15f);
		GetComponent<ButtonFeedback>().enabled = active;
	}
}
