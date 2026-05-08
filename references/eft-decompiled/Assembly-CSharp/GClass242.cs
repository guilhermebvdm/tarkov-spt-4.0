using EFT;

public class GClass242 : GClass200<GClass26>
{
	public GClass242(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (method_0() == DoorInteractionStatus.CanRun)
		{
			BotOwner_0.WeaponManager.Melee.RunToEnemyUpdate();
		}
	}
}
