using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotsMinotaurLabirint
{
	[NonSerialized]
	public BotOwner Bot;

	public const string EVENT_FOR_KILLA = "EVENT_FOR_KILLA";

	[NonSerialized]
	public bool IsInited;

	[NonSerialized]
	public bool IsBossDeath;

	[NonSerialized]
	public Vector3? ControlPosition_1;

	[NonSerialized]
	public float NextSendInfo;

	[NonSerialized]
	public float NextSendToBossInfo;

	[NonSerialized]
	public BotsController Bots;

	[NonSerialized]
	public Dictionary<int, float> LastEnemySeenByHelpers = new Dictionary<int, float>();

	public const float SEEN_TOO_LONG_TIME_AGO = 10f;

	public Vector3? ControlPosition => ControlPosition_1;

	public void Init(BotOwner bot, BotsController bots)
	{
		if (!IsInited)
		{
			Bots = bots;
			Bots.Bots.OnBotAdd += method_0;
			Bots.Bots.OnBotRemove += method_2;
			IsInited = true;
			Bot = bot;
			Bot.BotsGroup.OnReportEnemy += method_4;
		}
	}

	public void method_0(BotOwner obj)
	{
		if (obj.Profile.Info.Settings.Role == WildSpawnType.tagillaHelperAgro)
		{
			obj.BotsGroup.OnReportEnemy += method_3;
			obj.Memory.OnGoalEnemyChanged += method_1;
		}
		if (obj.Profile.Info.Settings.Role == WildSpawnType.bossKillaAgro)
		{
			obj.BotsGroup.OnReportEnemy += method_4;
		}
	}

	public void method_1(BotOwner obj)
	{
		if (NextSendToBossInfo > Time.time)
		{
			return;
		}
		NextSendToBossInfo = Time.time + 3f;
		EnemyInfo goalEnemy = obj.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return;
		}
		foreach (BotOwner botOwner in Bots.Bots.BotOwners)
		{
			if (botOwner.Profile.Info.Settings.Role == WildSpawnType.bossTagillaAgro || botOwner.Profile.Info.Settings.Role == WildSpawnType.bossKillaAgro)
			{
				botOwner.BotsGroup.ReportAboutEnemy(goalEnemy.Person, EEnemyPartVisibleType.Sence, botOwner);
			}
		}
	}

	public void method_2(BotOwner obj)
	{
		if (obj.Profile.Info.Settings.Role == WildSpawnType.tagillaHelperAgro)
		{
			obj.BotsGroup.OnReportEnemy -= method_3;
			obj.Memory.OnGoalEnemyChanged -= method_1;
		}
		if (obj.Profile.Info.Settings.Role == WildSpawnType.bossKillaAgro)
		{
			obj.BotsGroup.OnReportEnemy -= method_4;
		}
	}

	public void method_3(IPlayer enemy, Vector3 enemypos, Vector3 weaponrootlast, EEnemyPartVisibleType isvisibleonlybysense, BotOwner reporter)
	{
		if (NextSendToBossInfo > Time.time || (reporter.EnemiesController.EnemyInfos.TryGetValue(enemy, out var value) && Time.time - value.PersonalLastSeenTime > 10f))
		{
			return;
		}
		NextSendToBossInfo = Time.time + 3f;
		if (!LastEnemySeenByHelpers.TryGetValue(enemy.Id, out var _))
		{
			LastEnemySeenByHelpers.Add(enemy.Id, Time.time);
		}
		else
		{
			LastEnemySeenByHelpers[enemy.Id] = Time.time;
		}
		foreach (BotOwner botOwner in Bots.Bots.BotOwners)
		{
			if (botOwner.Profile.Info.Settings.Role == WildSpawnType.bossTagillaAgro || botOwner.Profile.Info.Settings.Role == WildSpawnType.bossKillaAgro)
			{
				botOwner.BotsGroup.ReportAboutEnemy(enemy, EEnemyPartVisibleType.Sence, botOwner);
			}
		}
	}

	public void method_4(IPlayer enemy, Vector3 enemypos, Vector3 weaponrootlast, EEnemyPartVisibleType isvisibleonlybysense, BotOwner reporter)
	{
		if (NextSendInfo > Time.time || (reporter.EnemiesController.EnemyInfos.TryGetValue(enemy, out var value) && Time.time - value.PersonalLastSeenTime > 10f))
		{
			return;
		}
		NextSendInfo = Time.time + 3f;
		foreach (BotOwner botOwner in Bots.Bots.BotOwners)
		{
			if (botOwner.Profile.Info.Settings.Role == WildSpawnType.tagillaHelperAgro)
			{
				botOwner.BotsGroup.ReportAboutEnemy(enemy, EEnemyPartVisibleType.Sence, botOwner);
			}
		}
	}

	public void Dispose()
	{
		if (Bots != null && Bots.Bots != null)
		{
			Bots.Bots.OnBotAdd -= method_0;
			Bots.Bots.OnBotRemove -= method_2;
		}
		if (Bot != null)
		{
			Bot.BotsGroup.OnReportEnemy -= method_4;
		}
	}

	public bool IsHelpersSeen(int enemyId, float period = 2f)
	{
		if (LastEnemySeenByHelpers.TryGetValue(enemyId, out var value))
		{
			return Time.time - value < period;
		}
		return false;
	}

	public void ReportInfo(IPlayer enemyPerson, BotOwner reporter)
	{
		method_3(enemyPerson, enemyPerson.Position, enemyPerson.Position, EEnemyPartVisibleType.Visible, reporter);
	}

	public void TagillaDeath(BotOwner obj)
	{
		if (!IsBossDeath)
		{
			IsBossDeath = true;
			ControlPosition_1 = obj.Position;
			Singleton<BotEventHandler>.Instance.AnyEvent("EVENT_FOR_KILLA");
		}
	}
}
