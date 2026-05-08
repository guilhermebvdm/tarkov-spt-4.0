using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass316 : BaseBrain
{
	[Serializable]
	[CompilerGenerated]
	public class Class231
	{
		public static readonly Class231 class231_0 = new Class231();

		public static Func<bool> func_0;

		public bool method_0()
		{
			return true;
		}
	}

	[NonSerialized]
	public const int Int_0 = 1;

	[NonSerialized]
	public const int Int_1 = 2;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const int Int_3 = 4;

	[NonSerialized]
	public const int Int_4 = 5;

	[NonSerialized]
	public const int Int_5 = 6;

	[NonSerialized]
	public const int Int_6 = 7;

	[NonSerialized]
	public const int Int_7 = 12;

	[NonSerialized]
	public const int Int_8 = 14;

	[NonSerialized]
	public const int Int_9 = 17;

	[NonSerialized]
	public const int Int_10 = 18;

	[NonSerialized]
	public const int Int_11 = 16;

	[NonSerialized]
	public const int Int_12 = 15;

	[NonSerialized]
	public float Float_0 = float.MinValue;

	[NonSerialized]
	public GClass135 Gclass135_0;

	public GClass316(BotOwner owner)
		: base(owner)
	{
		GClass48 gClass = new GClass48(owner, 130);
		method_0(5, gClass, activeOnStart: true);
		GClass118 layer = new GClass118(owner, 120);
		method_0(12, layer, activeOnStart: true);
		GClass127 layer2 = new GClass127(owner, 110);
		method_0(14, layer2, activeOnStart: true);
		GClass129 layer3 = new GClass129(owner, 100);
		method_0(7, layer3, activeOnStart: true);
		GClass128 layer4 = new GClass128(owner, 95);
		method_0(16, layer4, activeOnStart: true);
		GClass126 layer5 = new GClass126(owner, 90);
		method_0(1, layer5, activeOnStart: true);
		GClass121 layer6 = new GClass121(owner, 85);
		method_0(15, layer6, activeOnStart: true);
		GClass131 layer7 = new GClass131(owner, 80);
		method_0(4, layer7, activeOnStart: true);
		GClass130 layer8 = new GClass130(owner, 70);
		method_0(3, layer8, activeOnStart: true);
		GClass123 layer9 = new GClass123(owner, 60, gClass);
		method_0(6, layer9, activeOnStart: true);
		GClass124 layer10 = new GClass124(owner, 50, gClass);
		method_0(17, layer10, activeOnStart: true);
		GClass100 layer11 = new GClass100(owner, 40, () => true);
		method_0(18, layer11, activeOnStart: true);
		Gclass135_0 = new GClass135(owner, 11, tryFinGreenFirst: true, CoverLevel.Sit);
		method_0(2, Gclass135_0, activeOnStart: true);
		Owner.Memory.OnGoalEnemyChanged += method_6;
		if (Owner.Memory.HaveEnemy)
		{
			Gclass135_0.SetCorePosition(Owner.Memory.GoalEnemy.CurrPosition);
		}
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
	}

	public void method_6(BotOwner obj)
	{
		if (Time.time - Float_0 > 180f && Owner.Memory.HaveEnemy)
		{
			Owner.MinesData.CacheAllInRadius(Owner.Memory.GoalEnemy.CurrPosition, 50f);
			Float_0 = Time.time;
			Gclass135_0.SetCorePosition(Owner.Memory.GoalEnemy.CurrPosition);
		}
	}

	public void method_7()
	{
	}

	public override void Dispose()
	{
		Owner.Memory.OnGoalEnemyChanged -= method_6;
		base.Dispose();
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 45, -1);
	}

	public override string ShortName()
	{
		return "BossPartisan";
	}
}
