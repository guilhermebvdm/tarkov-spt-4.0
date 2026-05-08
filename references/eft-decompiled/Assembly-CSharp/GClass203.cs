using System;
using EFT;

public class GClass203 : GClass200<GClass26>
{
	[NonSerialized]
	public GClass178 Gclass178_0;

	[NonSerialized]
	public GClass274 Gclass274_0;

	public GClass203(BotOwner bot)
		: base(bot)
	{
		Gclass178_0 = new GClass183(bot);
		Gclass274_0 = new GClass274(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.DogFight.Fight();
		BotOwner_0.SetPose(0.5f);
		BotOwner_0.Sprint(val: false);
		if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			if (BotOwner_0.WeaponManager.IsMelee)
			{
				BotOwner_0.WeaponManager.Selector.ChangeToMain();
			}
			else if (!BotOwner_0.Settings.FileSettings.Grenade.CAN_THROW_FROM_ANY_PLACE || !Gclass274_0.UpdateTryThrow())
			{
				BotOwner_0.Steering.LookToPoint(goalEnemy.CurrPosition);
				if (BotOwner_0.DoorOpener.DogFightHaveNearestDoor())
				{
					BotOwner_0.Mover.Stop();
				}
				Gclass178_0.UpdateNodeByBrain(data as GClass27);
			}
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing();
			method_0();
		}
	}
}
