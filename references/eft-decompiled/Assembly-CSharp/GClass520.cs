using EFT;

public class GClass520 : BotSubTactic
{
	public GClass520(BotOwner owner)
		: base(owner)
	{
		LastTactic = BotsGroup.BotCurrentTactic.Ambush;
	}

	public override CoverSearchType SearchTypeGoToCover(CoverShootType shootType)
	{
		return CoverSearchType.distToBot;
	}

	public override CoverSearchType SearchRunToCover(CoverShootType shootType)
	{
		return CoverSearchType.distToBot;
	}

	public override float SetTactic(BotsGroup.BotCurrentTactic tactic, bool shallAutoReturnToAttack = false, float delta = -1f)
	{
		tactic = BotsGroup.BotCurrentTactic.Ambush;
		return base.SetTactic(BotsGroup.BotCurrentTactic.Ambush, shallAutoReturnToAttack, delta);
	}
}
