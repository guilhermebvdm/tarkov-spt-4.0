using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;

public abstract class GClass922
{
	public static EClippingCustoms CalculateRules(InventoryEquipment equipment)
	{
		EClippingCustoms clippingCustoms = (EClippingCustoms)0;
		EquipmentSlot[] slotNames = PlayerBody.SlotNames;
		foreach (EquipmentSlot equipmentSlot in slotNames)
		{
			Slot slot = equipment.GetSlot(equipmentSlot);
			if (slot.ContainedItem != null)
			{
				switch (equipmentSlot)
				{
				case EquipmentSlot.FaceCover:
					smethod_1(ref clippingCustoms, slot.ContainedItem);
					break;
				case EquipmentSlot.Eyewear:
					smethod_0(ref clippingCustoms);
					break;
				case EquipmentSlot.Headwear:
					smethod_2(ref clippingCustoms, slot.ContainedItem);
					break;
				case EquipmentSlot.Earpiece:
					clippingCustoms |= EClippingCustoms.Hat;
					break;
				}
			}
		}
		return clippingCustoms;
	}

	public static void smethod_0(ref EClippingCustoms clippingCustoms)
	{
		clippingCustoms |= EClippingCustoms.FaceCover | EClippingCustoms.Hat;
	}

	public static void smethod_1(ref EClippingCustoms clippingCustoms, Item containedItem)
	{
		clippingCustoms |= EClippingCustoms.Hat;
		clippingCustoms |= EClippingCustoms.Helmet;
		if (containedItem is FaceCoverItemClass { Template: FaceCoverTemplateClass { IsHalfMask: false } })
		{
			clippingCustoms |= EClippingCustoms.Visor;
		}
	}

	public static void smethod_2(ref EClippingCustoms clippingCustoms, Item item)
	{
		if (!smethod_3(item))
		{
			clippingCustoms |= EClippingCustoms.Headphones;
		}
	}

	public static bool smethod_3(Item item)
	{
		if (!(item is HeadwearItemClass headwearItemClass))
		{
			return false;
		}
		if (!(headwearItemClass.Template is HeadwearTemplateClass headwearTemplateClass))
		{
			return false;
		}
		return headwearTemplateClass.MaterialType == MaterialType.Helmet;
	}
}
