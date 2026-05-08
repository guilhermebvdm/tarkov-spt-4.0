using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

public class GClass336 : BaseBrain
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
	public const int Int_10 = 11;

	[NonSerialized]
	public const int Int_11 = 12;

	public GClass336(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 80);
		method_0(6, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass89 layer3 = new GClass89(owner, 70);
		method_0(9, layer3, activeOnStart: true);
		GClass84 layer4 = new GClass84(owner, 65);
		method_0(11, layer4, activeOnStart: true);
		GClass39 layer5 = new GClass39(owner, 55);
		method_0(1, layer5, activeOnStart: true);
		GClass139 layer6 = new GClass139(owner, 30);
		method_0(5, layer6, activeOnStart: true);
		GClass46 layer7 = new GClass46(owner, 20);
		method_0(3, layer7, activeOnStart: true);
		Class103 layer8 = new Class103(owner, 4);
		method_0(8, layer8, activeOnStart: true);
		GClass174 layer9 = new GClass174(owner, 3);
		method_0(7, layer9, activeOnStart: true);
		GClass132 layer10 = new GClass132(owner, 2);
		method_0(10, layer10, activeOnStart: true);
		GClass133 layer11 = new GClass133(owner, 1);
		method_0(2, layer11, activeOnStart: true);
		Owner.ItemTaker.OnItemTaken += method_6;
		Singleton<BotEventHandler>.Instance.OnKill += method_7;
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, -1, -1, -1);
	}

	public void method_6(Item itm, IPlayer itemLastOwner)
	{
		if (itemLastOwner != null)
		{
			Owner.GiftData.GetGiftTarget(itemLastOwner).AllowToDropAdditionalGift();
		}
	}

	public void method_7(IPlayer killer, IPlayer target)
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
		Singleton<BotEventHandler>.Instance.OnKill -= method_7;
	}

	public override string ShortName()
	{
		return "Gifter";
	}

	public override void Dispose()
	{
		base.Dispose();
		Owner.ItemTaker.OnItemTaken -= method_6;
		Singleton<BotEventHandler>.Instance.OnKill -= method_7;
	}
}
