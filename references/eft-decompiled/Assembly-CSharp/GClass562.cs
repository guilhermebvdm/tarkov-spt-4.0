using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass562(BotOwner owner) : PatrolPointChooserBasic(owner)
{
	[Serializable]
	[CompilerGenerated]
	public class Class255
	{
		public static readonly Class255 class255_0 = new Class255();

		public static Func<PatrolWay, bool> func_0;

		public static Func<PatrolWay, bool> func_1;

		public bool method_0(PatrolWay x)
		{
			return x.PatrolType == PatrolType.boss;
		}

		public bool method_1(PatrolWay x)
		{
			return x.PatrolType == PatrolType.boss;
		}
	}

	[CompilerGenerated]
	public class Class256
	{
		public GClass562 gclass562_0;

		public int minSubTargets;

		public GDelegate4 pointFilter;

		public bool method_0(PatrolPoint point)
		{
			if (point.Owner == null && (point.transform.position - gclass562_0.Owner.Transform.position).sqrMagnitude > 4f && point.SubPointsCount > minSubTargets && point.IsFreeFor(gclass562_0.Owner))
			{
				if (pointFilter != null)
				{
					return pointFilter(point);
				}
				return true;
			}
			return false;
		}
	}

	[NonSerialized]
	public float Float_0 = 20f;

	[NonSerialized]
	public float Float_1 = 80f;

	public override PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		bool flag = base.PointControl.Way.PatrolType == PatrolType.patrolling && canCut;
		PatrolPointContainer patrolPointContainer = null;
		if (withoutNext)
		{
			base.PointControl.SetReservedPatrolPoint(null);
		}
		else
		{
			patrolPointContainer = GetTargetNext();
		}
		if (patrolPointContainer == null)
		{
			List<PatrolPoint> points = base.PointControl.Way.Points.Where((PatrolPoint point) => point.Owner == null && (point.transform.position - Owner.Transform.position).sqrMagnitude > 4f && point.SubPointsCount > minSubTargets && point.IsFreeFor(Owner) && (pointFilter == null || pointFilter(point))).ToList();
			points = method_3(points);
			if (points.Count > 0)
			{
				PatrolPoint patrolPoint = GClass856.RandomElement(points);
				if (patrolPoint != null)
				{
					patrolPointContainer = new PatrolPointContainer(patrolPoint);
				}
			}
		}
		if (patrolPointContainer != null)
		{
			if (flag && GClass856.IsTrue100(Owner.Settings.FileSettings.Patrol.CHANCE_TO_CUT_WAY_0_100))
			{
				base.PathDistCoef = GClass856.Random(Owner.Settings.FileSettings.Patrol.CUT_WAY_MIN_0_1, Owner.Settings.FileSettings.Patrol.CUT_WAY_MAX_0_1);
			}
			else
			{
				base.PathDistCoef = 1f;
			}
			base.WasCutted = base.PathDistCoef < 0.95f;
			if (withSetting)
			{
				base.PointControl.SetTarget(patrolPointContainer);
			}
			return patrolPointContainer;
		}
		List<PatrolPoint> list = method_3(base.PointControl.Way.Points);
		if (!ErrorPrinted)
		{
			ErrorPrinted = true;
			_ = base.PointControl.Way.name;
			_ = base.PointControl.Way.MaxPersons;
			_ = base.PointControl.Way.Points.Count;
			_ = Owner.BotsGroup.BotZone;
			ToString();
		}
		return new PatrolPointContainer(GClass856.RandomElement(list));
	}

	public List<PatrolPoint> method_3(List<PatrolPoint> points)
	{
		List<PatrolPoint> list = new List<PatrolPoint>();
		for (int i = 0; i < points.Count; i++)
		{
			PatrolPoint patrolPoint = points[i];
			bool flag = true;
			for (int j = 0; j < Owner.BotsGroup.MembersCount; j++)
			{
				BotOwner botOwner = Owner.BotsGroup.Member(j);
				PatrolPointContainer curPatrolPoint = botOwner.PatrollingData.CurPatrolPoint;
				if (!(botOwner == Owner) && curPatrolPoint != null)
				{
					float num = GClass856.SqrDistance(patrolPoint.Position, curPatrolPoint.Position);
					if (num < Float_0 * Float_0 || !(num <= Float_1 * Float_1))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				list.Add(patrolPoint);
			}
		}
		if (list.Count == 0)
		{
			return points;
		}
		return list;
	}

	public override bool TryToFindWay(out PatrolWay way, out float delta)
	{
		List<PatrolWay> list;
		if (!GClass856.IsTrue100(Owner.Settings.FileSettings.Boss.CHANCE_USE_RESERVE_PATROL_100))
		{
			list = ((!Owner.BotFollower.HaveBoss) ? Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.PatrolType == PatrolType.boss).ToList() : new List<PatrolWay> { Owner.BotFollower.BossToFollow.Player().AIData.BotOwner.PatrollingData.Way });
		}
		else
		{
			List<PatrolWay> list2 = Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.PatrolType == PatrolType.reserved && x.Points.Count >= Owner.Boss.Followers.Count && x.IsCloseToSelect(Owner, Owner.Settings.FileSettings.Patrol.CLOSE_TO_SELECT_RESERV_WAY)).ToList();
			list = ((list2.Count != 0) ? list2 : Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.PatrolType == PatrolType.boss).ToList());
		}
		if (list.Count == 0)
		{
			LogPatrolData();
			return base.TryToFindWay(out way, out delta);
		}
		way = GClass856.RandomElement(list);
		if (way == null)
		{
			way = GClass856.RandomElement(Owner.BotsGroup.BotZone.PatrolWays);
		}
		delta = Owner.Settings.FileSettings.Patrol.CHANGE_WAY_TIME;
		if (way.PatrolType == PatrolType.reserved)
		{
			delta = Owner.Settings.FileSettings.Patrol.RESERVE_OUT_TIME;
		}
		NextChangeWay = Time.time + delta * GClass856.Random(0.6f, 1.4f);
		return true;
	}

	[CompilerGenerated]
	public bool method_4(PatrolWay x)
	{
		if (x.PatrolType == PatrolType.reserved && x.Points.Count >= Owner.Boss.Followers.Count)
		{
			return x.IsCloseToSelect(Owner, Owner.Settings.FileSettings.Patrol.CLOSE_TO_SELECT_RESERV_WAY);
		}
		return false;
	}
}
