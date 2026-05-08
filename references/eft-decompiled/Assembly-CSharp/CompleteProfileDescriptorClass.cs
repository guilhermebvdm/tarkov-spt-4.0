using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

[GAttribute29]
public class CompleteProfileDescriptorClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1419
	{
		public static readonly Class1419 class1419_0 = new Class1419();

		public static Func<KeyValuePair<MongoID, bool>, MongoID> func_0;

		public static Func<KeyValuePair<MongoID, bool>, bool> func_1;

		public static Func<KeyValuePair<EBodyModelPart, MongoID>, EBodyModelPart> func_2;

		public static Func<KeyValuePair<EBodyModelPart, MongoID>, MongoID> func_3;

		public static Func<KeyValuePair<MongoID, TaskConditionCounterClass>, MongoID> func_4;

		public static Func<KeyValuePair<MongoID, TaskConditionCounterClass>, GClass2227> func_5;

		public static Func<KeyValuePair<MongoID, EWishlistGroup>, MongoID> func_6;

		public static Func<KeyValuePair<MongoID, EWishlistGroup>, byte> func_7;

		public static Func<KeyValuePair<MongoID, int>, MongoID> func_8;

		public static Func<KeyValuePair<MongoID, int>, int> func_9;

		public static Func<GClass2660, MongoID> func_10;

		public static Func<GClass2660, int> func_11;

		public static Func<KeyValuePair<MongoID, int>, MongoID> func_12;

		public static Func<KeyValuePair<MongoID, int>, int> func_13;

		public static Func<KeyValuePair<MongoID, Profile.TraderInfo>, MongoID> func_14;

		public static Func<KeyValuePair<MongoID, Profile.TraderInfo>, TraderInfoClass> func_15;

		public static Func<KeyValuePair<MongoID, int>, MongoID> func_16;

		public static Func<KeyValuePair<MongoID, int>, int> func_17;

		public MongoID method_0(KeyValuePair<MongoID, bool> item)
		{
			return item.Key;
		}

		public bool method_1(KeyValuePair<MongoID, bool> item)
		{
			return item.Value;
		}

		public EBodyModelPart method_2(KeyValuePair<EBodyModelPart, MongoID> item)
		{
			return item.Key;
		}

		public MongoID method_3(KeyValuePair<EBodyModelPart, MongoID> item)
		{
			return item.Value;
		}

		public MongoID method_4(KeyValuePair<MongoID, TaskConditionCounterClass> item)
		{
			return item.Key;
		}

		public GClass2227 method_5(KeyValuePair<MongoID, TaskConditionCounterClass> item)
		{
			return new GClass2227(item.Value);
		}

		public MongoID method_6(KeyValuePair<MongoID, EWishlistGroup> pair)
		{
			return pair.Key;
		}

		public byte method_7(KeyValuePair<MongoID, EWishlistGroup> pair)
		{
			return (byte)pair.Value;
		}

		public MongoID method_8(KeyValuePair<MongoID, int> pair)
		{
			return pair.Key;
		}

		public int method_9(KeyValuePair<MongoID, int> pair)
		{
			return pair.Value;
		}

		public MongoID method_10(GClass2660 data)
		{
			return data.TemplateId;
		}

		public int method_11(GClass2660 data)
		{
			return data.Timestamp;
		}

		public MongoID method_12(KeyValuePair<MongoID, int> pair)
		{
			return pair.Key;
		}

		public int method_13(KeyValuePair<MongoID, int> pair)
		{
			return pair.Value;
		}

		public MongoID method_14(KeyValuePair<MongoID, Profile.TraderInfo> pair)
		{
			return pair.Key;
		}

		public TraderInfoClass method_15(KeyValuePair<MongoID, Profile.TraderInfo> pair)
		{
			return new TraderInfoClass(pair.Value);
		}

		public MongoID method_16(KeyValuePair<MongoID, int> pair)
		{
			return pair.Key;
		}

		public int method_17(KeyValuePair<MongoID, int> pair)
		{
			return pair.Value;
		}
	}

	[JsonProperty("_id")]
	public MongoID Id;

	[JsonProperty("aid")]
	public string AccountId;

	[JsonProperty("savage")]
	public MongoID? PetId;

	[JsonProperty("karmaValue")]
	public float KarmaValue;

	[JsonProperty("Info")]
	public ProfileInfoClass Info;

	[JsonProperty("Customization")]
	public Dictionary<EBodyModelPart, MongoID> Customization = new Dictionary<EBodyModelPart, MongoID>();

	[JsonProperty("Encyclopedia")]
	[GAttribute30]
	[CanBeNull]
	public Dictionary<MongoID, bool> Encyclopedia = new Dictionary<MongoID, bool>();

	[JsonProperty("Health")]
	[GAttribute30]
	[CanBeNull]
	public Profile.ProfileHealthClass Health = new Profile.ProfileHealthClass();

	[JsonProperty("Inventory")]
	public EFTInventoryClass Inventory;

	[JsonProperty("InsuredItems")]
	public InsuredItemClass[] InsuredItems = Array.Empty<InsuredItemClass>();

	[JsonProperty("Skills")]
	public SkillsDescriptorClass Skills = new SkillsDescriptorClass();

	[JsonProperty("Notes")]
	public NotesManagerClass.GClass3109 Notes = new NotesManagerClass.GClass3109();

	[JsonProperty("TaskConditionCounters")]
	public Dictionary<MongoID, GClass2227> TaskConditionCounters = new Dictionary<MongoID, GClass2227>();

	[JsonProperty("Quests")]
	public List<QuestDataClass> QuestsData = new List<QuestDataClass>();

	[JsonProperty("Achievements")]
	public Dictionary<MongoID, int> AchievementsData = new Dictionary<MongoID, int>();

	[JsonProperty("Prestige")]
	public Dictionary<MongoID, int> PrestigeData = new Dictionary<MongoID, int>();

	[JsonProperty("Variables")]
	public Dictionary<MongoID, int> VariableData = new Dictionary<MongoID, int>();

	[JsonProperty("UnlockedInfo")]
	public Profile.GClass2208 UnlockedRecipeInfo = new Profile.GClass2208();

	[JsonProperty("moneyTransferLimitData")]
	public Profile.GClass2209 TransferLimitData = new Profile.GClass2209();

	[JsonProperty("Bonuses")]
	public ProfileBonusesClass[] Bonuses = Array.Empty<ProfileBonusesClass>();

	[JsonProperty("Hideout")]
	[GAttribute31]
	public HideoutProfileDescriptorClass Hideout = new HideoutProfileDescriptorClass();

	[JsonProperty("RagfairInfo")]
	[GAttribute31]
	public RagfairInfoClass RagfairInfo = new RagfairInfoClass();

	[JsonProperty("WishList")]
	public Dictionary<MongoID, byte> WishList = new Dictionary<MongoID, byte>();

	[JsonProperty("Stats")]
	public ProfileStatsClass Stats = new ProfileStatsClass();

	[JsonProperty("CheckedMagazines")]
	public Dictionary<MongoID, int> CheckedMagazines = new Dictionary<MongoID, int>();

	[JsonProperty("CheckedChambers")]
	public List<MongoID> CheckedChambers = new List<MongoID>();

	[JsonProperty("TradersInfo")]
	public Dictionary<MongoID, TraderInfoClass> TradersInfo = new Dictionary<MongoID, TraderInfoClass>();

	public CompleteProfileDescriptorClass()
	{
	}

	public CompleteProfileDescriptorClass(Profile profile, ISearchController searchController)
	{
		Id = profile.ProfileId;
		AccountId = profile.AccountId;
		PetId = profile.PetId;
		KarmaValue = profile.KarmaValue;
		Info = new ProfileInfoClass(profile.Info);
		Dictionary<MongoID, bool> encyclopedia = profile.Encyclopedia;
		object obj;
		if (encyclopedia == null)
		{
			obj = null;
		}
		else
		{
			obj = encyclopedia.ToDictionary((KeyValuePair<MongoID, bool> item) => item.Key, (KeyValuePair<MongoID, bool> item) => item.Value);
			if (obj != null)
			{
				goto IL_018d;
			}
		}
		obj = new Dictionary<MongoID, bool>();
		goto IL_018d;
		IL_018d:
		Encyclopedia = (Dictionary<MongoID, bool>)obj;
		Customization = profile.Customization.ToDictionary((KeyValuePair<EBodyModelPart, MongoID> item) => item.Key, (KeyValuePair<EBodyModelPart, MongoID> item) => item.Value);
		Health = GClass3694.ClonePolymorph(profile.Health);
		Inventory = new EFTInventoryClass(profile.Inventory, searchController);
		Skills = new SkillsDescriptorClass(profile.Skills);
		Stats = new ProfileStatsClass(profile.Stats);
		TaskConditionCounters = profile.TaskConditionCounters?.ToDictionary((KeyValuePair<MongoID, TaskConditionCounterClass> item) => item.Key, (KeyValuePair<MongoID, TaskConditionCounterClass> item) => new GClass2227(item.Value));
		InsuredItemClass[] insuredItems = profile.InsuredItems;
		object obj2;
		if (insuredItems == null)
		{
			obj2 = null;
		}
		else
		{
			obj2 = insuredItems.Select(GClass3694.ClonePolymorph).ToArray();
			if (obj2 != null)
			{
				goto IL_02a9;
			}
		}
		obj2 = Array.Empty<InsuredItemClass>();
		goto IL_02a9;
		IL_02a9:
		InsuredItems = (InsuredItemClass[])obj2;
		Bonuses = profile.BonusController.ToDescriptor();
		WishList = profile.WishlistManager.UserItems.ToDictionary((KeyValuePair<MongoID, EWishlistGroup> pair) => pair.Key, (KeyValuePair<MongoID, EWishlistGroup> pair) => (byte)pair.Value);
		Notes = new NotesManagerClass.GClass3109(profile.Notes);
		QuestsData = profile.QuestsData.ToList();
		AchievementsData = profile.AchievementsData.ToDictionary((KeyValuePair<MongoID, int> pair) => pair.Key, (KeyValuePair<MongoID, int> pair) => pair.Value);
		PrestigeData = profile.PrestigeData.ToDictionary((GClass2660 data) => data.TemplateId, (GClass2660 data) => data.Timestamp);
		VariableData = profile.ProfileVariables.Values.ToDictionary((KeyValuePair<MongoID, int> pair) => pair.Key, (KeyValuePair<MongoID, int> pair) => pair.Value);
		UnlockedRecipeInfo = profile.UnlockedRecipeInfo;
		TransferLimitData = profile.TransferLimitData;
		TradersInfo = profile.TradersInfo.ToDictionary((KeyValuePair<MongoID, Profile.TraderInfo> pair) => pair.Key, (KeyValuePair<MongoID, Profile.TraderInfo> pair) => new TraderInfoClass(pair.Value));
		CheckedMagazines = profile.CheckedMagazines.ToDictionary((KeyValuePair<MongoID, int> pair) => pair.Key, (KeyValuePair<MongoID, int> pair) => pair.Value);
		CheckedChambers = profile.CheckedChambers.ToList();
		Hideout = profile.Hideout;
		RagfairInfo = profile.RagfairInfo;
	}
}
