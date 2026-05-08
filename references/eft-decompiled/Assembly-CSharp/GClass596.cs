using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.Interactive;

public class GClass596 : BotRequest
{
	[NonSerialized]
	public Action Action_0;

	[CompilerGenerated]
	private Action<bool> action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Door Door_0;

	public Door Door
	{
		[CompilerGenerated]
		get
		{
			return Door_0;
		}
	}

	public override EBotRequestMode RequestMode => EBotRequestMode.Peace;

	public event Action<bool> OnComplete
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_1;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_1;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass596(Door door, Player requester, Action completeCallback = null)
		: base(requester, BotRequestType.doorOpen)
	{
		Action_0 = completeCallback;
		Door_0 = door;
	}

	public override void PlayerDestroy(Player player)
	{
	}

	public override void Take(BotOwner executor)
	{
		base.Take(executor);
		SetEndTimeOnTake(LocalBotSettingsProviderClass.Core.HOLD_REQUEST_TIME_SEC);
	}

	public override bool CanProceed()
	{
		if (Door.DoorState == EDoorState.Open)
		{
			Complete();
		}
		return true;
	}

	public override void Complete()
	{
		if (action_1 != null)
		{
			action_1(obj: true);
		}
		base.Complete();
	}

	public override bool CanRequest(BotOwner requester)
	{
		return true;
	}

	public override void Dispose()
	{
		if (action_1 != null)
		{
			action_1(obj: false);
		}
		if (Action_0 != null)
		{
			Action_0();
		}
		base.Dispose();
	}
}
