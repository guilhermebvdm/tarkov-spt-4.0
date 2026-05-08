using System;
using Comfort.Common;
using EFT;

public class GClass310 : BaseBrain
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
	public const int Int_9 = 12;

	public GClass310(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass84 layer3 = new GClass84(owner, 70);
		method_0(9, layer3, activeOnStart: true);
		GClass142 layer4 = new GClass142(owner, 60);
		method_0(1, layer4, activeOnStart: true);
		GClass39 layer5 = new GClass39(owner, 50);
		method_0(6, layer5, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 30);
		method_0(4, layer6, activeOnStart: true);
		Class99 layer7 = new Class99(owner, 9, withSearch: false);
		method_0(3, layer7, activeOnStart: true);
		GClass132 layer8 = new GClass132(owner, 3);
		method_0(8, layer8, activeOnStart: true);
		GClass86 layer9 = new GClass86(owner, 2);
		method_0(7, layer9, activeOnStart: true);
		GClass133 layer10 = new GClass133(owner, 0);
		method_0(2, layer10, activeOnStart: true);
		if (Owner.Boss.IamBoss)
		{
			Singleton<BotEventHandler>.Instance.OnKill += method_6;
		}
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(77, 75, 55, 76);
	}

	public void method_6(IPlayer killer, IPlayer target)
	{
		if (!Owner.Boss.IamBoss || target.Id != Owner.Id)
		{
			return;
		}
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!allAlivePlayers.AIData.IsAI)
			{
				Owner.BotsGroup.AddEnemy(allAlivePlayers, EBotEnemyCause.bossKillArena);
			}
		}
		Owner.BotsGroup.SetAggressiveToAllNewPlayers(state: true);
		Singleton<BotEventHandler>.Instance.OnKill -= method_6;
	}

	public override string ShortName()
	{
		return "ArenaFighter";
	}
}
