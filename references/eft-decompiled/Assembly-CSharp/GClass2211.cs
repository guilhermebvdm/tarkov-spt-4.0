using System.Runtime.Serialization;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;

[GAttribute29]
public class GClass2211
{
	[JsonProperty("Id")]
	public MongoID MongoID_0;

	[JsonProperty("Items")]
	public FlatItemsDataClass[] Gclass1390_0;

	[JsonIgnore]
	public InventoryDescriptorClass Items;

	public GClass2211()
	{
	}

	public GClass2211(InventoryEquipment equipment)
	{
		Items = EFTItemSerializerClass.SerializeItem(equipment, GClass2240.Instance);
	}

	public InventoryEquipment ToEquipment()
	{
		return Items.Deserialize<InventoryEquipment>();
	}

	[OnDeserialized]
	public void method_0(StreamingContext context)
	{
		Item item = Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(Gclass1390_0).Items[MongoID_0];
		Items = EFTItemSerializerClass.SerializeItem(item, GClass2240.Instance);
	}

	[OnSerializing]
	public void method_1(StreamingContext context)
	{
		Item item = Items.Deserialize();
		MongoID_0 = Items.Id;
		Gclass1390_0 = Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(item);
	}
}
