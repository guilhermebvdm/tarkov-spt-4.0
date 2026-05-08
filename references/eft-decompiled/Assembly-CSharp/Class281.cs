using System;
using EFT;
using UnityEngine;

public class Class281 : BotRequest
{
	[NonSerialized]
	public float Float_0 = -1f;

	[NonSerialized]
	public const float Float_1 = 5f;

	public override EBotRequestMode RequestMode => EBotRequestMode.Fight;

	public Class281(Player requester)
		: base(requester, BotRequestType.wait)
	{
		EndIfCantExecute = true;
	}

	public override bool CanRequest(BotOwner requester)
	{
		if (requester.Memory.HaveEnemy && requester.Memory.GoalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public override void Take(BotOwner executor)
	{
		Float_0 = Time.time + 5f;
		base.Take(executor);
	}

	public override bool CanProceed()
	{
		if (base.Taken && Float_0 < Time.time && Float_0 > 0f)
		{
			return false;
		}
		if (Executor.Memory.HaveEnemy && Executor.Memory.GoalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return new AICoreActionEndStruct(false);
	}
}
