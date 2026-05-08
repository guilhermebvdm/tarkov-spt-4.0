using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using JetBrains.Annotations;

public abstract class GClass928
{
	[Serializable]
	[CompilerGenerated]
	public class Class571
	{
		public static readonly Class571 class571_0 = new Class571();

		public static Func<int, int> func_0;

		public int method_0(int subHash)
		{
			return subHash;
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_0 = "WHAT".GetHashCode();

	public static int Default
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public static int GetClothingHash(GClass3677 clothing)
	{
		return 0x11 ^ clothing.Name.GetHashCode() ^ clothing.Prefab.path.GetHashCode();
	}

	public static int GetItemHash(Item item)
	{
		int num = 17;
		foreach (int item2 in from subHash in smethod_0(item)
			orderby subHash
			select subHash)
		{
			num ^= item2;
		}
		if (item is AmmoItemClass ammoItemClass)
		{
			num ^= 27 * (ammoItemClass.IsUsed ? 42 : 56);
		}
		return num;
	}

	public static IEnumerable<int> smethod_0(Item topLevelItem, int hashSeed = 1)
	{
		if (topLevelItem != null)
		{
			yield return smethod_1(topLevelItem) * hashSeed;
		}
		if (!(topLevelItem is GClass3248 gClass) || topLevelItem.HideEntrails)
		{
			yield break;
		}
		hashSeed *= 6529;
		foreach (EFT.InventoryLogic.IContainer container in gClass.Containers)
		{
			int num = 0;
			foreach (Item item in container.Items)
			{
				ItemAddress currentAddress = item.CurrentAddress;
				int num2 = hashSeed ^ currentAddress.GetHashSum();
				if (currentAddress is GClass3392)
				{
					int num3 = num + 1;
					num = num3;
					num2 ^= 2879 * num;
				}
				foreach (int item2 in smethod_0(item, num2))
				{
					yield return item2;
				}
			}
		}
	}

	public static int smethod_1([CanBeNull] Item item)
	{
		int num = 0;
		if (item == null)
		{
			return num;
		}
		num ^= item.TemplateId.GetHashCode();
		TogglableComponent itemComponent = item.GetItemComponent<TogglableComponent>();
		if (itemComponent != null)
		{
			num ^= 23 + (itemComponent.On ? 1 : 0);
		}
		FoldableComponent itemComponent2 = item.GetItemComponent<FoldableComponent>();
		if (itemComponent2 != null)
		{
			num ^= 23 + (itemComponent2.Folded ? 1 : 0) << 1;
		}
		if (item is MagazineItemClass magazineItemClass)
		{
			int maxVisibleAmmo = magazineItemClass.GetMaxVisibleAmmo();
			num ^= 23 + maxVisibleAmmo << 2;
		}
		return num;
	}
}
