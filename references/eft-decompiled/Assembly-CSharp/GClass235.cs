using EFT;

public class GClass235 : GClass200<GClass26>
{
	public GClass235(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.SearchData.UpdateByNode();
	}
}
