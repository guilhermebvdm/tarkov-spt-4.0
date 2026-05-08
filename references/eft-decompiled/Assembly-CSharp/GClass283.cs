using EFT;

public class GClass283 : GClass177<GClass26>
{
	public GClass283(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!BotOwner_0.Medecine.Using)
		{
			BotOwner_0.Medecine.Stimulators.TryApply();
		}
	}
}
