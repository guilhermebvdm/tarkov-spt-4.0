using System;
using Comfort.Common;
using EFT;
using UnityEngine;

public class CultistEventsClass
{
	[NonSerialized]
	public const float Float_0 = 5f;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public BotsGroup BotsGroup_0;

	public CultistEventsClass(BotOwner owner)
	{
		BotsGroup_0 = owner.BotsGroup;
	}

	public void method_0()
	{
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!allAlivePlayers.AIData.IsAI)
			{
				if (method_1(allAlivePlayers))
				{
					method_2(allAlivePlayers);
				}
				else
				{
					method_3(allAlivePlayers);
				}
			}
		}
	}

	public bool method_1(IPlayer player)
	{
		return player.FindCultistAmulet() != null;
	}

	public void method_2(Player player)
	{
		Singleton<BotEventHandler>.Instance.OnBeingHit += method_4;
		BotsGroup_0.AddAlly(player);
		BotsGroup_0.RemoveEnemy(player, EBotEnemyCause.lighthouseKeeperServices);
	}

	public void method_3(Player player)
	{
		Singleton<BotEventHandler>.Instance.OnBeingHit -= method_4;
		BotsGroup_0.AddEnemy(player, EBotEnemyCause.lighthouseKeeperServices);
	}

	public void ManualUpdate()
	{
		if (Time.time > Float_1)
		{
			Float_1 = Time.time + 5f;
			method_0();
		}
	}

	public void method_4(DamageInfoStruct damageInfo, Player victim)
	{
		if (!(victim == null) && victim.AIData != null && victim.AIData.IsAI)
		{
			Float_1 = Time.time + 5f;
			IPlayer iPlayer = damageInfo.Player.iPlayer;
			if (BotSettingsRepoClass.IsSectant(victim.Profile.Info.Settings.Role))
			{
				BotsGroup_0.AddEnemy(iPlayer, EBotEnemyCause.lighthouseKeeperServices);
			}
		}
	}

	public void Dispose()
	{
		Singleton<BotEventHandler>.Instance.OnBeingHit -= method_4;
	}
}
