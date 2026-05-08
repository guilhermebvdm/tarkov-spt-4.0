using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerIcons;

[Serializable]
public class PlayerIconImage
{
	[SerializeField]
	public Image _pmcPreview;

	[SerializeField]
	public GameObject _progress;

	[NonSerialized]
	public GClass932 PlayerIconRequest;

	[NonSerialized]
	public GClass929 PresetIcon;

	public void SetPresetIcon(GClass2197 customization, InventoryEquipment equipment)
	{
		PlayerIconRequest = new GClass932(equipment, customization);
		SetPresetIcon(new GClass932(equipment, customization));
	}

	public void SetPresetIcon(Profile profile)
	{
		if (profile == null)
		{
			_pmcPreview.enabled = false;
			_progress.SetActive(value: false);
		}
		else
		{
			SetPresetIcon(profile.Customization, GClass3380.CloneVisibleItem(profile.Inventory.Equipment));
		}
	}

	public void SetPresetIcon(GClass932 iconRequest)
	{
		_pmcPreview.preserveAspect = true;
		PlayerIconRequest = iconRequest;
		PresetIcon = Singleton<GClass927>.Instance.GetIcon(iconRequest);
		method_0();
		if (PresetIcon.Sprite == null)
		{
			PresetIcon.Changed.Bind(method_0);
		}
	}

	public void method_0()
	{
		bool flag = PresetIcon.Sprite != null;
		_progress.SetActive(!flag);
		_pmcPreview.enabled = flag;
		_pmcPreview.sprite = PresetIcon.Sprite;
	}
}
