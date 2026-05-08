using System;
using Comfort.Common;
using EFT;

public class GClass348 : BaseBrain
{
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
	public const int Int_7 = 8;

	[NonSerialized]
	public const int Int_8 = 9;

	[NonSerialized]
	public const int Int_9 = 10;

	[NonSerialized]
	public const int Int_10 = 12;

	[NonSerialized]
	public const int Int_11 = 13;

	[NonSerialized]
	public const int Int_12 = 14;

	public GClass348(BotOwner owner, bool withPursuit)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass75 layer2 = new GClass75(owner, 79);
		method_0(10, layer2, activeOnStart: true);
		GClass118 layer3 = new GClass118(owner, 78);
		method_0(12, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 70);
		method_0(9, layer4, activeOnStart: true);
		GClass141 layer5 = new GClass141(owner, 60);
		method_0(1, layer5, activeOnStart: true);
		GClass39 layer6 = new GClass39(owner, 50);
		method_0(6, layer6, activeOnStart: true);
		GClass139 layer7 = new GClass139(owner, 30);
		method_0(4, layer7, activeOnStart: true);
		if (withPursuit)
		{
			Class105 layer8 = new Class105(owner, 25);
			method_0(13, layer8, activeOnStart: true);
		}
		Class99 layer9 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer9, activeOnStart: true);
		GClass132 layer10 = new GClass132(owner, 5);
		method_0(8, layer10, activeOnStart: true);
		GClass117 layer11 = new GClass117(owner, 3);
		method_0(14, layer11, activeOnStart: true);
		GClass86 layer12 = new GClass86(owner, 2);
		method_0(7, layer12, activeOnStart: true);
		GClass133 layer13 = new GClass133(owner, 0);
		method_0(2, layer13, activeOnStart: true);
		if (Owner.Boss.IamBoss)
		{
			Singleton<BotEventHandler>.Instance.OnKill += method_7;
		}
		Singleton<BotEventHandler>.Instance.OnBeingHit += method_6;
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 75, 55, 76);
	}

	public void method_6(DamageInfoStruct damageInfo, Player victim)
	{
		if (damageInfo.Player?.iPlayer.Id == Owner.GetPlayer.Id && victim.IsAI)
		{
			Owner.GetPlayer.Loyalty.TryAddDamagedScavToHashSet(victim);
		}
	}

	public void method_7(IPlayer killer, IPlayer target)
	{
		if (!Owner.Boss.IamBoss || !(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(target.ProfileId) == Owner.GetPlayer))
		{
			return;
		}
		foreach (Player value in Singleton<GameWorld>.Instance.allAlivePlayersByID.Values)
		{
			if (!value.AIData.IsAI)
			{
				Owner.BotsGroup.AddEnemy(value, EBotEnemyCause.pmcBossKill);
			}
		}
		Owner.BotsGroup.SetAggressiveToAllNewPlayers(state: true);
		Singleton<BotEventHandler>.Instance.OnKill -= method_7;
	}

	public override string ShortName()
	{
		return "PmcBear";
	}
}
