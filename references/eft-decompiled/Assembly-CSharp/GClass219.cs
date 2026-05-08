using EFT;

public class GClass219 : GClass200<GClass30>
{
	public GClass219(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass30 data)
	{
		method_0();
		if (data != null && !data.Used)
		{
			data.Used = true;
			BotOwner_0.GoToSomePointData.SetPoint(data.Point);
		}
		BotOwner_0.GoToSomePointData.UpdateToGo(BotOwner_0.Settings.FileSettings.Move.CAN_SPRINT_GO_TO_SOME_POINT);
	}
}
