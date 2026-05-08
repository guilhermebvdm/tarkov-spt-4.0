using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass593 : BotRequest
{
	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public Action Action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
	}

	public override EBotRequestMode RequestMode => EBotRequestMode.Peace;

	public GClass593(Player requester, Vector3 pos, Action completeCallback, Action disposeCallback)
		: base(requester, BotRequestType.goToPoint)
	{
		Action_0 = completeCallback;
		Action_1 = disposeCallback;
		Vector3_0 = pos;
		base.CanExecuteByMyself = false;
		EndIfCantExecute = true;
	}

	public override bool CanProceed()
	{
		if (!(Executor != null))
		{
			return true;
		}
		if (Executor.Memory.GoalEnemy != null && Executor.Memory.GoalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public override void Complete()
	{
		base.Complete();
		if (Action_0 != null)
		{
			Action_0();
		}
	}

	public override bool CanRequest(BotOwner requester)
	{
		return true;
	}

	public override void Dispose()
	{
		base.Dispose();
		if (Action_1 != null)
		{
			Action_1();
		}
	}
}
