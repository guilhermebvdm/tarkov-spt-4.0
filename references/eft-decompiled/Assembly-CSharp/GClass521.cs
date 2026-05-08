using EFT;

public class GClass521 : BotSubTactic
{
	public GClass521(BotOwner owner)
		: base(owner)
	{
		LastTactic = BotsGroup.BotCurrentTactic.Protect;
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
		tactic = BotsGroup.BotCurrentTactic.Protect;
		return base.SetTactic(BotsGroup.BotCurrentTactic.Protect, shallAutoReturnToAttack, delta);
	}
}
