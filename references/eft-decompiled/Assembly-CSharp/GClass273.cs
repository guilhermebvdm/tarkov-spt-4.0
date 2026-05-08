using EFT;

public class GClass273 : GClass177<GClass26>
{
	public GClass273(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.WeaponManager.Malfunctions.Update();
	}
}
