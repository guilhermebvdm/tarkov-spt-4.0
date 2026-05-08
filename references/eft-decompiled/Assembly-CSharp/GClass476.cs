using System;
using EFT;
using JetBrains.Annotations;

public class GClass476 : BotEnemyChooser
{
	[NonSerialized]
	public GClass435 Gclass435_0;

	public GClass476([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override EnemyInfo FindDangerEnemy()
	{
		if (Gclass435_0 != null)
		{
			return Gclass435_0.FindDangerEnemyFor(BotOwner_0);
		}
		return base.FindDangerEnemy();
	}
}
