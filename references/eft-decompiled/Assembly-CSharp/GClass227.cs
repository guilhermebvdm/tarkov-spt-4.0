using EFT;

public class GClass227 : GClass222
{
	public GClass227(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		bool num = method_0() == DoorInteractionStatus.CanRun;
		BotOwner_0.SetTargetMoveSpeed(1f);
		NotMovingCheck();
		method_6();
		BotOwner_0.SetPose(1f);
		if (num && BotOwner_0.Mover.HasPathAndNoComplete)
		{
			BotOwner_0.Steering.LookToMovingDirection();
			BotOwner_0.Sprint(val: true);
		}
		else
		{
			BotOwner_0.LookData.SetLookPointByHearing();
			BotOwner_0.Sprint(val: false);
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			method_7();
		}
	}

	public void method_7()
	{
		BotOwner_0.StopMove();
	}
}
