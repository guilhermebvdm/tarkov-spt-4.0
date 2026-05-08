using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.Quests;
using UnityEngine;

namespace UI.Hideout;

public class HideoutCustomizationSimpleOptionsPanel : HideoutCustomizationOptionsPanel
{
	[CompilerGenerated]
	public class Class731
	{
		public HideoutCustomizationGlobalOffer offer;

		public bool method_0(GClass3679 i)
		{
			return i.Id == offer.ItemId;
		}

		public bool method_1(GClass3679 i)
		{
			return i.Id == offer.ItemId;
		}
	}

	public void Show(EHideoutCustomizationType customizationType, MongoID selectedItemId, IEnumerable<HideoutCustomizationGlobalOffer> globalOffers, HideoutCustomizationIcons icons, IEnumerable<GClass3679> availableCustomizations, Action<HideoutCustomizationOffer> slotClickHandler)
	{
		_slotClickHandler = slotClickHandler;
		_icons = icons;
		_header.text = GClass2348.Localized($"Hideout/Customization/{customizationType}/WindowHeader");
		_cells.Clear();
		method_0(globalOffers, selectedItemId, availableCustomizations);
		ShowGameObject();
	}

	public void method_0(IEnumerable<HideoutCustomizationGlobalOffer> globalOffers, MongoID selectedItemId, IEnumerable<GClass3679> availableCustomizations)
	{
		int num = 0;
		GameObject currentCellsRow = CreateNewCellsRowIfNeeded(null);
		Tab initialTab = null;
		foreach (HideoutCustomizationGlobalOffer offer in globalOffers)
		{
			num++;
			currentCellsRow = CreateNewCellsRowIfNeeded(currentCellsRow);
			GClass3679 gClass = Singleton<CustomizationSolverClass>.Instance.HideoutCustomizationItems.FirstOrDefault((GClass3679 i) => i.Id == offer.ItemId);
			string spriteId = ((gClass != null) ? gClass.Name : "");
			Dictionary<HideoutCustomizationOffer, Slot> offerSlotAssociations = new Dictionary<HideoutCustomizationOffer, Slot> { { offer, null } };
			bool flag = availableCustomizations.Any((GClass3679 i) => i.Id == offer.ItemId);
			HideoutCustomizationCell hideoutCustomizationCell = CreateCell(offerSlotAssociations, currentCellsRow, spriteId, null, !flag && offer.QuestStatus != EQuestStatus.Success, num);
			if (gClass != null && selectedItemId == gClass.Id)
			{
				initialTab = hideoutCustomizationCell;
			}
		}
		CreateTabGroup(initialTab);
	}

	public override void OnCellSelected(HideoutCustomizationCell selectedCell)
	{
		base.OnCellSelected(selectedCell);
		HideoutCustomizationOffer key = selectedCell.OfferSlotAssociations.First().Key;
		_slotClickHandler?.Invoke(key);
	}
}
