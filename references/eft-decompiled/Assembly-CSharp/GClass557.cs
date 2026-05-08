using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass557 : PatrolPointChooserBasic
{
	[Serializable]
	[CompilerGenerated]
	public class Class254
	{
		public static readonly Class254 class254_0 = new Class254();

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

	public GClass557(BotOwner owner)
		: base(owner)
	{
	}

	public override PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		if (Owner.BotFollower.HaveBoss)
		{
			PatrolPoint patrolPosByIndex = Owner.BotFollower.BossToFollow.GetPatrolPosByIndex(Owner.BotFollower.Index);
			if (patrolPosByIndex != null)
			{
				return new PatrolPointContainer(patrolPosByIndex);
			}
		}
		return base.FindNextPoint(withSetting, withoutNext, Owner.Boss.Followers.Count, canCut: false, pointFilter);
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
	public bool method_3(PatrolWay x)
	{
		if (x.PatrolType == PatrolType.reserved && x.Points.Count >= Owner.Boss.Followers.Count)
		{
			return x.IsCloseToSelect(Owner, Owner.Settings.FileSettings.Patrol.CLOSE_TO_SELECT_RESERV_WAY);
		}
		return false;
	}
}
