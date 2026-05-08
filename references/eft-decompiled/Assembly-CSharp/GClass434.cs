using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass434 : GClass433
{
	[NonSerialized]
	public GClass25 Gclass25_0;

	public GClass434([NotNull] BotOwner owner, [NotNull] BotBoss bossLogic)
		: base(owner, bossLogic)
	{
		Gclass25_0 = new GClass25(5f, method_7);
	}

	public void method_7()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && !(Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 10f))
		{
			BotOwner_0.BotsGroup.BotGame.BotsController.EventsController.BotsMinotaurLabirint.ReportInfo(goalEnemy.Person, BotOwner_0);
		}
	}

	public override void BossLogicUpdate()
	{
		base.BossLogicUpdate();
		Gclass25_0.Update();
	}
}
