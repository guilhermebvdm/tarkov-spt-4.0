using System;
using EFT;
using UnityEngine;

public class GClass211 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	public GClass211(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		if (!BotOwner_0.DeadBodyData.IsNear)
		{
			Float_0 -= Time.deltaTime;
			if (Float_0 < 0f)
			{
				method_6();
			}
			BotOwner_0.LookData.SetLookPointByHearing();
			if (!Bool_0)
			{
				Vector3_0 = BotOwner_0.DeadBodyData.TargetDeadBody.Position;
				BotOwner_0.GoToPoint(Vector3_0);
				Bool_0 = true;
			}
			if (!BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
			{
				method_5();
			}
		}
	}

	public void method_5()
	{
		BotOwner_0.DeadBodyData.Come();
		BotOwner_0.StopMove();
	}

	public void method_6()
	{
		Float_0 = BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		Bool_0 = false;
	}
}
