using System;
using EFT;
using JetBrains.Annotations;

public class GClass68 : GClass65
{
	[NonSerialized]
	public bool Bool_4;

	public GClass68([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public void ShallAttack(bool val)
	{
		Bool_4 = val;
	}

	public override string Name()
	{
		return "FlGlScout";
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (!Bool_4 && BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot) && !BotOwner_0.Memory.IsInCover)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("enemy");
	}
}
