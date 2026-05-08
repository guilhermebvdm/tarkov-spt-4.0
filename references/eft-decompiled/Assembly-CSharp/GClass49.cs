using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass49 : GClass48
{
	[Serializable]
	[CompilerGenerated]
	public class Class211
	{
		public static readonly Class211 class211_0 = new Class211();

		public static Func<GroupPoint, bool> func_0;

		public bool method_0(GroupPoint x)
		{
			return x.Special.HasFlag(ECoverPointSpecial.forFollowers);
		}
	}

	[NonSerialized]
	public List<GroupPoint> List_0;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_4;

	public GClass49(BotOwner bot, int priority)
		: base(bot, priority)
	{
		method_20();
	}

	public void method_20()
	{
		List_0 = GClass856.Shuffle(BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.Points.Where((GroupPoint x) => x.Special.HasFlag(ECoverPointSpecial.forFollowers)).ToList());
		Bool_4 = List_0.Count > 0;
		Float_4 = Time.time + 30f;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (Bool_4)
		{
			if (Float_4 < Time.time)
			{
				Float_4 = Time.time + 30f;
				List_0 = GClass856.Shuffle(List_0);
			}
			List<GroupPoint> list_ = List_0;
			if (BotOwner_0.BewareGrenade.GrenadeDangerPoint != null)
			{
				foreach (GroupPoint item in list_)
				{
					float sqrMagnitude = (item.Position - BotOwner_0.BewareGrenade.GrenadeDangerPoint.DangerPoint).sqrMagnitude;
					if (!item.IsSpotted && item.IsFreeById(BotOwner_0.Id) && !(sqrMagnitude < LocalBotSettingsProviderClass.Core.DELTA_GRENADE_BEWARE_DIST_SQRT))
					{
						return item.GetById(BotOwner_0.Id);
					}
				}
			}
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override string Name()
	{
		return "BoarGrenadeDanger";
	}
}
