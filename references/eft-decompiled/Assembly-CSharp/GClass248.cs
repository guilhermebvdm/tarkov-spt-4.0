using EFT;

public class GClass248 : GClass200<GClass26>
{
	public GClass248(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SetPose(1f);
		if (BotOwner_0.BotFollower != null && BotOwner_0.BotFollower.PatrolDataFollower.IsInited)
		{
			BotOwner_0.BotFollower.PatrolDataFollower.ManualUpdate();
			return;
		}
		BotOwner_0.Sprint(val: false);
		if (BotOwner_0.PatrollingData.CurPatrolPoint == null)
		{
			BotOwner_0.PatrollingData.FindNextPoint(withSetting: true, withoutNext: false);
			return;
		}
		BotOwner_0.SetPose(1f);
		BotOwner_0.PatrollingData.ManualUpdate(canLookAround: true);
		BotOwner_0.PatrollingData.MoveUpdate();
	}
}
