using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;

public class GClass347 : BaseBrain
{
	[NonSerialized]
	public const int Int_0 = 6;

	[NonSerialized]
	public const int Int_1 = 1;

	[NonSerialized]
	public const int Int_2 = 5;

	[NonSerialized]
	public const int Int_3 = 12;

	[NonSerialized]
	public const int Int_4 = 2;

	[NonSerialized]
	public const int Int_5 = 3;

	public GClass347(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass89 layer2 = new GClass89(owner, 70);
		method_0(6, layer2, activeOnStart: true);
		GClass39 layer3 = new GClass39(owner, 55);
		method_0(1, layer3, activeOnStart: true);
		GClass46 layer4 = new GClass46(owner, 20);
		method_0(3, layer4, activeOnStart: true);
		GClass93 layer5 = new GClass93(owner, 0);
		method_0(2, layer5, activeOnStart: true);
		Singleton<BotEventHandler>.Instance.OnKill += method_6;
	}

	public override string ShortName()
	{
		return "PeaceZryachiy";
	}

	public void method_6(IPlayer killer, IPlayer target)
	{
		if (!target.IsAI || !(target.AIData.BotOwner == Owner))
		{
			return;
		}
		killer.AIData.SetAttackByLoyalityScav();
		foreach (KeyValuePair<BotZone, GClass575> item in Owner.BotsController.Groups())
		{
			item.Deconstruct(out var _, out var value);
			foreach (BotsGroup group in value.GetGroups(notNull: true))
			{
				group?.AddEnemy(killer, EBotEnemyCause.gifterKill);
			}
		}
		Singleton<BotEventHandler>.Instance.OnKill -= method_6;
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}

	public override void Dispose()
	{
		base.Dispose();
		Singleton<BotEventHandler>.Instance.OnKill -= method_6;
	}
}
