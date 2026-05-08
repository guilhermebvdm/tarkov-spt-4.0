using System;
using EFT;
using UnityEngine;

public class BotPeaceLook : GClass429
{
	[NonSerialized]
	public const float AIM_SDIST_REPLY = 400f;

	[NonSerialized]
	public const float STANDARD_LOOK_PERIOD = 3f;

	[NonSerialized]
	public const float STANDARD_AIM_REQUEST_CHANCE_100 = 50f;

	[NonSerialized]
	public bool Doing;

	[NonSerialized]
	public IPlayer LookTo;

	[NonSerialized]
	public float AimEndTime;

	public BotPeaceLook(BotOwner owner)
		: base(owner)
	{
	}

	public void StartLook(GClass413 closest)
	{
		StartLook(closest.GetAnother(BotOwner_0));
	}

	public void StartLook(IPlayer lookTo)
	{
		if (BotOwner_0.Settings.FileSettings.Patrol.CAN_PEACEFUL_LOOK && BotOwner_0.Memory.IsPeace)
		{
			LookTo = lookTo;
			Doing = true;
			AimEndTime = Time.time + GClass856.GreateRandom(3f);
		}
	}

	public bool HaveActions()
	{
		return Doing;
	}

	public void EndAim()
	{
		LookTo = null;
		Doing = false;
	}

	public void Activate()
	{
		BotOwner_0.Memory.OnPeaceChange += method_0;
	}

	public void ManualUpdate()
	{
		if (Doing)
		{
			try
			{
				BotOwner_0.WeaponManager.ShootController.SetAim(value: false);
				Vector3 point = LookTo.Position + BotOwner.STAY_HEIGHT;
				BotOwner_0.Steering.LookToPoint(point);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("{0} - cant process {1}, probably _lookTo [{2}] is unspaned :\n{3}", BotOwner_0.ProfileId, "BotPeaceHardAim", LookTo.ProfileId, ex);
				Doing = false;
				throw;
			}
			if (AimEndTime < Time.time)
			{
				EndAim();
			}
		}
	}

	public void TryAddRequest(IPlayer player)
	{
		if (GClass856.IsTrue100(50f) && (player.Position - BotOwner_0.Position).sqrMagnitude < 400f && BotReceiver.CanReceiveFromPoint(player.Transform.position, BotOwner_0.Transform.position, BotOwner_0.LookDirection, BotOwner_0.LookSensor.VISIBLE_ANGLE))
		{
			StartLook(player);
		}
	}

	public void method_0(bool isPeace)
	{
		if (!isPeace)
		{
			EndAim();
		}
	}

	public void Dispose()
	{
		BotOwner_0.Memory.OnPeaceChange -= method_0;
	}
}
