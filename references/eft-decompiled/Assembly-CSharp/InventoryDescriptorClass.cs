using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

[GAttribute29]
public class InventoryDescriptorClass
{
	public MongoID Id;

	public MongoID TemplateId;

	public int StackCount;

	public bool SpawnedInSession;

	public byte ActiveCamora;

	public bool IsUnderBarrelDeviceActive;

	[GAttribute30]
	public GClass1917 Malfunction;

	[GAttribute30]
	public GClass1924 UnsearchedInfo;

	[GAttribute30]
	public List<GClass1923> Components;

	[GAttribute30]
	public List<GClass1915> Slots;

	[GAttribute30]
	public List<GClass1916> ShellsInWeapon;

	[GAttribute30]
	public List<GClass1919> Grids;

	[GAttribute30]
	public List<GClass1920> StackSlots;

	public Item Deserialize(Dictionary<MongoID, Item> items = null)
	{
		return Deserialize<Item>(items);
	}

	public TItem Deserialize<TItem>(Dictionary<MongoID, Item> items = null) where TItem : Item
	{
		return (TItem)EFTItemSerializerClass.DeserializeItem(this, Singleton<ItemFactoryClass>.Instance, items);
	}
}
