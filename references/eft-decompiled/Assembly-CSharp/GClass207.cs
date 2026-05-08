using System;
using EFT;
using UnityEngine;

public class GClass207 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	public GClass207(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 4f;
			method_5();
		}
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.Steering.LookToMovingDirection();
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			BotOwner_0.Memory.ComeToPoint();
			BotOwner_0.StopMove();
		}
	}

	public void method_5()
	{
		BotOwner_0.BotLay.TryLay();
		if (BotOwner_0.Memory.CurCustomCoverPoint == null || BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			CustomNavigationPoint value = BotOwner_0.BotAttackManager.PointToGo();
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(value);
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			BotOwner_0.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint.Position, slowAtTheEnd: false, -1f, getUpWithCheck: false, mustHaveWay: true, mustGetUp: false);
		}
	}
}
