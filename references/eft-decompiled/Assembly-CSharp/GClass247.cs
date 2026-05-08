using System;
using EFT;

public class GClass247 : GClass200<GClass26>
{
	public bool CanLookAround = true;

	[NonSerialized]
	public GClass270 Gclass270_0;

	[NonSerialized]
	public GClass218 Gclass218_0;

	[NonSerialized]
	public GClass184 Gclass184_0;

	[NonSerialized]
	public GClass265 Gclass265_0;

	[NonSerialized]
	public GClass264 Gclass264_0;

	public GClass247(BotOwner bot)
		: base(bot)
	{
		Gclass264_0 = new GClass264(bot);
		Gclass265_0 = new GClass265(bot);
		Gclass270_0 = new GClass270(bot);
		Gclass218_0 = new GClass218(bot);
		Gclass184_0 = new GClass184(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		if (BotOwner_0.PatrollingData.CurPatrolPoint == null)
		{
			BotOwner_0.PatrollingData.FindNextPoint(withSetting: true, withoutNext: false);
			return;
		}
		if (BotOwner_0.PatrollingData.Status == PatrolStatus.go && GClass510.IsCome(BotOwner_0, BotOwner_0.PatrollingData.CurPatrolPoint.Position, extraTarget: false, out var _))
		{
			BotOwner_0.PatrollingData.ComeToPoint();
		}
		BotOwner_0.PatrollingData.ReserveGoNextPoint();
		if (BotOwner_0.PatrollingData.Status == PatrolStatus.stay)
		{
			if (BotOwner_0.PatrollingData.CurPatrolPoint.ActionData == null)
			{
				Gclass270_0.UpdateNodeByBrain(data);
				return;
			}
			switch (BotOwner_0.PatrollingData.CurPatrolPoint.ActionData.ManualUpdate(BotOwner_0))
			{
			case ReserveWayResult.stay:
				Gclass270_0.UpdateNodeByBrain(data);
				return;
			case ReserveWayResult.move:
				Gclass218_0.UpdateNodeByBrain(data);
				return;
			case ReserveWayResult.shoot:
				Gclass184_0.UpdateNodeByBrain(data as GClass27);
				return;
			case ReserveWayResult.looting:
				Gclass265_0.UpdateNodeByBrain(data);
				return;
			case ReserveWayResult.drop:
				Gclass264_0.UpdateNodeByBrain(data);
				return;
			}
		}
		BotOwner_0.SetPose(1f);
		BotOwner_0.PatrollingData.ManualUpdate(CanLookAround);
		BotOwner_0.PatrollingData.MoveUpdate();
	}
}
