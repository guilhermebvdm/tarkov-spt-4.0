using System;
using System.Threading.Tasks;
using EFT.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AchievementsSystem;

public class AchievementIconView : UIElement
{
	[SerializeField]
	private Sprite _commonIconBorder;

	[SerializeField]
	private Sprite _rareIconBorder;

	[SerializeField]
	private Sprite _legendaryIconBorder;

	[SerializeField]
	private Sprite _commonIconBackground;

	[SerializeField]
	private Sprite _rareIconBackground;

	[SerializeField]
	private Sprite _legendaryIconBackground;

	[SerializeField]
	private HoverTrigger _iconHoverTrigger;

	[SerializeField]
	private Image _iconImage;

	[SerializeField]
	private Image _iconBorder;

	[SerializeField]
	private Image _iconBackground;

	[SerializeField]
	private CanvasGroup _iconCanvasGroup;

	[Space]
	[SerializeField]
	private float _notCompletedAlpha = 0.25f;

	public void Show(bool unlocked, GClass4061 template, IBackEndSession session, Action<PointerEventData> onHoverStart = null, Action<PointerEventData> onHoverEnd = null)
	{
		ShowGameObject();
		method_0(unlocked, template, session);
		if (onHoverStart != null && onHoverEnd != null)
		{
			_iconHoverTrigger.Init(onHoverStart, onHoverEnd);
		}
	}

	public void method_0(bool unlocked, GClass4061 template, IBackEndSession session)
	{
		if (template.Sprite == null)
		{
			method_1(template, session).HandleExceptions();
		}
		else
		{
			_iconImage.sprite = template.Sprite;
		}
		switch (template.Rarity)
		{
		case EAchievementRarity.Common:
			_iconBackground.sprite = _commonIconBackground;
			_iconBorder.sprite = _commonIconBorder;
			break;
		case EAchievementRarity.Rare:
			_iconBackground.sprite = _rareIconBackground;
			_iconBorder.sprite = _rareIconBorder;
			break;
		case EAchievementRarity.Legendary:
			_iconBackground.sprite = _legendaryIconBackground;
			_iconBorder.sprite = _legendaryIconBorder;
			break;
		}
		_iconCanvasGroup.alpha = (unlocked ? 1f : _notCompletedAlpha);
	}

	public async Task method_1(GClass4061 template, GInterface221 session)
	{
		await template.LoadIconSprite(session);
		if (_iconImage != null)
		{
			_iconImage.sprite = template.Sprite;
			_iconImage.enabled = template.Sprite != null;
		}
	}
}
