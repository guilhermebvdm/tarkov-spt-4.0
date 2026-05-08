using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;

public interface GInterface37
{
	List<GClass904> RepairKitsCollections { get; }

	IEnumerable<IRepairer> Repairers { get; }

	IRepairer CurrentRepairer { get; set; }

	bool TryGetEnhancementChance(float repairAmount, out double enhancementChance);

	string RepairabilityText(bool isRepairKit);

	int TemplateDurability();

	float Durability();

	float MaxDurability();

	float GetCurrentDurabilityFraction();

	float GetMaxDurabilityFraction();

	float HowMuchRepairScoresCanAccept();

	(bool hasCommonBuff, bool hasRareBuff) GetBuffState();

	Vector2 GetDegradationValues(float repairAmount);

	double GetPriceRate();

	bool CanRepair(IRepairer repairer, MongoID[] excludedCategories);

	float GetRepairQualityForDisplay();

	float GetRepairQuality();

	double GetRepairPrice(float repairValue, GClass904 repairKit);

	bool IsNoCorrespondingArea();

	bool IsEligibleForBuff();

	bool BrokenItemError();

	int GetCurrencyPrice(float amount);

	void AddRepairKitToRepairers(RepairKitsItemClass repairKit);

	Task<IResult> RepairItem(float repairAmount, RepairKitsItemClass draggedRepairKit);
}
