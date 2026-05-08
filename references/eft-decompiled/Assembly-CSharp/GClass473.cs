using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;

public class GClass473 : BotEnemiesController
{
	[CompilerGenerated]
	public class Class239
	{
		public GClass473 gclass473_0;

		public IPlayer enemy;

		public GClass541 enemyInfo;

		public void method_0()
		{
			if (gclass473_0.BotOwner_0.BotFollower.HaveBoss && gclass473_0.BotOwner_0.BotFollower.BossToFollow.IsAI && gclass473_0.BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass && zyriachyBossLogicClass.HaveHitFrom(enemy.Id))
			{
				enemyInfo.SetGetHitFrom();
			}
		}
	}

	public GClass473([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override EnemyInfo AddNew(BotsGroup botsGroup, IPlayer enemy, BotSettingsClass groupInfo)
	{
		GClass541 enemyInfo = new GClass541(botsGroup, enemy, BotOwner_0, groupInfo);
		BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 3f, delegate
		{
			if (BotOwner_0.BotFollower.HaveBoss && BotOwner_0.BotFollower.BossToFollow.IsAI && BotOwner_0.BotFollower.BossToFollow.Player().AIData.BotOwner.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass && zyriachyBossLogicClass.HaveHitFrom(enemy.Id))
			{
				enemyInfo.SetGetHitFrom();
			}
		});
		return enemyInfo;
	}
}
