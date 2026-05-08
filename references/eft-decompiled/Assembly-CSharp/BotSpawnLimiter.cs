using System;
using System.Collections.Generic;
using EFT;

public class BotSpawnLimiter
{
	[NonSerialized]
	public BotsController BotsController;

	[NonSerialized]
	public Dictionary<string, int> PlayerIndividualSpawnLimits = new Dictionary<string, int>();

	public int Int32_0
	{
		get
		{
			if (BotsController.IsPvE)
			{
				return BotsController.BotLocationModifier.NonWaveSpawnBotsLimitPerPlayerPvE;
			}
			return BotsController.BotLocationModifier.NonWaveSpawnBotsLimitPerPlayer;
		}
	}

	public BotSpawnLimiter(BotsController controller)
	{
		BotsController = controller;
	}

	public bool IsInPlayerSpawnLimit(Player player, BotCreationDataClass creationData)
	{
		foreach (Profile profile in creationData.Profiles)
		{
			if ((BotSettingsRepoClass.IsBossOrFollower(profile.Info.Settings.Role) || profile.Info.Settings.Role == WildSpawnType.marksman) && profile.Info.Settings.Role != WildSpawnType.assaultGroup)
			{
				return true;
			}
		}
		if (PlayerIndividualSpawnLimits.TryGetValue(player.ProfileId, out var value))
		{
			return value < Int32_0;
		}
		return true;
	}

	public float method_0()
	{
		if (TarkovApplication.Exist(out var tarkovApplication))
		{
			return tarkovApplication.CurrentRaidSettings.SelectedLocation.BotStart;
		}
		return 0f;
	}

	public bool method_1()
	{
		if (TarkovApplication.Exist(out var tarkovApplication))
		{
			return tarkovApplication.CurrentRaidSettings.SelectedLocation.OfflineNewSpawn;
		}
		return false;
	}

	public void IncreaseUsedPlayerSpawns(Player player, BotCreationDataClass creationData)
	{
		if (creationData == null || !method_1() || GClass1891.PastTime <= method_0())
		{
			return;
		}
		foreach (Profile profile in creationData.Profiles)
		{
			if ((BotSettingsRepoClass.IsBossOrFollower(profile.Info.Settings.Role) || profile.Info.Settings.Role == WildSpawnType.marksman) && profile.Info.Settings.Role != WildSpawnType.assaultGroup)
			{
				return;
			}
		}
		if (PlayerIndividualSpawnLimits.TryGetValue(player.ProfileId, out var _))
		{
			PlayerIndividualSpawnLimits[player.ProfileId]++;
		}
		else
		{
			PlayerIndividualSpawnLimits.Add(player.ProfileId, 1);
		}
	}
}
