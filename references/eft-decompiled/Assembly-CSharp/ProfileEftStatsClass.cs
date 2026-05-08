using System.Collections.Generic;
using System.Linq;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

[GAttribute29]
public class ProfileEftStatsClass
{
	[JsonProperty("SessionCounters", NullValueHandling = NullValueHandling.Ignore)]
	public GClass2219 SessionCounters = new GClass2219();

	[JsonProperty("OverallCounters", NullValueHandling = NullValueHandling.Ignore)]
	public GClass2219 OverallCounters = new GClass2219();

	[JsonProperty("SessionExperienceMult")]
	public float SessionExperienceMult;

	[JsonProperty("ExperienceBonusMult")]
	public float ExperienceBonusMult;

	[JsonProperty("TotalSessionExperience")]
	public int TotalSessionExperience;

	[JsonProperty("LastSessionDate")]
	public int LastSessionDate;

	[JsonProperty("Aggressor", NullValueHandling = NullValueHandling.Ignore)]
	[GAttribute30]
	[CanBeNull]
	public GClass788 Aggressor;

	[JsonProperty("DroppedItems", NullValueHandling = NullValueHandling.Ignore)]
	public List<GClass2185> DroppedItems = new List<GClass2185>();

	[JsonProperty("FoundInRaidItems", NullValueHandling = NullValueHandling.Ignore)]
	public List<GClass2186> FoundInRaidItems = new List<GClass2186>();

	[JsonProperty("Victims", NullValueHandling = NullValueHandling.Ignore)]
	public List<GClass2201> Victims = new List<GClass2201>();

	[JsonProperty("CarriedQuestItems", NullValueHandling = NullValueHandling.Ignore)]
	public List<MongoID> CarriedQuestItems = new List<MongoID>();

	[JsonProperty("DamageHistory", NullValueHandling = NullValueHandling.Ignore)]
	[GAttribute30]
	[CanBeNull]
	public GClass2216 DamageHistory;

	[JsonProperty("DeathCause", NullValueHandling = NullValueHandling.Ignore)]
	[GAttribute30]
	[CanBeNull]
	public GClass2199 DeathCause;

	[JsonProperty("LastPlayerState", NullValueHandling = NullValueHandling.Ignore)]
	[GAttribute30]
	[CanBeNull]
	public GClass2214 LastPlayerState;

	[JsonProperty("TotalInGameTime")]
	public long TotalInGameTime;

	[JsonProperty("SurvivorClass")]
	public ProfileStats.ESurvivorClass SurvivorClass;

	public ProfileEftStatsClass()
	{
	}

	public ProfileEftStatsClass(ProfileStats stats)
	{
		SessionCounters = new GClass2219(stats.SessionCounters);
		OverallCounters = new GClass2219(stats.OverallCounters);
		SessionExperienceMult = stats.SessionExperienceMult;
		ExperienceBonusMult = stats.ExperienceBonusMult;
		TotalSessionExperience = stats.TotalSessionExperience;
		LastSessionDate = stats.LastSessionDate;
		Aggressor = GClass3694.Clone(stats.Aggressor);
		DroppedItems = stats.DroppedItems.Select(GClass3694.ClonePolymorph).ToList();
		FoundInRaidItems = stats.FoundInRaidItems.Select(GClass3694.ClonePolymorph).ToList();
		Victims = stats.Victims.Select(GClass3694.ClonePolymorph).ToList();
		CarriedQuestItems = stats.CarriedQuestItems.ToList();
		if (stats.DamageHistory != null)
		{
			DamageHistory = new GClass2216(stats.DamageHistory);
		}
		if (stats.DeathCause != null)
		{
			DeathCause = GClass3694.ClonePolymorph(stats.DeathCause);
		}
		if (stats.LastPlayerState != null)
		{
			LastPlayerState = new GClass2214(stats.LastPlayerState);
		}
		TotalInGameTime = stats.TotalInGameTime;
		SurvivorClass = stats.SurvivorClass;
	}
}
