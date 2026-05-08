using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Diz.Binding;

public class BindableEvent : IBindableEvent
{
	[CompilerGenerated]
	public class Class1035
	{
		public BindableEvent bindableEvent_0;

		public Action handler;

		public void method_0()
		{
			bindableEvent_0.Event_0 -= handler;
		}
	}

	[CompilerGenerated]
	private Action _onChange;

	public event Action Event_0
	{
		[CompilerGenerated]
		add
		{
			Action action = _onChange;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref _onChange, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = _onChange;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref _onChange, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public Action Subscribe(Action handler)
	{
		Event_0 += handler;
		return delegate
		{
			Event_0 -= handler;
		};
	}

	public Action Bind(Action handler)
	{
		Action result = Subscribe(handler);
		handler();
		return result;
	}

	public void Invoke()
	{
		_onChange?.Invoke();
	}
}
