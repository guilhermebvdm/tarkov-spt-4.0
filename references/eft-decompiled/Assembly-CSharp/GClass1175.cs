using System;
using System.Collections.Generic;
using Audio.Data;
using Comfort.Common;
using EFT;

public class GClass1175 : GInterface463
{
	[NonSerialized]
	public BtrDriverSoundBankContainer BtrDriverSoundBankContainer_0;

	[NonSerialized]
	public Dictionary<string, EBtrDriverPhraseTrigger> Dictionary_0 = new Dictionary<string, EBtrDriverPhraseTrigger>
	{
		{
			LocationID + "/p1",
			EBtrDriverPhraseTrigger.CinemaStation
		},
		{
			LocationID + "/p2",
			EBtrDriverPhraseTrigger.TramwayStation
		},
		{
			LocationID + "/p3",
			EBtrDriverPhraseTrigger.CenterStation
		},
		{
			LocationID + "/p4",
			EBtrDriverPhraseTrigger.FallingCraneStation
		},
		{
			LocationID + "/p5",
			EBtrDriverPhraseTrigger.ScavsBlockadeStation
		},
		{
			LocationID + "/p6",
			EBtrDriverPhraseTrigger.HotelStation
		},
		{
			LocationID + "/p7",
			EBtrDriverPhraseTrigger.HotelStation
		}
	};

	[NonSerialized]
	public Dictionary<EPlayerSide, EBtrDriverPhraseTrigger> Dictionary_1 = new Dictionary<EPlayerSide, EBtrDriverPhraseTrigger>
	{
		{
			EPlayerSide.Bear,
			EBtrDriverPhraseTrigger.GreetingBear
		},
		{
			EPlayerSide.Usec,
			EBtrDriverPhraseTrigger.GreetingUsec
		},
		{
			EPlayerSide.Savage,
			EBtrDriverPhraseTrigger.GreetingScav
		}
	};

	public static string LocationID => Singleton<GameWorld>.Instance.LocationId;

	public void SetPhrasesData(BtrDriverSoundBankContainer soundContainer)
	{
		BtrDriverSoundBankContainer_0 = soundContainer;
	}

	public bool TryGetNamedStationTrigger(string pathID, out EBtrDriverPhraseTrigger trigger)
	{
		return Dictionary_0.TryGetValue(pathID, out trigger);
	}

	public bool TryGetGreetingTrigger(EPlayerSide playerSide, out EBtrDriverPhraseTrigger trigger)
	{
		return Dictionary_1.TryGetValue(playerSide, out trigger);
	}

	public bool TryGetBank(EBtrDriverPhraseTrigger trigger, out SoundBankWithSettings bank)
	{
		bank = null;
		if (BtrDriverSoundBankContainer_0 != null)
		{
			return BtrDriverSoundBankContainer_0.TryGetBank(trigger, out bank);
		}
		return false;
	}
}
