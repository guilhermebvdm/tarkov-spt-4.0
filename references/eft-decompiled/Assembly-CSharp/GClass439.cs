using EFT;

public class GClass439 : GClass437
{
	public GClass439(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossRoundProtect, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossRoundProtect, pointChooser);
	}
}
