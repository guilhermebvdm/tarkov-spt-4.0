using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass905 : GInterface37
{
	[Serializable]
	[CompilerGenerated]
	public class Class526
	{
		public static readonly Class526 class526_0 = new Class526();

		public static Predicate<ItemAttributeClass> predicate_0;

		public static Predicate<ItemAttributeClass> predicate_1;

		public bool method_0(ItemAttributeClass x)
		{
			return x.Id.Equals(EItemAttributeId.Durability);
		}

		public bool method_1(ItemAttributeClass x)
		{
			return x.Id.Equals(EItemAttributeId.MaxDurability);
		}
	}

	[NonSerialized]
	public Item Item_0;

	[NonSerialized]
	public RepairableComponent RepairableComponent_0;

	[NonSerialized]
	public ItemAttributeClass ItemAttributeClass;

	[NonSerialized]
	public ItemAttributeClass ItemAttributeClass_1;

	[NonSerialized]
	public List<IRepairer> List_0;

	[NonSerialized]
	public RepairControllerClass RepairControllerClass;

	[NonSerialized]
	[CompilerGenerated]
	public List<GClass904> List_1;

	[NonSerialized]
	[CompilerGenerated]
	public IRepairer IRepairer;

	public List<GClass904> RepairKitsCollections
	{
		[CompilerGenerated]
		get
		{
			return List_1;
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
			foreach (IRepairer item in List_0)
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

	public GClass905(Item item, RepairControllerClass repairController)
	{
		Item_0 = item;
		RepairControllerClass = repairController;
		List_0 = repairController.TraderRepairers.Where((TraderClass trader) => CanRepair(trader, trader.Targets)).OfType<IRepairer>().ToList();
		RepairableComponent_0 = Item_0.GetItemComponent<RepairableComponent>();
		List_1 = repairController.GetSuitableRepairersCollections(item).ToList();
		CurrentRepairer = Repairers.FirstOrDefault();
		ItemAttributeClass = Item_0.Attributes.Find((ItemAttributeClass x) => x.Id.Equals(EItemAttributeId.Durability));
		ItemAttributeClass_1 = Item_0.Attributes.Find((ItemAttributeClass x) => x.Id.Equals(EItemAttributeId.MaxDurability));
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

	public float Durability()
	{
		return RepairableComponent_0.Durability;
	}

	public float MaxDurability()
	{
		return RepairableComponent_0.MaxDurability;
	}

	public float GetCurrentDurabilityFraction()
	{
		return Mathf.InverseLerp(0f, TemplateDurability(), ItemAttributeClass?.Base() ?? 0f);
	}

	public float GetMaxDurabilityFraction()
	{
		return Mathf.InverseLerp(0f, TemplateDurability(), ItemAttributeClass_1?.Base() ?? 0f);
	}

	public float HowMuchRepairScoresCanAccept()
	{
		return RepairableComponent_0.MaxDurability - GetDegradationValues(0f).x - RepairableComponent_0.Durability;
	}

	public bool TryGetEnhancementChance(float repairAmount, out double enhancementChance)
	{
		return CurrentRepairer.TryGetEnhancementChance(Item_0, repairAmount / (float)RepairableComponent_0.TemplateDurability, out enhancementChance);
	}

	public string RepairabilityText(bool isRepairKit)
	{
		Vector2 repairDegradation = RepairableComponent_0.RepairDegradation;
		float num = (repairDegradation.x + repairDegradation.y) / 2f;
		if (repairDegradation.y - repairDegradation.x > 0.5f)
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
		return (float)RepairControllerClass.GetBaseRepairQualityForDisplay(Item_0);
	}

	public float GetRepairQuality()
	{
		return CurrentRepairer.GetRepairQuality(Item_0);
	}

	public double GetRepairPrice(float repairValue, GClass904 repairKit)
	{
		return repairKit.GetRepairPrice(repairValue, Item_0);
	}

	public bool IsNoCorrespondingArea()
	{
		return !RepairControllerClass.IsRepairUnlocked(Item_0);
	}

	public bool IsEligibleForBuff()
	{
		return RepairControllerClass.IsEligibleForBuff(Item_0);
	}

	public bool BrokenItemError()
	{
		return ItemAttributeClass.Base() <= 1f;
	}

	public int TemplateDurability()
	{
		return RepairableComponent_0.TemplateDurability;
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

	public Vector2 GetDegradationValues(float repairAmount)
	{
		return CurrentRepairer.GetDegradationValues(RepairableComponent_0, Item_0);
	}

	public double GetPriceRate()
	{
		return CurrentRepairer.GetRepairPriceCoefficient(Item_0);
	}

	public bool CanRepair(IRepairer repairer, MongoID[] excludedCategories)
	{
		if (!(repairer is TraderClass))
		{
			if (!(repairer is GClass904) && repairer != null)
			{
				throw new NotImplementedException("IRepairer type is not specified!");
			}
			return ItemFilter.CheckItem(Item_0, excludedCategories);
		}
		return !ItemFilter.CheckItem(Item_0, excludedCategories);
	}

	public int GetCurrencyPrice(float amount)
	{
		return (int)Math.Floor(((double)Item_0.RepairCost + (double)Item_0.RepairCost * CurrentRepairer.GetRepairPriceCoefficient(Item_0) / 100.0) * (double)amount * (double)CurrentRepairer.CurrencyCoefficient);
	}

	public async Task<IResult> RepairItem(float repairAmount, RepairKitsItemClass draggedRepairKit)
	{
		if (Item_0.GetItemComponent<RepairableComponent>() == null)
		{
			return new FailedResult("You can't repair this kind of item: " + Item_0.GetType());
		}
		if (repairAmount <= 0f)
		{
			return new FailedResult("Zero repair amount");
		}
		if (CurrentRepairer is GClass904 gClass)
		{
			repairAmount = (float)gClass.GetRepairPrice(repairAmount, Item_0);
		}
		IResult result = await CurrentRepairer.RepairItems(new RepairItem(Item_0.Id, repairAmount), draggedRepairKit);
		if (!result.Succeed)
		{
			return result;
		}
		Item_0.UpdateAttributes();
		Item_0.RaiseRefreshEvent(refreshIcon: true);
		return result;
	}

	[CompilerGenerated]
	public bool method_0(TraderClass trader)
	{
		return CanRepair(trader, trader.Targets);
	}
}
