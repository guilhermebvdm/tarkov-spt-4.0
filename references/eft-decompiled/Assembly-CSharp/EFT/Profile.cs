using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT.Counters;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.Quests;
using Newtonsoft.Json;
using UnityEngine;

namespace EFT;

public class Profile : GInterface214, IInventoryProfileInfo
{
	[GAttribute29]
	public class ProfileHealthClass
	{
		[GAttribute29]
		public class ValueInfo
		{
			[JsonProperty("Current")]
			public float Current;

			[JsonProperty("Minimum")]
			public float Minimum;

			[JsonProperty("Maximum")]
			public float Maximum;

			[JsonProperty("OverDamageReceivedMultiplier")]
			public float OverDamageReceivedMultiplier;

			[JsonProperty("EnvironmentDamageMultiplier")]
			public float EnvironmentDamageMultiplier;
		}

		[GAttribute29]
		public class GClass2206
		{
			[JsonProperty("Time")]
			public float Time = -1f;
		}

		[GAttribute29]
		public class ProfileBodyPartHealthClass
		{
			[JsonProperty("Health")]
			public ValueInfo Health = new ValueInfo();

			[JsonProperty("Effects")]
			public Dictionary<string, GClass2206> Effects = new Dictionary<string, GClass2206>();
		}

		[JsonProperty("BodyParts")]
		public Dictionary<EBodyPart, ProfileBodyPartHealthClass> BodyParts = new Dictionary<EBodyPart, ProfileBodyPartHealthClass>();

		[JsonProperty("Energy")]
		public ValueInfo Energy = new ValueInfo();

		[JsonProperty("Hydration")]
		public ValueInfo Hydration = new ValueInfo();

		[JsonProperty("Temperature")]
		public ValueInfo Temperature = Singleton<BackendConfigSettingsClass>.Instance.Health.ProfileHealthSettings.HealthFactorsSettings[EHealthFactorType.Temperature].ValueInfo;

		[JsonProperty("Poison")]
		public ValueInfo Poison = new ValueInfo
		{
			Current = 0f,
			Minimum = 0f,
			Maximum = 100f
		};

		[JsonProperty("UpdateTime")]
		public int? UpdateTime;
	}

	public enum ETraderType
	{
		None = -1,
		Lightkeeper,
		Btr,
		Ragman,
		ArenaManager,
		Prapor,
		Terapevt,
		Fence,
		Mechanic,
		Skier,
		Peacekeeper
	}

	public class TraderInfo : BackendConfigSettingsClass.GInterface168
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1411
		{
			public static readonly Class1411 class1411_0 = new Class1411();

			public static Func<Item, bool> func_0;

			public bool method_0(Item subItem)
			{
				return !(subItem is StashItemClass);
			}

			public ETraderType method_1(KeyValuePair<MongoID, ETraderType> kvp)
			{
				return kvp.Value;
			}

			public MongoID method_2(KeyValuePair<MongoID, ETraderType> kvp)
			{
				return kvp.Key;
			}
		}

		public const string LIGHT_KEEPER_TRADER_ID = "638f541a29ffd1183d187f57";

		public const string BTR_TRADER_ID = "656f0f98d80a697f855d34b1";

		public const string RAGMAN_TRADER_ID = "5ac3b934156ae10c4430e83c";

		public const string ARENA_MANAGER_TRADER_ID = "6617beeaa9cfa777ca915b7c";

		public const string FENCE_ID = "579dc571d53a0658a154fbec";

		public const string PEACEKEEPER_ID = "5935c25fb3acc3127c3d8cd9";

		public const string SKIER_ID = "58330581ace78e27b8b10cee";

		public const string PRAPOR_TRADER_ID = "54cb50c76803fa8b248b4571";

		public const string TERAPEVT_TRADER_ID = "54cb57776803fa99248b456e";

		public const string STORYTELLER_TRADER_ID = "6864e812f9fe664cb8b8e152";

		public const float LIGHTHOUSE_KEEPER_SERVICES_SCREW_UP_STANDING = 0.01f;

		public const float DEFAULT_MAX_STANDING_LEVEL = 6f;

		[NonSerialized]
		public const int DEFAULT_MAX_LOYALTY_LEVEL = 4;

		public static readonly IReadOnlyDictionary<MongoID, ETraderType> TraderIdToType = new Dictionary<MongoID, ETraderType>
		{
			{
				"5ac3b934156ae10c4430e83c",
				ETraderType.Ragman
			},
			{
				"6617beeaa9cfa777ca915b7c",
				ETraderType.ArenaManager
			},
			{
				"638f541a29ffd1183d187f57",
				ETraderType.Lightkeeper
			},
			{
				"656f0f98d80a697f855d34b1",
				ETraderType.Btr
			},
			{
				"54cb50c76803fa8b248b4571",
				ETraderType.Prapor
			},
			{
				"54cb57776803fa99248b456e",
				ETraderType.Terapevt
			},
			{
				"579dc571d53a0658a154fbec",
				ETraderType.Fence
			},
			{
				"5935c25fb3acc3127c3d8cd9",
				ETraderType.Peacekeeper
			},
			{
				"58330581ace78e27b8b10cee",
				ETraderType.Skier
			}
		};

		public static readonly IReadOnlyDictionary<ETraderType, MongoID> TraderTypeToId = TraderIdToType.ToDictionary((KeyValuePair<MongoID, ETraderType> kvp) => kvp.Value, (KeyValuePair<MongoID, ETraderType> kvp) => kvp.Key, GClass866<ETraderType>.EqualityComparer);

		[NonSerialized]
		public InfoClass ProfileInfo;

		[NonSerialized]
		public bool Banned;

		[NonSerialized]
		public HashSet<ETraderServiceType> AvailableServices = new HashSet<ETraderServiceType>();

		[NonSerialized]
		public HashSet<ETraderServiceType> AvailableForPurchaseServices = new HashSet<ETraderServiceType>();

		[NonSerialized]
		public HashSet<ETraderServiceType> AlreadyPurchasedServices = new HashSet<ETraderServiceType>();

		public readonly string Id;

		[field: NonSerialized]
		public int LoyaltyLevel { get; set; }

		[field: NonSerialized]
		public long SalesSum { get; set; }

		[field: NonSerialized]
		public double Standing { get; set; }

		[field: NonSerialized]
		public int NextResupply { get; set; }

		[field: NonSerialized]
		public bool Unlocked { get; set; }

		[field: NonSerialized]
		public bool Disabled { get; set; }

		public bool Available
		{
			get
			{
				if (!Disabled && Unlocked)
				{
					return !Banned;
				}
				return false;
			}
		}

		public int ProfileLevel => ProfileInfo.Level;

		public string GameVersion => ProfileInfo.GameVersion;

		[field: NonSerialized]
		public BackendConfigSettingsClass.TraderSettings Settings { get; set; }

		[field: NonSerialized]
		public BackendConfigSettingsClass.TraderLoyaltyLevel CurrentLoyalty { get; set; }

		public int MaxLoyaltyLevel => Settings?.LoyaltyLevels?.Length ?? 4;

		public virtual double SellToTraderPriceModifier => 1.0 - CurrentLoyalty.SellToTraderPriceCoef / 100.0;

		public event Action OnStandingChanged;

		public event Action OnSalesSumChanged;

		public event Action OnLoyaltyChanged;

		public event Action OnAvailabilityChanged;

		public event Action OnResupplyTimeChanged;

		public event Action OnTraderServicesUpdated;

		public event Action<string, int> OnSoldToTrader;

		public TraderInfo(MongoID id, TraderInfoClass descriptor, InfoClass profileInfo)
		{
			Id = id;
			LoyaltyLevel = descriptor.LoyaltyLevel;
			SalesSum = descriptor.SalesSum;
			Standing = descriptor.Standing;
			NextResupply = descriptor.NextResupply;
			Unlocked = descriptor.Unlocked;
			Disabled = descriptor.Disabled;
			ProfileInfo = profileInfo;
			Settings = Singleton<BackendConfigSettingsClass>.Instance.TradersSettings[Id];
			Unlocked |= Settings.UnlockedByDefault;
			NextResupply = Settings.NextResupply;
			profileInfo.OnLevelChanged += delegate
			{
				UpdateLevel();
			};
			profileInfo.OnBanChanged += method_0;
			method_0();
			UpdateLevel();
		}

		public void SetSoldToTraderItem(string templateId, int count)
		{
			this.OnSoldToTrader?.Invoke(templateId, count);
		}

		public virtual double ApplyPriceModifier(double basePrice)
		{
			return Math.Floor(basePrice * SellToTraderPriceModifier);
		}

		public bool IsServiceAvailable(ETraderServiceType serviceType)
		{
			return AvailableServices.Contains(serviceType);
		}

		public bool IsServiceAvailableForPurchase(ETraderServiceType serviceType)
		{
			return AvailableForPurchaseServices.Contains(serviceType);
		}

		public bool IsServiceAlreadyPurchased(ETraderServiceType serviceType)
		{
			return AlreadyPurchasedServices.Contains(serviceType);
		}

		public void SetServiceAvailability(ETraderServiceType serviceType, bool availabilityState, bool wasPurchasedInRaid)
		{
			if (availabilityState)
			{
				AvailableForPurchaseServices.Add(serviceType);
			}
			else
			{
				AvailableForPurchaseServices.Remove(serviceType);
			}
			if (wasPurchasedInRaid)
			{
				AlreadyPurchasedServices.Add(serviceType);
			}
			else
			{
				AlreadyPurchasedServices.Remove(serviceType);
			}
			AvailableServices.Add(serviceType);
			this.OnTraderServicesUpdated?.Invoke();
		}

		public bool CanBuyItem(Item item)
		{
			return (from subItem in GClass3380.GetAllItems(item)
				where !(subItem is StashItemClass)
				select subItem).All((Item subItem) => CanBuyItem(subItem.Template));
		}

		public bool CanBuyItem(ItemTemplate itemTemplate)
		{
			if (!Settings.DoesNotBuyItems.Contains(itemTemplate))
			{
				return Settings.BuysItems.Contains(itemTemplate);
			}
			return false;
		}

		public bool CanTransferItem(ItemTemplate itemTemplate)
		{
			if (!Settings.ProhibitedTransferableItems.Contains(itemTemplate))
			{
				return Settings.TransferableItems.Contains(itemTemplate);
			}
			return false;
		}

		public bool CanBuyRootItem(Item item, out IReadOnlyList<Item> unsellableParts)
		{
			List<Item> list = (List<Item>)(unsellableParts = new List<Item>());
			bool flag = false;
			foreach (Item allItem in GClass3380.GetAllItems(item))
			{
				bool flag2 = CanBuyItem(allItem.Template);
				if (flag || flag2)
				{
					flag = true;
					if (!flag2)
					{
						list.Add(allItem);
					}
					continue;
				}
				return false;
			}
			return true;
		}

		public bool TryGetNextLoyalty(out BackendConfigSettingsClass.TraderLoyaltyLevel nextLoyalty)
		{
			BackendConfigSettingsClass.TraderLoyaltyLevel? traderLoyaltyLevel = Settings?.GetLoyaltyLevelSettings(LoyaltyLevel + 1);
			nextLoyalty = traderLoyaltyLevel.GetValueOrDefault();
			return traderLoyaltyLevel.HasValue;
		}

		public virtual void UpdateLevel()
		{
			if (Settings == null)
			{
				return;
			}
			int loyaltyLevel = Settings.GetLoyaltyLevel(this);
			if (loyaltyLevel != LoyaltyLevel)
			{
				LoyaltyLevel = loyaltyLevel;
				BackendConfigSettingsClass.TraderLoyaltyLevel? loyaltyLevelSettings = Settings.GetLoyaltyLevelSettings(LoyaltyLevel);
				if (!loyaltyLevelSettings.HasValue)
				{
					throw new IndexOutOfRangeException($"Loyalty level {LoyaltyLevel} not found.");
				}
				CurrentLoyalty = loyaltyLevelSettings.Value;
				this.OnLoyaltyChanged?.Invoke();
			}
		}

		public void method_0(EBanType banType = EBanType.Trading)
		{
			if (banType == EBanType.Trading)
			{
				bool flag = ProfileInfo.GetBan(EBanType.Trading) != null;
				if (Banned != flag)
				{
					Banned = flag;
					this.OnAvailabilityChanged?.Invoke();
				}
			}
		}

		public virtual void SetStanding(double value)
		{
			if (!GClass855.ApproxEquals(Standing, value))
			{
				Standing = value;
				UpdateLevel();
				this.OnStandingChanged?.Invoke();
			}
		}

		public void SetSalesSum(long value)
		{
			if (SalesSum != value)
			{
				SalesSum = value;
				UpdateLevel();
				this.OnSalesSumChanged?.Invoke();
			}
		}

		public void SetUnlocked(bool unlocked)
		{
			unlocked |= Settings.UnlockedByDefault;
			if (Unlocked != unlocked)
			{
				Unlocked = unlocked;
				this.OnAvailabilityChanged?.Invoke();
			}
		}

		public void SetDisabled(bool disabled)
		{
			if (Disabled != disabled)
			{
				Disabled = disabled;
				this.OnAvailabilityChanged?.Invoke();
			}
		}

		public void SetResupplyTime(int resupplyTime)
		{
			if (NextResupply != resupplyTime)
			{
				NextResupply = resupplyTime;
				this.OnResupplyTimeChanged?.Invoke();
			}
		}

		[CompilerGenerated]
		public void method_1(int _, int __)
		{
			UpdateLevel();
		}

		[CompilerGenerated]
		public bool method_2(Item subItem)
		{
			return CanBuyItem(subItem.Template);
		}
	}

	public class FenceTraderInfo : TraderInfo
	{
		[NonSerialized]
		public SessionCountersClass SessionCounters;

		[NonSerialized]
		public float CharismaFenceDiscount;

		[field: NonSerialized]
		public FenceLoyaltyLevel FenceLoyalty { get; set; }

		public int AvailableExitsCount => FenceLoyalty.AvailableExits;

		public double FenceLoyaltyPriceModifier => FenceLoyalty.PriceModifier;

		public override double ApplyPriceModifier(double basePrice)
		{
			basePrice = Math.Floor(basePrice * Math.Round(SellToTraderPriceModifier, 3));
			return Math.Floor(basePrice / Math.Round(FenceLoyaltyPriceModifier, 3));
		}

		public FenceTraderInfo(MongoID id, TraderInfoClass descriptor, Profile profile)
			: base(id, descriptor, profile.Info)
		{
			SessionCounters = profile.EftStats.SessionCounters;
			CharismaFenceDiscount = profile.Skills.CharismaEliteFenceRepPenaltyReduction;
		}

		public override void UpdateLevel()
		{
			FenceLoyalty = Singleton<BackendConfigSettingsClass>.Instance.FenceSettings.GetSettings(base.Standing);
			base.UpdateLevel();
		}

		public int NewExfiltrationPrice(int price)
		{
			return (int)(FenceLoyalty.ExfiltrationPriceModifier * (float)price);
		}

		public void AddStanding(double dif, EFenceStandingSource standingSource, bool charismaFenceDiscount = true)
		{
			if ((standingSource == EFenceStandingSource.ScavKill || standingSource == EFenceStandingSource.BossKill) && GClass855.Negative(dif) && charismaFenceDiscount)
			{
				dif *= (double)(1f - CharismaFenceDiscount);
				dif = Math.Floor(dif * 100.0) / 100.0;
			}
			if (standingSource == EFenceStandingSource.PmcBotKill)
			{
				dif *= (double)Singleton<BackendConfigSettingsClass>.Instance.FenceSettings.PmcBotKillStandingMultiplier;
			}
			base.SetStanding(base.Standing + dif);
			SessionCounters.AddDouble(dif, CounterTag.FenceStanding, standingSource);
		}

		public override void SetStanding(double value)
		{
			base.SetStanding(value);
		}
	}

	[GAttribute29]
	public class GClass2208
	{
		[JsonProperty("unlockedProductionRecipe")]
		public List<MongoID> unlockedSchemeList = new List<MongoID>();
	}

	[GAttribute29]
	public class GClass2209
	{
		[JsonProperty("nextResetTime")]
		public int nextResetTime;

		[JsonProperty("remainingLimit")]
		public int remainingLimit;

		[JsonProperty("totalLimit")]
		public int totalLimit;

		[JsonProperty("resetInterval")]
		public int resetInterval;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1412
	{
		public static readonly Class1412 class1412_0 = new Class1412();

		public static Func<KeyValuePair<MongoID, int>, GClass2660> func_0;

		public static Func<KeyValuePair<MongoID, GClass2227>, MongoID> func_1;

		public static Func<KeyValuePair<MongoID, GClass2227>, TaskConditionCounterClass> func_2;

		public static Func<ProfileBonusesClass, SkillBonusAbstractClass> func_3;

		public static Func<Item, ItemTemplate> func_4;

		public GClass2660 method_0(KeyValuePair<MongoID, int> data)
		{
			return new GClass2660(data.Key, data.Value);
		}

		public MongoID method_1(KeyValuePair<MongoID, GClass2227> counter)
		{
			return counter.Key;
		}

		public TaskConditionCounterClass method_2(KeyValuePair<MongoID, GClass2227> counter)
		{
			return new TaskConditionCounterClass(counter.Value);
		}

		public SkillBonusAbstractClass method_3(ProfileBonusesClass bonus)
		{
			return bonus.ToBonus();
		}

		public ItemTemplate method_4(Item x)
		{
			return x.Template;
		}
	}

	[CompilerGenerated]
	public class Class1413
	{
		public TraderInfo traderInfo;

		public Profile profile_0;

		public void method_0()
		{
			profile_0.OnTraderLoyaltyChanged?.Invoke(traderInfo);
		}

		public void method_1()
		{
			profile_0.OnTraderStandingChanged?.Invoke(traderInfo);
		}
	}

	public string Id;

	public readonly string AccountId;

	public readonly MongoID? PetId;

	public readonly float KarmaValue;

	public readonly InfoClass Info;

	public GClass2197 Customization;

	public readonly Dictionary<MongoID, bool> Encyclopedia;

	public ProfileHealthClass Health;

	public Inventory Inventory;

	public InsuredItemClass[] InsuredItems;

	public SkillManager Skills;

	public readonly NotesManagerClass Notes;

	public readonly Dictionary<MongoID, TaskConditionCounterClass> TaskConditionCounters;

	public List<QuestDataClass> QuestsData;

	public readonly Dictionary<MongoID, int> AchievementsData;

	public readonly IReadOnlyList<GClass2660> PrestigeData;

	public readonly GClass2232 ProfileVariables;

	public readonly GClass2208 UnlockedRecipeInfo;

	public readonly GClass2209 TransferLimitData;

	public readonly HideoutProfileDescriptorClass Hideout;

	public readonly RagfairInfoClass RagfairInfo;

	public readonly GClass2067 WishlistManager;

	[NonSerialized]
	public Dictionary<MongoID, int> ProfileVariables_1;

	public readonly GClass789 Stats;

	public readonly Dictionary<MongoID, int> CheckedMagazines;

	public readonly List<MongoID> CheckedChambers;

	[field: NonSerialized]
	public Dictionary<MongoID, TraderInfo> TradersInfo { get; }

	[field: NonSerialized]
	public BonusController BonusController { get; set; }

	public string Nickname => Info.Nickname;

	public string ProfileId => Id;

	[JsonIgnore]
	public IInventoryProfileSkillInfo SkillsInfo => Skills;

	[JsonIgnore]
	public Inventory InventoryInfo => Inventory;

	public int Experience
	{
		get
		{
			return Info.Experience;
		}
		set
		{
			Info.Experience = value;
		}
	}

	public EPlayerSide Side => Info.Side;

	public int PrestigeLevel => Info.PrestigeLevel;

	string GInterface214.ProfileId => Id;

	[field: NonSerialized]
	public FenceTraderInfo FenceInfo { get; set; }

	public int MagDrillsMastering => Skills.MagDrills.Level / 10;

	public ProfileStats EftStats
	{
		get
		{
			return Stats.Eft ?? (Stats.Eft = new ProfileStats(new ProfileEftStatsClass()));
		}
		set
		{
			Stats.Eft = value;
		}
	}

	public ProfileStats.ESurvivorClass SurvivorClass
	{
		get
		{
			if (Info.Level >= 20)
			{
				return EftStats.SurvivorClass;
			}
			return ProfileStats.ESurvivorClass.Unknown;
		}
	}

	public event Action<string, string> OnItemZoneDropped;

	public event Action<TraderInfo> OnTraderStandingChanged;

	public event Action<TraderInfo> OnTraderLoyaltyChanged;

	public Profile(CompleteProfileDescriptorClass descriptor)
	{
		Id = descriptor.Id;
		AccountId = descriptor.AccountId;
		PetId = descriptor.PetId;
		KarmaValue = descriptor.KarmaValue;
		Info = ProfileInfoFactory(descriptor.Info);
		Customization = new GClass2197(descriptor.Customization);
		Encyclopedia = descriptor.Encyclopedia;
		Health = descriptor.Health;
		Inventory = descriptor.Inventory.ToInventory();
		Skills = new SkillManager(descriptor.Skills);
		Stats = new GClass789(descriptor.Stats);
		QuestsData = descriptor.QuestsData;
		AchievementsData = descriptor.AchievementsData;
		PrestigeData = descriptor.PrestigeData.Select((KeyValuePair<MongoID, int> data) => new GClass2660(data.Key, data.Value)).ToList();
		ProfileVariables = new GClass2232(descriptor.VariableData);
		UnlockedRecipeInfo = descriptor.UnlockedRecipeInfo;
		TransferLimitData = descriptor.TransferLimitData;
		CheckedMagazines = descriptor.CheckedMagazines;
		CheckedChambers = descriptor.CheckedChambers;
		TaskConditionCounters = descriptor.TaskConditionCounters.ToDictionary((KeyValuePair<MongoID, GClass2227> counter) => counter.Key, (KeyValuePair<MongoID, GClass2227> counter) => new TaskConditionCounterClass(counter.Value));
		InsuredItems = descriptor.InsuredItems;
		WishlistManager = new GClass2067(descriptor.WishList);
		Notes = new NotesManagerClass(descriptor.Notes);
		IEnumerable<SkillBonusAbstractClass> bonuses = descriptor.Bonuses.Select((ProfileBonusesClass bonus) => bonus.ToBonus());
		BonusController = new BonusController(bonuses, Skills);
		Inventory.Stash?.UpdateBonuses(BonusController);
		Skills.Init(BonusController);
		Hideout = descriptor.Hideout;
		RagfairInfo = descriptor.RagfairInfo;
		MongoID fenceId = Singleton<BackendConfigSettingsClass>.Instance.FenceSettings.FenceId;
		Dictionary<MongoID, BackendConfigSettingsClass.TraderSettings>.KeyCollection keys = Singleton<BackendConfigSettingsClass>.Instance.TradersSettings.Keys;
		TradersInfo = new Dictionary<MongoID, TraderInfo>();
		foreach (MongoID item in (IEnumerable<MongoID>)keys)
		{
			if (!descriptor.TradersInfo.TryGetValue(item, out var value))
			{
				value = new TraderInfoClass();
			}
			TraderInfo traderInfo;
			if (item != fenceId)
			{
				traderInfo = new TraderInfo(item, value, Info);
			}
			else
			{
				FenceInfo = new FenceTraderInfo(item, value, this);
				traderInfo = FenceInfo;
			}
			TradersInfo[item] = traderInfo;
			traderInfo.OnLoyaltyChanged += delegate
			{
				this.OnTraderLoyaltyChanged?.Invoke(traderInfo);
			};
			traderInfo.OnStandingChanged += delegate
			{
				this.OnTraderStandingChanged?.Invoke(traderInfo);
			};
		}
		if (Customization.TryGetValue(EBodyModelPart.DogTag, out var value2))
		{
			SetUpDogTagSlot(value2);
		}
	}

	public virtual InfoClass ProfileInfoFactory(ProfileInfoClass descriptor)
	{
		return new InfoClass(descriptor);
	}

	public Profile Clone()
	{
		return new Profile(new CompleteProfileDescriptorClass(this, GClass2240.Instance));
	}

	public void SetUpDogTagSlot(MongoID dogTagId, TraderControllerClass itemController = null)
	{
		bool flag;
		if (flag = itemController == null)
		{
			itemController = new TraderControllerClass(Singleton<ItemFactoryClass>.Instance.CreateFakeStash(), ProfileId, ProfileId);
		}
		Item itemClone = Singleton<CustomizationSolverClass>.Instance.GetDogTag(dogTagId).GetItemClone(Side);
		Slot slot = Inventory.Equipment.GetSlot(EquipmentSlot.Dogtag);
		if (slot.ContainedItem != null)
		{
			GStruct154<GClass3413> gStruct = slot.RemoveItemWithoutRestrictions();
			if (gStruct.Failed)
			{
				Debug.LogError(gStruct.Error);
				return;
			}
			if (!flag)
			{
				gStruct.Value.RaiseEvents(itemController, CommandStatus.Begin);
				gStruct.Value.RaiseEvents(itemController, CommandStatus.Succeed);
			}
		}
		GStruct154<GClass3405> gStruct2 = InteractionsHandlerClass.AddWithoutRestrictions(itemClone, slot.CreateItemAddress(), itemController);
		if (gStruct2.Failed)
		{
			Debug.LogError(gStruct2.Error);
		}
		else if (!flag)
		{
			gStruct2.Value.RaiseEvents(itemController, CommandStatus.Begin);
			gStruct2.Value.RaiseEvents(itemController, CommandStatus.Succeed);
		}
	}

	public bool ForgetInEncyclopedia(MongoID itemId)
	{
		return Encyclopedia.Remove(itemId);
	}

	public void LearnInEncyclopedia(MongoID itemId)
	{
		Encyclopedia.Add(itemId, value: false);
	}

	public bool IsCheckedChambers(MongoID weaponId)
	{
		return CheckedChambers.Contains(weaponId);
	}

	public void CheckChamber(MongoID weaponId)
	{
		CheckedChambers.Add(weaponId);
	}

	public void UnCheckChamber(MongoID weaponId)
	{
		CheckedChambers.Remove(weaponId);
	}

	public bool IsCheckedMagazines(MongoID magazineId)
	{
		return CheckedMagazines.ContainsKey(magazineId);
	}

	public void CheckMagazines(MongoID magazineId, int value)
	{
		CheckedMagazines[magazineId] = value;
	}

	public void UnCheckMagazines(MongoID magazineId)
	{
		CheckedMagazines.Remove(magazineId);
	}

	public int CheckMagazinesValue(MongoID magazineId)
	{
		return CheckedMagazines[magazineId];
	}

	public void AddSessionCounterLong(int examineExperience, SessionCountersClass.SessionCounterIdentifierValueClass identifier)
	{
		EftStats.SessionCounters.AddLong(examineExperience, identifier);
	}

	public LastPlayerStateClass GetVisualEquipmentState(bool firstLevelItemsOnly = true)
	{
		return new LastPlayerStateClass(new GClass1410
		{
			Level = Info.Level,
			MemberCategory = ((Info.Side != EPlayerSide.Savage) ? Info.SelectedMemberCategory : EMemberCategory.Default),
			SelectedMemberCategory = Info.SelectedMemberCategory,
			Nickname = Info.Nickname,
			Side = Info.Side,
			Health = Health
		}, Customization, firstLevelItemsOnly ? GClass3380.CloneVisibleItem(Inventory.Equipment) : Inventory.Equipment);
	}

	public bool TryGetTraderInfo(string traderId, out TraderInfo traderInfo)
	{
		return TradersInfo.TryGetValue(traderId, out traderInfo);
	}

	public double GetTraderStanding(string traderId)
	{
		if (TryGetTraderInfo(traderId, out var traderInfo))
		{
			return traderInfo.Standing;
		}
		return -1.0;
	}

	public int GetTraderLoyalty(string traderId)
	{
		if (TryGetTraderInfo(traderId, out var traderInfo))
		{
			return traderInfo.LoyaltyLevel;
		}
		return -1;
	}

	public int GetExfiltrationPrice(int price, MarkOfUnknownItemClass markOfUnknown)
	{
		float num = FenceInfo.FenceLoyalty.ExfiltrationPriceModifier * (float)price;
		float num2 = markOfUnknown?.TradersDiscount ?? 1f;
		return Math.Max((int)(num * num2 * (1f - (float)Skills.CharismaExfiltrationDiscount)), 1);
	}

	public TaskConditionCounterClass GetTaskConditionCounter(IConditional source, MongoID conditionId)
	{
		Condition conditionCounterTemplate = GClass3998.GetConditionCounterTemplate(source, conditionId);
		if (!TaskConditionCounters.ContainsKey(conditionId))
		{
			TaskConditionCounters[conditionId] = new TaskConditionCounterClass(source, conditionCounterTemplate);
		}
		return TaskConditionCounters[conditionId];
	}

	public int CheckedMagazineSkillLevel(MongoID id)
	{
		if (!CheckedMagazines.TryGetValue(id, out var value))
		{
			return 0;
		}
		return value;
	}

	public void AddToCarriedQuestItems(string id)
	{
		if (!EftStats.CarriedQuestItems.Contains(id))
		{
			EftStats.CarriedQuestItems.Add(id);
		}
	}

	public bool Examined(Item item)
	{
		if (Encyclopedia == null)
		{
			return true;
		}
		if (!Encyclopedia.ContainsKey(item.TemplateId))
		{
			return item.ExaminedByDefault;
		}
		return true;
	}

	public bool Examined(MongoID templateId)
	{
		if (Encyclopedia != null)
		{
			return Encyclopedia.ContainsKey(templateId);
		}
		return true;
	}

	public void LearnAll()
	{
		foreach (Item item in from item in Inventory.GetPlayerItems(EPlayerItems.NonQuestItems)
			where !Encyclopedia.ContainsKey(item.TemplateId)
			select item)
		{
			Encyclopedia.Add(item.TemplateId, value: false);
		}
	}

	public void ItemDroppedAtPlace(string itemId, string zoneId)
	{
		this.OnItemZoneDropped?.Invoke(itemId, zoneId);
	}

	public IEnumerable<ResourceKey> GetAllPrefabPaths(bool allCustomization = true)
	{
		CustomizationSolverClass instance = Singleton<CustomizationSolverClass>.Instance;
		GClass2197 partsWithVisual = Customization.GetPartsWithVisual();
		if (instance != null)
		{
			foreach (KeyValuePair<EBodyModelPart, MongoID> item in partsWithVisual)
			{
				item.Deconstruct(out var key, out var value);
				EBodyModelPart eBodyModelPart = key;
				MongoID templateId = value;
				bool flag = eBodyModelPart == EBodyModelPart.Body || eBodyModelPart == EBodyModelPart.Feet || eBodyModelPart == EBodyModelPart.Head;
				if (allCustomization || flag)
				{
					yield return instance.GetBundle(templateId);
				}
			}
		}
		ResourceKey resourceKey = instance?.GetWatchBundle(partsWithVisual[EBodyModelPart.Hands]).WatchPrefab;
		if (resourceKey != null && !string.IsNullOrEmpty(resourceKey.path))
		{
			yield return resourceKey;
		}
		IEnumerable<ItemTemplate> enumerable = from x in GClass3380.GetAllItemsFromCollection(Inventory.Equipment)
			select x.Template;
		foreach (ItemTemplate item2 in enumerable)
		{
			foreach (ResourceKey allResource in item2.AllResources)
			{
				yield return allResource;
			}
		}
		object obj;
		object obj2;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			obj = instance.GetVoice(Customization[EBodyModelPart.Voice]);
			if (obj != null)
			{
				obj2 = ((GClass3672)obj).Name;
				goto IL_0234;
			}
		}
		obj2 = null;
		goto IL_0234;
		IL_0234:
		string text = (string)obj2;
		if (!GClass1673.IsNullOrEmpty(text))
		{
			yield return new ResourceKey
			{
				path = ResourceKeyManagerAbstractClass.TakePhrasePath(text),
				rcid = text
			};
		}
	}

	public void SetSpawnedInSession(bool value)
	{
		foreach (Item playerItem in Inventory.GetPlayerItems())
		{
			playerItem.SpawnedInSession = value;
		}
	}

	public void SetDefaultFenceInfo()
	{
		FenceInfo = new FenceTraderInfo(Singleton<BackendConfigSettingsClass>.Instance.FenceSettings.FenceId, new TraderInfoClass(), this);
	}

	[CompilerGenerated]
	public bool method_0(Item item)
	{
		return !Encyclopedia.ContainsKey(item.TemplateId);
	}
}
