using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using Newtonsoft.Json;

[GAttribute29]
public class EFTInventoryClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1199
	{
		public static readonly Class1199 class1199_0 = new Class1199();

		public static Func<KeyValuePair<EBoundItem, Item>, EBoundItem> func_0;

		public static Func<KeyValuePair<EBoundItem, Item>, MongoID> func_1;

		public static Func<KeyValuePair<MongoID, int>, MongoID> func_2;

		public static Func<KeyValuePair<MongoID, int>, int> func_3;

		public static Func<KeyValuePair<EAreaType, CompoundItem>, EAreaType> func_4;

		public static Func<KeyValuePair<EAreaType, CompoundItem>, MongoID> func_5;

		public static Func<Item, bool> func_6;

		public EBoundItem method_0(KeyValuePair<EBoundItem, Item> boundItem)
		{
			return boundItem.Key;
		}

		public MongoID method_1(KeyValuePair<EBoundItem, Item> boundItem)
		{
			return boundItem.Value.Id;
		}

		public MongoID method_2(KeyValuePair<MongoID, int> limit)
		{
			return limit.Key;
		}

		public int method_3(KeyValuePair<MongoID, int> limit)
		{
			return limit.Value;
		}

		public EAreaType method_4(KeyValuePair<EAreaType, CompoundItem> pair)
		{
			return pair.Key;
		}

		public MongoID method_5(KeyValuePair<EAreaType, CompoundItem> pair)
		{
			return new MongoID(pair.Value.Id);
		}

		public bool method_6(Item container)
		{
			return container != null;
		}
	}

	[GAttribute31]
	[JsonProperty("items")]
	public FlatItemsDataClass[] Gclass1390_0;

	[GAttribute31]
	[JsonProperty("equipment")]
	public MongoID MongoID_0;

	[DefaultValue(null)]
	[GAttribute31]
	[JsonProperty(PropertyName = "stash", NullValueHandling = NullValueHandling.Ignore)]
	public MongoID? Nullable_0;

	[DefaultValue(null)]
	[GAttribute31]
	[JsonProperty(PropertyName = "questRaidItems", NullValueHandling = NullValueHandling.Ignore)]
	public MongoID? Nullable_1;

	[DefaultValue(null)]
	[GAttribute31]
	[JsonProperty(PropertyName = "questStashItems", NullValueHandling = NullValueHandling.Ignore)]
	public MongoID? Nullable_2;

	[DefaultValue(null)]
	[GAttribute31]
	[JsonProperty(PropertyName = "sortingTable", NullValueHandling = NullValueHandling.Ignore)]
	public MongoID? Nullable_3;

	[DefaultValue(null)]
	[GAttribute31]
	[JsonProperty("hideoutAreaStashes", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<EAreaType, MongoID> Dictionary_0;

	[JsonProperty("fastPanel")]
	public Dictionary<EBoundItem, MongoID> FastAccess = new Dictionary<EBoundItem, MongoID>();

	[JsonProperty("favoriteItems")]
	public List<MongoID> FavoriteItemsStorage = new List<MongoID>();

	[DefaultValue(null)]
	[GAttribute31]
	[JsonProperty(PropertyName = "hideoutCustomizationStashId", NullValueHandling = NullValueHandling.Ignore)]
	public MongoID? Nullable_4;

	[JsonIgnore]
	public bool CheckInventoryHash;

	[JsonIgnore]
	public Dictionary<MongoID, int> DiscardLimits = new Dictionary<MongoID, int>();

	[NonSerialized]
	public InventoryDescriptorClass InventoryDescriptorClass;

	[NonSerialized]
	public InventoryDescriptorClass InventoryDescriptorClass_1;

	[NonSerialized]
	public InventoryDescriptorClass InventoryDescriptorClass_2;

	[NonSerialized]
	public InventoryDescriptorClass InventoryDescriptorClass_3;

	[NonSerialized]
	public InventoryDescriptorClass InventoryDescriptorClass_4;

	[NonSerialized]
	public InventoryDescriptorClass InventoryDescriptorClass_5;

	[NonSerialized]
	public Dictionary<EAreaType, InventoryDescriptorClass> Dictionary_1;

	[NonSerialized]
	public Inventory Inventory_0;

	[NonSerialized]
	public ISearchController IsearchController_0;

	[NonSerialized]
	public List<Error> List_0;

	[JsonIgnore]
	public InventoryDescriptorClass Equipment
	{
		get
		{
			method_0();
			return InventoryDescriptorClass;
		}
		set
		{
			InventoryDescriptorClass = value;
		}
	}

	[JsonIgnore]
	[GAttribute30]
	[CanBeNull]
	public InventoryDescriptorClass Stash
	{
		get
		{
			method_0();
			return InventoryDescriptorClass_1;
		}
		set
		{
			InventoryDescriptorClass_1 = value;
		}
	}

	[JsonIgnore]
	[GAttribute30]
	[CanBeNull]
	public InventoryDescriptorClass QuestRaidItems
	{
		get
		{
			method_0();
			return InventoryDescriptorClass_2;
		}
		set
		{
			InventoryDescriptorClass_2 = value;
		}
	}

	[JsonIgnore]
	[GAttribute30]
	[CanBeNull]
	public InventoryDescriptorClass QuestStashItems
	{
		get
		{
			method_0();
			return InventoryDescriptorClass_3;
		}
		set
		{
			InventoryDescriptorClass_3 = value;
		}
	}

	[JsonIgnore]
	[GAttribute30]
	[CanBeNull]
	public InventoryDescriptorClass SortingTable
	{
		get
		{
			method_0();
			return InventoryDescriptorClass_4;
		}
		set
		{
			InventoryDescriptorClass_4 = value;
		}
	}

	[JsonIgnore]
	[GAttribute30]
	[CanBeNull]
	public InventoryDescriptorClass HideoutCustomizationStash
	{
		get
		{
			method_0();
			return InventoryDescriptorClass_5;
		}
		set
		{
			InventoryDescriptorClass_5 = value;
		}
	}

	[JsonIgnore]
	[GAttribute30]
	[CanBeNull]
	public Dictionary<EAreaType, InventoryDescriptorClass> HideoutAreaStashes
	{
		get
		{
			method_0();
			return Dictionary_1;
		}
		set
		{
			Dictionary_1 = value;
		}
	}

	public EFTInventoryClass()
	{
	}

	public EFTInventoryClass(Inventory inventory, ISearchController searchController)
	{
		Inventory_0 = inventory;
		IsearchController_0 = searchController;
		CheckInventoryHash = inventory.CheckHash;
		FavoriteItemsStorage = new List<MongoID>(inventory.FavoriteItemsStorage);
		FastAccess = ((IEnumerable<KeyValuePair<EBoundItem, Item>>)inventory.FastAccess.BoundItems).ToDictionary((Func<KeyValuePair<EBoundItem, Item>, EBoundItem>)((KeyValuePair<EBoundItem, Item> boundItem) => boundItem.Key), (Func<KeyValuePair<EBoundItem, Item>, MongoID>)((KeyValuePair<EBoundItem, Item> boundItem) => boundItem.Value.Id));
		DiscardLimits = GClass853.EmptyIfNull(inventory.DiscardLimits).ToDictionary((KeyValuePair<MongoID, int> limit) => limit.Key, (KeyValuePair<MongoID, int> limit) => limit.Value);
	}

	public void method_0()
	{
		if (InventoryDescriptorClass == null)
		{
			Inventory_0 = Inventory_0 ?? ToInventory();
			InventoryDescriptorClass = EFTItemSerializerClass.SerializeItem(Inventory_0.Equipment, IsearchController_0);
			InventoryDescriptorClass_2 = EFTItemSerializerClass.SerializeItem(Inventory_0.QuestRaidItems, IsearchController_0);
			InventoryDescriptorClass_3 = EFTItemSerializerClass.SerializeItem(Inventory_0.QuestStashItems, IsearchController_0);
			InventoryDescriptorClass_1 = EFTItemSerializerClass.SerializeItem(Inventory_0.Stash, IsearchController_0);
			InventoryDescriptorClass_4 = EFTItemSerializerClass.SerializeItem(Inventory_0.SortingTable, IsearchController_0);
		}
	}

	public Inventory ToInventory()
	{
		Dictionary<EAreaType, CompoundItem> dictionary = new Dictionary<EAreaType, CompoundItem>();
		if (InventoryDescriptorClass == null && Inventory_0 != null)
		{
			method_0();
		}
		InventoryEquipment equipment;
		StashItemClass stash;
		StashItemClass questRaidItems;
		StashItemClass questStashItems;
		CompoundItem hideoutCustomizationStash;
		SortingTableItemClass sortingTable;
		EAreaType key;
		if (InventoryDescriptorClass != null)
		{
			equipment = InventoryDescriptorClass.Deserialize<InventoryEquipment>();
			stash = InventoryDescriptorClass_1?.Deserialize<StashItemClass>();
			questRaidItems = InventoryDescriptorClass_2?.Deserialize<StashItemClass>();
			questStashItems = InventoryDescriptorClass_3?.Deserialize<StashItemClass>();
			hideoutCustomizationStash = InventoryDescriptorClass_5?.Deserialize<CompoundItem>();
			sortingTable = InventoryDescriptorClass_4?.Deserialize<SortingTableItemClass>();
			foreach (KeyValuePair<EAreaType, InventoryDescriptorClass> item in GClass853.EmptyIfNull(Dictionary_1))
			{
				item.Deconstruct(out key, out var value);
				EAreaType key2 = key;
				InventoryDescriptorClass inventoryDescriptorClass = value;
				dictionary[key2] = inventoryDescriptorClass.Deserialize<CompoundItem>();
			}
		}
		else
		{
			if (Gclass1390_0 == null)
			{
				throw new Exception("Cannot serialize empty InventoryDescriptor");
			}
			ItemFactoryClass.GStruct181 gStruct = Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(Gclass1390_0);
			Dictionary<string, Item> items = gStruct.Items;
			List_0 = gStruct.DeserializationErrors;
			equipment = (InventoryEquipment)items[MongoID_0];
			stash = ((!Nullable_0.HasValue || !items.TryGetValue(Nullable_0.Value, out var value2)) ? null : ((StashItemClass)value2));
			questRaidItems = ((!Nullable_1.HasValue || !items.TryGetValue(Nullable_1.Value, out var value3)) ? null : ((StashItemClass)value3));
			questStashItems = ((!Nullable_2.HasValue || !items.TryGetValue(Nullable_2.Value, out var value4)) ? null : ((StashItemClass)value4));
			sortingTable = ((!Nullable_3.HasValue || !items.TryGetValue(Nullable_3.Value, out var value5)) ? null : ((SortingTableItemClass)value5));
			hideoutCustomizationStash = ((!Nullable_4.HasValue || !items.TryGetValue(Nullable_4.Value, out var value6)) ? null : ((CompoundItem)value6));
			if (Dictionary_0 != null)
			{
				foreach (KeyValuePair<EAreaType, MongoID> item2 in Dictionary_0)
				{
					item2.Deconstruct(out key, out var value7);
					EAreaType key3 = key;
					MongoID mongoID = value7;
					if (items.TryGetValue(mongoID, out var value8))
					{
						dictionary[key3] = (CompoundItem)value8;
					}
				}
			}
		}
		return new Inventory(equipment, stash, questRaidItems, questStashItems, sortingTable, hideoutCustomizationStash, dictionary, FastAccess ?? new Dictionary<EBoundItem, MongoID>(), DiscardLimits ?? new Dictionary<MongoID, int>(), FavoriteItemsStorage ?? new List<MongoID>(), List_0 ?? new List<Error>(), CheckInventoryHash);
	}

	[OnSerializing]
	public void method_1(StreamingContext context)
	{
		if (Gclass1390_0 != null)
		{
			return;
		}
		Inventory_0 = Inventory_0 ?? ToInventory();
		MongoID_0 = Inventory_0.Equipment.Id;
		Nullable_0 = ((Inventory_0.Stash != null) ? ((MongoID?)Inventory_0.Stash.Id) : ((MongoID?)null));
		Nullable_1 = ((Inventory_0.QuestRaidItems != null) ? ((MongoID?)Inventory_0.QuestRaidItems?.Id) : ((MongoID?)null));
		Nullable_2 = ((Inventory_0.QuestStashItems != null) ? ((MongoID?)Inventory_0.QuestStashItems?.Id) : ((MongoID?)null));
		Nullable_3 = ((Inventory_0.SortingTable != null) ? ((MongoID?)Inventory_0.SortingTable?.Id) : ((MongoID?)null));
		Nullable_4 = ((Inventory_0.HideoutCustomizationStash != null) ? ((MongoID?)Inventory_0.HideoutCustomizationStash?.Id) : ((MongoID?)null));
		List<Item> list = new List<Item> { Inventory_0.Equipment, Inventory_0.Stash, Inventory_0.QuestRaidItems, Inventory_0.QuestStashItems, Inventory_0.SortingTable, Inventory_0.HideoutCustomizationStash };
		if (Inventory_0.HideoutAreaStashes != null)
		{
			Dictionary_0 = Inventory_0.HideoutAreaStashes.ToDictionary((KeyValuePair<EAreaType, CompoundItem> pair) => pair.Key, (KeyValuePair<EAreaType, CompoundItem> pair) => new MongoID(pair.Value.Id));
			list.AddRange(Inventory_0.HideoutAreaStashes.Values);
		}
		IEnumerable<Item> rootItems = list.Where((Item container) => container != null);
		Gclass1390_0 = Singleton<ItemFactoryClass>.Instance.TreeToFlatItems(rootItems);
	}
}
