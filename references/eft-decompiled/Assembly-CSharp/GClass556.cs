using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.MovingPlatforms;

public class GClass556 : GClass555
{
	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public IBossToFollow IbossToFollow_0;

	public GClass556(BotOwner owner, IGetProfileData data, string name, string nameAlt, IBossToFollow bossToFollow)
		: base(owner, data, name)
	{
		IbossToFollow_0 = bossToFollow;
		String_1 = nameAlt;
		if (IbossToFollow_0.IsAI && IbossToFollow_0.Player().AIData.BotOwner.Boss.BossLogic is GClass435 { HaveTrainInfo: not false })
		{
			BotEventHandler instance = Singleton<BotEventHandler>.Instance;
			if (instance != null)
			{
				instance.OnTrainCome += method_6;
			}
		}
	}

	public override PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		if (Owner.Profile.Info.Settings.Role != WildSpawnType.followerGluharSecurity)
		{
			PatrolPointContainer patrolPointContainer = method_4();
			base.PointControl.SetTarget(patrolPointContainer);
			return patrolPointContainer;
		}
		return base.FindNextPoint(withSetting, withoutNext, Owner.Boss.Followers.Count, false, pointFilter);
	}

	public override bool IsWaySuitableByNameId(string nameId)
	{
		if (Bool_0)
		{
			return String_1 == nameId;
		}
		return String_0 == nameId;
	}

	public override bool TryToFindWay(out PatrolWay way, out float delta)
	{
		way = null;
		delta = Owner.Settings.FileSettings.Patrol.CHANGE_WAY_TIME;
		List<PatrolWay> list = Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x.PatrolType == PatrolType.boss && x.Suitable(Owner, IgetProfileData_0)).ToList();
		if (list.Count == 0)
		{
			return false;
		}
		PatrolWay patrolWay = GClass856.RandomElement(list);
		if (patrolWay != null)
		{
			way = patrolWay;
			return true;
		}
		return false;
	}

	public override string GetDebugName()
	{
		if (Bool_0)
		{
			return String_1;
		}
		return String_0;
	}

	public override void Dispose()
	{
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnTrainCome -= method_6;
		}
		base.Dispose();
	}

	public PatrolPointContainer method_4()
	{
		PatrolPointContainer patrolPointContainer = method_5();
		if (patrolPointContainer == null)
		{
			if (TryToFindWay(out var way, out var _))
			{
				base.PointControl.SetWay(way, base.FindNextPoint);
			}
			patrolPointContainer = method_5();
			if (patrolPointContainer == null)
			{
				patrolPointContainer = new PatrolPointContainer(GClass856.RandomElement(base.PointControl.Way.Points));
			}
		}
		return patrolPointContainer;
	}

	public PatrolPointContainer method_5()
	{
		float num = float.MaxValue;
		PatrolPointContainer result = null;
		foreach (PatrolPoint point in base.PointControl.Way.Points)
		{
			if (point.IsFreeFor(Owner))
			{
				float sqrMagnitude = (point.Position - IbossToFollow_0.PatrollingData.CurTargetPoint).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = new PatrolPointContainer(point);
				}
			}
		}
		return result;
	}

	public void method_6(Locomotive.ETravelState obj)
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

	[CompilerGenerated]
	public bool method_7(PatrolWay x)
	{
		if (x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x.PatrolType == PatrolType.boss)
		{
			return x.Suitable(Owner, IgetProfileData_0);
		}
		return false;
	}
}
