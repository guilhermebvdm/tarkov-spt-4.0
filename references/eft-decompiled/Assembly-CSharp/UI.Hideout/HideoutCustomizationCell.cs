using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.Quests;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Hideout;

public class HideoutCustomizationCell : Tab
{
	[Serializable]
	[CompilerGenerated]
	public class Class726
	{
		public static readonly Class726 class726_0 = new Class726();

		public static Func<Slot, bool> func_0;

		public bool method_0(Slot s)
		{
			if (s != null)
			{
				return s.ContainedItem != null;
			}
			return false;
		}
	}

	[Space(10f)]
	[SerializeField]
	private Image _imageIdle;

	[SerializeField]
	private Image _imageSelected;

	[SerializeField]
	private TMP_Text _name;

	[Space(10f)]
	[SerializeField]
	private Image _lockedImage;

	[SerializeField]
	private Image _lockedBorderImage;

	[Space(10f)]
	[SerializeField]
	private GameObject _cellNumberObject;

	[SerializeField]
	private Image _cellNumberImage;

	[SerializeField]
	private Sprite _cellNumberIdleSprite;

	[SerializeField]
	private Sprite _cellNumberSelectedSprite;

	[SerializeField]
	private TMP_Text _cellNumberText;

	[Space(10f)]
	[SerializeField]
	private TMP_Text _slotsNumberText;

	private TaskConditionsTooltip taskConditionsTooltip_0;

	private bool bool_1;

	[CompilerGenerated]
	private bool bool_2;

	[CompilerGenerated]
	private Dictionary<HideoutCustomizationOffer, Slot> dictionary_0;

	public bool IsLocked
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public Dictionary<HideoutCustomizationOffer, Slot> OfferSlotAssociations
	{
		[CompilerGenerated]
		get
		{
			return dictionary_0;
		}
		[CompilerGenerated]
		set
		{
			dictionary_0 = value;
		}
	}

	public void Show(Dictionary<HideoutCustomizationOffer, Slot> offerSlotAssociations, Sprite sprite = null, string cellName = null, bool isLocked = false, int? cellNumber = null)
	{
		OfferSlotAssociations = offerSlotAssociations;
		if (_cellNumberObject != null)
		{
			_cellNumberText.text = (cellNumber.HasValue ? cellNumber.Value.ToString() : "-1");
			_cellNumberObject.SetActive(cellNumber.HasValue);
		}
		if (sprite != null)
		{
			if (_imageIdle != null)
			{
				_imageIdle.sprite = sprite;
			}
			if (_imageSelected != null)
			{
				_imageSelected.sprite = sprite;
			}
		}
		if (!GClass856.IsNullOrEmpty(cellName))
		{
			_name.text = cellName;
		}
		SetLocked(isLocked);
		method_2();
		method_4();
		taskConditionsTooltip_0 = ItemUiContext.Instance.TaskConditionsTooltip;
		base.gameObject.SetActive(value: true);
	}

	public void method_1()
	{
		if (OfferSlotAssociations.Count <= 0 || _slotsNumberText == null)
		{
			return;
		}
		foreach (var (_, slot2) in OfferSlotAssociations)
		{
			if (slot2 != null)
			{
				slot2.OnAddOrRemoveItem -= method_3;
			}
		}
	}

	public void method_2()
	{
		if (OfferSlotAssociations.Count <= 0 || _slotsNumberText == null)
		{
			return;
		}
		foreach (var (_, slot2) in OfferSlotAssociations)
		{
			if (slot2 != null)
			{
				slot2.OnAddOrRemoveItem += method_3;
			}
		}
	}

	public void method_3(Item _)
	{
		method_4();
	}

	public override void Init(GInterface486 controller)
	{
		base.Init(controller);
		Hover(isHovered: false);
		UpdateVisual(selected: false);
		SetShaded(shaded: false);
	}

	public override void Select(bool sendCallback = true, bool uiOnly = false)
	{
		base.Select(sendCallback, uiOnly);
		if (_cellNumberObject != null)
		{
			_cellNumberImage.sprite = _cellNumberSelectedSprite;
		}
		SetShaded(shaded: false);
	}

	public override async Task<bool> Deselect()
	{
		if (_cellNumberObject != null)
		{
			_cellNumberImage.sprite = _cellNumberIdleSprite;
		}
		return await base.Deselect();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		HideoutCustomizationOffer key = OfferSlotAssociations.First().Key;
		if (IsLocked)
		{
			if (OfferSlotAssociations.Count > 0 && key.QuestStatus != EQuestStatus.Success)
			{
				taskConditionsTooltip_0.Show((key is HideoutCustomizationGlobalOffer) ? key.Title : _name.text, key.ProgressCheckers, key.Conditions[EQuestStatus.AvailableForFinish], key.QuestStatus == EQuestStatus.Success);
			}
		}
		else if (key is HideoutCustomizationGlobalOffer)
		{
			taskConditionsTooltip_0.Show(key.Title, null, null, isConditionalDone: true);
		}
		if (Interactable)
		{
			base.OnPointerEnter(eventData);
			if (bool_1)
			{
				Shade(shade: false);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		taskConditionsTooltip_0.Close();
		if (Interactable)
		{
			base.OnPointerExit(eventData);
			if (bool_1)
			{
				Shade(shade: true);
			}
		}
	}

	public override void HandlePointerClick(bool isSelectedNow)
	{
		if (Interactable)
		{
			base.HandlePointerClick(isSelectedNow);
		}
	}

	public void SetLocked(bool isLocked)
	{
		IsLocked = isLocked;
		_lockedImage.gameObject.SetActive(isLocked);
		_lockedBorderImage.gameObject.SetActive(isLocked);
		if (_name != null)
		{
			_name.gameObject.SetActive(!isLocked);
		}
		if (_slotsNumberText != null)
		{
			_slotsNumberText.gameObject.SetActive(!isLocked);
		}
		vmethod_0(!isLocked);
	}

	public void SetShaded(bool shaded)
	{
		if (!IsLocked)
		{
			bool_1 = shaded;
			Shade(bool_1);
		}
	}

	public void Shade(bool shade)
	{
		_canvasGroup.alpha = (shade ? 0.7f : 1f);
	}

	public void method_4()
	{
		if (OfferSlotAssociations.Count > 0 && _slotsNumberText != null)
		{
			int num = OfferSlotAssociations.Values.Count((Slot s) => s != null && s.ContainedItem != null);
			_slotsNumberText.text = $"{num} / {OfferSlotAssociations.Values.Count}";
		}
	}

	public void Close()
	{
		method_1();
		taskConditionsTooltip_0.Close();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	public Task<bool> method_5()
	{
		return base.Deselect();
	}
}
