using System;
using EFT;
using UnityEngine;

public class BotPeaceHardAim : GClass429
{
	[NonSerialized]
	public const float AIM_SDIST_REPLY = 400f;

	[NonSerialized]
	public const float STANDARD_AIM_PERIOD = 7f;

	[NonSerialized]
	public const float STANDARD_AIM_REQUEST_CHANCE_100 = 50f;

	[NonSerialized]
	public bool Doing;

	[NonSerialized]
	public IPlayer LookTo;

	[NonSerialized]
	public float AimEndTime;

	public BotPeaceHardAim(BotOwner owner)
		: base(owner)
	{
	}

	public void StartAim(GClass413 closest)
	{
		StartAim(closest.GetAnother(BotOwner_0));
	}

	public void StartAim(IPlayer lookTo)
	{
		if (BotOwner_0.Settings.FileSettings.Patrol.CAN_HARD_AIM && BotOwner_0.Memory.IsPeace)
		{
			if (LookTo != null)
			{
				LookTo.OnIPlayerDeadOrUnspawn -= method_0;
			}
			LookTo = lookTo;
			if (LookTo != null)
			{
				LookTo.OnIPlayerDeadOrUnspawn += method_0;
			}
			Doing = true;
			AimEndTime = Time.time + GClass856.GreateRandom(7f);
		}
	}

	public bool HaveActions()
	{
		return Doing;
	}

	public void EndAim()
	{
		if (LookTo != null)
		{
			LookTo.OnIPlayerDeadOrUnspawn -= method_0;
		}
		if (BotOwner_0.BotState == EBotState.Active)
		{
			BotOwner_0.WeaponManager.ShootController.SetAim(value: false);
		}
		LookTo = null;
		Doing = false;
	}

	public void method_0(IPlayer obj)
	{
		EndAim();
	}

	public void Activate()
	{
		BotOwner_0.Memory.OnPeaceChange += method_1;
	}

	public void ManualUpdate()
	{
		if (Doing)
		{
			try
			{
				BotOwner_0.WeaponManager.ShootController.SetAim(value: true);
				Vector3 point = LookTo.Position + BotOwner.STAY_HEIGHT;
				BotOwner_0.Steering.LookToPoint(point);
				BotOwner_0.StopMove();
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
			StartAim(player);
		}
	}

	public void method_1(bool isPeace)
	{
		if (!isPeace)
		{
			EndAim();
		}
	}

	public void Dispose()
	{
		if (BotOwner_0.Memory != null)
		{
			BotOwner_0.Memory.OnPeaceChange -= method_1;
		}
		if (LookTo != null)
		{
			LookTo.OnIPlayerDeadOrUnspawn -= method_0;
		}
	}
}
