using EFT;
using JetBrains.Annotations;

public class GClass433 : GClass432
{
	public GClass433([NotNull] BotOwner owner, [NotNull] BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public override void SetPatrolMode()
	{
		PatrolPointChooserBasic pointChooser = PatrollingData.GetPointChooser(BotOwner_0, PatrolMode.bossRoundProtect, BotOwner_0.SpawnProfileData);
		BotOwner_0.PatrollingData.SetMode(PatrolMode.bossRoundProtect, pointChooser);
	}
}
