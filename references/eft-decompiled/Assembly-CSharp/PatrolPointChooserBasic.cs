using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EFT;
using UnityEngine;

public class PatrolPointChooserBasic
{
	[Serializable]
	[CompilerGenerated]
	public class Class251
	{
		public static readonly Class251 class251_0 = new Class251();

		public static Func<PatrolWay, bool> func_0;

		public static Func<PatrolWay, bool> func_1;

		public static Func<PatrolPoint, bool> func_2;

		public bool method_0(PatrolWay x)
		{
			if (x.HaveFreeSpace())
			{
				return x.PatrolType == PatrolType.reserved;
			}
			return false;
		}

		public bool method_1(PatrolWay x)
		{
			if (x.HaveFreeSpace())
			{
				return x.PatrolType != PatrolType.reserved;
			}
			return false;
		}

		public bool method_2(PatrolPoint x)
		{
			return !x.CanUseByBoss;
		}
	}

	[CompilerGenerated]
	public class Class252
	{
		public BotOwner follower;

		public bool method_0(PatrolPoint x)
		{
			return x.IsFreeFor(follower);
		}
	}

	[CompilerGenerated]
	public class Class253
	{
		public PatrolPointChooserBasic patrolPointChooserBasic_0;

		public int minSubTargets;

		public GDelegate4 pointFilter;

		public bool method_0(PatrolPoint point)
		{
			if (point.Owner == null && (point.transform.position - patrolPointChooserBasic_0.Owner.Transform.position).sqrMagnitude > 4f && point.SubPointsCount > minSubTargets && point.IsFreeFor(patrolPointChooserBasic_0.Owner))
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
	public const float NEXT_POINT_DIST_SQRT = 4f;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public float NextChangeWay;

	[NonSerialized]
	public bool CanChangeWays;

	[NonSerialized]
	public bool ErrorPrinted;

	[field: NonSerialized]
	public bool WasCutted { get; set; }

	[field: NonSerialized]
	public float PathDistCoef { get; set; } = 1f;

	public GClass504 PointControl => Owner.PatrollingData.PointControl;

	public GClass509 PathControl => Owner.PatrollingData.PatrolPathControl;

	public PatrolPointChooserBasic(BotOwner owner)
	{
		Owner = owner;
		CanChangeWays = Owner.BotsGroup.BotZone.PatrolWays.Length > 1;
	}

	public virtual void ChooseStartWay()
	{
		List<PatrolWay> list;
		if (Owner.Settings.FileSettings.Patrol.TRY_CHOOSE_RESERV_WAY_ON_START)
		{
			list = Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x.PatrolType == PatrolType.reserved).ToList();
			if (list.Count == 0)
			{
				_ = "THIS bot need a RESERV type patrol way to start. But he can't find anyone!! " + Owner.Profile.Info.Settings.Role.ToString() + "  zone:" + Owner.BotsGroup.BotZone.NameZone + ".HOW TO FIX: Add patrol patrol reserve way to this zone.";
				list = Owner.BotsGroup.BotZone.PatrolWays.ToList();
			}
		}
		else
		{
			if (TryToFindWay(out var way, out var delta))
			{
				NextChangeWay = Time.time + delta * GClass856.Random(0.6f, 1.4f);
				PointControl.SetWay(way, FindNextPoint);
				return;
			}
			if (Owner.PatrollingData.Way != null)
			{
				LogPatrolData();
			}
			list = Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x.PatrolType != PatrolType.reserved).ToList();
		}
		if (list.Count == 0)
		{
			LogPatrolData();
			list = Owner.BotsGroup.BotZone.PatrolWays.ToList();
		}
		PointControl.SetWay(GClass856.RandomElement(list), FindNextPoint);
	}

	public PatrolPointContainer FindPointForFollower(BotOwner follower, PatrolWay way)
	{
		PatrolPointContainer patrolPointContainer;
		if (way.PatrolType == PatrolType.reserved)
		{
			List<PatrolPoint> list = way.Points.Where((PatrolPoint x) => x.IsFreeFor(follower)).ToList();
			if (list.Count > 0)
			{
				PatrolPoint patrolPoint = GClass856.RandomElement(list.Where((PatrolPoint x) => !x.CanUseByBoss));
				if (patrolPoint != null)
				{
					patrolPointContainer = new PatrolPointContainer(patrolPoint);
					follower.PatrollingData.PointControl.SetTarget(patrolPointContainer);
					return patrolPointContainer;
				}
				patrolPoint = GClass856.RandomElement(list);
				patrolPointContainer = new PatrolPointContainer(patrolPoint);
				follower.PatrollingData.PointControl.SetTarget(patrolPointContainer);
				return patrolPointContainer;
			}
		}
		patrolPointContainer = Owner.PatrollingData.CurPatrolPoint;
		follower.PatrollingData.PointControl.SetTarget(patrolPointContainer, follower.BotFollower.Index);
		return patrolPointContainer;
	}

	public virtual PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		bool flag = PointControl.Way.PatrolType == PatrolType.patrolling && canCut;
		PatrolPointContainer patrolPointContainer = null;
		if (withoutNext)
		{
			PointControl.SetReservedPatrolPoint(null);
		}
		else
		{
			patrolPointContainer = GetTargetNext();
		}
		if (patrolPointContainer == null)
		{
			List<PatrolPoint> list = PointControl.Way.Points.Where((PatrolPoint point) => point.Owner == null && (point.transform.position - Owner.Transform.position).sqrMagnitude > 4f && point.SubPointsCount > minSubTargets && point.IsFreeFor(Owner) && (pointFilter == null || pointFilter(point))).ToList();
			if (list.Count > 0)
			{
				PatrolPoint patrolPoint = GClass856.RandomElement(list);
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
				PathDistCoef = GClass856.Random(Owner.Settings.FileSettings.Patrol.CUT_WAY_MIN_0_1, Owner.Settings.FileSettings.Patrol.CUT_WAY_MAX_0_1);
			}
			else
			{
				PathDistCoef = 1f;
			}
			WasCutted = PathDistCoef < 0.95f;
			if (withSetting)
			{
				PointControl.SetTarget(patrolPointContainer);
			}
			return patrolPointContainer;
		}
		List<PatrolPoint> points = PointControl.Way.Points;
		if (!ErrorPrinted)
		{
			ErrorPrinted = true;
			_ = PointControl.Way.name;
			_ = PointControl.Way.MaxPersons;
			_ = PointControl.Way.Points.Count;
			_ = Owner.BotsGroup.BotZone;
			ToString();
		}
		return new PatrolPointContainer(GClass856.RandomElement(points));
	}

	public void Disable()
	{
		if (PointControl.PatrolPoint != null)
		{
			PointControl.PatrolPoint.TargetPoint.SetOwner(null);
		}
		if (PointControl.Way != null)
		{
			PointControl.Way.RemoveMe(Owner);
		}
	}

	public virtual void ShallChangeWay(bool force = false)
	{
		if (CanChangeWays && (NextChangeWay < Time.time || force))
		{
			if (TryToFindWay(out var way, out var delta))
			{
				PointControl.SetWay(way, FindNextPoint);
			}
			NextChangeWay = Time.time + delta * GClass856.Random(0.6f, 1.4f);
		}
	}

	public void NextPointDecision(Action callback)
	{
		PatrolPointContainer patrolPointContainer = FindNextPoint(withSetting: false, withoutNext: false);
		if (patrolPointContainer != null && !(patrolPointContainer.TargetPoint == null))
		{
			Vector3[] array = Owner.Mover.CalcPath(patrolPointContainer.Position);
			Vector3 vector = ((array == null || array.Length < 2) ? patrolPointContainer.Position : array[1]);
			if (Vector3.Dot(vector - Owner.Transform.position, Owner.LookDirection) > 0f)
			{
				PointControl.SetTarget(patrolPointContainer);
				return;
			}
			callback();
			PointControl.SetReservedPatrolPoint(patrolPointContainer);
		}
	}

	public virtual bool IsWaySuitableByNameId(string nameId)
	{
		return false;
	}

	public void LogPatrolData()
	{
		if (GClass399.Instance.IsTraceEnable())
		{
			StringBuilder stringBuilder = new StringBuilder("all ways data:");
			int num = 1;
			PatrolWay[] patrolWays = Owner.BotsGroup.BotZone.PatrolWays;
			foreach (PatrolWay patrolWay in patrolWays)
			{
				bool flag = patrolWay.HaveFreeSpace();
				int num2 = patrolWay.UsersCount();
				int maxPersons = patrolWay.MaxPersons;
				PatrolType patrolType = patrolWay.PatrolType;
				stringBuilder.Append($"\nway:{num} {patrolWay.name} haveFreeSpace:{flag}  type:{patrolType.ToString()}  count:{num2}/{maxPersons}");
				num++;
			}
		}
	}

	public virtual bool TryToFindWay(out PatrolWay way, out float delta)
	{
		way = null;
		delta = Owner.Settings.FileSettings.Patrol.CHANGE_WAY_TIME;
		if (GClass856.IsTrue100(Owner.Settings.FileSettings.Patrol.CHANCE_TO_CHANGE_WAY_0_100))
		{
			return false;
		}
		List<PatrolWay> list = (Owner.Settings.FileSettings.Patrol.USE_ONLY_RESERV ? Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x != PointControl.Way && x.PatrolType == PatrolType.reserved).ToList() : ((!Owner.Settings.FileSettings.Patrol.CAN_CHOOSE_RESERV) ? Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x != PointControl.Way && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x.PatrolType != PatrolType.reserved && x.PatrolType != PatrolType.boss).ToList() : Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x != PointControl.Way && x.PatrolType != PatrolType.boss).ToList()));
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

	public PatrolPointContainer GetTargetNext()
	{
		return PointControl.GetPreferableNext();
	}

	public virtual void Dispose()
	{
	}

	[CompilerGenerated]
	public bool method_0(PatrolWay x)
	{
		if (x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x != PointControl.Way)
		{
			return x.PatrolType == PatrolType.reserved;
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_1(PatrolWay x)
	{
		if (x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x != PointControl.Way)
		{
			return x.PatrolType != PatrolType.boss;
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_2(PatrolWay x)
	{
		if (x.HaveFreeSpace() && x != PointControl.Way && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x.PatrolType != PatrolType.reserved)
		{
			return x.PatrolType != PatrolType.boss;
		}
		return false;
	}
}
