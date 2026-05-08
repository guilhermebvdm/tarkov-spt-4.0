using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AnimationEventSystem;
using EFT;

public class GClass2100 : GClass2097, IDropBackPackEvents
{
	[CompilerGenerated]
	private Action<IAnimatorEventParameter> action_0;

	public event Action<IAnimatorEventParameter> OnBackpackDropEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IAnimatorEventParameter> action = action_0;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IAnimatorEventParameter> action = action_0;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass2100()
	{
		Dictionary_0 = new Dictionary<int, Action<IAnimatorEventParameter>> { { 224242560, method_0 } };
	}

	public void method_0(IAnimatorEventParameter param)
	{
		action_0?.Invoke(param);
	}
}
