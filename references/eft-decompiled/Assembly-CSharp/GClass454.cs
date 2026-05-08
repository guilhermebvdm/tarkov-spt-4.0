using EFT;
using JetBrains.Annotations;

public class GClass454 : BotFollower
{
	public GClass454([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override void SetToFollow(IBossToFollow boss, int index, bool changeLogicMode = false)
	{
		base.Index = index;
		if (base.BossToFollow == null || changeLogicMode)
		{
			base.BossToFollow = boss;
			base.PatrolDataFollower.Activate();
			base.PatrolDataFollower.SetIndex(base.Index);
			PatrolPointChooserBasic pointChooser = ((BotOwner_0.Profile.Info.Settings.Role != WildSpawnType.followerGluharSecurity) ? ((PatrolPointChooserBasic)new GClass556(BotOwner_0, BotOwner_0.SpawnProfileData, BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWER_PATH_NAME, BotOwner_0.Settings.FileSettings.Boss.GLUHAR_FOLLOWER_PATH_NAME + "2", base.BossToFollow)) : ((PatrolPointChooserBasic)new GClass557(BotOwner_0)));
			BotOwner_0.PatrollingData.SetMode(PatrolMode.byNameAndStay, pointChooser);
			BotsGroup.BotCurrentTactic tactic = ((BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.followerGluharSecurity) ? BotsGroup.BotCurrentTactic.Protect : BotsGroup.BotCurrentTactic.Attack);
			BotOwner_0.Tactic.SetTactic(tactic);
			BossFindAction();
		}
	}
}
