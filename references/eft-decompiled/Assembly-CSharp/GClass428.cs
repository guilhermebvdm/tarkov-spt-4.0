using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass428
{
	public float ComeInsideTime;

	public float WarnedTime = -1f;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Dictionary<int, int> Dictionary_0 = new Dictionary<int, int>();

	public float StartWarnedTime
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public GClass428()
	{
		ComeInsideTime = Time.time;
	}

	public bool ShallAttack(BotOwner boss, IPlayer targetPlayer)
	{
		if (method_2(boss, targetPlayer))
		{
			return true;
		}
		if (Bool_0)
		{
			return false;
		}
		if (Int_0 > boss.Settings.FileSettings.Boss.COME_INSIDE_TIMES)
		{
			return true;
		}
		if (Time.time - ComeInsideTime > boss.Settings.FileSettings.Boss.TOTAL_TIME_KILL)
		{
			return true;
		}
		if (StartWarnedTime <= 0f)
		{
			return false;
		}
		if (Time.time - StartWarnedTime > boss.Settings.FileSettings.Boss.TOTAL_TIME_KILL_AFTER_START_WARN)
		{
			return true;
		}
		if (WarnedTime <= 0f)
		{
			return false;
		}
		if (Time.time - WarnedTime > boss.Settings.FileSettings.Boss.TOTAL_TIME_KILL_AFTER_WARN)
		{
			return true;
		}
		return false;
	}

	public void ComeAway()
	{
		Bool_0 = true;
	}

	public void SetStartWarnTime()
	{
		if (!Bool_1)
		{
			Bool_1 = true;
			StartWarnedTime = Time.time;
		}
	}

	public void Return()
	{
		Bool_1 = false;
		StartWarnedTime = -1f;
		Int_0++;
		Bool_0 = false;
		ComeInsideTime = Time.time;
	}

	public int method_0(int playerId)
	{
		if (Dictionary_0.ContainsKey(playerId))
		{
			Dictionary_0[playerId]++;
			return Dictionary_0[playerId];
		}
		Dictionary_0.Add(playerId, 1);
		return 1;
	}

	public void method_1(int playerId)
	{
		if (Dictionary_0.ContainsKey(playerId))
		{
			Dictionary_0[playerId] = 0;
		}
		else
		{
			Dictionary_0.Add(playerId, 0);
		}
	}

	public bool method_2(BotOwner boss, IPlayer targetPlayer)
	{
		if (method_3(boss, targetPlayer))
		{
			return true;
		}
		int num = 0;
		while (true)
		{
			if (num < boss.Boss.Followers.Count)
			{
				BotOwner bot = boss.Boss.Followers[num];
				if (method_3(bot, targetPlayer))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public bool method_3(BotOwner bot, IPlayer targetPlayer)
	{
		if (method_4(bot, targetPlayer))
		{
			int num = method_0(bot.Id);
			if (!targetPlayer.IsAI && num > LocalBotSettingsProviderClass.Core.LOOK_TIMES_TO_KILL_PLAYER)
			{
				return true;
			}
			if (targetPlayer.IsAI && num > LocalBotSettingsProviderClass.Core.LOOK_TIMES_TO_KILL_BOT)
			{
				return true;
			}
		}
		else
		{
			method_1(bot.Id);
		}
		return false;
	}

	public bool method_4(BotOwner bot, IPlayer player)
	{
		if (Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId).HandsController == null)
		{
			return false;
		}
		if (Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId).HandsController.IsAiming)
		{
			return bot.IsEnemyLookingAtMe(player);
		}
		return false;
	}
}
