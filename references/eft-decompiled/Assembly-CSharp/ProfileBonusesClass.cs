using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;

[GAttribute29]
public class ProfileBonusesClass
{
	[JsonProperty("type")]
	public EBonusType BonusType;

	[JsonProperty("id")]
	public MongoID Id;

	[JsonProperty("value")]
	public double Value;

	[JsonProperty("visible")]
	public bool IsVisible = true;

	[JsonProperty("passive")]
	public bool Passive = true;

	[JsonProperty("production")]
	public bool Production;

	[JsonProperty("icon")]
	public string Icon;

	[GAttribute30]
	[JsonProperty("filter", NullValueHandling = NullValueHandling.Ignore)]
	public List<MongoID> Filters;

	[JsonProperty("skillType", NullValueHandling = NullValueHandling.Ignore)]
	public ESkillClass? SkillType;

	[JsonProperty("skillName", NullValueHandling = NullValueHandling.Ignore)]
	public ESkillId? SkillName;

	[JsonProperty("templateId", NullValueHandling = NullValueHandling.Ignore)]
	public MongoID? TemplateId;

	public SkillBonusAbstractClass ToBonus()
	{
		return BonusType switch
		{
			EBonusType.EnergyRegeneration => new GClass1819(this), 
			EBonusType.HydrationRegeneration => new GClass1820(this), 
			EBonusType.HealthRegeneration => new GClass1821(this), 
			EBonusType.MaximumEnergyReserve => new GClass1834(this), 
			EBonusType.ExperienceRate => new GClass1829(this), 
			EBonusType.QuestMoneyReward => new GClass1823(this), 
			EBonusType.ScavCooldownTimer => new GClass1825(this), 
			EBonusType.StashSize => new GClass1816(this), 
			EBonusType.UnlockItemCraft => new GClass1844(this), 
			EBonusType.UnlockItemPassiveCreation => new GClass1845(this), 
			EBonusType.UnlockRandomItemCreation => new GClass1846(this), 
			EBonusType.SkillGroupLevelingBoost => new GClass1827(this), 
			EBonusType.SkillLevelingBoost => new GClass1828(this), 
			EBonusType.DebuffEndDelay => new GClass1822(this), 
			EBonusType.RagfairCommission => new GClass1826(this), 
			EBonusType.InsuranceReturnTime => new GClass1824(this), 
			EBonusType.UnlockWeaponModification => new GClass1841(this), 
			EBonusType.UnlockScavPlay => new GClass1840(this), 
			EBonusType.UnlockAddOffer => new GClass1842(this), 
			EBonusType.UnlockItemCharge => new GClass1847(this), 
			EBonusType.ReceiveItemBonus => new GClass1836(this), 
			EBonusType.UnlockUniqueId => new GClass1843(this), 
			EBonusType.IncreaseCanisterSlots => new GClass1837(this), 
			EBonusType.AdditionalSlots => new GClass1838(this), 
			EBonusType.FuelConsumption => new GClass1830(this), 
			EBonusType.TextBonus => new GClass1850(this), 
			EBonusType.RepairWeaponBonus => new GClass1831(this), 
			EBonusType.RepairArmorBonus => new GClass1832(this), 
			EBonusType.UnlockWeaponRepair => new GClass1848(this), 
			EBonusType.UnlockArmorRepair => new GClass1849(this), 
			EBonusType.StashRows => new GClass1835(this), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
