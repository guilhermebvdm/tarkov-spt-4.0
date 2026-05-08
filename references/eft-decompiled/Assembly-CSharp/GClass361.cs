using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass361 : GClass356
{
	[NonSerialized]
	public const float Float_0 = 30f;

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
	public const int Int_5 = 7;

	[NonSerialized]
	public const int Int_6 = 12;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GClass135 Gclass135_0;

	[NonSerialized]
	public GClass147 Gclass147_0;

	[NonSerialized]
	public Class103 Class103_0;

	[NonSerialized]
	public float Float_2;

	public GClass361(BotOwner owner)
		: base(owner)
	{
		GClass48 layer = new GClass48(owner, 130);
		method_0(5, layer, activeOnStart: true);
		GClass118 layer2 = new GClass118(owner, 128);
		method_0(12, layer2, activeOnStart: true);
		Gclass147_0 = new GClass147(owner, 100);
		method_0(1, Gclass147_0, activeOnStart: true);
		Class99 layer3 = new Class99(owner, 47, withSearch: false);
		method_0(3, layer3, activeOnStart: true);
		Gclass135_0 = new GClass135(owner, 11, tryFinGreenFirst: true);
		method_0(2, Gclass135_0, activeOnStart: true);
		Owner.Boss.OnFollowerStatusChange += method_6;
	}

	public override GClass671 EventsPriority()
	{
		return new GClass671(-1, 125, 55, -1);
	}

	public override void ManualUpdate()
	{
		method_8();
		base.ManualUpdate();
	}

	public override void SetCorePosition(Vector3 pos)
	{
		Gclass135_0.SetCorePosition(pos);
	}

	public override string ShortName()
	{
		return "SctPredvst";
	}

	public void method_6(BotOwner arg1, FollowerStatusChange arg2)
	{
		if (Owner.Boss.Followers.Count < 1)
		{
			method_7();
		}
	}

	public void method_7()
	{
		if (Owner.WeaponManager.Grenades.HaveGrenade)
		{
			method_1(1);
		}
		else
		{
			UnityEngine.Debug.LogError("sectant priest can't change logic to suicide because have no grenade");
		}
	}

	public void method_8()
	{
		if (!Bool_0 || !(Float_2 < Time.time))
		{
			return;
		}
		Float_2 = Time.time + 5f;
		if (Owner.Memory.GoalEnemy != null)
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in Owner.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.Distance < 30f)
			{
				enemyInfo.Value.SetVisible(value: true);
			}
		}
	}

	public override void Dispose()
	{
		Owner.Boss.OnFollowerStatusChange -= method_6;
		base.Dispose();
	}
}
