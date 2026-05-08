using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass286 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass599 GClass599_0 => BotOwner_0.BotRequestController.CurRequest as GClass599;

	public GClass286(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_1();
		BotOwner_0.Mover.Sprint(val: false);
		if (Float_0 < Time.time)
		{
			method_3();
		}
		method_0();
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			method_2();
			BotOwner_0.WeaponManager.Grenades.DoThrow();
		}
	}

	public void method_0()
	{
		BotOwner_0.Memory.botObserveData.SetVector();
		BotOwner_0.Memory.botObserveData.Update();
	}

	public DoorInteractionStatus method_1()
	{
		return BotOwner_0.DoorOpener.UpdateDoorInteractionStatus();
	}

	public void method_2()
	{
		GClass599_0.Complete();
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.StopMove();
	}

	public void method_3()
	{
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.Mover.SetPose(1f);
		if (BotOwner_0.GoToPoint(GClass599_0.PlaceToThrow) != NavMeshPathStatus.PathComplete)
		{
			GClass599_0.Dispose();
		}
	}
}
