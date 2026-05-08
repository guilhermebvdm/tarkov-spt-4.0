using System;
using EFT;
using UnityEngine;

public class BotStroboscopeLight : GClass429
{
	[NonSerialized]
	public bool CanUse;

	[NonSerialized]
	public bool Switcher;

	[NonSerialized]
	public bool Enabled;

	[NonSerialized]
	public float NextSwitch;

	[NonSerialized]
	public float EndTotal;

	[NonSerialized]
	public BotLight Light;

	[NonSerialized]
	public const float SWITCH_PERIOD = 0.1f;

	public BotStroboscopeLight(BotOwner owner, BotLight light)
		: base(owner)
	{
		Light = light;
		CanUse = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Look.CAN_USE_STRIBOSCOPE);
		CanUse = false;
	}

	public void EnableFor(float period)
	{
		if (CanUse)
		{
			Enabled = true;
			EndTotal = Time.time + period;
		}
	}

	public void ManualUpdate()
	{
		if (!Enabled)
		{
			return;
		}
		if (EndTotal < Time.time)
		{
			Enabled = false;
		}
		else if (NextSwitch < Time.time)
		{
			NextSwitch = Time.time + 0.1f;
			Switcher = !Switcher;
			if (Switcher)
			{
				Light.TurnOn(anyTime: true);
			}
			else
			{
				Light.TurnOff(dependceOnEnemy: false, anyTime: true);
			}
		}
	}
}
