using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass315 : BaseBrain
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
	public const int Int_7 = 12;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	public GClass315(BotOwner owner)
		: base(owner)
	{
		Gclass25_0 = new GClass25(10f, method_6);
		GClass48 layer = new GClass48(owner, 80);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 78);
		method_0(12, layer2, activeOnStart: true);
		GClass84 layer3 = new GClass84(owner, 70);
		method_0(7, layer3, activeOnStart: true);
		GClass156 layer4 = new GClass156(owner, 50);
		method_0(2, layer4, activeOnStart: true);
		GClass139 layer5 = new GClass139(owner, 40);
		method_0(6, layer5, activeOnStart: true);
		GClass47 layer6 = new GClass47(owner, 9);
		method_0(4, layer6, activeOnStart: true);
		GClass135 gClass = new GClass135(owner, 0, tryFinGreenFirst: false);
		Vector3? controlPosition = Owner.BotsController.EventsController.BotsMinotaurLabirint.ControlPosition;
		if (controlPosition.HasValue)
		{
			Vector3_0 = controlPosition.Value;
		}
		else
		{
			Vector3_0 = Owner.Position;
		}
		gClass.SetCorePosition(Vector3_0);
		method_0(3, gClass, activeOnStart: true);
	}

	public override void ManualUpdate()
	{
		Gclass25_0.Update();
		base.ManualUpdate();
	}

	public void method_6()
	{
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in Owner.EnemiesController.EnemyInfos)
		{
			IPlayer key = enemyInfo.Key;
			if (!((Vector3_0 - key.Position).sqrMagnitude > 4f))
			{
				Player.FirearmController firearmController = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(key.ProfileId).HandsController as Player.FirearmController;
				if (firearmController != null && firearmController.IsInventoryOpen())
				{
					Owner.BotsGroup.ReportAboutEnemy(key, EEnemyPartVisibleType.Sence, Owner);
				}
			}
		}
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(77, -1, 45, 76);
	}

	public override string ShortName()
	{
		return "KillaAgro";
	}
}
