using EFT;

public class GClass523 : BotSubTactic
{
	public GClass523(BotOwner owner)
		: base(owner)
	{
	}

	public override float SetTactic(BotsGroup.BotCurrentTactic tactic, bool shallAutoReturnToAttack = false, float delta = -1f)
	{
		tactic = BotsGroup.BotCurrentTactic.Attack;
		return base.SetTactic(BotsGroup.BotCurrentTactic.Attack, shallAutoReturnToAttack, delta);
	}

	public override CoverSearchType SearchTypeAttackMoving(CoverShootType shootType)
	{
		if (shootType == CoverShootType.hide)
		{
			return CoverSearchType.distToBot;
		}
		return CoverSearchType.distToToCenter;
	}

	public override CoverSearchType SearchRunToCover(CoverShootType shootType)
	{
		if (BotOwner_0.Memory.GoalEnemy.ShallKnowEnemyLate())
		{
			return CoverSearchType.distToToCenter;
		}
		return CoverSearchType.distToBot;
	}
}
