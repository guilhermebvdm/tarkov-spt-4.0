using EFT;
using UnityEngine;

public class GClass187 : GClass178
{
	public override bool _checkIsReady => false;

	public GClass187(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		Vector3 partToShoot;
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			if (goalEnemy == null)
			{
				return BotOwner_0.SuppressShoot.GetPoint();
			}
			partToShoot = goalEnemy.GetPartToShoot();
		}
		else
		{
			if (goalEnemy == null || !goalEnemy.CanShoot)
			{
				return BotOwner_0.SuppressShoot.GetPoint();
			}
			partToShoot = goalEnemy.GetPartToShoot();
		}
		return partToShoot;
	}
}
