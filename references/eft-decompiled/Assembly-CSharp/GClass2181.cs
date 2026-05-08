using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass2181 : HashSet<MongoID>
{
	[CompilerGenerated]
	public class Class1408
	{
		public CompoundItem stashItem;

		public bool method_0(CompoundItem s)
		{
			return s.TemplateId == stashItem.TemplateId;
		}
	}

	public readonly global::OnEventClass<Item> OnFavoriteItemsChanged = new global::OnEventClass<Item>();

	public GClass2181(List<MongoID> items)
	{
		foreach (MongoID item in items)
		{
			Add(item);
		}
	}

	public bool TryAddItemToFavorites(Item item, Inventory inventory)
	{
		if (CanAddItem(item, inventory, out var _))
		{
			return method_3(item);
		}
		return false;
	}

	public void method_0(Inventory inventory, string parentId, out int currentItemsCount, out int maxItemsCount, out string error)
	{
		currentItemsCount = 0;
		maxItemsCount = 0;
		error = null;
		EAreaType areaType;
		IEnumerable<CompoundItem> enumerable = method_1(inventory, parentId, out areaType);
		if (enumerable == null)
		{
			Debug.LogError("Area stash wasn't found!");
			return;
		}
		currentItemsCount = enumerable.Sum((CompoundItem stashItem) => GClass3380.GetAllItems(stashItem).Count((Item item) => Contains(item.Id)));
		maxItemsCount = smethod_0(areaType);
		if (currentItemsCount >= maxItemsCount)
		{
			error = method_2(maxItemsCount, areaType);
		}
	}

	public IEnumerable<CompoundItem> method_1(Inventory inventory, string parentId, out EAreaType areaType)
	{
		areaType = EAreaType.Vents;
		List<CompoundItem> list = null;
		foreach (KeyValuePair<EAreaType, CompoundItem> hideoutAreaStash in inventory.HideoutAreaStashes)
		{
			var (eAreaType2, stashItem) = (KeyValuePair<EAreaType, CompoundItem>)(ref hideoutAreaStash);
			if (!(parentId != stashItem.Id))
			{
				areaType = eAreaType2;
				list = new List<CompoundItem>();
				list.AddRange(inventory.HideoutAreaStashes.Values.Where((CompoundItem s) => s.TemplateId == stashItem.TemplateId));
			}
		}
		return list;
	}

	public string method_2(int maxItemsCount, EAreaType areaType)
	{
		switch (areaType)
		{
		case EAreaType.WeaponStand:
		case EAreaType.WeaponStandSecondary:
			return string.Format(GClass2348.Localized("InventoryError/AddToFavorites/NoFreeSpaceForWeapons"), maxItemsCount);
		default:
			return string.Format(GClass2348.Localized("InventoryError/AddToFavorites/NoFreeSpaceForNonWeaponItems"), maxItemsCount);
		case EAreaType.PlaceOfFame:
			return string.Format(GClass2348.Localized("InventoryError/AddToFavorites/NoFreeSpaceForNonWeaponItems"), maxItemsCount);
		}
	}

	public static int smethod_0(EAreaType areaType)
	{
		int result = 0;
		switch (areaType)
		{
		case EAreaType.WeaponStand:
		case EAreaType.WeaponStandSecondary:
			result = Singleton<BackendConfigSettingsClass>.Instance.FavoriteItemsSettings.WeaponStandMaxItemsCount;
			break;
		case EAreaType.PlaceOfFame:
			result = Singleton<BackendConfigSettingsClass>.Instance.FavoriteItemsSettings.PlaceOfFameMaxItemsCount;
			break;
		}
		return result;
	}

	public bool method_3(Item item)
	{
		if (!Add(item.Id))
		{
			return false;
		}
		OnFavoriteItemsChanged?.Invoke(item);
		return true;
	}

	public bool RemoveItemFromFavorites(Item item)
	{
		if (!Remove(item.Id))
		{
			return false;
		}
		OnFavoriteItemsChanged?.Invoke(item);
		return true;
	}

	public bool CanAddItem(Item item, Inventory inventory, out string error)
	{
		method_0(inventory, item.Parent.Container.ParentItem.Id, out var currentItemsCount, out var maxItemsCount, out error);
		return currentItemsCount < maxItemsCount;
	}

	[CompilerGenerated]
	public int method_4(CompoundItem stashItem)
	{
		return GClass3380.GetAllItems(stashItem).Count((Item item) => Contains(item.Id));
	}

	[CompilerGenerated]
	public bool method_5(Item item)
	{
		return Contains(item.Id);
	}
}
