using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass287 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public GClass601 GClass601_0 => BotOwner_0.BotRequestController.CurRequest as GClass601;

	public GClass287(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_1();
		BotOwner_0.Mover.Sprint(val: false);
		if (Float_0 < Time.time)
		{
			method_2();
		}
		if (GClass601_0 == null)
		{
			return;
		}
		method_0();
		Vector3 vector = GClass601_0.PlaceToThrow - BotOwner_0.Position;
		vector.y = 0f;
		if (!(vector.sqrMagnitude < 0.25f))
		{
			return;
		}
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.StopMove();
		if (!Bool_1)
		{
			BotOwner_0.WeaponManager.Grenades.SetThrowData(GClass601_0.ThrowData);
			BotOwner_0.WeaponManager.Grenades.DoThrow();
			Bool_1 = true;
			BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowComplete += delegate
			{
				GClass601_0?.Complete();
			};
			BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 10f, delegate
			{
				GClass601_0?.Complete();
			});
		}
		else
		{
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
		Float_0 = Time.time + BotOwner_0.Settings.FileSettings.Move.UPDATE_TIME_RECAL_WAY;
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		BotOwner_0.Mover.SetPose(1f);
		_ = BotOwner_0.Position;
		GClass601 gClass601_ = GClass601_0;
		if (gClass601_ != null && BotOwner_0.GoToPoint(gClass601_.PlaceToThrow) != NavMeshPathStatus.PathComplete)
		{
			gClass601_.Dispose();
		}
	}

	[CompilerGenerated]
	public void method_3(ThrowWeapItemClass weap)
	{
		GClass601_0?.Complete();
	}

	[CompilerGenerated]
	public void method_4()
	{
		GClass601_0?.Complete();
	}
}
