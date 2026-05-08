using System;
using System.Runtime.CompilerServices;
using System.Threading;

public class OnEventClass<T> : GInterface152<T>
{
	[CompilerGenerated]
	public class Class1036
	{
		public global::OnEventClass<T> gclass1626_0;

		public Action<T> handler;

		public void method_0()
		{
			gclass1626_0.Event_0 -= handler;
		}
	}

	[CompilerGenerated]
	public class Class1037
	{
		public global::OnEventClass<T> gclass1626_0;

		public Action<T> handler;

		public void method_0()
		{
			gclass1626_0.Event_0 -= handler;
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public Action<T> Action_0;

	public event Action<T> Event_0
	{
		[CompilerGenerated]
		add
		{
			Action<T> action = Action_0;
			Action<T> action2;
			do
			{
				action2 = action;
				Action<T> value2 = (Action<T>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<T> action = Action_0;
			Action<T> action2;
			do
			{
				action2 = action;
				Action<T> value2 = (Action<T>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public Action Bind(Action<T> handler, T value)
	{
		Event_0 += handler;
		handler(value);
		return delegate
		{
			Event_0 -= handler;
		};
	}

	public Action Subscribe(Action<T> handler)
	{
		Event_0 += handler;
		return delegate
		{
			Event_0 -= handler;
		};
	}

	public void Invoke(T arg)
	{
		Action_0?.Invoke(arg);
	}
}
