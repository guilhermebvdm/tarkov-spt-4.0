using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass560 : PatrolPointChooserBasic
{
	[NonSerialized]
	public IGetProfileData IgetProfileData_0;

	public GClass560(BotOwner owner, IGetProfileData data)
		: base(owner)
	{
		IgetProfileData_0 = data;
	}

	public override PatrolPointContainer FindNextPoint(bool withSetting, bool withoutNext, int minSubTargets = -1, bool canCut = true, GDelegate4 pointFilter = null)
	{
		return base.FindNextPoint(withSetting, withoutNext, Owner.Boss.Followers.Count, canCut: false, pointFilter);
	}

	public override bool TryToFindWay(out PatrolWay way, out float delta)
	{
		List<PatrolWay> list = Owner.BotsGroup.BotZone.PatrolWays.Where((PatrolWay x) => x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role) && x.Suitable(Owner, IgetProfileData_0)).ToList();
		if (list.Count == 0)
		{
			LogPatrolData();
			return base.TryToFindWay(out way, out delta);
		}
		way = GClass856.RandomElement(list);
		delta = Owner.Settings.FileSettings.Patrol.CHANGE_WAY_TIME;
		NextChangeWay = Time.time + delta * GClass856.Random(0.6f, 1.4f);
		return true;
	}

	[CompilerGenerated]
	public bool method_3(PatrolWay x)
	{
		if (x.HaveFreeSpace() && x.CanBeUsedByRole(Owner.Profile.Info.Settings.Role))
		{
			return x.Suitable(Owner, IgetProfileData_0);
		}
		return false;
	}
}
