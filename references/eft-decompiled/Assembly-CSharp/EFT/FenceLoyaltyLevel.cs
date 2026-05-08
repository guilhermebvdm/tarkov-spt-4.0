using System;
using Newtonsoft.Json;
using UnityEngine;

namespace EFT;

[Serializable]
public class FenceLoyaltyLevel
{
	[JsonProperty("HostileBosses")]
	public readonly bool HostileBosses = true;

	[JsonProperty("ScavAttackSupport")]
	public readonly bool ScavAttackSupport;

	[JsonProperty("ReactOnMarkOnUnknowns")]
	public readonly bool ReactOnMarkOnUnknowns;

	[JsonProperty("HostileScavs")]
	public readonly bool HostileScavs;

	[JsonProperty("BotSpreadoutChance")]
	public readonly float BotSpreadoutChance;

	[JsonProperty("BotGetInCoverChance")]
	public readonly float BotGetInCoverChance;

	[JsonProperty("BotApplySilenceChance")]
	public readonly float BotApplySilenceChance;

	[JsonProperty("BotStopChance")]
	public readonly float BotStopChance;

	[JsonProperty("BotHelpChance")]
	public readonly float BotHelpChance;

	[JsonProperty("BotFollowChance")]
	public readonly float BotFollowChance;

	[JsonProperty("SavageCooldownModifier")]
	public readonly float SavageCooldownModifier = 1f;

	[JsonProperty("ScavCaseTimeModifier")]
	public readonly float ScavCaseTimeModifier = 1f;

	[JsonProperty("PaidExitCostModifier")]
	public readonly float PaidExitCostModifier = 1f;

	[JsonProperty("ScavEquipmentSpawnChanceModifier")]
	public readonly float ScavEquipmentSpawnChanceModifier = 1f;

	[JsonProperty("PriceModifier")]
	public readonly float PriceModifier = 1f;

	[JsonProperty("ExfiltrationPriceModifier")]
	public readonly float ExfiltrationPriceModifier = 1f;

	[JsonProperty("AvailableExits")]
	public readonly int AvailableExits = 2;

	[JsonProperty("PriceModTaxi")]
	public readonly float PriceModTaxi = 1f;

	[JsonProperty("PriceModDelivery")]
	public readonly float PriceModDelivery = 1f;

	[JsonProperty("PriceModCleanUp")]
	public readonly float PriceModCleanUp = 1f;

	[JsonProperty("CanInteractWithBtr")]
	public readonly bool CanInteractWithBtr = true;

	[JsonProperty("DeliveryGridSize")]
	public readonly Vector3 DeliveryGridSize = new Vector3(1f, 1f, 0f);

	[JsonProperty("CircleOfCultistsBonusPercent")]
	public readonly float CircleOfCultistsBonusPercent;

	[JsonProperty("TransitGridSize")]
	public readonly Vector3 TransitGridSize = new Vector3(1f, 1f, 0f);

	[field: NonSerialized]
	public double StandingRequired { get; set; }

	[field: NonSerialized]
	public bool IsMaxLevel { get; set; }

	[field: NonSerialized]
	public bool IsMinLevel { get; set; }

	public void Init(double minStanding, bool first, bool last)
	{
		StandingRequired = minStanding;
		IsMinLevel = first;
		IsMaxLevel = last;
	}
}
