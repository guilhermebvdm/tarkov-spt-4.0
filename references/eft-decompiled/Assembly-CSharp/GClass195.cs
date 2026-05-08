using EFT;

public class GClass195 : GClass177<GClass26>
{
	public GClass195(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.WeaponManager.Grenades.DoThrow();
	}
}
