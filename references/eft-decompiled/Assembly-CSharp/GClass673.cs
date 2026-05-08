using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass673
{
	[Serializable]
	[CompilerGenerated]
	public class Class340
	{
		public static readonly Class340 class340_0 = new Class340();

		public static Func<BotOwner, bool> func_0;

		public bool method_0(BotOwner x)
		{
			return BotSettingsRepoClass.IsInfected(x.Profile.Info.Settings.Role);
		}
	}

	[NonSerialized]
	public BotsClass BotsClass;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public const int Int_0 = 8;

	[NonSerialized]
	public Dictionary<int, List<BotOwner>> Dictionary_0 = new Dictionary<int, List<BotOwner>>();

	public GClass673(BotsClass list)
	{
		BotsClass = list;
		Gclass25_0 = new GClass25(10f, method_0, withStep: true);
	}

	public void ManualUpdate()
	{
		Gclass25_0.Update();
	}

	public void method_0()
	{
		Dictionary_0.Clear();
		foreach (BotOwner item in BotsClass.BotOwners.Where((BotOwner x) => BotSettingsRepoClass.IsInfected(x.Profile.Info.Settings.Role)))
		{
			if (item.Memory.HaveEnemy)
			{
				int id = item.Memory.GoalEnemy.Person.Id;
				if (!Dictionary_0.TryGetValue(id, out var value))
				{
					value = new List<BotOwner>();
					Dictionary_0.Add(id, value);
				}
				value.Add(item);
			}
		}
		foreach (KeyValuePair<int, List<BotOwner>> item2 in Dictionary_0)
		{
			List<BotOwner> value2 = item2.Value;
			value2.Sort(method_1);
			int num = Mathf.Min(8, value2.Count);
			for (int num2 = 0; num2 < value2.Count; num2++)
			{
				bool canAttack = num2 < num;
				if (value2[num2].Brain.BaseBrain is GClass324 { CurrentZombieMode: not GClass98.EZombieMode.Shooting } gClass)
				{
					gClass.SetCanAttack(canAttack);
				}
			}
		}
	}

	public int method_1(BotOwner x, BotOwner y)
	{
		EnemyInfo goalEnemy = x.Memory.GoalEnemy;
		EnemyInfo goalEnemy2 = y.Memory.GoalEnemy;
		if (goalEnemy == null && goalEnemy2 == null)
		{
			return 1;
		}
		if (goalEnemy2 == null)
		{
			return -1;
		}
		if (goalEnemy == null)
		{
			return -1;
		}
		if (goalEnemy.Distance < goalEnemy2.Distance)
		{
			return -1;
		}
		if (goalEnemy.Distance > goalEnemy2.Distance)
		{
			return 1;
		}
		return 0;
	}
}
