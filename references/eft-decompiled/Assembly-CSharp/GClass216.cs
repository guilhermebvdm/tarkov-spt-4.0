using System;
using EFT;
using UnityEngine;

public class GClass216 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	public GClass598 GClass598_0 => BotOwner_0.BotRequestController.CurRequest as GClass598;

	public GClass216(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.Mover.Sprint(val: false);
		if (Float_0 < Time.time)
		{
			method_6();
		}
		BotOwner_0.SetPose(1f);
		BotOwner_0.LookData.SetLookPointByHearing();
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			method_5();
		}
	}

	public void method_5()
	{
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.StopMove();
	}

	public void method_6()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.GoToPoint(GClass598_0.ThrowFromPos.Value);
	}
}
