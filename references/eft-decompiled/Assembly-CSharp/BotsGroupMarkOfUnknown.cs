using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotsGroupMarkOfUnknown
{
	[NonSerialized]
	public BotsGroup Groups;

	[NonSerialized]
	public float SDIST_TO_ENEMY = 3600f;

	[NonSerialized]
	public List<IPlayer> ListToEnemies = new List<IPlayer>();

	[NonSerialized]
	public bool HaveSomeBodyWithMark;

	[NonSerialized]
	public float NextCheck;

	public BotsGroupMarkOfUnknown(BotsGroup groups)
	{
		Groups = groups;
		HaveSomeBodyWithMark = true;
		Groups.OnAddNeutral += method_0;
	}

	public void Dispose()
	{
		if (Groups != null)
		{
			Groups.OnAddNeutral -= method_0;
		}
		Groups = null;
	}

	public void method_0(IPlayer player)
	{
		if (!player.IsAI && Groups.InitialBotMindSettings.CHECK_MARK_OF_UNKNOWS && HasMarkOfUnknown(player))
		{
			HaveSomeBodyWithMark = true;
		}
	}

	public void ManualUpdate()
	{
		if (HaveSomeBodyWithMark && NextCheck < Time.time)
		{
			NextCheck = Time.time + 3f;
			method_1();
		}
	}

	public void method_1()
	{
		ListToEnemies.Clear();
		bool haveSomeBodyWithMark = false;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in Groups.Neutrals)
		{
			if (!HasMarkOfUnknown(neutral.Key))
			{
				continue;
			}
			haveSomeBodyWithMark = true;
			for (int i = 0; i < Groups.MembersCount; i++)
			{
				BotOwner botOwner = Groups.Member(i);
				if (botOwner.Settings.FileSettings.Mind.CHECK_MARK_OF_UNKNOWS && (botOwner.Position - neutral.Key.Position).sqrMagnitude < SDIST_TO_ENEMY)
				{
					ListToEnemies.Add(neutral.Key);
				}
			}
		}
		HaveSomeBodyWithMark = haveSomeBodyWithMark;
		if (ListToEnemies.Count <= 0)
		{
			return;
		}
		foreach (IPlayer listToEnemy in ListToEnemies)
		{
			Groups.AddEnemy(listToEnemy, EBotEnemyCause.MarkOfUnknowsDist);
		}
	}

	public bool HasMarkOfUnknown(IPlayer player)
	{
		if (player.HasMarkOfUnknown(out var _))
		{
			return player.Loyalty.ReactOnMarkOnUnknowns;
		}
		return false;
	}
}
