using System;
using EFT;

public class GClass209 : GClass208
{
	[NonSerialized]
	public GClass178 Gclass178_0;

	public GClass209(BotOwner bot)
		: base(bot, 5f)
	{
		Gclass178_0 = new GClass183(bot);
	}

	public override void UpdateNodeByBrain(GClass29 data)
	{
		base.UpdateNodeByBrain(data);
		AimingAndShoot(data);
	}

	public virtual void AimingAndShoot(GClass26 data)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.CanSwitchInFight(BotOwner_0))
		{
			BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryEnable();
		}
		if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing();
		}
	}
}
