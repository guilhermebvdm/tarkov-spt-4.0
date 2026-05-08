using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass326 : BaseBrain
{
	[NonSerialized]
	public HashSet<BotsGroup> HashSet_0 = new HashSet<BotsGroup>();

	public GClass326(BotOwner owner)
		: base(owner)
	{
	}

	public void method_6()
	{
		BotsController botsController = Owner.BotsGroup.BotGame.BotsController;
		botsController.Bots.OnBotAdd += method_7;
		foreach (BotOwner botOwner in botsController.Bots.BotOwners)
		{
			method_8(botOwner.BotsGroup);
		}
	}

	public void method_7(BotOwner bot)
	{
		method_8(bot.BotsGroup);
	}

	public void method_8(BotsGroup botsGroup)
	{
		if (botsGroup != Owner.BotsGroup && botsGroup.HaveBoss(out var botBoss))
		{
			WildSpawnType role = botBoss.Profile.Info.Settings.Role;
			if ((role == WildSpawnType.exUsec || role == WildSpawnType.bossKnight) && HashSet_0.Add(botsGroup))
			{
				botsGroup.OnReportEnemy += method_9;
			}
		}
	}

	public void method_9(IPlayer enemy, Vector3 enemyPos, Vector3 weaponRootLast, EEnemyPartVisibleType isVisibleOnlyBySense, BotOwner reporter)
	{
		Owner.BotsGroup.SetEnemyPos(enemy, enemyPos, weaponRootLast, EEnemyPartVisibleType.Sence);
	}

	public override void Dispose()
	{
		method_10();
		base.Dispose();
	}

	public void method_10()
	{
		foreach (BotsGroup item in HashSet_0)
		{
			item.OnReportEnemy -= method_9;
		}
		Owner.BotsGroup.BotGame.BotsController.Bots.OnBotAdd -= method_7;
		HashSet_0.Clear();
	}
}
