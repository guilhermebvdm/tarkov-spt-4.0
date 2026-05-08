using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;

public class ProfileStats
{
	public class GClass787
	{
		public string Caption;

		public object Value;

		public EStatType Type;
	}

	public enum ESurvivorClass
	{
		Unknown,
		Neutralizer,
		Marauder,
		Paramedic,
		Survivor
	}

	public enum EStatType
	{
		Date,
		DateTime,
		Experience,
		Litres,
		Number,
		BodyPart,
		Health,
		SurvivorClass,
		Text
	}

	public SessionCountersClass SessionCounters;

	public readonly SessionCountersClass OverallCounters;

	public float SessionExperienceMult;

	public float ExperienceBonusMult;

	public int TotalSessionExperience;

	public int LastSessionDate;

	public GClass788 Aggressor;

	public readonly List<GClass2185> DroppedItems;

	public readonly List<GClass2186> FoundInRaidItems;

	public readonly GClass1630<GClass2201> Victims;

	public readonly List<MongoID> CarriedQuestItems;

	[CanBeNull]
	public readonly GClass2198 DamageHistory;

	[CanBeNull]
	public GClass2199 DeathCause;

	[CanBeNull]
	public LastPlayerStateClass LastPlayerState;

	public long TotalInGameTime;

	public ESurvivorClass SurvivorClass
	{
		get
		{
			ESurvivorClass result = ESurvivorClass.Unknown;
			long num = 0L;
			long num2 = OverallCounters.GetLong(SessionCounterTypesAbstractClass.ExpKill);
			if (num2 > 0L)
			{
				num = num2;
				result = ESurvivorClass.Neutralizer;
			}
			long num3 = OverallCounters.GetLong(SessionCounterTypesAbstractClass.ExpLooting);
			if (num3 > num)
			{
				num = num3;
				result = ESurvivorClass.Marauder;
			}
			long num4 = OverallCounters.GetLong(SessionCounterTypesAbstractClass.ExpHeal);
			if (num4 > num)
			{
				num = num4;
				result = ESurvivorClass.Paramedic;
			}
			if (OverallCounters.GetLong(SessionCounterTypesAbstractClass.ExpExitStatus) <= num)
			{
				return result;
			}
			result = ESurvivorClass.Survivor;
			return ESurvivorClass.Survivor;
		}
	}

	public ProfileStats(ProfileEftStatsClass descriptor)
	{
		SessionCounters = new SessionCountersClass(descriptor.SessionCounters);
		OverallCounters = new SessionCountersClass(descriptor.OverallCounters);
		SessionExperienceMult = descriptor.SessionExperienceMult;
		ExperienceBonusMult = descriptor.ExperienceBonusMult;
		TotalSessionExperience = descriptor.TotalSessionExperience;
		LastSessionDate = descriptor.LastSessionDate;
		TotalInGameTime = descriptor.TotalInGameTime;
		Aggressor = descriptor.Aggressor;
		DroppedItems = descriptor.DroppedItems;
		FoundInRaidItems = descriptor.FoundInRaidItems;
		Victims = new GClass1630<GClass2201>(descriptor.Victims);
		CarriedQuestItems = descriptor.CarriedQuestItems;
		DeathCause = descriptor.DeathCause;
		if (descriptor.DamageHistory != null)
		{
			DamageHistory = new GClass2198(descriptor.DamageHistory);
		}
		if (descriptor.LastPlayerState != null)
		{
			LastPlayerState = new LastPlayerStateClass(descriptor.LastPlayerState);
		}
	}
}
