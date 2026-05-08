using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass1406 : ISerializer<GClass1405>
{
	[Serializable]
	[CompilerGenerated]
	public class Class921
	{
		public static readonly Class921 class921_0 = new Class921();

		public static Func<Item, bool> func_0;

		public static Func<Item, MongoID> func_1;

		public bool method_0(Item x)
		{
			if (!(x is MagazineItemClass))
			{
				return !(x is AmmoItemClass);
			}
			return false;
		}

		public MongoID method_1(Item x)
		{
			return x.TemplateId;
		}
	}

	public string _id;

	public FlatItemsDataClass[] _items;

	public string _name;

	public string _parent;

	public string _encyclopedia;

	public bool _changeWeaponName;

	public NodeType _type;

	public GClass1405 Deserialize()
	{
		ItemFactoryClass.GStruct181 gStruct = Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(_items);
		if (!gStruct.Items.ContainsKey(_parent))
		{
			Debug.LogError("FlatItems.Items does not contains preset key " + _parent + "!");
			return new GClass1405
			{
				Id = _id,
				Parent = _parent,
				Name = _name,
				Encyclopedia = _encyclopedia,
				ChangeWeaponName = _changeWeaponName,
				Type = _type
			};
		}
		Item item = gStruct.Items[_parent];
		return new GClass1405
		{
			Id = _id,
			Parent = _parent,
			Item = item,
			Children = (from x in GClass3380.GetAllItems(item)
				where !(x is MagazineItemClass) && !(x is AmmoItemClass)
				select x.TemplateId).ToArray(),
			Name = _name,
			Encyclopedia = _encyclopedia,
			ChangeWeaponName = _changeWeaponName,
			Type = _type
		};
	}

	public object Serialize(GClass1405 t)
	{
		_id = t.Id;
		_parent = t.Parent;
		_items = Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(t.Item);
		_parent = t.Item.Id;
		_encyclopedia = t.Item.TemplateId;
		_changeWeaponName = t.ChangeWeaponName;
		_name = t.Name;
		_type = t.Type;
		return this;
	}
}
