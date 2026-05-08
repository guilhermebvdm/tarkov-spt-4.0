using System;
using EFT;
using UnityEngine;

public class BotHealingBySomebody : GClass429
{
	[NonSerialized]
	public bool HealInProcess;

	[field: NonSerialized]
	public float StartWaitHeal { get; set; }

	public BotHealingBySomebody(BotOwner owner)
		: base(owner)
	{
	}

	public void StartDoHeal()
	{
		HealInProcess = true;
	}

	public void StartWait()
	{
		StartWaitHeal = Time.time;
	}

	public void ManualUpdate()
	{
	}

	public void method_0()
	{
		HealInProcess = false;
	}
}
