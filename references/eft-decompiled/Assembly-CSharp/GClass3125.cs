using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

public class GClass3125 : Slot
{
	public GClass3125(Slot slot, CompoundItem parentItem)
		: base(slot, parentItem)
	{
	}

	public GClass3125(string id, ItemFilter[] filters, bool required, EParentMergeType mergeSlotWithChildren)
		: base(id, filters, required, mergeSlotWithChildren)
	{
	}

	public override bool CanAcceptRaid(out InventoryError error)
	{
		error = null;
		if (!Singleton<AbstractGame>.Instantiated)
		{
			return true;
		}
		if (!Singleton<AbstractGame>.Instance.InRaid)
		{
			return true;
		}
		if (!(ParentItem.Owner is InventoryController inventoryController))
		{
			return true;
		}
		if (!GClass3373.IsItemEquipped(inventoryController, ParentItem))
		{
			return true;
		}
		error = new GClass1570();
		return false;
	}
}
