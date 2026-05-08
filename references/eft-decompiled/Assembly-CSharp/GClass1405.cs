using System;
using EFT;
using EFT.InventoryLogic;

public class GClass1405
{
	[NonSerialized]
	public const string String_0 = "Stock build";

	public string Id;

	public string Parent;

	public Item Item;

	public MongoID[] Children;

	public string Name;

	public bool ChangeWeaponName;

	public NodeType Type;

	public string Encyclopedia;

	public bool SaveAsPreset => !string.IsNullOrEmpty(Encyclopedia);

	public string BuildName => GClass2348.Localized("Stock build");

	public static GClass1405 FromItem(Item item)
	{
		return new GClass1405
		{
			Item = item,
			Encyclopedia = item.TemplateId
		};
	}

	public override string ToString()
	{
		return "Id: " + Id + ", Parent: " + Parent + ", " + string.Format("{0}: {1}, ", "Item", Item) + "Children: " + GClass851.EnumerableToString(Children, ", ") + ", Name: " + Name + ", " + string.Format("{0}: {1}, ", "ChangeWeaponName", ChangeWeaponName) + string.Format("{0}: {1}, ", "Type", Type) + "Encyclopedia: " + Encyclopedia;
	}
}
