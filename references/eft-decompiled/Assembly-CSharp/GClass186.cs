using EFT;
using UnityEngine;

public class GClass186 : GClass178
{
	public GClass186(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.CanShoot)
		{
			return goalEnemy.GetPartToShoot();
		}
		return BotOwner_0.SuppressStationary.PointToArtSuppress();
	}
}
