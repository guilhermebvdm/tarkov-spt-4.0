using EFT;

public class GClass525 : BotSubTactic
{
	public GClass525(BotOwner owner)
		: base(owner)
	{
	}

	public override CoverSearchType SearchRunToCover(CoverShootType shootType)
	{
		if (BotOwner_0.Memory.GoalEnemy.ShallKnowEnemyLate())
		{
			return CoverSearchType.distToToCenter;
		}
		return CoverSearchType.distToBot;
	}

	public override CoverSearchType SearchTypeAttackMoving(CoverShootType shootType)
	{
		if (shootType == CoverShootType.hide)
		{
			return CoverSearchType.distToBot;
		}
		return CoverSearchType.distToToCenter;
	}
}
