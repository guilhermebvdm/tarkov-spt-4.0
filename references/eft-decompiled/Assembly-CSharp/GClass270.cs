using EFT;
using UnityEngine;

public class GClass270 : GClass177<GClass26>
{
	public GClass270(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
		if (BotOwner_0.PatrollingData.CurPatrolPoint.ActionData != null)
		{
			Vector3 lookShootTo = BotOwner_0.PatrollingData.CurPatrolPoint.ActionData.LookShootTo;
			BotOwner_0.Steering.LookToPoint(lookShootTo);
		}
		BotOwner_0.SetPose(0f);
	}
}
