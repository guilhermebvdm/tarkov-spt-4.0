using EFT;

public class GClass260 : GClass177<GClass26>
{
	public GClass260(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		float panicPower = BotOwner_0.DangerPointsData.PanicPower;
		BotOwner_0.Sprint(val: false);
		BotOwner_0.GetPlayer.MovementContext.SetPoseLevel(panicPower);
		BotOwner_0.StopMove();
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			BotOwner_0.Steering.LookToPoint(BotOwner_0.Memory.GoalEnemy.CurrPosition);
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing(BotOwner_0.Memory.CurCustomCoverPoint);
		}
	}
}
