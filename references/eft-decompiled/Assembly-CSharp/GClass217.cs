using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass217 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass593 GClass593_0 => BotOwner_0.BotRequestController.CurRequest as GClass593;

	public GClass217(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.Mover.Sprint(val: false);
		if (Float_0 < Time.time)
		{
			method_7();
		}
		method_5();
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			method_6();
		}
	}

	public void method_5()
	{
		BotOwner_0.Memory.botObserveData.SetVector();
		BotOwner_0.Memory.botObserveData.Update();
	}

	public void method_6()
	{
		GClass593_0.Complete();
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.StopMove();
	}

	public void method_7()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.Mover.SetPose(1f);
		if (BotOwner_0.GoToPoint(GClass593_0.Position) != NavMeshPathStatus.PathComplete)
		{
			GClass593_0.Dispose();
		}
	}
}
