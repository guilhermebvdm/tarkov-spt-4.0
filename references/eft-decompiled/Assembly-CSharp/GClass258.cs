using EFT;

public class GClass258 : GClass177<GClass26>
{
	public GClass258(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		CustomNavigationPoint customNavigationPoint = BotOwner_0.BotAttackManager.PointToGo();
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
		if (customNavigationPoint != null)
		{
			BotOwner_0.BotLay.DelayPosibleLayFor(1f);
			BotOwner_0.Mover.Teleport(customNavigationPoint.Position);
			BotOwner_0.Memory.ComeToPoint();
			BotOwner_0.StopMove();
		}
	}
}
