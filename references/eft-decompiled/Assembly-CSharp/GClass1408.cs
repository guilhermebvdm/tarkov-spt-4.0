using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.SynchronizableObjects;
using UnityEngine;

public class GClass1408 : Dictionary<MongoID, ItemTemplate>
{
	[NonSerialized]
	public Dictionary<string, ResourceTypeStruct> Dictionary_0;

	public void Init()
	{
		Dictionary_0 = new Dictionary<string, ResourceTypeStruct>(base.Values.Count);
		foreach (ItemTemplate value in base.Values)
		{
			value.OnInit();
			if (value.ParentId.HasValue)
			{
				base[value.ParentId.Value].AddChild(value);
			}
			if (value._type != NodeType.Item)
			{
				continue;
			}
			foreach (ResourceKey allResource in value.AllResources)
			{
				if (!Dictionary_0.ContainsKey(allResource.path))
				{
					Dictionary_0.Add(allResource.path, new ResourceTypeStruct
					{
						ItemTemplate = value,
						ResourceType = GetResourceType(value, allResource)
					});
				}
			}
		}
		Dictionary_0["assets/content/weapons/empty hands/weapon_empty_hands_container.bundle"] = new ResourceTypeStruct
		{
			ResourceType = ResourceType.Weapon
		};
		foreach (KeyValuePair<SynchronizableObjectType, ResourceKey> item in ResourceKeyManagerAbstractClass.SynchronizableObjectPath)
		{
			Dictionary_0[item.Value.path] = new ResourceTypeStruct
			{
				ResourceType = ResourceType.SynchronizableObject
			};
		}
	}

	public static ResourceType GetResourceType(ItemTemplate template, ResourceKey resource)
	{
		if (!(template is WeaponTemplate) && !(template is ThrowWeapTemplateClass) && !(template is KnifeTemplateClass) && !(template is LauncherTemplateClass))
		{
			if (!(template is MagazineTemplateClass magazineTemplateClass))
			{
				if (!(template is AmmoTemplate))
				{
					if (!object.Equals(template.UsePrefab, resource))
					{
						if (!(template is ModTemplate))
						{
							return ResourceType.Item;
						}
						return ResourceType.WeaponModItem;
					}
					return ResourceType.UsablePrefab;
				}
				return ResourceType.Ammo;
			}
			if (!magazineTemplateClass.IsMagazineForStationaryWeapon)
			{
				return ResourceType.Magazine;
			}
			return ResourceType.StationaryWeaponMagazine;
		}
		return ResourceType.Weapon;
	}

	public bool method_0(CompoundItemTemplateClass template, MongoID key)
	{
		if (ContainsKey(key))
		{
			return true;
		}
		Debug.LogError(string.Concat("Filter is missing: ", key, ", template: ", template._id));
		return false;
	}

	public ResourceTypeStruct GetResourceTypeByBundlePath(string bundlePath)
	{
		if (!Dictionary_0.TryGetValue(bundlePath, out var value))
		{
			return new ResourceTypeStruct
			{
				ResourceType = ResourceType.Other
			};
		}
		return value;
	}

	public static IEnumerable<ItemTemplate> GetAllCompatibleTemplates(IEnumerable<ItemTemplate> parentTemplates, Dictionary<ItemTemplate, ItemTemplate[]> allChildrenDict)
	{
		Stack<ItemTemplate> stack = new Stack<ItemTemplate>(parentTemplates);
		HashSet<ItemTemplate> hashSet = new HashSet<ItemTemplate>();
		while (stack.Count > 0)
		{
			ItemTemplate itemTemplate = stack.Pop();
			if (!hashSet.Add(itemTemplate))
			{
				continue;
			}
			yield return itemTemplate;
			if (!(itemTemplate is CompoundItemTemplateClass parentTemplate))
			{
				continue;
			}
			foreach (ItemTemplate item in smethod_0(parentTemplate, allChildrenDict))
			{
				if (!hashSet.Contains(item))
				{
					stack.Push(item);
				}
			}
		}
	}

	public static IEnumerable<ItemTemplate> smethod_0(CompoundItemTemplateClass parentTemplate, Dictionary<ItemTemplate, ItemTemplate[]> allChildrenDict)
	{
		GClass1408 itemTemplates = Singleton<ItemFactoryClass>.Instance.ItemTemplates;
		HashSet<ItemTemplate> hashSet = new HashSet<ItemTemplate>();
		for (int i = 0; i < parentTemplate.Slots.Length; i++)
		{
			Slot slot = parentTemplate.Slots[i];
			for (int j = 0; j < slot.Filters.Length; j++)
			{
				ItemFilter itemFilter = slot.Filters[j];
				for (int k = 0; k < itemFilter.Filter.Length; k++)
				{
					MongoID key = itemFilter.Filter[k];
					if (!itemTemplates.method_0(parentTemplate, key))
					{
						continue;
					}
					foreach (ItemTemplate item in smethod_1(itemTemplates[key], allChildrenDict))
					{
						hashSet.Add(item);
					}
				}
			}
		}
		return hashSet;
	}

	public static IEnumerable<ItemTemplate> smethod_1(ItemTemplate template, IDictionary<ItemTemplate, ItemTemplate[]> allChildrenDict)
	{
		if (allChildrenDict.TryGetValue(template, out var value))
		{
			return value;
		}
		List<ItemTemplate> list = new List<ItemTemplate> { template };
		IEnumerable<ItemTemplate> children = template.Children;
		Stack<ItemTemplate> stack = new Stack<ItemTemplate>(children ?? Enumerable.Empty<ItemTemplate>());
		while (stack.Count > 0)
		{
			ItemTemplate itemTemplate = stack.Pop();
			if (allChildrenDict.TryGetValue(itemTemplate, out var value2))
			{
				list.AddRange(value2);
				continue;
			}
			list.Add(itemTemplate);
			if (itemTemplate.Children == null)
			{
				continue;
			}
			foreach (ItemTemplate item in itemTemplate.Children.Reverse())
			{
				stack.Push(item);
			}
		}
		return allChildrenDict[template] = list.ToArray();
	}
}
