using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass559 : PatrolPointChooserBasic
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public IGetProfileData IgetProfileData_0;

	[NonSerialized]
	public GClass436 Gclass436_0;

	public GClass559(BotOwner owner, IGetProfileData data, string nameAlt)
		: base(owner)
	{
		String_0 = nameAlt;
		Gclass436_0 = owner.Boss.BossLogic as GClass436;
		if (Gclass436_0 != null && Gclass436_0.HaveSecurityInfo)
		{
			Gclass436_0.OnAlarmChange += method_3;
		}
	}

	public override bool IsWaySuitableByNameId(string nameTest)
	{
		if (Gclass436_0.AlarmActivated)
		{
			return String_0 == nameTest;
		}
		return true;
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

	public void method_3(bool obj)
	{
		Owner.PatrollingData.PointChooser.ShallChangeWay(force: true);
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
