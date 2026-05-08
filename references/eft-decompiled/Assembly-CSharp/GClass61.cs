using EFT;

public class GClass61 : GClass60
{
	public GClass61(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.BotsController.EventsController.BotHalloweenWithZombies.InfectionPercentage > 0;
	}

	public override void OnActivate()
	{
		_ = BotOwner_0.BotsController.EventsController.BotHalloweenWithZombies.InfectionPercentage;
		base.OnActivate();
	}

	public override string Name()
	{
		return "HideHW";
	}
}
