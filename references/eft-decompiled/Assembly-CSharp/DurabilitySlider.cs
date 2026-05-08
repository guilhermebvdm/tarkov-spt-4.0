using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.UI;

public class DurabilitySlider : Slider
{
	[SerializeField]
	private Image _background;

	[SerializeField]
	private Sprite _backgroundDangerZone;

	[SerializeField]
	private Sprite _backgroundDefault;

	[SerializeField]
	private Image _fill;

	[SerializeField]
	private Image _fillWear;

	[SerializeField]
	private Sprite _fillDangerZone;

	[SerializeField]
	private Sprite _fillDefault;

	[SerializeField]
	private GameObject _glow;

	[SerializeField]
	private GameObject _repairRequiredIcon;

	[CompilerGenerated]
	private float float_0;

	private bool bool_0;

	public bool ShowDurability
	{
		set
		{
			bool_0 = value;
			base.gameObject.SetActive(bool_0);
			_repairRequiredIcon.SetActive(bool_0);
			if (bool_0)
			{
				method_2(this.value <= Single_0);
			}
		}
	}

	public float Single_0
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

	public override void Awake()
	{
		base.Awake();
		base.interactable = false;
		_background.sprite = _backgroundDefault;
		_fill.sprite = _fillDefault;
	}

	public void method_0(RepairableComponent repairable, float durabilityThreshold)
	{
		base.maxValue = repairable.TemplateDurability;
		Single_0 = durabilityThreshold;
		_fillWear.fillAmount = ((float)repairable.TemplateDurability - repairable.MaxDurability) / 100f;
		method_1(repairable.Durability);
	}

	public void method_1(float durability)
	{
		value = durability;
		if (bool_0)
		{
			method_2(durability <= Single_0);
		}
		else if (_repairRequiredIcon != null)
		{
			_repairRequiredIcon.gameObject.SetActive(value: false);
		}
	}

	public void method_2(bool inDangerZone)
	{
		if (_glow != null)
		{
			_glow.gameObject.SetActive(inDangerZone);
		}
		if (_repairRequiredIcon != null)
		{
			_repairRequiredIcon.gameObject.SetActive(inDangerZone);
		}
		_background.sprite = (inDangerZone ? _backgroundDangerZone : _backgroundDefault);
		_fill.sprite = (inDangerZone ? _fillDangerZone : _fillDefault);
	}
}
