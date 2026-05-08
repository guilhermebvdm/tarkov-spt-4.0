using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class ItemFactoryClass
{
	public readonly struct Struct306
	{
		[NonSerialized]
		public const int Int_0 = 30;

		public readonly int Version;

		public readonly string WeaponName;

		public readonly string FoundName;

		public string BriefName => WeaponName + " " + GClass856.SubstringIfNecessary(FoundName, 30);

		public Struct306(int version, string weaponName, string foundName)
		{
			Version = version;
			WeaponName = weaponName;
			FoundName = foundName;
		}
	}

	public class Class1193
	{
		public readonly Item Item;

		public readonly MongoID[] Children;

		public readonly string Name;

		public Class1193(Item item, MongoID[] children, string name)
		{
			Item = item;
			Children = children;
			Name = name;
		}
	}

	public struct GStruct181
	{
		public Dictionary<string, Item> Items;

		public List<Error> DeserializationErrors;
	}

	public class GClass1535 : Error
	{
		[NonSerialized]
		public FlatItemsDataClass FlatItemsDataClass;

		public GClass1535(FlatItemsDataClass flatItem)
		{
			FlatItemsDataClass = flatItem;
		}

		public override string ToString()
		{
			return $"No parent with id {FlatItemsDataClass.parentId} found for item {FlatItemsDataClass._id}";
		}
	}

	public class GClass1536 : Error
	{
		[NonSerialized]
		public FlatItemsDataClass FlatItemsDataClass;

		public GClass1536(FlatItemsDataClass flatItem)
		{
			FlatItemsDataClass = flatItem;
		}

		public override string ToString()
		{
			return string.Format("No slot with id {0} found in {1} for item {2}", FlatItemsDataClass.slotId, GClass2348.Localized(string.Concat(FlatItemsDataClass._tpl, " Name")), FlatItemsDataClass._id);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1194
	{
		public static readonly Class1194 class1194_0 = new Class1194();

		public static Func<KeyValuePair<MongoID, int>, MongoID> func_0;

		public static Func<KeyValuePair<MongoID, int>, int> func_1;

		public static Func<Slot, bool> func_2;

		public static Func<Slot, MongoID> func_3;

		public static Func<KeyValuePair<MongoID, ItemTemplate>, bool> func_4;

		public static Func<ItemFilter, bool> func_5;

		public static Func<ItemFilter, bool> func_6;

		public static Func<KeyValuePair<MongoID, ItemTemplate>, bool> func_7;

		public static Func<KeyValuePair<MongoID, ItemTemplate>, bool> func_8;

		public static Func<Item, string> func_9;

		public static Func<GClass3248, string> func_10;

		public static Func<EFT.InventoryLogic.IContainer, string> func_11;

		public static Func<EFT.InventoryLogic.IContainer, EFT.InventoryLogic.IContainer> func_12;

		public static Func<GClass3248, Dictionary<string, EFT.InventoryLogic.IContainer>> func_13;

		public static Func<EFT.InventoryLogic.IContainer, IEnumerable<Item>> func_14;

		public static Func<GClass3248, IEnumerable<GClass3248>> func_15;

		public static Func<Item, bool> func_16;

		public MongoID method_0(KeyValuePair<MongoID, int> limits)
		{
			return limits.Key;
		}

		public int method_1(KeyValuePair<MongoID, int> limits)
		{
			return limits.Value;
		}

		public bool method_2(Slot x)
		{
			return x.ContainedItem != null;
		}

		public MongoID method_3(Slot x)
		{
			return x.ContainedItem.TemplateId;
		}

		public bool method_4(KeyValuePair<MongoID, ItemTemplate> x)
		{
			return x.Value._type == NodeType.Item;
		}

		public bool method_5(ItemFilter filter)
		{
			return !filter.locked;
		}

		public bool method_6(ItemFilter filter)
		{
			return filter.Plate.HasValue;
		}

		public bool method_7(KeyValuePair<MongoID, ItemTemplate> x)
		{
			return x.Value is ModTemplate;
		}

		public bool method_8(KeyValuePair<MongoID, ItemTemplate> x)
		{
			return x.Value._type == NodeType.Item;
		}

		public string method_9(Item x)
		{
			return x.Id;
		}

		public string method_10(GClass3248 x)
		{
			return x.Id;
		}

		public Dictionary<string, EFT.InventoryLogic.IContainer> method_11(GClass3248 x)
		{
			return x.Containers.ToDictionary((EFT.InventoryLogic.IContainer slot) => slot.ID, (EFT.InventoryLogic.IContainer slot) => slot);
		}

		public string method_12(EFT.InventoryLogic.IContainer slot)
		{
			return slot.ID;
		}

		public EFT.InventoryLogic.IContainer method_13(EFT.InventoryLogic.IContainer slot)
		{
			return slot;
		}

		public IEnumerable<GClass3248> method_14(GClass3248 collection)
		{
			return collection.Containers.SelectMany((EFT.InventoryLogic.IContainer container) => container.Items).OfType<GClass3248>();
		}

		public IEnumerable<Item> method_15(EFT.InventoryLogic.IContainer container)
		{
			return container.Items;
		}

		public bool method_16(Item item)
		{
			return item != null;
		}
	}

	[CompilerGenerated]
	public class Class1195
	{
		public List<MongoID> itemNamesSet;

		public Func<MongoID, bool> func_0;

		public bool method_0(MongoID key)
		{
			return itemNamesSet.Contains(key);
		}
	}

	[CompilerGenerated]
	public class Class1196
	{
		public GClass3248 collection;

		public ItemFactoryClass ItemFactoryClass;

		public IEnumerable<FlatItemsDataClass> method_0(EFT.InventoryLogic.IContainer container)
		{
			return from item in container.Items
				where item != null
				select new FlatItemsDataClass
				{
					_id = item.Id,
					_tpl = item.TemplateId,
					parentId = collection.Id,
					slotId = container.ID,
					location = ((container is StashGridClass stashGridClass) ? GClass1911.SaveItemLocation(stashGridClass.GetItemLocation(item)) : ((container is StackSlot stackSlot) ? GClass1911.SaveItemLocation(stackSlot.GetItemPosition((AmmoItemClass)item)) : null)),
					upd = ItemFactoryClass.GetDiff(item)
				};
		}
	}

	[CompilerGenerated]
	public class Class1197
	{
		public EFT.InventoryLogic.IContainer container;

		public Class1196 class1196_0;

		public FlatItemsDataClass method_0(Item item)
		{
			return new FlatItemsDataClass
			{
				_id = item.Id,
				_tpl = item.TemplateId,
				parentId = class1196_0.collection.Id,
				slotId = container.ID,
				location = ((container is StashGridClass stashGridClass) ? GClass1911.SaveItemLocation(stashGridClass.GetItemLocation(item)) : ((container is StackSlot stackSlot) ? GClass1911.SaveItemLocation(stackSlot.GetItemPosition((AmmoItemClass)item)) : null)),
				upd = class1196_0.ItemFactoryClass.GetDiff(item)
			};
		}
	}

	public static readonly MongoID UNKNOWN_TEMPLATE_ID = new MongoID("FFFFFFFFFFFFFFFFFFFFFFFF");

	public const string OBSERVED_PLAYER_TEMP_CONTAINER_NAME = "TempObservedContainer";

	[NonSerialized]
	public const string String_0 = "6050cac987d3f925bf016837";

	public readonly GClass1408 ItemTemplates;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1405[] Gclass1405_0;

	[NonSerialized]
	public Dictionary<MongoID, Item> Dictionary_0;

	[NonSerialized]
	public List<Class1193> List_0 = new List<Class1193>();

	[NonSerialized]
	public Dictionary<MongoID, int> Dictionary_1 = new Dictionary<MongoID, int>();

	[NonSerialized]
	public Dictionary<string, Struct306> Dictionary_2 = new Dictionary<string, Struct306>();

	[NonSerialized]
	public WeaponBuildsStorageClass WeaponBuildsStorageClass;

	[NonSerialized]
	public MongoID MongoID_0_1 = MongoID.Generate();

	public GClass1405[] SavedPresets
	{
		[CompilerGenerated]
		get
		{
			return Gclass1405_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1405_0 = value;
		}
	}

	public MongoID MongoID_0
	{
		get
		{
			MongoID mongoID_0_ = MongoID_0_1;
			MongoID_0_1 = MongoID_0_1.Next();
			return mongoID_0_;
		}
	}

	public ItemFactoryClass(GClass1408 itemTemplates)
	{
		ItemTemplates = itemTemplates;
		foreach (KeyValuePair<MongoID, ItemTemplate> itemTemplate3 in ItemTemplates)
		{
			itemTemplate3.Deconstruct(out var key, out var value);
			MongoID mongoID = key;
			ItemTemplate itemTemplate = value;
			int discardLimit = itemTemplate.DiscardLimit;
			if (discardLimit < 0)
			{
				continue;
			}
			ItemTemplate itemTemplate2 = itemTemplate;
			do
			{
				if (itemTemplate2.ParentId.HasValue)
				{
					itemTemplate2 = ItemTemplates[itemTemplate2.ParentId.Value];
					continue;
				}
				Debug.Log($"<color=blue>Discard Limit: {itemTemplate._name} - {discardLimit}</color> type: {itemTemplate._type} ({mongoID})");
				break;
			}
			while (itemTemplate2.DiscardLimit != discardLimit);
			if (itemTemplate._type == NodeType.Item)
			{
				Dictionary_1[mongoID] = discardLimit;
			}
		}
	}

	public IEnumerable<Type> GetFilterTypes(MongoID[] filters)
	{
		return filters.Select(delegate(MongoID node)
		{
			if (!TemplateIdToObjectMappingsClass.TypeTable.TryGetValue(node, out var value))
			{
				if (!ItemTemplates.TryGetValue(node, out var value2))
				{
					return (Type)null;
				}
				Dictionary<string, Type> typeTable = TemplateIdToObjectMappingsClass.TypeTable;
				MongoID? parentId = value2.ParentId;
				return typeTable[parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null];
			}
			return value;
		});
	}

	public Dictionary<MongoID, int> GetDiscardLimits()
	{
		if (!Singleton<BackendConfigSettingsClass>.Instance.DiscardLimitsEnabled)
		{
			return new Dictionary<MongoID, int>();
		}
		return Dictionary_1.ToDictionary((KeyValuePair<MongoID, int> limits) => limits.Key, (KeyValuePair<MongoID, int> limits) => limits.Value);
	}

	public IEnumerable<Item> method_0(FlatItemsDataClass[] flatItems)
	{
		foreach (FlatItemsDataClass flatItemsDataClass in flatItems)
		{
			Item item = null;
			try
			{
				item = CreateItem(flatItemsDataClass._id, flatItemsDataClass._tpl, flatItemsDataClass.upd);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("Error: Couldn't create item with template id: ", flatItemsDataClass._tpl, " exception: ", ex?.ToString()));
			}
			if (item != null)
			{
				yield return item;
			}
		}
	}

	public Item CreateItem(string itemId, string templateId, [CanBeNull] GClass846 itemDiff)
	{
		if (!ItemTemplates.TryGetValue(templateId, out var value))
		{
			throw new Exception("Cannot find template with id \"" + templateId + "\" for item \"" + itemId + "\"");
		}
		MongoID? mongoID = ((value._type == NodeType.Node) ? new MongoID?(value._id) : value.ParentId);
		Dictionary<string, Func<string, object, Item>> itemConstructors = TemplateIdToObjectMappingsClass.ItemConstructors;
		MongoID? mongoID2 = mongoID;
		Item item = itemConstructors[mongoID2.HasValue ? ((string)mongoID2.GetValueOrDefault()) : null](itemId, value);
		if (itemDiff != null)
		{
			item = GClass1911.CreateItem(item, itemDiff);
		}
		return item;
	}

	public StashItemClass CreateFakeStash(MongoID? stashId = null)
	{
		return (StashItemClass)CreateItem(stashId ?? MongoID_0, "566abbc34bdc2d92178b4576", null);
	}

	[CanBeNull]
	public GClass846 GetDiff(Item item)
	{
		Dictionary<string, object> itemProperties = GClass1911.GetItemProperties(item);
		if (itemProperties.Count != 0)
		{
			return JsonParserClass.ToUnparsedData(itemProperties);
		}
		return null;
	}

	public void SetItemPresets(GClass1405[] itemPresets)
	{
		SavedPresets = itemPresets;
		Dictionary_0 = new Dictionary<MongoID, Item>();
		List_0 = new List<Class1193>();
		foreach (GClass1405 gClass in itemPresets)
		{
			if (gClass.Encyclopedia == null || gClass.ChangeWeaponName)
			{
				List_0.Add(new Class1193(gClass.Item, gClass.Children, gClass.Id));
			}
			if (gClass.Encyclopedia != null)
			{
				if (Dictionary_0.ContainsKey(gClass.Encyclopedia))
				{
					Debug.LogError("Item presets is already contains key: " + gClass.Encyclopedia);
				}
				else
				{
					Dictionary_0.Add(gClass.Encyclopedia, gClass.Item);
				}
			}
		}
	}

	public string BriefItemName(Item item, string defaultName)
	{
		if (!(item is Weapon weapon))
		{
			return defaultName;
		}
		if (Dictionary_2.ContainsKey(weapon.Id + defaultName))
		{
			Struct306 @struct = Dictionary_2[weapon.Id + defaultName];
			if (@struct.Version == weapon.GetModsHashSumWithoutMag())
			{
				return @struct.BriefName;
			}
			Dictionary_2.Remove(weapon.Id + defaultName);
		}
		string text = method_1(weapon);
		if (!string.IsNullOrEmpty(text))
		{
			Struct306 value = new Struct306(weapon.GetModsHashSumWithoutMag(), defaultName, text);
			Dictionary_2.Add(weapon.Id + defaultName, value);
			return value.BriefName;
		}
		string text2 = method_2(weapon);
		if (!string.IsNullOrEmpty(text2))
		{
			Struct306 value2 = new Struct306(weapon.GetModsHashSumWithoutMag(), defaultName, text2);
			Dictionary_2.Add(weapon.Id + defaultName, value2);
			return value2.BriefName;
		}
		return defaultName;
	}

	public void RemoveCachedItemName(string name)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Struct306> item in Dictionary_2)
		{
			if (item.Value.FoundName == name)
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			Dictionary_2.Remove(item2);
		}
	}

	public string method_1(Weapon weapon)
	{
		string result = string.Empty;
		WeaponBuildClass weaponBuildClass = WeaponBuildsStorageClass?.FindBuild(weapon);
		if (weaponBuildClass != null)
		{
			result = weaponBuildClass.ItemIconName;
		}
		return result;
	}

	public string method_2(Weapon weapon)
	{
		List<MongoID> itemNamesSet = (from x in weapon.AllSlots
			where x.ContainedItem != null
			select x.ContainedItem.TemplateId).Append(weapon.TemplateId).Distinct().ToList();
		string result = string.Empty;
		foreach (Class1193 item in List_0)
		{
			if (item.Children.All((MongoID key) => itemNamesSet.Contains(key)))
			{
				result = GClass2348.Localized(item.Name);
				break;
			}
		}
		return result;
	}

	public void SetWeaponBuildsStorage(WeaponBuildsStorageClass storage)
	{
		WeaponBuildsStorageClass = storage;
	}

	public Item GetHideoutSchemeItem(MongoID templateId, string id, EOwnerType ownerType, bool spawnedInSession)
	{
		Item presetItem = GetPresetItem(templateId);
		TraderControllerClass traderControllerClass = new TraderControllerClass(presetItem, id, "Hideout item controller", canBeLocalized: false, ownerType);
		presetItem.CurrentAddress = traderControllerClass.CreateItemAddress();
		presetItem.SpawnedInSession = spawnedInSession;
		return presetItem;
	}

	public Item GetPresetItem(string templateId)
	{
		if (!Dictionary_0.TryGetValue(templateId, out var value))
		{
			return CreateItem(MongoID_0, templateId, null);
		}
		return GClass3380.CloneItem(value);
	}

	public IEnumerable<Item> CreateAllItemsEver()
	{
		return from x in ItemTemplates
			where x.Value._type == NodeType.Item
			select method_3(x.Key);
	}

	public Item method_3(MongoID templateId)
	{
		Item item = CreateItem(MongoID_0, templateId, null);
		if (!(item is CompoundItem { Slots: var slots } compoundItem))
		{
			return item;
		}
		foreach (Slot slot in slots)
		{
			if (slot.Filters.All((ItemFilter filter) => !filter.locked))
			{
				continue;
			}
			MongoID? mongoID = slot.Filters.FirstOrDefault((ItemFilter filter) => filter.Plate.HasValue)?.Plate;
			if (mongoID.HasValue)
			{
				Item item2 = CreateItem(MongoID_0, mongoID.Value, null);
				GStruct154<GClass3414> gStruct = slot.AddWithoutRestrictions(item2);
				if (gStruct.Failed)
				{
					Debug.LogError(gStruct.Error);
				}
			}
		}
		return compoundItem;
	}

	public Item[] CreateAllModsEver()
	{
		return (from x in ItemTemplates
			where x.Value is ModTemplate
			where x.Value._type == NodeType.Item
			select CreateItem(MongoID_0, x.Key, null)).ToArray();
	}

	public StashItemClass FlatItemsToStash(FlatItemsDataClass[] flatItems)
	{
		if (flatItems == null)
		{
			return null;
		}
		StashItemClass stashItemClass = CreateFakeStash();
		StashGridClass stashGridClass = new StashGridClass("fakeStashGrid", 20, 1, canStretchVertically: true, Array.Empty<ItemFilter>(), stashItemClass);
		stashItemClass.Grids[0] = stashGridClass;
		foreach (FlatItemsDataClass flatItemsDataClass in flatItems)
		{
			if (!flatItemsDataClass.parentId.HasValue)
			{
				flatItemsDataClass.parentId = stashItemClass.Id;
				flatItemsDataClass.slotId = stashGridClass.ID;
			}
		}
		FlatItemsToTree(flatItems, silentMode: false, new Dictionary<string, Item> { { stashItemClass.Id, stashItemClass } });
		return stashItemClass;
	}

	public GStruct181 FlatItemsToTree(FlatItemsDataClass[] flatItems, bool silentMode = false, Dictionary<string, Item> preexistingItems = null)
	{
		List<Error> list = new List<Error>();
		Dictionary<string, Item> dictionary = method_0(flatItems).ToDictionary((Item x) => x.Id);
		if (preexistingItems != null)
		{
			foreach (KeyValuePair<string, Item> preexistingItem in preexistingItems)
			{
				dictionary[preexistingItem.Key] = preexistingItem.Value;
			}
		}
		Dictionary<string, Dictionary<string, EFT.InventoryLogic.IContainer>> dictionary2 = dictionary.Values.OfType<GClass3248>().ToDictionary((GClass3248 x) => x.Id, (GClass3248 x) => x.Containers.ToDictionary((EFT.InventoryLogic.IContainer container2) => container2.ID, (EFT.InventoryLogic.IContainer result) => result));
		HashSet<StashGridClass> hashSet = new HashSet<StashGridClass>();
		HashSet<StackSlot> hashSet2 = new HashSet<StackSlot>();
		foreach (FlatItemsDataClass item2 in from x in flatItems
			where x.parentId.HasValue && !x.parentId.Value.Equals("6050cac987d3f925bf016837") && ItemTemplates.ContainsKey(x._tpl)
			orderby ItemTemplates[x._tpl] is WeaponTemplate
			select x)
		{
			if (!dictionary2.TryGetValue(item2.parentId.Value, out var value))
			{
				list.Add(new GClass1535(item2));
				continue;
			}
			if (!value.TryGetValue(item2.slotId, out var value2))
			{
				list.Add(new GClass1536(item2));
				continue;
			}
			if (!dictionary.TryGetValue(item2._id, out var value3))
			{
				Debug.LogErrorFormat("There is no deserializedItems for {0}", item2._id);
				continue;
			}
			try
			{
				Error error = null;
				EFT.InventoryLogic.IContainer container = value2;
				if (!(container is StashGridClass stashGridClass))
				{
					if (!(container is Slot slot))
					{
						if (!(container is StackSlot stackSlot) || !(value3 is AmmoItemClass item))
						{
							throw new Exception("Inventory configuration error");
						}
						stackSlot.AddAtPosition(item, GClass1911.CreateItemLocation<int>(item2.location));
						hashSet2.Add(stackSlot);
					}
					else
					{
						error = slot.AddWithoutRestrictions(value3).Error;
					}
				}
				else
				{
					hashSet.Add(stashGridClass);
					LocationInGrid location = ((item2.location != null) ? GClass1911.CreateItemLocation<LocationInGrid>(item2.location) : stashGridClass.FindFreeSpace(value3));
					error = stashGridClass.AddItemWithoutRestrictions(value3, location).Error;
				}
				if (error != null)
				{
					list.Add(error);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not add " + value3?.ToString() + " to " + value2?.ToString() + ". Reason: " + ex);
			}
		}
		foreach (StackSlot item3 in hashSet2)
		{
			GStruct155 gStruct = item3.FinalizeDeserialization();
			if (gStruct.Failed)
			{
				list.Add(gStruct.Error);
			}
		}
		foreach (StashGridClass item4 in hashSet)
		{
			item4.RevalidateSpaceBuffer();
		}
		if (!silentMode)
		{
			foreach (Error item5 in list)
			{
				Debug.LogError("Item deserialization error: " + item5);
			}
		}
		return new GStruct181
		{
			Items = dictionary,
			DeserializationErrors = list
		};
	}

	public FlatItemsDataClass[] TreeToFlatItems(IEnumerable<Item> rootItems)
	{
		Item[] source = rootItems.ToArray();
		IEnumerable<FlatItemsDataClass> first = source.Select((Item item) => new FlatItemsDataClass
		{
			_id = item.Id,
			_tpl = item.TemplateId,
			upd = GetDiff(item)
		});
		IEnumerable<FlatItemsDataClass> second = GClass1518.Flatten(source.OfType<GClass3248>(), (GClass3248 collection) => collection.Containers.SelectMany((EFT.InventoryLogic.IContainer container) => container.Items).OfType<GClass3248>()).SelectMany((GClass3248 collection) => collection.Containers.SelectMany((EFT.InventoryLogic.IContainer container) => from item in container.Items
			where item != null
			select new FlatItemsDataClass
			{
				_id = item.Id,
				_tpl = item.TemplateId,
				parentId = collection.Id,
				slotId = container.ID,
				location = ((container is StashGridClass stashGridClass) ? GClass1911.SaveItemLocation(stashGridClass.GetItemLocation(item)) : ((container is StackSlot stackSlot) ? GClass1911.SaveItemLocation(stackSlot.GetItemPosition((AmmoItemClass)item)) : null)),
				upd = GetDiff(item)
			}));
		return first.Concat(second).ToArray();
	}

	public FlatItemsDataClass[] TreeToFlatItems(params Item[] rootItems)
	{
		return TreeToFlatItems((IEnumerable<Item>)rootItems);
	}

	public static void LogErrors(List<Error> errors, string caption)
	{
		if (errors.Count == 0)
		{
			return;
		}
		Debug.LogError($"{errors.Count} errors encountered while deserializing {caption}");
		foreach (Error error in errors)
		{
			Debug.LogError(error);
		}
	}

	[CompilerGenerated]
	public Type method_4(MongoID node)
	{
		if (!TemplateIdToObjectMappingsClass.TypeTable.TryGetValue(node, out var value))
		{
			if (!ItemTemplates.TryGetValue(node, out var value2))
			{
				return null;
			}
			Dictionary<string, Type> typeTable = TemplateIdToObjectMappingsClass.TypeTable;
			MongoID? parentId = value2.ParentId;
			return typeTable[parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null];
		}
		return value;
	}

	[CompilerGenerated]
	public Item method_5(KeyValuePair<MongoID, ItemTemplate> x)
	{
		return method_3(x.Key);
	}

	[CompilerGenerated]
	public Item method_6(KeyValuePair<MongoID, ItemTemplate> x)
	{
		return CreateItem(MongoID_0, x.Key, null);
	}

	[CompilerGenerated]
	public bool method_7(FlatItemsDataClass x)
	{
		if (x.parentId.HasValue && !x.parentId.Value.Equals("6050cac987d3f925bf016837"))
		{
			return ItemTemplates.ContainsKey(x._tpl);
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_8(FlatItemsDataClass x)
	{
		return ItemTemplates[x._tpl] is WeaponTemplate;
	}

	[CompilerGenerated]
	public FlatItemsDataClass method_9(Item item)
	{
		return new FlatItemsDataClass
		{
			_id = item.Id,
			_tpl = item.TemplateId,
			upd = GetDiff(item)
		};
	}

	[CompilerGenerated]
	public IEnumerable<FlatItemsDataClass> method_10(GClass3248 collection)
	{
		return collection.Containers.SelectMany((EFT.InventoryLogic.IContainer container) => from item in container.Items
			where item != null
			select new FlatItemsDataClass
			{
				_id = item.Id,
				_tpl = item.TemplateId,
				parentId = collection.Id,
				slotId = container.ID,
				location = ((container is StashGridClass stashGridClass) ? GClass1911.SaveItemLocation(stashGridClass.GetItemLocation(item)) : ((container is StackSlot stackSlot) ? GClass1911.SaveItemLocation(stackSlot.GetItemPosition((AmmoItemClass)item)) : null)),
				upd = GetDiff(item)
			});
	}
}
