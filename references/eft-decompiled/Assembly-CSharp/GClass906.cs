using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass906 : GInterface37
{
	public class Class528
	{
		[Serializable]
		[CompilerGenerated]
		public class Class529
		{
			public static readonly Class529 class529_0 = new Class529();

			public static Predicate<ItemAttributeClass> predicate_0;

			public static Predicate<ItemAttributeClass> predicate_1;

			public bool method_0(ItemAttributeClass attribute)
			{
				return attribute.Id.Equals(EItemAttributeId.Durability);
			}

			public bool method_1(ItemAttributeClass attribute)
			{
				return attribute.Id.Equals(EItemAttributeId.MaxDurability);
			}
		}

		public Item Item;

		public ItemAttributeClass Durability;

		public ItemAttributeClass MaxDurability;

		public RepairableComponent Repairable;

		public Class528(Item item)
		{
			Item = item;
			Durability = item.Attributes.Find((ItemAttributeClass attribute) => attribute.Id.Equals(EItemAttributeId.Durability));
			MaxDurability = item.Attributes.Find((ItemAttributeClass attribute) => attribute.Id.Equals(EItemAttributeId.MaxDurability));
			Repairable = item.GetItemComponent<RepairableComponent>();
		}

		public bool NotNull()
		{
			if (Item != null && Durability != null && MaxDurability != null)
			{
				return Repairable != null;
			}
			return false;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class530
	{
		public static readonly Class530 class530_0 = new Class530();

		public static Func<RepairableComponent, Item> func_0;

		public static Func<Item, Class528> func_1;

		public static Func<Class528, bool> func_2;

		public static Func<Class528, bool> func_3;

		public static Func<Class528, int> func_4;

		public static Func<Class528, float> func_5;

		public static Func<Class528, float> func_6;

		public static Func<Class528, float> func_7;

		public static Func<Class528, float> func_8;

		public Item method_0(RepairableComponent repairable)
		{
			return repairable.Item;
		}

		public Class528 method_1(Item subItem)
		{
			return new Class528(subItem);
		}

		public bool method_2(Class528 info)
		{
			return info.NotNull();
		}

		public bool method_3(Class528 repairable)
		{
			return repairable.Durability.Base() <= 1f;
		}

		public int method_4(Class528 repairableItem)
		{
			return repairableItem.Repairable.TemplateDurability;
		}

		public float method_5(Class528 repairable)
		{
			return repairable.Repairable.Durability;
		}

		public float method_6(Class528 subItem)
		{
			return subItem.Repairable.MaxDurability;
		}

		public float method_7(Class528 subItem)
		{
			return subItem.Durability?.Base() ?? 0f;
		}

		public float method_8(Class528 subItem)
		{
			return subItem.MaxDurability?.Base() ?? 0f;
		}
	}

	[CompilerGenerated]
	public class Class531
	{
		public GClass906 gclass906_0;

		public float repairAmount;

		public bool any;

		public double method_0(Class528 itemInfo)
		{
			if (!gclass906_0.CurrentRepairer.TryGetEnhancementChance(itemInfo.Item, repairAmount / (float)itemInfo.Repairable.TemplateDurability, out var enhancementChance))
			{
				return 0.0;
			}
			any = true;
			return enhancementChance;
		}
	}

	[CompilerGenerated]
	public class Class532
	{
		public GClass906 gclass906_0;

		public IRepairer repairer;

		public MongoID[] excludedCategories;

		public bool method_0(Class528 subItem)
		{
			return gclass906_0.method_1(repairer, subItem.Item, excludedCategories);
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public List<GClass904> List_0;

	[NonSerialized]
	public Item Item_0;

	[NonSerialized]
	public Class528[] Class528_0;

	[NonSerialized]
	public RepairControllerClass RepairControllerClass;

	[NonSerialized]
	public List<IRepairer> List_1;

	[NonSerialized]
	[CompilerGenerated]
	public IRepairer IRepairer;

	public List<GClass904> RepairKitsCollections
	{
		[CompilerGenerated]
		get
		{
			return List_0;
		}
	}

	public IEnumerable<IRepairer> Repairers
	{
		get
		{
			foreach (GClass904 repairKitsCollection in RepairKitsCollections)
			{
				yield return repairKitsCollection;
			}
			foreach (IRepairer item in List_1)
			{
				yield return item;
			}
		}
	}

	public IRepairer CurrentRepairer
	{
		[CompilerGenerated]
		get
		{
			return IRepairer;
		}
		[CompilerGenerated]
		set
		{
			IRepairer = value;
		}
	}

	public GClass906(Item item, RepairControllerClass repairController)
	{
		Item_0 = item;
		RepairControllerClass = repairController;
		if (!item.TryGetItemComponent<ArmorHolderComponent>(out var _))
		{
			throw new ArgumentException();
		}
		List<Item> list = (from repairable in GClass3380.GetItemComponentsInChildren<RepairableComponent>(item)
			select repairable.Item).OrderBy(method_0).ToList();
		Class528_0 = (from subItem in list
			select new Class528(subItem) into info
			where info.NotNull()
			select info).ToArray();
		List_1 = repairController.TraderRepairers.Where((TraderClass trader) => CanRepair(trader, trader.Targets)).OfType<IRepairer>().ToList();
		List_0 = new List<GClass904>();
		RepairKitsCollections.AddRange(repairController.GetSuitableRepairersCollections(list));
		CurrentRepairer = Repairers.FirstOrDefault();
	}

	public int method_0(Item item)
	{
		if (!item.TryGetItemComponent<ArmorComponent>(out var component))
		{
			return 0;
		}
		if (item.CurrentAddress is GClass3391 gClass && !gClass.Slot.Locked)
		{
			return -100;
		}
		foreach (EBodyPartColliderType armorCollider in component.ArmorColliders)
		{
			switch (armorCollider)
			{
			case EBodyPartColliderType.HeadCommon:
				return -30;
			case EBodyPartColliderType.ParietalHead:
				return -29;
			case EBodyPartColliderType.Eyes:
				return -28;
			case EBodyPartColliderType.Jaw:
				return -27;
			case EBodyPartColliderType.RibcageUp:
				return -26;
			case EBodyPartColliderType.RibcageLow:
				return -25;
			case EBodyPartColliderType.BackHead:
				return -24;
			case EBodyPartColliderType.Ears:
				return -23;
			case EBodyPartColliderType.NeckFront:
				return -22;
			case EBodyPartColliderType.SpineTop:
				return -21;
			case EBodyPartColliderType.SpineDown:
				return -20;
			case EBodyPartColliderType.NeckBack:
				return -19;
			case EBodyPartColliderType.Pelvis:
				return -18;
			case EBodyPartColliderType.PelvisBack:
				return -17;
			case EBodyPartColliderType.RightSideChestUp:
				return -16;
			case EBodyPartColliderType.LeftSideChestUp:
				return -15;
			case EBodyPartColliderType.RightSideChestDown:
				return -14;
			case EBodyPartColliderType.LeftSideChestDown:
				return -13;
			case EBodyPartColliderType.RightUpperArm:
				return -12;
			case EBodyPartColliderType.LeftUpperArm:
				return -11;
			case EBodyPartColliderType.RightForearm:
				return -10;
			case EBodyPartColliderType.LeftForearm:
				return -9;
			case EBodyPartColliderType.RightThigh:
				return -8;
			case EBodyPartColliderType.LeftThigh:
				return -7;
			case EBodyPartColliderType.RightCalf:
				return -6;
			case EBodyPartColliderType.LeftCalf:
				return -5;
			}
		}
		return 0;
	}

	public void AddRepairKitToRepairers(RepairKitsItemClass repairKit)
	{
		foreach (GClass904 repairKitsCollection in RepairKitsCollections)
		{
			if (repairKitsCollection.AddRepairKit(repairKit))
			{
				return;
			}
		}
		RepairKitsCollections.Insert(0, RepairControllerClass.CreateRepairKitsCollection(repairKit));
	}

	public bool TryGetEnhancementChance(float repairAmount, out double enhancementChance)
	{
		bool any = false;
		enhancementChance = Class528_0.Average(delegate(Class528 itemInfo)
		{
			if (!CurrentRepairer.TryGetEnhancementChance(itemInfo.Item, repairAmount / (float)itemInfo.Repairable.TemplateDurability, out var enhancementChance2))
			{
				return 0.0;
			}
			any = true;
			return enhancementChance2;
		});
		return any;
	}

	public string RepairabilityText(bool isRepairKit)
	{
		Vector2 vector = default(Vector2);
		Class528[] class528_ = Class528_0;
		foreach (Class528 @class in class528_)
		{
			vector += @class.Repairable.RepairDegradation;
		}
		vector /= (float)Class528_0.Length;
		float num = (vector.x + vector.y) / 2f;
		if (vector.y - vector.x > 0.5f)
		{
			return "Unpredictable";
		}
		if (num < 0.05f)
		{
			return "Very good";
		}
		if (num < 0.11f)
		{
			return "Good";
		}
		if (num < 0.19f)
		{
			return "Average";
		}
		if (num < 0.32f)
		{
			return "Bad";
		}
		if (num < 0.55f)
		{
			return "Very bad";
		}
		if (!(num < 0.85f))
		{
			return "Unpredictable";
		}
		return "Terrible";
	}

	public float GetRepairQualityForDisplay()
	{
		return Class528_0.Average((Class528 item) => (float)RepairControllerClass.GetBaseRepairQualityForDisplay(item.Item));
	}

	public float GetRepairQuality()
	{
		return Class528_0.Average((Class528 item) => CurrentRepairer.GetRepairQuality(item.Item));
	}

	public double GetRepairPrice(float repairAmountLeft, GClass904 repairKit)
	{
		double num = 0.0;
		Class528[] class528_ = Class528_0;
		foreach (Class528 @class in class528_)
		{
			if (repairAmountLeft <= Mathf.Epsilon)
			{
				break;
			}
			Vector2 degradationValues = CurrentRepairer.GetDegradationValues(@class.Repairable, @class.Item);
			float a = @class.Repairable.MaxDurability - degradationValues.x - @class.Repairable.Durability;
			a = Mathf.Min(a, repairAmountLeft);
			a = Mathf.Max(a, 0f);
			repairAmountLeft -= a;
			num += repairKit.GetRepairPrice(a, @class.Item);
		}
		return num;
	}

	public bool IsNoCorrespondingArea()
	{
		return !Class528_0.Any((Class528 item) => RepairControllerClass.IsRepairUnlocked(item.Item));
	}

	public bool IsEligibleForBuff()
	{
		return Class528_0.Any((Class528 item) => RepairControllerClass.IsEligibleForBuff(item.Item));
	}

	public bool BrokenItemError()
	{
		return Class528_0.All((Class528 repairable) => repairable.Durability.Base() <= 1f);
	}

	public int TemplateDurability()
	{
		return Class528_0.Sum((Class528 repairableItem) => repairableItem.Repairable.TemplateDurability);
	}

	public float Durability()
	{
		return Class528_0.Sum((Class528 repairable) => repairable.Repairable.Durability);
	}

	public float MaxDurability()
	{
		return Class528_0.Sum((Class528 subItem) => subItem.Repairable.MaxDurability);
	}

	public float HowMuchRepairScoresCanAccept()
	{
		float num = 0f;
		Class528[] class528_ = Class528_0;
		foreach (Class528 @class in class528_)
		{
			Vector2 degradationValues = CurrentRepairer.GetDegradationValues(@class.Repairable, @class.Item);
			float a = @class.Repairable.MaxDurability - degradationValues.x - @class.Repairable.Durability;
			a = Mathf.Max(a, 0f);
			num += a;
		}
		return num;
	}

	public (bool hasCommonBuff, bool hasRareBuff) GetBuffState()
	{
		BuffComponent itemComponent = Item_0.GetItemComponent<BuffComponent>();
		if (itemComponent != null && GClass866<ERepairBuffType>.IsDefined(itemComponent.BuffType))
		{
			if (itemComponent.Rarity != EBuffRarity.Common)
			{
				return (hasCommonBuff: false, hasRareBuff: true);
			}
			return (hasCommonBuff: true, hasRareBuff: false);
		}
		return (hasCommonBuff: false, hasRareBuff: false);
	}

	public float GetCurrentDurabilityFraction()
	{
		return Mathf.InverseLerp(0f, TemplateDurability(), Class528_0.Sum((Class528 subItem) => subItem.Durability?.Base() ?? 0f));
	}

	public float GetMaxDurabilityFraction()
	{
		return Mathf.InverseLerp(0f, TemplateDurability(), Class528_0.Sum((Class528 subItem) => subItem.MaxDurability?.Base() ?? 0f));
	}

	public Vector2 GetDegradationValues(float repairAmountLeft)
	{
		Vector2 result = default(Vector2);
		Class528[] class528_ = Class528_0;
		foreach (Class528 @class in class528_)
		{
			if (repairAmountLeft <= Mathf.Epsilon)
			{
				break;
			}
			Vector2 degradationValues = CurrentRepairer.GetDegradationValues(@class.Repairable, @class.Item);
			if (!(@class.Repairable.MaxDurability - @class.Repairable.Durability - degradationValues.x < 0.1f))
			{
				float a = @class.Repairable.MaxDurability - @class.Repairable.Durability;
				a = Mathf.Min(a, repairAmountLeft);
				repairAmountLeft -= a;
				result += degradationValues;
			}
		}
		return result;
	}

	public double GetPriceRate()
	{
		return Class528_0.Average((Class528 subItem) => CurrentRepairer.GetRepairPriceCoefficient(subItem.Item));
	}

	public bool CanRepair(IRepairer repairer, MongoID[] excludedCategories)
	{
		return Class528_0.Any((Class528 subItem) => method_1(repairer, subItem.Item, excludedCategories));
	}

	public bool method_1(IRepairer repairer, Item item, MongoID[] excludedCategories)
	{
		if (!(repairer is TraderClass))
		{
			if (!(repairer is GClass904) && repairer != null)
			{
				throw new NotImplementedException("IRepairer type is not specified!");
			}
			return ItemFilter.CheckItem(item, excludedCategories);
		}
		return !ItemFilter.CheckItem(item, excludedCategories);
	}

	public int GetCurrencyPrice(float repairAmountLeft)
	{
		int num = 0;
		Class528[] class528_ = Class528_0;
		foreach (Class528 @class in class528_)
		{
			if (repairAmountLeft <= Mathf.Epsilon)
			{
				break;
			}
			Vector2 degradationValues = CurrentRepairer.GetDegradationValues(@class.Repairable, @class.Item);
			float b = @class.Repairable.MaxDurability - degradationValues.x - @class.Repairable.Durability;
			b = Mathf.Max(0f, b);
			b = Mathf.Min(b, repairAmountLeft);
			repairAmountLeft -= b;
			num += method_2(@class.Item, b);
		}
		return num;
	}

	public int method_2(Item item, float amount)
	{
		return (int)Math.Floor(((double)item.RepairCost + (double)item.RepairCost * CurrentRepairer.GetRepairPriceCoefficient(item) / 100.0) * (double)amount * (double)CurrentRepairer.CurrencyCoefficient);
	}

	public async Task<IResult> RepairItem(float repairAmount, RepairKitsItemClass draggedRepairKit)
	{
		float num = repairAmount;
		Class528[] class528_ = Class528_0;
		foreach (Class528 @class in class528_)
		{
			if (num <= Mathf.Epsilon)
			{
				break;
			}
			Vector2 degradationValues = CurrentRepairer.GetDegradationValues(@class.Repairable, @class.Item);
			float a = @class.Repairable.MaxDurability - degradationValues.x - @class.Repairable.Durability;
			a = Mathf.Min(a, num);
			num -= a;
			if (a > Mathf.Epsilon)
			{
				if (@class.Repairable == null)
				{
					return new FailedResult("You can't repair this kind of item: " + @class.GetType());
				}
				if (CurrentRepairer is GClass904 gClass)
				{
					a = (float)gClass.GetRepairPrice(a, @class.Item);
				}
				IResult result = await CurrentRepairer.RepairItems(new RepairItem(@class.Item.Id, a), draggedRepairKit);
				if (!result.Succeed)
				{
					return result;
				}
				@class.Item.UpdateAttributes();
				@class.Item.RaiseRefreshEvent(refreshIcon: true);
			}
		}
		return new SuccessfulResult();
	}

	[CompilerGenerated]
	public bool method_3(TraderClass trader)
	{
		return CanRepair(trader, trader.Targets);
	}

	[CompilerGenerated]
	public float method_4(Class528 item)
	{
		return (float)RepairControllerClass.GetBaseRepairQualityForDisplay(item.Item);
	}

	[CompilerGenerated]
	public float method_5(Class528 item)
	{
		return CurrentRepairer.GetRepairQuality(item.Item);
	}

	[CompilerGenerated]
	public bool method_6(Class528 item)
	{
		return RepairControllerClass.IsRepairUnlocked(item.Item);
	}

	[CompilerGenerated]
	public bool method_7(Class528 item)
	{
		return RepairControllerClass.IsEligibleForBuff(item.Item);
	}

	[CompilerGenerated]
	public double method_8(Class528 subItem)
	{
		return CurrentRepairer.GetRepairPriceCoefficient(subItem.Item);
	}
}
