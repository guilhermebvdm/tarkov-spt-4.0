using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;

public class GClass482 : BotEnemyChooser
{
	[Serializable]
	[CompilerGenerated]
	public class Class242
	{
		public static readonly Class242 class242_0 = new Class242();

		public static Func<EnemyInfo, bool> func_0;

		public bool method_0(EnemyInfo x)
		{
			return x.Person.HealthController.IsAlive;
		}
	}

	public GClass482([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public override EnemyInfo FindDangerEnemy()
	{
		List<EnemyInfo> list = BotOwner_0.EnemiesController.EnemyInfos.Values.Where((EnemyInfo x) => x.Person.HealthController.IsAlive).ToList();
		if (list.Count > 0)
		{
			return list[0];
		}
		return base.FindDangerEnemy();
	}
}
