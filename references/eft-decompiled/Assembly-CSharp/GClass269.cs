using EFT;
using UnityEngine;

public class GClass269 : GClass177<GClass26>
{
	public GClass269(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
		Vector3 lookShootTo = BotOwner_0.PatrollingData.CurPatrolPoint.ActionData.LookShootTo;
		BotOwner_0.Steering.LookToPoint(lookShootTo);
	}
}
