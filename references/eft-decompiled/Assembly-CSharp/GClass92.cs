using System;
using EFT;

public class GClass92 : GClass91
{
	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	public GClass92(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass441>(bot);
		Gclass453_0.FindBoss();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (Gclass453_0.HaveLogic())
		{
			data.PlaceInfo = Gclass453_0.BossLogic.AreaId;
		}
		return base.FindPoint(data, p, checkCurrent);
	}
}
