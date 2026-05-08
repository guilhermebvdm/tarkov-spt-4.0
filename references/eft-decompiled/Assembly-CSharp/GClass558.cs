using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;
using UnityEngine;

public class GClass558 : PatrolPointChooserBasic
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public IGetProfileData IgetProfileData_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass558(BotOwner owner, IGetProfileData data, string name, string nameAlt)
		: base(owner)
	{
		String_0 = nameAlt;
		String_1 = name;
		if (owner.Boss.BossLogic is GClass435 { HaveTrainInfo: not false })
		{
			BotEventHandler instance = Singleton<BotEventHandler>.Instance;
			if (instance != null)
			{
				instance.OnTrainCome += method_3;
			}
		}
	}

	public override void Dispose()
	{
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnTrainCome -= method_3;
		}
		base.Dispose();
	}

	public override PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		float num = float.MaxValue;
		PatrolPoint patrolPoint = null;
		foreach (PatrolPoint point in base.PointControl.Way.Points)
		{
			if (point.IsFreeFor(Owner))
			{
				float sqrMagnitude = (point.Position - Owner.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					patrolPoint = point;
				}
			}
		}
		if (patrolPoint == null)
		{
			method_4();
			patrolPoint = GClass856.RandomElement(base.PointControl.Way.Points);
		}
		return new PatrolPointContainer(patrolPoint);
	}

	public override bool IsWaySuitableByNameId(string nameTest)
	{
		if (Bool_0)
		{
			return String_0 == nameTest;
		}
		return String_1 == nameTest;
	}

	public override bool TryToFindWay(out PatrolWay way, out float delta)
	{
		List<PatrolWay> list = Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.PatrolType == PatrolType.boss && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x.Suitable(Owner, IgetProfileData_0)).ToList();
		if (list.Count == 0)
		{
			method_4();
			return base.TryToFindWay(out way, out delta);
		}
		way = GClass856.RandomElement(list);
		delta = Owner.Settings.FileSettings.Patrol.CHANGE_WAY_TIME;
		if (way.PatrolType == PatrolType.reserved)
		{
			delta = Owner.Settings.FileSettings.Patrol.RESERVE_OUT_TIME;
		}
		NextChangeWay = Time.time + delta * GClass856.Random(0.6f, 1.4f);
		return true;
	}

	public void method_3(Locomotive.ETravelState obj)
	{
		switch (obj)
		{
		case Locomotive.ETravelState.OnRouteBack:
			Bool_0 = false;
			Owner.PatrollingData.PointChooser.ShallChangeWay(force: true);
			break;
		case Locomotive.ETravelState.OnRouteToDestination:
			Bool_0 = true;
			Owner.PatrollingData.PointChooser.ShallChangeWay(force: true);
			break;
		}
	}

	public void method_4()
	{
	}

	[CompilerGenerated]
	public bool method_5(PatrolWay x)
	{
		if (x.PatrolType == PatrolType.boss && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role))
		{
			return x.Suitable(Owner, IgetProfileData_0);
		}
		return false;
	}
}
