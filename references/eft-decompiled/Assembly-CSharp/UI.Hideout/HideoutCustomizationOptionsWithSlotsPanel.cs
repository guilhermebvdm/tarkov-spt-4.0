using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

namespace UI.Hideout;

public class HideoutCustomizationOptionsWithSlotsPanel : HideoutCustomizationOptionsPanel
{
	[Serializable]
	[CompilerGenerated]
	public class Class727
	{
		public static readonly Class727 class727_0 = new Class727();

		public static Func<HideoutCustomizationCell, bool> func_0;

		public static Func<AreaData, bool> func_1;

		public bool method_0(HideoutCustomizationCell cell)
		{
			return !cell.IsLocked;
		}

		public bool method_1(AreaData x)
		{
			return x.DisplayInterface;
		}
	}

	[CompilerGenerated]
	public class Class728
	{
		public GClass2421 customizationController;

		public HideoutCustomizationOptionsWithSlotsPanel hideoutCustomizationOptionsWithSlotsPanel_0;

		public void method_0()
		{
			customizationController.ItemSelectionInProcessAction -= hideoutCustomizationOptionsWithSlotsPanel_0.method_0;
		}
	}

	[CompilerGenerated]
	public class Class729
	{
		public EAreaType areaType;

		public bool method_0(AreaData a)
		{
			return a.Template.Type == areaType;
		}
	}

	[CompilerGenerated]
	public class Class730
	{
		public HideoutCustomizationSlotOffer offer;

		public bool method_0(Slot s)
		{
			return s.Name == offer.SlotId;
		}
	}

	[SerializeField]
	private HideoutCustomizationSlotsGroupView _slotsGroup;

	[SerializeField]
	private DefaultUIButton _resetAllButton;

	private Action<ItemAddress, Item> action_0;

	private Func<Item, bool> func_0;

	public void Show(EHideoutCustomizationType customizationType, GClass2421 customizationController, Action<HideoutCustomizationOffer> slotClickHandler)
	{
		action_0 = customizationController.OnSlotItemSelectionChanged;
		func_0 = customizationController.CanSelect;
		_slotClickHandler = slotClickHandler;
		_icons = customizationController.Icons;
		_header.text = GClass2348.Localized($"Hideout/Customization/{customizationType}/WindowHeader");
		customizationController.ItemSelectionInProcessAction += method_0;
		UI.AddDisposable(delegate
		{
			customizationController.ItemSelectionInProcessAction -= method_0;
		});
		_cells.Clear();
		_slotsGroup.transform.SetParent(_allCellsContainer.transform);
		method_2(customizationController.GetSlotOffers(customizationType), customizationController.GetStashSlots());
		_resetAllButton.OnClick.AddListener(method_1);
		UI.AddDisposable(_resetAllButton.OnClick.RemoveAllListeners);
		ShowGameObject();
	}

	public void method_0(bool itemSelectionInProcess)
	{
		foreach (HideoutCustomizationCell item in _cells.Where((HideoutCustomizationCell cell) => !cell.IsLocked))
		{
			item.vmethod_0(!itemSelectionInProcess);
		}
		_slotsGroup.SetInteractable(!itemSelectionInProcess);
	}

	public void method_1()
	{
	}

	public void method_2(IEnumerable<HideoutCustomizationSlotOffer> slotsOffers, Slot[] stashSlots)
	{
		AreaData[] source = Singleton<HideoutClass>.Instance.AreaDatas.Where((AreaData x) => x.DisplayInterface).ToArray();
		GameObject currentCellsRow = CreateNewCellsRowIfNeeded(null);
		Dictionary<EAreaType, List<HideoutCustomizationSlotOffer>> dictionary = smethod_1(slotsOffers);
		Tab tab = null;
		foreach (KeyValuePair<EAreaType, List<HideoutCustomizationSlotOffer>> item in dictionary)
		{
			item.Deconstruct(out var key, out var value);
			EAreaType areaType = key;
			List<HideoutCustomizationSlotOffer> offers = value;
			AreaData areaData = source.FirstOrDefault((AreaData a) => a.Template.Type == areaType);
			if (areaData != null)
			{
				int currentLevel = areaData.CurrentLevel;
				string areaName = areaData.Template.Name;
				currentCellsRow = CreateNewCellsRowIfNeeded(currentCellsRow);
				Dictionary<HideoutCustomizationOffer, Slot> offerSlotAssociations = smethod_0(stashSlots, offers);
				HideoutCustomizationCell hideoutCustomizationCell = CreateCell(offerSlotAssociations, currentCellsRow, areaData.Template.Type.ToString(), areaName, currentLevel <= 0);
				if (!hideoutCustomizationCell.IsLocked && tab == null)
				{
					tab = hideoutCustomizationCell;
				}
			}
		}
		if (tab != null)
		{
			CreateTabGroup(tab);
		}
	}

	public static Dictionary<HideoutCustomizationOffer, Slot> smethod_0(Slot[] stashSlots, IEnumerable<HideoutCustomizationSlotOffer> offers)
	{
		Dictionary<HideoutCustomizationOffer, Slot> dictionary = new Dictionary<HideoutCustomizationOffer, Slot>();
		foreach (HideoutCustomizationSlotOffer offer in offers)
		{
			Slot slot = stashSlots.FirstOrDefault((Slot s) => s.Name == offer.SlotId);
			if (slot != null)
			{
				dictionary.Add(offer, slot);
			}
			else
			{
				Debug.LogError("Could not find stash slot for " + offer.SlotId);
			}
		}
		return dictionary;
	}

	public static Dictionary<EAreaType, List<HideoutCustomizationSlotOffer>> smethod_1(IEnumerable<HideoutCustomizationSlotOffer> slotsOffers)
	{
		Dictionary<EAreaType, List<HideoutCustomizationSlotOffer>> dictionary = new Dictionary<EAreaType, List<HideoutCustomizationSlotOffer>>();
		foreach (HideoutCustomizationSlotOffer slotsOffer in slotsOffers)
		{
			EAreaType areaType = slotsOffer.AreaType;
			if (dictionary.ContainsKey(areaType))
			{
				dictionary[areaType].Add(slotsOffer);
				continue;
			}
			dictionary.Add(areaType, new List<HideoutCustomizationSlotOffer> { slotsOffer });
		}
		return dictionary;
	}

	public override void OnCellSelected(HideoutCustomizationCell selectedCell)
	{
		base.OnCellSelected(selectedCell);
		method_3(selectedCell);
	}

	public void method_3(HideoutCustomizationCell selectedCell)
	{
		_slotsGroup.Close();
		_slotsGroup.transform.SetSiblingIndex(_slotsGroup.transform.parent.childCount - 1);
		int siblingIndex = selectedCell.transform.parent.GetSiblingIndex();
		_slotsGroup.transform.SetSiblingIndex(siblingIndex + 1);
		_slotsGroup.Show(selectedCell.OfferSlotAssociations, action_0, func_0, _slotClickHandler).HandleExceptions();
	}

	public override void Close()
	{
		_slotsGroup.Close();
		_slotsGroup.transform.SetParent(base.transform);
		base.Close();
	}
}
