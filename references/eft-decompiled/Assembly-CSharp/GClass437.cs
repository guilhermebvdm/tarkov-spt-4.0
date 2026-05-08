using EFT;

public class GClass437 : GClass436
{
	public GClass437(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = new PatrolPointChooserBasic(BotOwner_0);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossRoundProtect, pointChooser);
	}

	public override void BossLogicUpdate()
	{
	}

	public override void Dispose()
	{
	}
}
