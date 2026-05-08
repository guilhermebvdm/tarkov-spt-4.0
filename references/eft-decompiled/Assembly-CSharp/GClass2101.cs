using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AnimationEventSystem;
using EFT;

public class GClass2101 : GClass2097, IVaultingSoundsEvents
{
	[CompilerGenerated]
	private Action<IAnimatorEventParameter> action_0;

	[CompilerGenerated]
	private Action<IAnimatorEventParameter> action_1;

	[CompilerGenerated]
	private Action<IAnimatorEventParameter> action_2;

	[CompilerGenerated]
	private Action<IAnimatorEventParameter> action_3;

	public event Action<IAnimatorEventParameter> OnStartEvent
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

	public event Action<IAnimatorEventParameter> OnEndEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IAnimatorEventParameter> action = action_1;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IAnimatorEventParameter> action = action_1;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IAnimatorEventParameter> OnGrabEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IAnimatorEventParameter> action = action_2;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IAnimatorEventParameter> action = action_2;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IAnimatorEventParameter> OnSlideEvent
	{
		[CompilerGenerated]
		add
		{
			Action<IAnimatorEventParameter> action = action_3;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IAnimatorEventParameter> action = action_3;
			Action<IAnimatorEventParameter> action2;
			do
			{
				action2 = action;
				Action<IAnimatorEventParameter> value2 = (Action<IAnimatorEventParameter>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass2101()
	{
		Dictionary_0 = new Dictionary<int, Action<IAnimatorEventParameter>>
		{
			{ -1499280491, method_0 },
			{ 202360014, method_1 },
			{ 105020291, method_2 },
			{ -612157090, method_3 }
		};
	}

	public void method_0(IAnimatorEventParameter param)
	{
		action_0?.Invoke(param);
	}

	public void method_1(IAnimatorEventParameter param)
	{
		action_1?.Invoke(param);
	}

	public void method_2(IAnimatorEventParameter param)
	{
		action_2?.Invoke(param);
	}

	public void method_3(IAnimatorEventParameter param)
	{
		action_3?.Invoke(param);
	}
}
