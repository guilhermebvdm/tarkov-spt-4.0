using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using UnityEngine;
using UnityEngine.UI;

public class GClass904 : IRepairer
{
	[Serializable]
	[CompilerGenerated]
	public class Class525
	{
		public static readonly Class525 class525_0 = new Class525();

		public static Func<RepairKitsItemClass, float> func_0;

		public static Func<RepairKitsItemClass, float> func_1;

		public float method_0(RepairKitsItemClass repairKit)
		{
			return repairKit.Resource;
		}

		public float method_1(RepairKitsItemClass repairKit)
		{
			return repairKit.Resource;
		}
	}

	[NonSerialized]
	public RepairControllerClass RepairControllerClass;

	[NonSerialized]
	public RepairKitsTemplateClass RepairKitsTemplateClass;

	[NonSerialized]
	public HashSet<RepairKitsItemClass> HashSet_0;

	[NonSerialized]
	public Sprite Sprite_0;

	[NonSerialized]
	public static MongoID MongoID_0 = MongoID.Generate();

	public string RepairerId => RepairKitsTemplateClass._id;

	public MongoID[] Targets => RepairKitsTemplateClass.TargetItemFilter;

	public string LocalizedName => GClass2348.Localized(RepairKitsTemplateClass.ShortNameLocalizationKey);

	public double Standing => 0.0;

	public int LoyaltyLevel => 0;

	public int MaxLoyaltyLevel => 0;

	public float CurrencyCoefficient => 1f;

	public GClass904(RepairKitsTemplateClass template, RepairControllerClass repairController)
	{
		RepairKitsTemplateClass = template;
		RepairControllerClass = repairController;
		HashSet_0 = new HashSet<RepairKitsItemClass>();
	}

	public bool AddRepairKit(RepairKitsItemClass repairKit)
	{
		if (repairKit.TemplateId != (MongoID)RepairerId)
		{
			return false;
		}
		HashSet_0.Add(repairKit);
		return true;
	}

	public bool RemoveRepairKit(RepairKitsItemClass repairKit)
	{
		if (repairKit.TemplateId == (MongoID)RepairerId)
		{
			return HashSet_0.Remove(repairKit);
		}
		return false;
	}

	public RepairKitsItemClass SelectMinRepairKit(RepairKitsItemClass draggedRepairKit)
	{
		if (draggedRepairKit != null && GClass855.Positive(draggedRepairKit.Resource))
		{
			return draggedRepairKit;
		}
		if (GClass856.IsNullOrEmpty(HashSet_0))
		{
			return null;
		}
		RepairKitsItemClass repairKitsItemClass = HashSet_0.First();
		foreach (RepairKitsItemClass item in HashSet_0)
		{
			if (repairKitsItemClass.Resource > item.Resource)
			{
				repairKitsItemClass = item;
			}
		}
		return repairKitsItemClass;
	}

	public async Task<bool> GetAndAssignAvatar(Image image, CancellationToken cancellationToken)
	{
		if (Sprite_0 == null)
		{
			Item item = HashSet_0.FirstOrDefault();
			if (item == null)
			{
				item = Singleton<ItemFactoryClass>.Instance.CreateItem(MongoID_0, RepairerId, null);
			}
			Sprite_0 = ItemViewFactory.LoadItemIcon(item).Sprite;
			if (Sprite_0 == null)
			{
				Sprite_0 = await ItemViewFactory.GetItemSpriteAsync(item);
			}
		}
		image.sprite = Sprite_0;
		return true;
	}

	public float GetRepairQuality(Item item)
	{
		return (float)RepairControllerClass.GetBaseRepairQuality(item);
	}

	public float GetRepairQualityForDisplay(Item item)
	{
		return (float)RepairControllerClass.GetBaseRepairQualityForDisplay(item);
	}

	public double GetRepairPriceCoefficient(Item item)
	{
		return (float)RepairControllerClass.GetBaseRepairPriceCoefficient(item);
	}

	public float GetRepairPoints()
	{
		return (float)Math.Round(HashSet_0.Select((RepairKitsItemClass repairKit) => repairKit.Resource).Sum(), 1);
	}

	public bool TryGetEnhancementChance(Item item, float receiveDurabilityPercent, out double enhancementChance)
	{
		return RepairControllerClass.TryGetEnhancementChance(item, receiveDurabilityPercent, out enhancementChance);
	}

	public Task<IResult> RepairItems(RepairItem repairItem, RepairKitsItemClass draggedRepairKit)
	{
		List<RepairKitsItemClass> list = HashSet_0.OrderBy((RepairKitsItemClass repairKit) => repairKit.Resource).ToList();
		if (draggedRepairKit != null)
		{
			list.Remove(draggedRepairKit);
			list.Insert(0, draggedRepairKit);
		}
		List<RepairItem> list2 = new List<RepairItem>();
		float num = repairItem.Count;
		foreach (RepairKitsItemClass item in list)
		{
			float resource = item.Resource;
			if (!(resource <= 0f))
			{
				if (!(resource < num))
				{
					item.RepairKitComponent.Resource -= num;
					list2.Add(new RepairItem(item.Id, num));
					break;
				}
				num -= resource;
				list2.Add(new RepairItem(item.Id, resource));
			}
		}
		return RepairControllerClass.RepairItemsByRepairKit(list2.ToArray(), repairItem.Id);
	}

	public Vector2 GetDegradationValues(RepairableComponent repairable, Item item)
	{
		Vector2 result = (item.IsArmorMod() ? 1f : GetRepairQuality(item)) * (float)repairable.TemplateDurability * repairable.RepairKitDegradation;
		if (RepairControllerClass.HasEliteNoWearRepair(item))
		{
			result.x = 0f;
		}
		return result;
	}

	public double GetRepairPrice(double repairValue, Item itemToRepair)
	{
		return RepairControllerClass.GetRepairPrice(RepairKitsTemplateClass, repairValue, itemToRepair);
	}
}
