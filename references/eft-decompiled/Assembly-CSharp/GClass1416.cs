using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;

public class GClass1416 : GInterface214
{
	public class GClass1417
	{
		[JsonProperty("Nickname")]
		public string Nickname;

		[JsonProperty("Side")]
		public EPlayerSide Side;

		[JsonProperty("Experience")]
		public int Experience;

		[JsonProperty("PrestigeLevel")]
		public int PrestigeLevel;

		[JsonProperty("MemberCategory")]
		public EMemberCategory MemberCategory;

		[JsonProperty("SelectedMemberCategory")]
		public EMemberCategory SelectedMemberCategory;

		[JsonProperty("RegistrationDate")]
		public int RegistrationDate;
	}

	public readonly string AccountId;

	public readonly GClass1417 Info;

	public readonly SkillManager Skills;

	public readonly SessionCountersClass OverallCounters;

	public readonly StashItemClass FavoriteItems;

	public readonly InventoryEquipment Equipment;

	public readonly GClass2197 Customization;

	public readonly TraderControllerClass ItemController;

	public readonly IReadOnlyDictionary<MongoID, int> AchievementsData;

	public readonly GClass789 PmcStats;

	public readonly GClass789 ScavStats;

	public readonly GClass2212 HideoutData;

	[NonSerialized]
	public LastPlayerStateClass LastPlayerStateClass;

	public LastPlayerStateClass PlayerVisualRepresentation
	{
		get
		{
			if (LastPlayerStateClass != null)
			{
				return LastPlayerStateClass;
			}
			LastPlayerStateClass = new LastPlayerStateClass(new GClass1410
			{
				Nickname = Info.Nickname,
				Level = InfoClass.GetLevel(Info.Experience),
				PrestigeLevel = Info.PrestigeLevel,
				MemberCategory = Info.MemberCategory,
				SelectedMemberCategory = Info.SelectedMemberCategory,
				Side = Info.Side
			}, Customization, Equipment);
			return LastPlayerStateClass;
		}
	}

	public string ProfileId => string.Empty;

	public string Nickname => Info.Nickname;

	public EPlayerSide Side => Info.Side;

	public int PrestigeLevel => Info.PrestigeLevel;

	public GClass1416(GClass2213 descriptor)
	{
		AccountId = descriptor.AccountId;
		Info = descriptor.Info;
		Equipment = descriptor.Equipment.ToEquipment();
		Skills = new SkillManager(descriptor.Skills);
		Customization = new GClass2197(descriptor.Customization);
		FavoriteItems = Singleton<ItemFactoryClass>.Instance.FlatItemsToStash(descriptor.FavoriteItems);
		ItemController = new TraderControllerClass(FavoriteItems, MongoID.Generate(), "favoriteItemsController");
		AchievementsData = descriptor.AchievementsData;
		PmcStats = new GClass789(descriptor.PmcStats);
		ScavStats = new GClass789(descriptor.ScavStats);
		OverallCounters = PmcStats.Eft.OverallCounters.SumCounters(ScavStats.Eft.OverallCounters);
		HideoutData = descriptor.HideoutData;
	}
}
