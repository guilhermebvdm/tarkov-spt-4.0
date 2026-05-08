using EFT;

public class GClass271 : GClass177<GClass26>
{
	public GClass271(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.SecondWeaponData.ManualUpdate();
	}
}
