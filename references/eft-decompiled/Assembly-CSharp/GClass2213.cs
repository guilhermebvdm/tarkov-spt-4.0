using System.Collections.Generic;
using System.Runtime.Serialization;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;

public class GClass2213
{
	[JsonProperty("favoriteItems")]
	public FlatItemsDataClass[] FavoriteItems;

	[JsonProperty("aid")]
	public string AccountId = string.Empty;

	[JsonProperty("info")]
	public GClass1416.GClass1417 Info;

	[JsonProperty("achievements")]
	public Dictionary<MongoID, int> AchievementsData;

	[JsonProperty("customization")]
	public Dictionary<EBodyModelPart, MongoID> Customization;

	[JsonProperty("equipment")]
	public GClass2211 Equipment;

	[JsonProperty("pmcStats")]
	public ProfileStatsClass PmcStats;

	[JsonProperty("scavStats")]
	public ProfileStatsClass ScavStats;

	[JsonProperty("skills")]
	public SkillsDescriptorClass Skills;

	[JsonProperty("hideout")]
	public readonly HideoutProfileDescriptorClass Hideout;

	[JsonProperty("hideoutAreaStashes", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<EAreaType, MongoID> Dictionary_0;

	[JsonProperty("customizationStash")]
	public MongoID? Nullable_0;

	[JsonProperty("items")]
	public FlatItemsDataClass[] Gclass1390_0;

	public GClass2212 HideoutData;

	[OnDeserialized]
	public void method_0(StreamingContext context)
	{
		if (Hideout == null)
		{
			return;
		}
		Dictionary<EAreaType, CompoundItem> dictionary = new Dictionary<EAreaType, CompoundItem>();
		CompoundItem customizationStash = null;
		Dictionary<string, Item> items = Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(Gclass1390_0).Items;
		foreach (var (key, mongoID2) in Dictionary_0)
		{
			if (items.TryGetValue(mongoID2, out var value))
			{
				dictionary[key] = (CompoundItem)value;
			}
		}
		MongoID? nullable_ = Nullable_0;
		if (items.TryGetValue(nullable_.HasValue ? ((string)nullable_.GetValueOrDefault()) : null, out var value2))
		{
			customizationStash = (CompoundItem)value2;
		}
		HideoutData = new GClass2212(Hideout, dictionary, customizationStash, isGuest: true, AccountId, Info.Nickname);
	}
}
