using System;
using EFT;
using UnityEngine;

public class GClass214 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass596 GClass596_0 => BotOwner_0.BotRequestController.CurRequest as GClass596;

	public GClass214(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		if (Float_0 < Time.time)
		{
			method_6();
		}
		BotOwner_0.SetPose(1f);
		BotOwner_0.SetTargetMoveSpeed(0.6f);
		BotOwner_0.LookData.SetLookPointByHearing();
		BotOwner_0.Sprint(val: false);
		if (!Bool_0)
		{
			Vector3 position = GClass596_0.Door.transform.position;
			BotOwner_0.GoToPoint(position);
			Bool_0 = true;
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			method_5();
		}
	}

	public void method_5()
	{
		BotOwner_0.StopMove();
	}

	public void method_6()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		Bool_0 = false;
	}
}
