using EFT;

public class GClass233 : GClass228
{
	public GClass233(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass31 data)
	{
		bool notOpeningDoor = method_0() != DoorInteractionStatus.OpeningDoor;
		if (BotOwner_0.BotRequestController.CurRequest is GClass597 gClass)
		{
			ShootPointClass shootPos = gClass.GetShootPos();
			BotOwner_0.BotRun.Run(CoverShootType.shoot, shootPos, BotOwner_0.Settings.FileSettings.Cover.CHANGE_RUN_TO_COVER_SEC, notOpeningDoor, checkRecalc: false, zigZag: false);
		}
	}
}
