using EFT;

public class GClass225 : GClass224
{
	public GClass225(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.Sprint(val: false);
		if (BotOwner_0.BotLay.IsLay)
		{
			BotOwner_0.BotLay.GetUp(withCheck: false);
		}
		NotMovingCheck();
		method_6();
		if (BotOwner_0.Mover.HasPathAndNoComplete)
		{
			BotOwner_0.Steering.LookToMovingDirection();
			BotOwner_0.SetTargetMoveSpeed(1f);
			BotOwner_0.SetPose(1f);
			if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
			{
				method_7();
			}
		}
		else
		{
			BotOwner_0.SetPose(0f);
			BotOwner_0.LookData.SetLookPointByHearing();
			BotOwner_0.StopMove();
		}
	}
}
