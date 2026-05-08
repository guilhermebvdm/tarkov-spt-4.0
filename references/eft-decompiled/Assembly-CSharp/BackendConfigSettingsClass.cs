using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommonAssets.Scripts.ArtilleryShelling;
using EFT;
using EFT.Counters;
using EFT.GameTriggers;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.SynchronizableObjects;
using EFT.Vaulting;
using EFT.Vehicle;
using EFT.WeaponMounting;
using JetBrains.Annotations;
using JsonType;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class BackendConfigSettingsClass
{
	[Serializable]
	public class ServiceData
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1120
		{
			public static readonly Class1120 class1120_0 = new Class1120();

			public static Func<KeyValuePair<string, int>, SubService> func_0;

			public SubService method_0(KeyValuePair<string, int> subServiceData)
			{
				return new SubService(subServiceData.Key, subServiceData.Value);
			}
		}

		[JsonProperty("TraderId")]
		[field: NonSerialized]
		public MongoID TraderId { get; set; }

		[JsonProperty("TraderServiceType")]
		[field: NonSerialized]
		public ETraderServiceType ServiceType { get; set; }

		[JsonProperty("SubServices")]
		[field: NonSerialized]
		public SubService[] SubServices { get; set; } = Array.Empty<SubService>();

		[JsonProperty("Requirements")]
		[field: NonSerialized]
		public ServiceRequirements TraderServiceRequirements { get; set; }

		[JsonProperty("ServiceItemCost")]
		[field: NonSerialized]
		public GClass2638<int> ServiceItemCost { get; set; }

		[JsonProperty("UniqueItems")]
		[field: NonSerialized]
		public MongoID[] UniqueItems { get; set; }

		public ServiceData(TraderServicesClass serviceData, ServiceRequirements requirements = null)
		{
			ServiceType = serviceData.ServiceType;
			TraderId = serviceData.TraderId;
			TraderServiceRequirements = requirements ?? new ServiceRequirements();
			ServiceItemCost = serviceData.ItemsToPay;
			UniqueItems = serviceData.UniqueItems;
			SubServices = serviceData.SubServices?.Select((KeyValuePair<string, int> subServiceData) => new SubService(subServiceData.Key, subServiceData.Value)).ToArray();
		}

		[JsonConstructor]
		public ServiceData()
		{
		}
	}

	[Serializable]
	public class FenceGlobalSettings
	{
		[JsonProperty("FenceId")]
		public MongoID FenceId;

		[JsonProperty("Levels")]
		public readonly SortedDictionary<double, FenceLoyaltyLevel> LoyaltyLevels;

		[JsonProperty("PmcBotKillStandingMultiplier")]
		public float PmcBotKillStandingMultiplier;

		[OnDeserialized]
		public void method_0(StreamingContext context)
		{
			int count = LoyaltyLevels.Count;
			int num = 0;
			foreach (var (minStanding, fenceLoyaltyLevel2) in LoyaltyLevels)
			{
				fenceLoyaltyLevel2.Init(minStanding, num == 0, num == count - 1);
				num++;
			}
		}

		public FenceLoyaltyLevel GetSettings(double standing)
		{
			FenceLoyaltyLevel result = LoyaltyLevels.First().Value;
			foreach (var (num2, fenceLoyaltyLevel2) in LoyaltyLevels)
			{
				if (num2 <= standing)
				{
					result = fenceLoyaltyLevel2;
				}
			}
			return result;
		}
	}

	public interface GInterface168
	{
		int ProfileLevel { get; }

		long SalesSum { get; }

		double Standing { get; }
	}

	[Serializable]
	public struct TraderLoyaltyLevel
	{
		[JsonProperty("minLevel")]
		public readonly int MinProfileLevel;

		[JsonProperty("minSalesSum")]
		public readonly long MinSalesSum;

		[JsonProperty("minStanding")]
		public readonly double MinStanding;

		[JsonProperty("buy_price_coef")]
		public readonly double SellToTraderPriceCoef;

		[JsonProperty("repair_price_coef")]
		public readonly double RepairPriceCoef;

		[JsonProperty("insurance_price_coef")]
		public readonly double InsurancePriceCoef;

		[JsonProperty("exchange_price_coef")]
		public readonly double ExchangePriceCoef;

		[JsonProperty("heal_price_coef")]
		public readonly float HealPriceCoef;

		public bool MeetsRequirements(GInterface168 info)
		{
			if (MinSalesSum <= info.SalesSum && MinProfileLevel <= info.ProfileLevel)
			{
				return MinStanding <= info.Standing;
			}
			return false;
		}
	}

	[Serializable]
	public class TraderSettings
	{
		[Serializable]
		public class HandbookEntities
		{
			public readonly IEnumerable<MongoID> Items;

			public readonly IEnumerable<MongoID> Categories;

			public HandbookEntities()
				: this(Enumerable.Empty<MongoID>(), Enumerable.Empty<MongoID>())
			{
			}

			[JsonConstructor]
			public HandbookEntities([JsonProperty("id_list")] MongoID[] items, [JsonProperty("category")] MongoID[] categories)
				: this((IEnumerable<MongoID>)items, (IEnumerable<MongoID>)categories)
			{
			}

			public HandbookEntities(IReadOnlyCollection<MongoID> entities)
			{
				Items = entities;
				Categories = entities;
			}

			public HandbookEntities(IEnumerable<MongoID> items, IEnumerable<MongoID> categories)
			{
				Items = items;
				Categories = categories;
			}

			public virtual bool Contains(ItemTemplate itemTemplate)
			{
				MongoID id = itemTemplate._id;
				foreach (MongoID item in Items)
				{
					if (item == id)
					{
						return true;
					}
				}
				foreach (MongoID category in Categories)
				{
					if (itemTemplate.IsChildOf(category))
					{
						return true;
					}
				}
				return false;
			}
		}

		[JsonProperty("_id")]
		public string Id;

		[JsonProperty("avatar")]
		public string AvatarURL;

		[JsonProperty("balance_rub")]
		public long BalanceRUB;

		[JsonProperty("balance_dol")]
		public long BalanceUSD;

		[JsonProperty("balance_eur")]
		public long BalanceEUR;

		[JsonProperty("discount")]
		public float Discount;

		[JsonProperty("discount_end")]
		public long DiscountEnd;

		[JsonProperty("buyer_up")]
		public bool BuyerUp;

		[JsonProperty("sell_modifier_for_prohibited_items")]
		public int ProhibitedItemsSellModifier;

		[JsonProperty("currency")]
		public ECurrencyType Currency;

		[JsonProperty("repair")]
		public TraderRepair Repair;

		[JsonProperty("insurance")]
		public TraderInsurance Insurance;

		[JsonProperty("customization_seller")]
		public bool CustomizationSeller;

		[JsonProperty("medic")]
		public bool Medic;

		[JsonProperty("availableInRaid")]
		public bool AvailableInRaid = true;

		[JsonProperty("unlockedByDefault")]
		public bool UnlockedByDefault;

		[JsonProperty("nextResupply")]
		public int NextResupply;

		[JsonProperty("mainDialogue")]
		public MongoID? MainDialog;

		[JsonProperty("isCanTransferItems")]
		public bool CanTransferItems;

		[JsonProperty("isCanTransferItemsFromPve")]
		public bool CanTransferItemsFromPve;

		[JsonProperty("loyaltyLevels")]
		public TraderLoyaltyLevel[] LoyaltyLevels;

		[NonSerialized]
		public Sprite Avatar;

		[NonSerialized]
		public Func<Task<Texture2D>> AvatarGetter;

		[NonSerialized]
		public Task<Texture2D> AvatarTask;

		[JsonProperty("items_buy")]
		[field: NonSerialized]
		public HandbookEntities BuysItems { get; set; }

		[JsonProperty("items_sell")]
		[field: NonSerialized]
		public Dictionary<int, HandbookEntities> SellItems { get; set; }

		[JsonProperty("items_buy_prohibited")]
		[field: NonSerialized]
		public HandbookEntities DoesNotBuyItems { get; set; }

		[JsonProperty("transferableItems")]
		[field: NonSerialized]
		public HandbookEntities TransferableItems { get; set; }

		[JsonProperty("prohibitedTransferableItems")]
		[field: NonSerialized]
		public HandbookEntities ProhibitedTransferableItems { get; set; }

		public string FirstName => Id + " FirstName";

		public string FullName => Id + " FullName";

		public string Nickname => Id + " Nickname";

		public string Description => Id + " Description";

		public string Location => Id + " Location";

		public int GetLoyaltyLevel(GInterface168 info)
		{
			int num = LoyaltyLevels.Length - 1;
			while (true)
			{
				if (num >= 0)
				{
					if (LoyaltyLevels[num].MeetsRequirements(info))
					{
						break;
					}
					num--;
					continue;
				}
				return 1;
			}
			return num + 1;
		}

		public TraderLoyaltyLevel? GetLoyaltyLevelSettings(int loyaltyLevel)
		{
			loyaltyLevel--;
			if (LoyaltyLevels.Length > loyaltyLevel)
			{
				return LoyaltyLevels[loyaltyLevel];
			}
			return null;
		}

		public void InitSession(Func<Task<Texture2D>> avatarGetter)
		{
			AvatarGetter = avatarGetter;
		}

		public async Task<Sprite> GetAvatar()
		{
			if (Avatar != null)
			{
				return Avatar;
			}
			if (AvatarGetter == null)
			{
				return null;
			}
			if (AvatarTask == null)
			{
				AvatarTask = AvatarGetter();
			}
			Texture2D texture2D = await AvatarTask;
			Avatar = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
			AvatarTask = null;
			return Avatar;
		}

		public Task<bool> GetAndAssignAvatar(Image image, CancellationToken cancellationToken)
		{
			return method_0(image, cancellationToken);
		}

		public async Task<bool> method_0(Image image, CancellationToken cancellationToken)
		{
			Sprite sprite = await GetAvatar();
			if (!cancellationToken.IsCancellationRequested && !(sprite == null) && !(image == null))
			{
				image.sprite = sprite;
				image.color = new Color(1f, 1f, 1f, 1f);
				return true;
			}
			return false;
		}
	}

	[Serializable]
	public class TraderRepair
	{
		[JsonProperty("quality")]
		public float Quality;

		[JsonProperty("excluded_category")]
		public MongoID[] ExcludedCategories;

		[JsonProperty("currency")]
		public string Currency;

		[JsonProperty("currency_coefficient")]
		public float CurrencyCoefficient;

		[JsonProperty("availability")]
		public bool Availability;
	}

	[Serializable]
	public class TraderInsurance
	{
		[JsonProperty("availability")]
		public bool Availability;

		[JsonProperty("min_payment")]
		public int MinPayment;

		[JsonProperty("min_return_hour")]
		public int MinReturnHours;

		[JsonProperty("max_return_hour")]
		public int MaxReturnHours;

		[JsonProperty("excluded_category")]
		public string[] ExcludedCategories;
	}

	public class GClass1717
	{
		public class GClass1718
		{
			public string[] Filter = new string[0];

			public ERepairBuffType[] BuffTypes = new ERepairBuffType[0];
		}

		public class GClass1719
		{
			public float PriceModifier = 1f;
		}

		public int armorClassDivisor = 3;

		public float durabilityPointCostArmor = 0.5f;

		public float durabilityPointCostGuns = 0.5f;

		public Dictionary<ERepairStrategyType, GClass1718> RepairStrategies;

		public float currentDurabilityLossToRemoveBuff = 0.33f;

		public float maxDurabilityLossToRemoveBuff = 0.05f;

		public int minimumLevelToApplyBuff = 25;

		public Dictionary<ERepairBuffType, GClass1719> ItemEnhancementSettings;
	}

	public class GClass1720
	{
		public class GClass1721
		{
			[JsonProperty("k_exp")]
			public float Mult;
		}

		public class GClass1722
		{
			public class GClass1723
			{
				[JsonProperty("percent")]
				public int Percent;
			}

			[JsonProperty("victimLevelExp")]
			public int VictimLevelExp;

			[JsonProperty("victimBotLevelExp")]
			public int VictimBotLevelExp;

			[JsonProperty("pmcHeadShotMult")]
			public float PmcHeadShotMult;

			[JsonProperty("botHeadShotMult")]
			public float BotHeadShotMult;

			[JsonProperty("pmcExpOnDamageAllHealth")]
			public float PmcExpOnDamageAllHealth;

			[JsonProperty("botExpOnDamageAllHealth")]
			public float BotExpOnDamageAllHealth;

			[JsonProperty("longShotDistance")]
			public float LongShotDistance;

			[JsonProperty("bloodLossToLitre")]
			public float BloodLossToLitre;

			[JsonProperty("combo")]
			public GClass1723[] Combo;

			public int GetKillingBonusPercent(int killed)
			{
				int num = Mathf.Clamp(killed - 1, 0, Combo.Length - 1);
				return Combo[num].Percent;
			}
		}

		public class GClass1724
		{
			[JsonProperty("exp_table")]
			public global::QuestDictionaryClass<int> Table;

			[JsonProperty("mastering1")]
			public int Mastering1;

			[JsonProperty("mastering2")]
			public int Mastering2;
		}

		public class GClass1725
		{
			[JsonProperty("expForHeal")]
			public float ExpForHeal;

			[JsonProperty("expForEnergy")]
			public float ExpForEnergy;

			[JsonProperty("expForHydration")]
			public float ExpForHydration;
		}

		public class GClass1726
		{
			public class GClass1727
			{
				[JsonProperty("mlp")]
				public float mlp;
			}

			[JsonProperty("survived_exp_requirement")]
			public int SurvivedExpRequirement;

			[JsonProperty("survived_seconds_requirement")]
			public int SurvivedTimeRequirement = 600;

			[JsonProperty("survived_exp_reward")]
			public int SurvivedReward;

			[JsonProperty("transit_exp_reward")]
			public int TransitReward;

			[JsonProperty("mia_exp_reward")]
			public int MiaReward;

			[JsonProperty("runner_exp_reward")]
			public int RunnerReward;

			[JsonProperty("leftMult")]
			public float LeftMult;

			[JsonProperty("killedMult")]
			public float KilledMult;

			[JsonProperty("miaMult")]
			public float MiaMult;

			[JsonProperty("survivedMult")]
			public float SurvivedMult;

			[JsonProperty("runnerMult")]
			public float RunnerMult;

			[JsonProperty("transit_mult")]
			public GClass1727[] transit_mult;

			public int GetReward(ExitStatus status)
			{
				return status switch
				{
					ExitStatus.Survived => SurvivedReward, 
					ExitStatus.Runner => RunnerReward, 
					ExitStatus.MissingInAction => MiaReward, 
					ExitStatus.Transit => TransitReward, 
					_ => 0, 
				};
			}

			public float GetMultiplier(ExitStatus exitStatus, int transitCount)
			{
				switch (exitStatus)
				{
				default:
					return 1f;
				case ExitStatus.Survived:
					if (transitCount <= 0)
					{
						return SurvivedMult;
					}
					return method_0(transitCount);
				case ExitStatus.Killed:
					return KilledMult;
				case ExitStatus.Left:
					return LeftMult;
				case ExitStatus.Runner:
					return RunnerMult;
				case ExitStatus.MissingInAction:
					return MiaMult;
				}
			}

			public float method_0(int count)
			{
				int num = Mathf.Clamp(count, 0, transit_mult.Length - 1);
				return transit_mult[num].mlp;
			}
		}

		[JsonProperty("loot_attempts")]
		public GClass1721[] LootAttempts;

		[JsonProperty("expForLockedDoorOpen")]
		public int ExpForLockedDoorOpen;

		[JsonProperty("expForLockedDoorBreach")]
		public int ExpForLockedDoorBreach;

		[JsonProperty("kill")]
		public GClass1722 Kill;

		[JsonProperty("level")]
		public GClass1724 Level;

		[JsonProperty("heal")]
		public GClass1725 Heal;

		[JsonProperty("match_end")]
		public GClass1726 MatchEnd;

		[JsonProperty("triggerMult")]
		public float TriggerMult;

		[JsonProperty("expForLevelOneDogtag")]
		public float ExpForLevelOneDogtag;

		public float GetMultForLootingAttempts(int count)
		{
			int num = count - 1;
			if (num >= 0 && num < LootAttempts.Length)
			{
				return LootAttempts[num].Mult;
			}
			return 0f;
		}
	}

	public class AimingConfiguration
	{
		public Vector3 ProceduralIntensityByPose = new Vector3(0.33f, 0.66f, 1f);

		public float AimProceduralIntensity = 1f;

		public float MinTimeHeavy = 0.95f;

		public float MaxTimeHeavy = 2.4f;

		public float MinTimeLight = 0.35f;

		public float MaxTimeLight = 2f;

		public float HeavyWeight = 9f;

		public float LightWeight = 0.6f;

		public float RecoilScaling = 1f;

		public float RecoilDamping = 0.66f;

		public float CameraSnapGlobalMult = 1f;

		public Vector3 RecoilXIntensityByPose = new Vector3(0.35f, 0.7f, 1f);

		public Vector3 RecoilYIntensityByPose = new Vector3(0.7f, 0.7f, 1f);

		public Vector3 RecoilZIntensityByPose = new Vector3(0.7f, 0.7f, 1f);

		public bool RecoilCrank;

		public float RecoilHandDamping = 0.5f;

		public float RecoilConvergenceMult = 1f;

		public float RecoilVertBonus;

		public float RecoilBackBonus;
	}

	public class GClass1728
	{
		public class HealthFactorData
		{
			public float Minimum;

			public float Maximum;

			public float Default;

			public float OverDamageReceivedMultiplier;

			public float EnvironmentDamageMultiplier;

			public ValueStruct ValueStruct => new ValueStruct
			{
				Maximum = Maximum,
				Minimum = Minimum,
				Current = Default,
				OverDamageReceivedMultiplier = OverDamageReceivedMultiplier,
				EnvironmentDamageMultiplier = EnvironmentDamageMultiplier
			};

			public Profile.ProfileHealthClass.ValueInfo ValueInfo => new Profile.ProfileHealthClass.ValueInfo
			{
				Maximum = Maximum,
				Minimum = Minimum,
				Current = Default,
				OverDamageReceivedMultiplier = OverDamageReceivedMultiplier,
				EnvironmentDamageMultiplier = EnvironmentDamageMultiplier
			};

			public HealthValue HealthValue => new HealthValue(ValueInfo);
		}

		[Serializable]
		public class PlayerHealthFactorsSettings
		{
			[JsonProperty]
			[field: NonSerialized]
			public Dictionary<EBodyPart, HealthFactorData> BodyPartsSettings { get; set; }

			[JsonProperty]
			[field: NonSerialized]
			public Dictionary<EHealthFactorType, HealthFactorData> HealthFactorsSettings { get; set; }

			[JsonProperty]
			[field: NonSerialized]
			public string DefaultStimulatorBuff { get; set; } = string.Empty;
		}

		public class GClass1729
		{
			public float DamagePerMeter;

			public float SafeHeight;
		}

		public class GClass1730
		{
			public float HealthPointPrice;

			public float HydrationPointPrice;

			public float EnergyPointPrice;

			public int TrialLevels;

			public int TrialRaids;

			public bool IsFastHealFree(Profile profile)
			{
				if (TrialLevels > 0 && profile.Info.Level <= TrialLevels && TrialRaids > 0)
				{
					return profile.EftStats.OverallCounters.GetLong(CounterTag.Sessions, CounterTag.Pmc) <= TrialRaids;
				}
				return false;
			}
		}

		[NonSerialized]
		[CompilerGenerated]
		public PlayerHealthFactorsSettings PlayerHealthFactorsSettings_0;

		public GClass3019 Effects;

		public GClass1729 Falling;

		public GClass1730 HealPrice;

		[JsonProperty]
		public PlayerHealthFactorsSettings ProfileHealthSettings
		{
			[CompilerGenerated]
			get
			{
				return PlayerHealthFactorsSettings_0;
			}
			[CompilerGenerated]
			set
			{
				PlayerHealthFactorsSettings_0 = value;
			}
		}
	}

	public class GClass1731
	{
		public float Destructibility;

		public float MinRepairDegradation;

		public float MaxRepairDegradation;

		public float ExplosionDestructibility;

		public float MinRepairKitDegradation;

		public float MaxRepairKitDegradation;
	}

	public class GClass1732
	{
		public class GClass1733
		{
			[JsonProperty("resistance")]
			public int Resistance;
		}

		[JsonProperty("class")]
		public GClass1733[] ArmorClass;

		public GClass1733 GetArmorClass(int classIndex)
		{
			classIndex = Mathf.Clamp(classIndex, 0, ArmorClass.Length - 1);
			return ArmorClass[classIndex];
		}
	}

	public class GClass1734
	{
		public string[] Templates;

		[JsonProperty("Name")]
		public string Id;

		public int Level2;

		public int Level3;
	}

	public class GClass1735
	{
		public float LowerLeftPoint = 1f;

		public float LowerRightPoint = 1f;

		public float LeftPlatoPoint = 1f;

		public float RightPlatoPoint = 1f;

		public float RightLimit = 1f;

		public float ZeroValue = 1f;

		public float GetAt(float normalEnergy)
		{
			normalEnergy = Mathf.Clamp(normalEnergy, 0f, RightLimit);
			if (!(normalEnergy > RightPlatoPoint))
			{
				if (!(normalEnergy > LeftPlatoPoint))
				{
					if (!(normalEnergy > 0f))
					{
						return ZeroValue;
					}
					return LowerLeftPoint + (1f - LowerLeftPoint) * normalEnergy / LeftPlatoPoint;
				}
				return 1f;
			}
			return (1f - LowerRightPoint) * (normalEnergy - RightPlatoPoint) / (RightPlatoPoint - RightLimit) + 1f;
		}
	}

	public class GClass1736
	{
		public float Capacity = 100f;

		public float SprintDrainRate = 4f;

		public float BaseRestorationRate = 5f;

		public float JumpConsumption = 7f;

		public float WeaponFastSwitchConsumption = 15f;

		public float GrenadeHighThrow = 8f;

		public float GrenadeLowThrow = 6f;

		public float AimDrainRate = 3f;

		public float AimRangeFinderDrainRate = 1.5f;

		public float OxygenCapacity = 150f;

		public float OxygenRestoration = 5f;

		public float HandsCapacity = 150f;

		public float HandsRestoration = 5f;

		public Vector2 WalkOverweightLimits = new Vector2(30f, 60f);

		public Vector2 BaseOverweightLimits = new Vector2(35f, 60f);

		public Vector2 SprintOverweightLimits = new Vector2(20f, 40f);

		public Vector2 WalkSpeedOverweightLimits = new Vector2(45f, 80f);

		public Vector2 CrouchConsumption = new Vector2(0.5f, 3f);

		public Vector2 WalkConsumption = new Vector2(0.5f, 2f);

		public Vector2 TransitionSpeed = new Vector2(1f, 0.7f);

		public Vector2 VaultLegsConsumption = new Vector2(0.5f, 3f);

		public Vector2 VaultOneHandConsumption = new Vector2(0.5f, 3f);

		public Vector3 ClimbLegsConsumption = new Vector3(0.5f, 3f);

		public Vector3 ClimbOneHandConsumption = new Vector3(0.5f, 3f);

		public Vector3 ClimbTwoHandsConsumption = new Vector3(0.5f, 3f);

		public Vector2 PoseLevelIncreaseSpeed = new Vector2(1.4f, 0.8f);

		public Vector2 PoseLevelDecreaseSpeed = new Vector2(2f, 1.4f);

		public Vector2 StandupConsumption = new Vector2(5f, 10f);

		public Vector2 PoseLevelConsumptionPerNotch = new Vector2(1f, 2f);

		public Vector2 SoundRadius = new Vector2(1f, 3f);

		public float SprintSpeedLowerLimit = 0.4f;

		public float SprintSensitivityLowerLimit = 0.5f;

		public float AimingSpeedMultiplier = 0.7f;

		public float WalkVisualEffectMultiplier = 3f;

		public Vector3 AimConsumptionByPose;

		public Vector3 RestorationMultiplierByPose;

		public Vector3 OverweightConsumptionByPose;

		public float ProneConsumption = 0.5f;

		public float BaseHoldBreathConsumption = 3f;

		public float ExhaustedMeleeSpeed = 0.8f;

		public float FatigueRestorationRate = 0.2f;

		public float FatigueAmountToCreateEffect = 10f;

		public float ExhaustedMeleeDamageMultiplier = 0.8f;

		public float FallDamageMultiplier = 1.5f;

		public float SafeHeightOverweight = 2f;

		public Vector2 HoldBreathStaminaMultiplier = new Vector2(2f, 1f);

		public bool StaminaExhaustionCausesJiggle = true;

		public bool StaminaExhaustionStartsBreathSound = true;

		public bool StaminaExhaustionRocksCamera = true;

		public float MountingVerticalAimDrainRateMultiplier = 0.85f;

		public float MountingHorizontalAimDrainRateMultiplier = 0.5f;

		public float BipodAimDrainRateMultiplier = 0.2f;

		public float LowerOverweightLimit => Mathf.Min(WalkOverweightLimits.x, BaseOverweightLimits.x, SprintOverweightLimits.x, WalkSpeedOverweightLimits.x);

		public float UpperOverweightLimit => BaseOverweightLimits.y;
	}

	public class GClass1737
	{
		public string TemplateId;

		public int MaxInLobby;

		public int MaxInRaid;
	}

	public class GClass1738
	{
		public float AmmoMalfChanceMult;

		public float MagazineMalfChanceMult;

		public float MalfRepairHardSlideMult;

		public float MalfRepairOneHandBrokenMult;

		public float MalfRepairTwoHandsBrokenMult;

		public bool AllowMalfForBots;

		public int ShowGlowAttemptsCount;

		public float OutToIdleSpeedMultForPistol;

		public float IdleToOutSpeedMultOnMalf;

		public float TimeToQuickdrawPistol;

		public Vector3 DurRangeToIgnoreMalfs = new Vector3(100f, 100f, 0f);

		public float DurFeedWt;

		public float DurMisfireWt;

		public float DurJamWt;

		public float DurSoftSlideWt;

		public float DurHardSlideMinWt;

		public float DurHardSlideMaxWt;

		public float AmmoMisfireWt;

		public float AmmoFeedWt;

		public float AmmoJamWt;

		public float OverheatFeedWt;

		public float OverheatJamWt;

		public float OverheatSoftSlideWt;

		public float OverheatHardSlideMinWt;

		public float OverheatHardSlideMaxWt;
	}

	public class GClass1739
	{
		public float MinOverheat;

		public float MaxOverheat = 200f;

		public float OverheatProblemsStart = 100f;

		public float ModHeatFactor;

		public float ModCoolFactor;

		public float MinWearOnOverheat;

		public float MaxWearOnOverheat;

		public float MinWearOnMaxOverheat;

		public float MaxWearOnMaxOverheat;

		public float OverheatWearLimit;

		public float MaxCOIIncreaseMult;

		public float MinMalfChance;

		public float MaxMalfChance;

		public float DurReduceMinMult;

		public float DurReduceMaxMult;

		public float BarrelMoveRndDuration;

		public float BarrelMoveMaxMult;

		public float FireratePitchMult;

		public float FirerateReduceMinMult;

		public float FirerateReduceMaxMult;

		public float FirerateOverheatBorder;

		public bool EnableSlideOnMaxOverheat;

		public float StartSlideOverheat;

		public float FixSlideOverheat;

		public float AutoshotMinOverheat;

		public float AutoshotChance;

		public float AutoshotPossibilityDuration;

		public float HeatVisibleFactor = 1f;

		public float HeatThermalFactor = 1f;

		public float MaxOverheatCoolCoef;
	}

	public class InertiaSettings
	{
		public Vector2 ExitMovementStateSpeedThreshold = new Vector2(0.02f, 0.02f);

		public Vector2 WalkInertia = new Vector2(0.05f, 0.5f);

		public float FallThreshold = 0.15f;

		public Vector2 SpeedLimitAfterFallMin = new Vector2(0f, 1f);

		public Vector2 SpeedLimitAfterFallMax = new Vector2(4f, 0f);

		public Vector2 SpeedLimitDurationMin = new Vector2(0.3f, 0f);

		public Vector2 SpeedLimitDurationMax = new Vector2(2f, 1f);

		public Vector2 SpeedInertiaAfterJump = new Vector2(1f, 1.4f);

		public float BaseJumpPenaltyDuration = 0.3f;

		public float DurationPower = 1.58f;

		public float BaseJumpPenalty = 0.15f;

		public float PenaltyPower = 1.12f;

		public Vector2 InertiaTiltCurveMin = new Vector2(0f, 0.3f);

		public Vector2 InertiaTiltCurveMax = new Vector2(1f, 0.1f);

		public Vector2 InertiaBackwardCoef = new Vector2(1f, 0.8f);

		public Vector2 TiltInertiaMaxSpeed = new Vector2(1.2f, 1f);

		public Vector2 TiltStartSideBackSpeed = new Vector2(0.5f, 0.2f);

		public Vector2 TiltMaxSideBackSpeed = new Vector2(1f, 0.7f);

		public Vector2 TiltAcceleration = new Vector2(4.5f, 1.5f);

		public int AverageRotationFrameSpan = 60;

		public Vector2 SprintSpeedInertiaCurveMin = new Vector2(0f, 1f);

		public Vector2 SprintSpeedInertiaCurveMax = new Vector2(2.5f, 0.3f);

		public Vector2 SprintBrakeInertia = new Vector2(0f, 55f);

		public Vector2 SprintTransitionMotionPreservation = new Vector2(0.7f, 0.9f);

		public Vector3 InertiaLimits = new Vector3(0f, 65f, 1f);

		public Vector2 WeaponFlipSpeed = new Vector2(0.5f, 2f);

		public float InertiaLimitsStep = 0.3f;

		public Vector2 SprintAccelerationLimits = new Vector2(5f, 0.8f);

		public Vector2 PreSprintAccelerationLimits = new Vector2(5f, 0.8f);

		public Vector2 SideTime = new Vector2(2f, 2f);

		public Vector2 DiagonalTime = new Vector2(1.5f, 1.5f);

		public float MinDirectionBlendTime = 0.1f;

		public Vector2 MoveTimeRange = new Vector2(0.15f, 0.5f);

		public Vector2 MinMovementAccelerationRangeRight = new Vector2(0.4f, 1f);

		public Vector2 MaxMovementAccelerationRangeRight = new Vector2(0.4f, 1f);

		public float SuddenChangesSmoothness = 3f;

		public Vector2 MaxTimeWithoutInput = new Vector2(0.1f, 0.3f);

		public Vector2 ProneDirectionAccelerationRange = new Vector2(2.5f, 2.5f);

		public Vector2 ProneSpeedAccelerationRange = new Vector2(1.2f, 1.2f);

		public Vector2 DiagonalStayTimeRange = new Vector2(0.1f, 0.2f);

		public Vector2 CrouchSpeedAccelerationRange = new Vector2(0.475f, 0.75f);
	}

	public class GClass1740
	{
		public float GlobalDamageDegradationCoefficient = 0.5f;
	}

	public class GClass1741
	{
		public int PlaneMinFlightHeight = 150;

		public int PlaneMaxFlightHeight = 200;

		public int PlaneSpeed = 40;

		public int PlaneAdditionalDistance = 120;

		public int ParachuteStartOpenHeight = 120;

		public int ParachuteEndOpenHeight = 100;

		public int PlaneAirdropDuration = 50;

		public int SmokeActivateHeight = 20;

		public int PlaneAirdropSmoke = 30;

		public int PlaneAirdropFlareWait = 30;

		public AirdropSynchronizableObject.EAirdropViewType AirdropViewType;
	}

	public class GClass1742
	{
		public Dictionary<string, DamageData[]> HandlerDamage = new Dictionary<string, DamageData[]>();
	}

	public class GClass1743
	{
		public float CustomerCriticalTimeStart = 10f;

		public float CustomerKickNotifTime = 60f;

		public float CustomerAccessTime = 600f;
	}

	public class GClass1744
	{
		public string[] AvailableVersions = new string[1] { "edge_of_darkness" };
	}

	public class GClass1745
	{
		public bool ModeEnabled = true;

		public string[] AvailableVersions = new string[1] { "unheard_edition" };
	}

	public class GClass1746
	{
		public bool EventActive;

		public int EventTime = 120;

		public int WeatherChangeTime = 5;

		public float ExitTimeMultiplier = 3f;

		public float StaminaMultiplier = 2f;

		public GClass1908 EventWeather;

		public GClass1908 SummonSuccessWeather;

		public GClass1908 SummonFailedWeather;

		public string[] LocationsToIgnore = new string[4] { "factory4_day", "factory4_night", "hideout", "laboratory" };
	}

	public class TransitSettingsClass
	{
		public bool active;

		public int DeliveryMinPrice = 1000;

		public int DeliveryPrice = 1000;

		public float ModDeliveryCost = 0.1f;

		public float BearPriceMod = 1f;

		public float UsecPriceMod = 1.5f;

		public float ScavPriceMod = 0.8f;

		public float CoefficientDiscountCharisma = 0.002f;

		public bool ClearAllPlayerEffectsOnTransit = true;

		public bool RestoreHealthOnDestroyedParts;

		public int PercentageOfMissingHealthRestore = 50;

		public int PercentageOfMissingWaterRestore = 50;

		public int PercentageOfMissingEnergyRestore = 50;
	}

	public class GClass1748
	{
		public bool active;

		public string[] locations = new string[6] { "Shoreline", "Interchange", "bigmap", "RezervBase", "Woods", "Lighthouse" };

		public string[] nonExitsLocations = new string[1] { "Interchange" };

		public string[] sleighLocations = new string[1] { "Interchange" };

		public string[] consumables;

		public string[] accessKeys;

		public float interactionDistance = 2f;

		public float repairSec = 8f;

		public float multitoolRepairSec = 2f;

		public Vector2 secToBreak = new Vector2(20f, 40f);

		public Vector2 durability = new Vector2(150f, 350f);

		public float grenadeDistanceToBreak = 30f;

		public float knifeCritChanceToBreak = 0.3f;

		public float rainForFrozen = 0.5f;

		public float initialFrozenDelaySec = 60f;

		public float applyFrozenEverySec = 5f;

		public float fireDistanceToHeat = 3f;

		public int drunkImmunitySec = 30;

		public int ApplyFrozenEveryMS => (int)(applyFrozenEverySec * 1000f);

		public float FireDistanceToHeatSqr => fireDistanceToHeat * fireDistanceToHeat;
	}

	[Serializable]
	public class BTRGlobalSettings
	{
		public string[] LocationsWithBTR = new string[2] { "develop", "TarkovStreets" };

		public int BasePriceTaxi = 10000;

		public int AddPriceTaxi = 200;

		public int CleanUpPrice = 100000;

		public int DeliveryPrice = 1000;

		public float ModDeliveryCost = 0.1f;

		public float BearPriceMod = 1f;

		public float UsecPriceMod = 1.5f;

		public float ScavPriceMod = 0.8f;

		public float CoefficientDiscountCharisma = 0.002f;

		public int TaxiMinPrice = 4000;

		public int DeliveryMinPrice = 1000;

		public int BotCoverMinPrice = 30000;

		public Dictionary<string, BTRMapPath> MapsConfigs;
	}

	[Serializable]
	public class ArtilleryShellingGlobalSettings
	{
		[JsonProperty("ProjectileExplosionParams")]
		public ArtilleryProjectileExplosionParams ProjectileExplosionParams;

		[JsonProperty("ArtilleryMapsConfigs")]
		public Dictionary<string, ArtilleryShellingMapConfiguration> ArtilleryMapsConfigs;

		[JsonProperty("MaxCalledShellingCount")]
		public int MaxCalledShellingCount;
	}

	[Serializable]
	public class ArtilleryProjectileExplosionParams
	{
		public Vector3 Blindness;

		public Vector3 Contusion;

		public Vector3 ArmorDistanceDistanceDamage;

		public float DeadlyDistance;

		public float MinExplosionDistance;

		public float MaxExplosionDistance;

		public int FragmentsCount;

		public float Strength;

		public string FragmentType;

		public float ArmorDamage;

		public float StaminaBurnRate;

		public float PenetrationPower;

		public float DirectionalDamageAngle;

		public float DirectionalDamageMultiplier;
	}

	[Serializable]
	public class ArtilleryShellingAirDropSettings
	{
		public bool UseAirDrop;

		public int AirDropTime;

		public string LootTemplateId;

		public Vector3 AirDropPosition;
	}

	[Serializable]
	public class TripwiresGlobalSettings
	{
		public float ShotSqrDistance = 625f;

		public float InteractionSqrDistance = 2f;

		public float MaxHeightDifference = 0.2f;

		public float MinLength = 0.8f;

		public float MaxLength = 3f;

		public float MaxPreviewLength = 4f;

		public double MaxTripwireToPlayerDistance = 8.0;

		public float CollisionCapsuleRadius = 0.02f;

		public float CollisionCapsuleCheckCoef = 2f;

		public double GroundDotProductTolerance = 0.95;

		public float DefuseTimeSeconds = 10f;

		public float MultitoolDefuseTimeSeconds = 5f;

		public int InertSeconds = 300;

		public int DestroyedSeconds = 30;
	}

	public class GClass1749
	{
		public float ItemRemoveAfterInterruptionTime = 1f;

		public int MaxBackpackInserting = -1;
	}

	[Serializable]
	public class TradingGlobalSettings
	{
		[Serializable]
		public class BuyRestrictionMaxBonusConfig
		{
			public float multiplier;
		}

		public BuyoutRestrictions BuyoutRestrictions;

		public Dictionary<string, BuyRestrictionMaxBonusConfig> BuyRestrictionMaxBonus = new Dictionary<string, BuyRestrictionMaxBonusConfig>();
	}

	[Serializable]
	public class BuyoutRestrictions
	{
		public float MinFoodDrinkResource;

		public float MinMedsResource;

		public float MinDurability;
	}

	[Serializable]
	public class VaultingGlobalSettings : IVaultingSettings
	{
		[JsonProperty("GridSettings")]
		public readonly GridSettings GridSettings;

		[JsonProperty("MovesSettings")]
		public readonly MovesSettings MovesSettings;

		[JsonProperty("IsActive")]
		[field: NonSerialized]
		public bool IsActive { get; set; }

		[JsonProperty("VaultingInputTime")]
		[field: NonSerialized]
		public float VaultingInputTime { get; set; }

		bool IVaultingSettings.IsActive => IsActive;

		float IVaultingSettings.VaultingInputTime => VaultingInputTime;

		IGridSettings IVaultingSettings.GridSettings => GridSettings;

		IMovesSettings IVaultingSettings.MovesSettings => MovesSettings;
	}

	[Serializable]
	public class GridSettings : IGridSettings
	{
		[field: NonSerialized]
		public float GridSizeX { get; set; }

		[field: NonSerialized]
		public float GridSizeY { get; set; }

		[field: NonSerialized]
		public float GridSizeZ { get; set; }

		[field: NonSerialized]
		public float SteppingLengthX { get; set; }

		[field: NonSerialized]
		public float SteppingLengthY { get; set; }

		[field: NonSerialized]
		public float SteppingLengthZ { get; set; }

		[field: NonSerialized]
		public float GridOffsetX { get; set; }

		[field: NonSerialized]
		public float GridOffsetY { get; set; }

		[field: NonSerialized]
		public float GridOffsetZ { get; set; }

		[field: NonSerialized]
		public float OffsetFactor { get; set; }
	}

	[Serializable]
	public class MovesSettings : IMovesSettings
	{
		[JsonProperty("VaultSettings")]
		public readonly VaultSettings VaultSettings;

		[JsonProperty("ClimbSettings")]
		public readonly ClimbSettings ClimbSettings;

		IVaultSettings IMovesSettings.VaultSettings => VaultSettings;

		IClimbSettings IMovesSettings.ClimbSettings => ClimbSettings;
	}

	[Serializable]
	public class VaultSettings : IVaultSettings, IBaseMoveSettings
	{
		[JsonProperty("IsActive")]
		public readonly bool IsActive;

		[JsonProperty("MaxWithoutHandHeight")]
		public readonly float MaxWithoutHandHeight;

		[JsonProperty("SpeedRange")]
		public readonly Vector2 SpeedRange;

		[JsonProperty("MoveRestrictions")]
		public readonly MoveRestrictions MoveRestrictions;

		[JsonProperty("AutoMoveRestrictions")]
		public readonly AutoMoveRestrictions AutoMoveRestrictions;

		bool IBaseMoveSettings.IsActive => IsActive;

		float IBaseMoveSettings.MaxWithoutHandHeight => MaxWithoutHandHeight;

		IMoveRestrictions IBaseMoveSettings.MoveRestrictions => MoveRestrictions;

		IAutoMoveRestrictions IBaseMoveSettings.AutoMoveRestrictions => AutoMoveRestrictions;
	}

	[Serializable]
	public class ClimbSettings : IClimbSettings, IBaseMoveSettings
	{
		[JsonProperty("IsActive")]
		public readonly bool IsActive;

		[JsonProperty("MaxWithoutHandHeight")]
		public readonly float MaxWithoutHandHeight;

		[JsonProperty("MaxOneHandHeight")]
		public readonly float MaxOneHandHeight;

		[JsonProperty("SpeedRange")]
		public readonly Vector2 SpeedRange;

		[JsonProperty("MoveRestrictions")]
		public MoveRestrictions MoveRestrictions;

		[JsonProperty("AutoMoveRestrictions")]
		public AutoMoveRestrictions AutoMoveRestrictions;

		bool IBaseMoveSettings.IsActive => IsActive;

		float IBaseMoveSettings.MaxWithoutHandHeight => MaxWithoutHandHeight;

		float IClimbSettings.MaxOneHandHeight => MaxOneHandHeight;

		IMoveRestrictions IBaseMoveSettings.MoveRestrictions => MoveRestrictions;

		IAutoMoveRestrictions IBaseMoveSettings.AutoMoveRestrictions => AutoMoveRestrictions;
	}

	[Serializable]
	public class MoveRestrictions : IMoveRestrictions, IVaultingRestrictions
	{
		public bool IsActive;

		public float MinDistantToInteract;

		public float MinHeight;

		public float MaxHeight;

		public float MinLength;

		public float MaxLength;

		bool IVaultingRestrictions.IsActive => IsActive;

		float IVaultingRestrictions.MinDistantToInteract => MinDistantToInteract;

		float IVaultingRestrictions.MinHeight => MinHeight;

		float IVaultingRestrictions.MaxHeight => MaxHeight;

		float IVaultingRestrictions.MinLength => MinLength;

		float IVaultingRestrictions.MaxLength => MaxLength;
	}

	[Serializable]
	public class AutoMoveRestrictions : IAutoMoveRestrictions, IVaultingRestrictions
	{
		public bool IsActive;

		public float MinDistantToInteract;

		public float MinHeight;

		public float MaxHeight;

		public float MinLength;

		public float MaxLength;

		bool IVaultingRestrictions.IsActive => IsActive;

		float IVaultingRestrictions.MinDistantToInteract => MinDistantToInteract;

		float IVaultingRestrictions.MinHeight => MinHeight;

		float IVaultingRestrictions.MaxHeight => MaxHeight;

		float IVaultingRestrictions.MinLength => MinLength;

		float IVaultingRestrictions.MaxLength => MaxLength;
	}

	public class GClass1750
	{
		public float WeaponPistolFastSwitchMaxSpeedMult = 2f;

		public float WeaponPistolFastSwitchMinSpeedMult = 1.5f;

		public float WeaponFastSwitchMaxSpeedMult = 2f;

		public float WeaponFastSwitchMinSpeedMult = 1.5f;

		public float HandShakeMaxDuration = 10f;

		public float HandShakeTremorIntensity = 3f;

		public float HandShakeCurveFrequency = 1f;

		public float HandShakeCurveIntensity = 1f;
	}

	public class GClass1751
	{
		public int WeaponStandMaxItemsCount;

		public int PlaceOfFameMaxItemsCount;
	}

	public class GClass1752
	{
		public float PenetrationLevel;

		public float PenetrationChance;

		public float PenetrationDamageMod;
	}

	public class GClass1753
	{
		[JsonProperty("PointDetectionSettings")]
		public MountingPointDetectionSettings PointDetectionSettings = new MountingPointDetectionSettings();

		[JsonProperty("MovementSettings")]
		public MountingMovementSettings MovementSettings = new MountingMovementSettings();
	}

	[Serializable]
	public class SeasonActivity
	{
		[Serializable]
		public class InfectionHalloween
		{
			public bool DisplayUIEnabled;

			public bool Enabled;

			public float ZombieBleedMul;

			[JsonIgnore]
			public int RedThreatValue = 50;

			[JsonIgnore]
			[field: NonSerialized]
			public IReadOnlyDictionary<string, int> LocationInfection { get; set; }

			public void Init(Dictionary<string, int> locationInfection)
			{
				LocationInfection = locationInfection;
			}
		}

		[JsonProperty("InfectionHalloween")]
		public InfectionHalloween InfectionHalloweenConfig;
	}

	public class GClass1754
	{
		public class GClass1755
		{
			public float ResourceSpent;

			public float PointsGained;

			public float PointsPerResource => PointsGained / ResourceSpent;
		}

		public class GClass1756
		{
			public int Slots;

			public int Container;
		}

		public float SkillPointsPerAreaUpgrade;

		public float SkillPointsPerCraft;

		public double SkillBoostPercent;

		public float ConsumptionReductionPerLevel;

		public Dictionary<EAreaType, GClass1755> SkillPointsRate;

		public Dictionary<EAreaType, GClass1756> EliteSlots;
	}

	public class GClass1757
	{
		public float PointsPerCraftingCycle;

		public float CraftingCycleHours;

		public float PointsPerUniqueCraftCycle;

		public float UniqueCraftsPerCycle;

		public float CraftTimeReductionPerLevel;

		public float ProductionTimeReductionPerLevel;

		public float EliteExtraProductions;

		[JsonProperty("CraftingPointsToInteligence")]
		public float CraftingPointsToIntelligence;

		public float PointsPerOriginalCraft => PointsPerUniqueCraftCycle / UniqueCraftsPerCycle;

		public float CraftingCycleSeconds => CraftingCycleHours * 60f * 60f;

		public float PointsPerSecond => PointsPerCraftingCycle / CraftingCycleSeconds;
	}

	public class GClass1758
	{
		public float HydrationRecoveryRate;

		public float EnergyRecoveryRate;

		public float IncreasePositiveEffectDurationRate;

		public float DecreaseNegativeEffectDurationRate;

		public float DecreasePoisonDurationRate;
	}

	public class GClass1759
	{
		public float ImmunityMiscEffects;

		public float ImmunityPoisonBuff;

		public float ImmunityPainKiller;

		public float HealthNegativeEffect;

		public float StimulatorNegativeBuff;
	}

	public class GClass1760
	{
		public float MovementAction;

		public float SprintAction;

		public float GainPerFatigueStack;
	}

	public class GClass1761
	{
		public float SprintActionMin;

		public float SprintActionMax;

		public float MovementActionMin;

		public float MovementActionMax;

		public float PushUpMin;

		public float PushUpMax;

		public float FistfightAction;

		public float ThrowAction;
	}

	public class GClass1762
	{
		public float DamageTakenAction;

		public float HealthNegativeEffect;
	}

	public class GClass1763
	{
		public float SkillProgress;
	}

	public class GClass1764
	{
		public float HealthNegativeEffect;

		public float LowHPDuration;
	}

	public class GClass1765
	{
		public float ThrowAction;
	}

	public class GClass1766
	{
		public float RecoilAction;

		public float RecoilBonusPerLevel = 0.004f;
	}

	public class GClass1767
	{
		public float WeaponReloadAction;

		public float WeaponShotAction;

		public float WeaponFixAction;

		public float WeaponChamberAction;
	}

	public class GClass1768
	{
		public float WeaponReloadAction;

		public float WeaponShotAction;

		public float WeaponFixAction;

		public float WeaponChamberAction;
	}

	public class GClass1769
	{
		public float WeaponReloadAction;

		public float WeaponShotAction;

		public float WeaponFixAction;

		public float WeaponChamberAction;
	}

	public class GClass1770
	{
		public float WeaponReloadAction;

		public float WeaponShotAction;

		public float WeaponFixAction;

		public float WeaponChamberAction;
	}

	public class GClass1771
	{
		public float WeaponReloadAction;

		public float WeaponShotAction;

		public float WeaponFixAction;

		public float WeaponChamberAction;
	}

	public class GClass1772
	{
		public float WeaponReloadAction;

		public float WeaponShotAction;

		public float WeaponFixAction;

		public float WeaponChamberAction;
	}

	public class GClass1773
	{
		public float MovementAction;
	}

	public class GClass1774
	{
		public float SearchAction;

		public float FindAction;
	}

	public class GClass1775
	{
		public float RaidLoadedAmmoAction;

		public float RaidUnloadedAmmoAction;

		public float MagazineCheckAction;
	}

	public class GClass1776
	{
		public float OnlineAction;

		public float UniqueLoot;
	}

	public class GClass1777
	{
		public float ExamineAction;

		public float SkillProgress;

		public float RepairAction;

		[JsonProperty("WearAmountReducePerLevel")]
		public float WearAmountReducePerLevel = 1f;

		[JsonProperty("RepairPointsCostReduction")]
		public float RepairPointsCostReduction = 1f;
	}

	public class GClass1778
	{
		public float ExamineWithInstruction;

		public float FindActionTrue;

		public float FindActionFalse;
	}

	public class GClass1779
	{
		public class GClass1780
		{
			public GClass1781 LevelBonusSettings;

			public GClass1782 EliteBonusSettings;
		}

		public class GClass1781
		{
			public float RepeatableQuestChangeDiscount = 0.05f;

			public float HealthRestoreDiscount = 0.05f;

			public float HealthRestoreTraderDiscount = 0.05f;

			public float InsuranceDiscount = 0.05f;

			public float InsuranceTraderDiscount = 0.05f;

			public float PaidExitDiscount = 0.05f;
		}

		public class GClass1782
		{
			public float ScavCaseDiscount = 0.1f;

			public float FenceStandingLossDiscount = 0.5f;

			public int RepeatableQuestExtraCount = 1;
		}

		public float SkillProgressInt = 0.5f;

		public float SkillProgressAtn = 0.5f;

		public float SkillProgressPer = 0.5f;

		public GClass1780 BonusSettings;
	}

	public class GClass1783
	{
		public float AnySkillUp;

		public float SkillProgress;
	}

	public class GClass1784
	{
		public float SurgeryAction;

		public float SkillProgress;
	}

	public class GClass1785
	{
		public float WeaponShotAction;
	}

	public class GClass1786
	{
		public float CommonBuffMinChanceValue = 1f;

		public float CommonBuffChanceLevelBonus;

		public float RareBuffChanceCoff;

		public float ReceivedDurabilityMaxPercent = 0.01f;
	}

	public class GClass1787
	{
		public float SkillPointsPerRepair;

		[JsonProperty("WearAmountRepairGunsReducePerLevel")]
		public float WearAmountRepairGunsReducePerLevel;

		[JsonProperty("DurLossReducePerLevel")]
		public float DurabilityLossReducePerLevel;

		public GClass1786 BuffSettings;
	}

	public class GClass1788
	{
		public float MalfRepairSpeedBonusPerLevel;

		public float SkillPointsPerMalfFix;

		public float EliteDurabilityChanceReduceMult;

		public float EliteAmmoChanceReduceMult;

		public float EliteMagChanceReduceMult;
	}

	public class GClass1789
	{
		public float WearAmountRepairLVestsReducePerLevel = 0.005f;

		public float WearChanceRepairLVestsReduceEliteLevel = 0.5f;

		public float MoveSpeedPenaltyReductionLVestsReducePerLevel = 0.006f;

		public float MeleeDamageLVestsReducePerLevel = 0.006f;

		public GClass1786 BuffSettings;
	}

	public class GClass1790
	{
		public float WearAmountRepairHVestsReducePerLevel = 0.005f;

		public float WearChanceRepairHVestsReduceEliteLevel = 0.5f;

		public float MoveSpeedPenaltyReductionHVestsReducePerLevel = 0.005f;

		public float BluntThroughputDamageHVestsReducePerLevel = 0.004f;

		public float RicochetChanceHVestsEliteLevel = 0.05f;

		public float RicochetChanceHVestsMaxDurabilityThreshold = 0.2f;

		public float RicochetChanceHVestsCurrentDurabilityThreshold = 0.5f;

		public GClass1786 BuffSettings;
	}

	public class GlobalSkillsSettings
	{
		public float SkillProgressRate;

		public float WeaponSkillProgressRate;

		public float WeaponSkillRecoilBonusPerLevel = 0.004f;

		public float MountingErgonomicsBonusPerLevel = 0.2f;

		public float BipodErgonomicsBonusPerLevel = 0.5f;

		public GClass1754 HideoutManagement;

		public GClass1757 Crafting;

		public GClass1758 Metabolism;

		public GClass1759 Immunity;

		public GClass1760 Endurance;

		public GClass1761 Strength;

		public GClass1762 Vitality;

		public GClass1763 Health;

		public GClass1764 StressResistance;

		public GClass1765 Throwing;

		public GClass1766 RecoilControl;

		public GClass1768 Pistol;

		public GClass1769 Assault;

		public GClass1770 Shotgun;

		public GClass1771 Sniper;

		public GClass1772 DMR;

		public GClass1773 CovertMovement;

		public GClass1774 Search;

		public GClass1775 MagDrills;

		public GClass1776 Perception;

		public GClass1777 Intellect;

		public GClass1778 Attention;

		public GClass1779 Charisma;

		public GClass1783 Memory;

		public GClass1784 Surgery;

		public GClass1785 AimDrills;

		public GClass1787 WeaponTreatment;

		public GClass1788 TroubleShooting;

		public GClass1767 Revolver;

		public GClass1789 LightVests;

		public GClass1790 HeavyVests;
	}

	public Dictionary<ETraderServiceType, ServiceData> ServicesData = new Dictionary<ETraderServiceType, ServiceData>();

	[JsonIgnore]
	public BTRServerSettings BTRLocalSettings = new BTRServerSettings();

	[JsonProperty("FenceSettings")]
	public readonly FenceGlobalSettings FenceSettings;

	[JsonProperty("TradingSettings")]
	public readonly TradingGlobalSettings TradingSettings;

	public readonly Dictionary<MongoID, TraderSettings> TradersSettings = new Dictionary<MongoID, TraderSettings>();

	[NonSerialized]
	[CompilerGenerated]
	public GClass1791 Gclass1791_0 = new GClass1791(new List<GClass2661>());

	public readonly GClass1717 RepairSettings = new GClass1717();

	[JsonProperty("exp")]
	public GClass1720 Experience;

	public AimingConfiguration Aiming;

	public GClass1728 Health;

	public Dictionary<EArmorMaterial, GClass1731> ArmorMaterials;

	public Dictionary<string, ExfiltrationRequirement[]> RequirementReferences;

	[JsonProperty("armor")]
	public GClass1732 Armor;

	public RagfairSettingsClass RagFair;

	[JsonProperty("handbook")]
	public GClass1714 Handbook;

	[JsonProperty("MaxBotsAliveOnMap")]
	public int MaxBotsAliveOnMap;

	public float MarksmanAccuracy = 1f;

	public int SavagePlayCooldown;

	public int TimeBeforeDeploy;

	public int TimeBeforeDeployLocal;

	public float LoadTimeSpeedProgress = 1f;

	public float BaseLoadTime = 0.85f;

	public float BaseUnloadTime = 0.3f;

	public float BaseCheckTime = 3f;

	public float WAVE_COEF_LOW = 1f;

	public float WAVE_COEF_MID = 1f;

	public float WAVE_COEF_HIGH = 1f;

	public float WAVE_COEF_HORDE = 1f;

	public bool UncheckOnShot = true;

	public int SessionsToShowHotKeys;

	public string TODSkyDate;

	public float SkillMinEffectiveness = 0.01f;

	public float SkillFatiguePerPoint = 0.5f;

	public float SkillFreshEffectiveness = 1.2f;

	public int SkillFreshPoints = 1;

	public int SkillPointsBeforeFatigue = 2;

	public int SkillFatigueReset = 300;

	public float SkillEnduranceWeightThreshold = 0.75f;

	public float AimPunchMagnitude = 7f;

	public bool SkillAtrophy;

	public bool AllowSelectEntryPoint;

	public float LegsOverdamage = 1f;

	public float HandsOverdamage = 0.7f;

	public float StomachOverdamage = 1.5f;

	public float BluntDamageReduceFromSoftArmorMod = 1f;

	public bool DiscardLimitsEnabled;

	public float MailItemsExpirationTimeLimitWarning = 24f;

	public GClass1734[] Mastering;

	public GClass1736 Stamina = new GClass1736();

	public GClass1735 StaminaRestoration = new GClass1735();

	public GClass1735 StaminaDrain = new GClass1735();

	[NonSerialized]
	public Dictionary<EBodyPart, float> Dictionary_0;

	[NonSerialized]
	public Dictionary<string, GClass1734> Dictionary_1;

	public bool AzimuthPanelShowsPlayerOrientation;

	public Vector3 WallContusionAbsorption = new Vector3(0.4f, 0.1f, 0f);

	public Vector2 WalkSpeed = new Vector2(0.7f, 1f);

	public Vector2 SprintSpeed = new Vector2(0.4f, 2f);

	[NonSerialized]
	public GClass1737[] Gclass1737_0;

	public EEventType[] EventType = new EEventType[0];

	public int TeamSearchingTimeout;

	public int GameSearchingTimeout;

	public GClass1738 Malfunction;

	public GClass1739 Overheat;

	public InertiaSettings Inertia = new InertiaSettings();

	public GClass1740 Ballistic = new GClass1740();

	public GClass1741 Airdrop = new GClass1741();

	public GClass1742 Triggers = new GClass1742();

	public GClass1743 BufferZone = new GClass1743();

	[JsonProperty("CoopSettings")]
	public readonly GClass1744 CoopSettings = new GClass1744();

	[JsonProperty("PveSettings")]
	public readonly GClass1745 PveSettings = new GClass1745();

	[JsonProperty("EventSettings")]
	public readonly GClass1746 EventSettings = new GClass1746();

	[JsonProperty("BTRSettings")]
	public readonly BTRGlobalSettings BTRSettings = new BTRGlobalSettings();

	[JsonProperty("TripwiresSettings")]
	public readonly TripwiresGlobalSettings TripwiresSettings = new TripwiresGlobalSettings();

	[JsonProperty("TransitSettings")]
	public readonly TransitSettingsClass transitSettings = new TransitSettingsClass();

	[JsonProperty("ArtilleryShelling")]
	public readonly ArtilleryShellingGlobalSettings ArtilleryShelling = new ArtilleryShellingGlobalSettings();

	[JsonProperty("RunddansSettings")]
	public readonly GClass1748 runddansSettings = new GClass1748();

	[JsonProperty("ItemsCommonSettings")]
	public readonly GClass1749 ItemsSettings = new GClass1749();

	[JsonProperty("VaultingSettings")]
	public readonly VaultingGlobalSettings VaultingSettings = new VaultingGlobalSettings();

	[JsonProperty("WeaponFastDrawSettings")]
	public readonly GClass1750 WeaponFastDrawGlobalSettings = new GClass1750();

	[JsonProperty("AudioSettings")]
	public GClass1499 AudioSettings = new GClass1499();

	[JsonProperty("FavoriteItemsSettings")]
	public GClass1751 FavoriteItemsSettings = new GClass1751();

	[JsonProperty("BodyPartColliderSettings")]
	public Dictionary<EBodyPartColliderType, GClass1752> BodyPartColliderSettings = new Dictionary<EBodyPartColliderType, GClass1752>();

	[JsonProperty("ArenaEftTransferSettings")]
	public GClass1795 ArenaEftTransferSettings = new GClass1795();

	[JsonProperty("MountingSettings")]
	public GClass1753 MountingSettings = new GClass1753();

	[JsonProperty("SeasonActivity")]
	public SeasonActivity SeasonActivityConfig;

	public GlobalSkillsSettings SkillsSettings;

	public GClass1791 PrestigeSettings
	{
		[CompilerGenerated]
		get
		{
			return Gclass1791_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1791_0 = value;
		}
	}

	public Dictionary<EBodyPart, float> OverDamageFactor
	{
		get
		{
			Dictionary<EBodyPart, float> dictionary = Dictionary_0;
			if (dictionary == null)
			{
				Dictionary<EBodyPart, float> obj = new Dictionary<EBodyPart, float>(GClass866<EBodyPart>.Count, GClass866<EBodyPart>.EqualityComparer)
				{
					{
						EBodyPart.Head,
						1f
					},
					{
						EBodyPart.Chest,
						1f
					},
					{
						EBodyPart.Stomach,
						StomachOverdamage
					},
					{
						EBodyPart.LeftArm,
						HandsOverdamage
					},
					{
						EBodyPart.RightArm,
						HandsOverdamage
					},
					{
						EBodyPart.LeftLeg,
						LegsOverdamage
					},
					{
						EBodyPart.RightLeg,
						LegsOverdamage
					},
					{
						EBodyPart.Common,
						1f
					}
				};
				Dictionary<EBodyPart, float> dictionary2 = obj;
				Dictionary_0 = obj;
				dictionary = dictionary2;
			}
			return dictionary;
		}
	}

	public Dictionary<string, GClass1734> Associations
	{
		get
		{
			if (Dictionary_1 != null)
			{
				return Dictionary_1;
			}
			Dictionary_1 = new Dictionary<string, GClass1734>();
			for (int i = 0; i < Mastering.Length; i++)
			{
				for (int j = 0; j < Mastering[i].Templates.Length; j++)
				{
					if (Dictionary_1.ContainsKey(Mastering[i].Templates[j]))
					{
						Debug.Log(Dictionary_1[Mastering[i].Templates[j]].Id + " " + Mastering[i].Id + " share same weapon");
					}
					else
					{
						Dictionary_1.Add(Mastering[i].Templates[j], Mastering[i]);
					}
				}
			}
			return Dictionary_1;
		}
	}

	[UsedImplicitly]
	public GClass1737[] RestrictionsInRaid
	{
		get
		{
			return Gclass1737_0;
		}
		set
		{
			foreach (GClass1737 gClass in value)
			{
				if (gClass.MaxInRaid != -1 && gClass.MaxInRaid < gClass.MaxInLobby)
				{
					gClass.MaxInRaid = gClass.MaxInLobby;
					Debug.LogError($"RestrictionsInRaid MaxInRaid ({gClass.MaxInRaid}) < MaxInLobby ({gClass.MaxInLobby}) for item {gClass.TemplateId}");
				}
			}
			Gclass1737_0 = value;
		}
	}

	public void UpdateTradersSettings(TraderSettings[] tradersSettings)
	{
		foreach (TraderSettings traderSettings in tradersSettings)
		{
			TradersSettings[traderSettings.Id] = traderSettings;
		}
	}

	public void UpdatePrestigeSettings(GClass1791 prestigeSettings)
	{
		PrestigeSettings = prestigeSettings;
	}
}
