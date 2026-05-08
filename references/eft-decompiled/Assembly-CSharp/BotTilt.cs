using System;
using EFT;
using UnityEngine;

public class BotTilt : GClass429
{
	[NonSerialized]
	public bool CoreTilt;

	[NonSerialized]
	public bool CanITilt;

	[NonSerialized]
	public float TiltOff;

	[NonSerialized]
	public float CurTilt;

	[NonSerialized]
	public BotTiltType BotTiltType;

	public BotTilt(BotOwner owner)
		: base(owner)
	{
		CanITilt = GClass856.IsTrue100(LocalBotSettingsProviderClass.Core.TILT_CHANCE);
	}

	public void ManualUpdate()
	{
		if (CoreTilt && TiltOff < Time.time)
		{
			Stop();
		}
	}

	public void Stop()
	{
		BotOwner_0.GetPlayer.MovementContext.SetTilt(0f);
		CoreTilt = false;
	}

	public void Set(BotTiltType tiltSide)
	{
		if (CanITilt && LocalBotSettingsProviderClass.Core.CAN_TILT)
		{
			TiltOff = Time.time + 1.5f;
			CurTilt = ((tiltSide == BotTiltType.left) ? (-5f) : 5f);
			BotOwner_0.GetPlayer.MovementContext.SetTilt(CurTilt);
			CoreTilt = true;
		}
	}
}
