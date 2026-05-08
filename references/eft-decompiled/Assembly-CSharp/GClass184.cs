using EFT;
using UnityEngine;

public class GClass184 : GClass178
{
	public GClass184(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		return BotOwner_0.PatrollingData.CurPatrolPoint.TargetPoint.ActionData.LookShootTo;
	}
}
