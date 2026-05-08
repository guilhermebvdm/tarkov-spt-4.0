using System;
using EFT;
using UnityEngine;

public class GClass431 : ABossLogic
{
	[NonSerialized]
	public BotOwner BotOwner_1;

	[NonSerialized]
	public GInterface8 Ginterface8_0;

	public GClass431(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossCoverScouts, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossCoverScouts, pointChooser);
	}

	public override void Activate()
	{
		method_0();
	}

	public override void BossLogicUpdate()
	{
	}

	public void method_0()
	{
		foreach (BotOwner botOwner in BotOwner_0.BotsGroup.BotGame.BotsController.Bots.BotOwners)
		{
			if (botOwner.Profile.Info.Settings.Role == WildSpawnType.bossBoar)
			{
				method_1(botOwner);
				break;
			}
		}
	}

	public void method_1(BotOwner bot)
	{
		BotOwner_1 = bot;
		Ginterface8_0 = bot.Boss.BossLogic as GInterface8;
		BotOwner_1.BotsGroup.OnReportEnemy += method_2;
		BotOwner_0.BotsGroup.OnReportEnemy += method_4;
		BotOwner_1.OnBotStateChange += method_3;
	}

	public void method_2(IPlayer enemy, Vector3 enemypos, Vector3 weaponrootlast, EEnemyPartVisibleType visibleType, BotOwner reporter)
	{
		BotOwner_0.BotsGroup.SetEnemyPos(enemy, enemypos, weaponrootlast, visibleType);
	}

	public void method_3(EBotState obj)
	{
		if (obj == EBotState.Disposed)
		{
			Dispose();
		}
	}

	public void method_4(IPlayer enemy, Vector3 enemypos, Vector3 weaponrootlast, EEnemyPartVisibleType visibleType, BotOwner reporter)
	{
		Ginterface8_0.ReportEnemy(enemy, enemypos, visibleType);
		Ginterface8_0.SetWantSuppress();
	}

	public override void Dispose()
	{
		BotOwner_0.BotsGroup.OnReportEnemy -= method_4;
		if (BotOwner_1 != null)
		{
			BotOwner_1.BotsGroup.OnReportEnemy -= method_2;
			BotOwner_1.OnBotStateChange -= method_3;
			BotOwner_1 = null;
		}
	}
}
