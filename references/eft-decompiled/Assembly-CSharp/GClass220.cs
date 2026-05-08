using System;
using EFT;
using UnityEngine;

public class GClass220 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	public GClass220(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.Mover.Sprint(val: false);
		if (BotOwner_0.SuppressShoot.PointToSuppressFrom != null)
		{
			if (!BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true))
			{
				if (Float_0 > Time.time)
				{
					Float_0 = Time.time + 1f;
					BotOwner_0.Mover.SetTargetMoveSpeed(1f);
					BotOwner_0.GoToPoint(BotOwner_0.SuppressShoot.PointToSuppressFrom);
				}
				BotOwner_0.LookData.SetLookPointByHearing();
				method_5();
			}
		}
		else
		{
			BotOwner_0.StopMove();
		}
	}

	public void method_5()
	{
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.StopMove();
	}
}
