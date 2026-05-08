using System;
using EFT;
using UnityEngine;

public class GClass252 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	public GClass252(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.GetPlayer.Physical.Stamina.Current = 10000f;
		BotOwner_0.Sprint(val: false);
		BotOwner_0.SetPose(1f);
		if (Float_1 < Time.time)
		{
			Float_1 = Time.time + 15f;
			BotOwner_0.DebugMemory.DebugCoversChecker.SetNewPoint();
			if (BotOwner_0.Memory.CurCustomCoverPoint != null)
			{
				BotOwner_0.GoToPoint(BotOwner_0.Memory.CurCustomCoverPoint);
			}
		}
		BotOwner_0.Steering.LookToMovingDirection();
		method_5();
	}

	public void method_5()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		}
	}
}
