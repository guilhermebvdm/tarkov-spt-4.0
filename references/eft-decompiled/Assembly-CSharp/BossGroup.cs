using System;
using System.Collections.Generic;
using EFT;

public class BossGroup
{
	[NonSerialized]
	public BotOwner Boss_1;

	public BotOwner Boss => Boss_1;

	public BossGroup(BotOwner boss)
	{
		Boss_1 = boss;
		Boss_1.Boss.OnBossDead += method_0;
	}

	public void method_0(BotOwner boss, List<BotOwner> followers)
	{
		if (followers.Count > 1)
		{
			BotOwner botOwner = GClass856.RandomElement(followers);
			if (botOwner.IsRole(boss.Profile.Info.Settings.Role))
			{
				botOwner.Boss.SetBoss(followers.Count - 1);
				botOwner.Boss.OnBossDead += method_0;
			}
		}
	}

	public void Dispose()
	{
		if (Boss_1 != null && Boss_1.Boss != null)
		{
			Boss_1.Boss.OnBossDead -= method_0;
		}
	}
}
