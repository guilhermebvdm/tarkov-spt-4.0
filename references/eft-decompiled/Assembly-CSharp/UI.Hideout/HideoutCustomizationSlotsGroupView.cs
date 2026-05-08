using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

namespace UI.Hideout;

public class HideoutCustomizationSlotsGroupView : UIElement
{
	[CompilerGenerated]
	public class Class732
	{
		public HideoutCustomizationSlotsGroupView hideoutCustomizationSlotsGroupView_0;

		public Action<HideoutCustomizationOffer> slotClickHandler;

		public Func<Item, bool> func_0;

		public bool method_0(Item itemToTest)
		{
			return hideoutCustomizationSlotsGroupView_0.method_1(itemToTest);
		}
	}

	[CompilerGenerated]
	public class Class733
	{
		public Slot slot;

		public HideoutCustomizationOffer offer;

		public Class732 class732_0;

		public void method_0(Item selectedItem)
		{
			class732_0.hideoutCustomizationSlotsGroupView_0.method_0(slot.CreateItemAddress(), selectedItem);
		}

		public void method_1()
		{
			class732_0.slotClickHandler?.Invoke(offer);
		}
	}

	[CompilerGenerated]
	public class Class734
	{
		public HideoutCustomizationSlotItemSelectionCell selector;

		public void method_0()
		{
			selector.Close();
			UnityEngine.Object.Destroy(selector.gameObject);
		}
	}

	[SerializeField]
	private HideoutCustomizationSlotItemSelectionCell _itemSelectionCellTemplate;

	[SerializeField]
	private Transform _cellsContainer;

	private List<ItemSelectionCell> list_0;

	private Action<ItemAddress, Item> action_0;

	private Func<Item, bool> func_0;

	public async Task Show(Dictionary<HideoutCustomizationOffer, Slot> offerSlotAssociation, Action<ItemAddress, Item> onSlotItemSelectionChangedAction, Func<Item, bool> canSelect, Action<HideoutCustomizationOffer> slotClickHandler)
	{
		if (offerSlotAssociation.Count == 0)
		{
			return;
		}
		ShowGameObject();
		action_0 = onSlotItemSelectionChangedAction;
		func_0 = canSelect;
		list_0 = new List<ItemSelectionCell>(offerSlotAssociation.Count);
		foreach (KeyValuePair<HideoutCustomizationOffer, Slot> item in offerSlotAssociation)
		{
			item.Deconstruct(out var key, out var value);
			HideoutCustomizationOffer offer = key;
			Slot slot = value;
			if (!(_itemSelectionCellTemplate == null) && !(this == null))
			{
				Item containedItem = slot.ContainedItem;
				HideoutCustomizationSlotItemSelectionCell selector = UnityEngine.Object.Instantiate(_itemSelectionCellTemplate, _cellsContainer);
				selector.Show(containedItem, (Item itemToTest) => method_1(itemToTest), delegate(Item selectedItem)
				{
					method_0(slot.CreateItemAddress(), selectedItem);
				}, delegate
				{
					slotClickHandler?.Invoke(offer);
				}, slot.Filters[0].Filter, offer);
				list_0.Add(selector);
				UI.AddDisposable(delegate
				{
					selector.Close();
					UnityEngine.Object.Destroy(selector.gameObject);
				});
				await Task.Yield();
				continue;
			}
			return;
		}
	}

	public void SetInteractable(bool interactable)
	{
		foreach (ItemSelectionCell item in list_0)
		{
			item.SetItemsSetAbility(interactable);
		}
	}

	public void method_0(ItemAddress address, Item selectedItem)
	{
		action_0?.Invoke(address, selectedItem);
	}

	public bool method_1(Item itemToTest)
	{
		bool? flag = func_0?.Invoke(itemToTest);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		return false;
	}
}
