using EFT;

public class GClass290 : GClass177<GClass26>
{
	public WarnPlayerRequest WarnPlayerRequest_0 => BotOwner_0.WarnData.WarnPlayerRequest;

	public GClass290(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (WarnPlayerRequest_0.UpdateSay())
		{
			WarnPlayerRequest_0.NextState();
		}
	}
}
