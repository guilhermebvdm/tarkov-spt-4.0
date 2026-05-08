using System;
using EFT;

public class GClass280 : GClass177<GClass26>
{
	[NonSerialized]
	public GClass180 Gclass180_0;

	public GClass280(BotOwner bot)
		: base(bot)
	{
		Gclass180_0 = new GClass180(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			return;
		}
		if (!BotOwner_0.WeaponManager.Stationary.Taken)
		{
			BotOwner_0.WeaponManager.Stationary.Take();
		}
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.WeaponManager.Stationary.CheckAmmonProcess())
		{
			BotOwner_0.StopMove();
			if (BotOwner_0.WeaponManager.Stationary.IsEnemyAtSector(BotOwner_0.WeaponManager.Stationary.CurLink) && method_0())
			{
				Gclass180_0.UpdateNodeByBrain(data as GClass27);
			}
		}
	}

	public bool method_0()
	{
		return BotOwner_0.Memory.GoalEnemy.CanShoot;
	}
}
