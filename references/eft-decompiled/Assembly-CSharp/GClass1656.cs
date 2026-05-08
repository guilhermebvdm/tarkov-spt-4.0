using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public class GClass1656<T> : GInterface156<T>, IEnumerable<T>, IEnumerable
{
	[NonSerialized]
	[CompilerGenerated]
	public Action<T> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<T> Action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Action<IEnumerable<T>> Action_2;

	[NonSerialized]
	[CompilerGenerated]
	public Action<IEnumerable<T>> Action_3;

	[NonSerialized]
	[CompilerGenerated]
	public Action Action_4;

	[NonSerialized]
	public Predicate<T> Predicate_0;

	[NonSerialized]
	public GInterface156<T> Ginterface156_0;

	public event Action<T> ItemAdded
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

	public event Action<T> ItemRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<T> action = Action_1;
			Action<T> action2;
			do
			{
				action2 = action;
				Action<T> value2 = (Action<T>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<T> action = Action_1;
			Action<T> action2;
			do
			{
				action2 = action;
				Action<T> value2 = (Action<T>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEnumerable<T>> ItemsAdded
	{
		[CompilerGenerated]
		add
		{
			Action<IEnumerable<T>> action = Action_2;
			Action<IEnumerable<T>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<T>> value2 = (Action<IEnumerable<T>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEnumerable<T>> action = Action_2;
			Action<IEnumerable<T>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<T>> value2 = (Action<IEnumerable<T>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEnumerable<T>> ItemsRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<IEnumerable<T>> action = Action_3;
			Action<IEnumerable<T>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<T>> value2 = (Action<IEnumerable<T>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEnumerable<T>> action = Action_3;
			Action<IEnumerable<T>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<T>> value2 = (Action<IEnumerable<T>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action AllItemsRemoved
	{
		[CompilerGenerated]
		add
		{
			Action action = Action_4;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = Action_4;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass1656(GInterface156<T> source, Predicate<T> predicate)
	{
		Predicate_0 = predicate;
		Ginterface156_0 = source;
		source.ItemAdded += delegate(T item)
		{
			if (Predicate_0(item))
			{
				Action_0?.Invoke(item);
			}
		};
		source.ItemsAdded += delegate(IEnumerable<T> items)
		{
			IEnumerable<T> obj = items.Where((T item) => Predicate_0(item));
			Action_2?.Invoke(obj);
		};
		source.ItemRemoved += delegate(T item)
		{
			if (Predicate_0(item))
			{
				Action_1?.Invoke(item);
			}
		};
		source.ItemsRemoved += delegate(IEnumerable<T> items)
		{
			IEnumerable<T> enumerable = items.Where((T item) => Predicate_0(item));
			if (enumerable.Any())
			{
				Action_3?.Invoke(enumerable);
			}
		};
		source.AllItemsRemoved += delegate
		{
			Action_4?.Invoke();
		};
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Ginterface156_0.Where((T x) => Predicate_0(x)).GetEnumerator();
	}

	public IEnumerator GetEnumerator_1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}

	[CompilerGenerated]
	public void method_0(T item)
	{
		if (Predicate_0(item))
		{
			Action_0?.Invoke(item);
		}
	}

	[CompilerGenerated]
	public void method_1(IEnumerable<T> items)
	{
		IEnumerable<T> obj = items.Where((T item) => Predicate_0(item));
		Action_2?.Invoke(obj);
	}

	[CompilerGenerated]
	public bool method_2(T item)
	{
		return Predicate_0(item);
	}

	[CompilerGenerated]
	public void method_3(T item)
	{
		if (Predicate_0(item))
		{
			Action_1?.Invoke(item);
		}
	}

	[CompilerGenerated]
	public void method_4(IEnumerable<T> items)
	{
		IEnumerable<T> enumerable = items.Where((T item) => Predicate_0(item));
		if (enumerable.Any())
		{
			Action_3?.Invoke(enumerable);
		}
	}

	[CompilerGenerated]
	public bool method_5(T item)
	{
		return Predicate_0(item);
	}

	[CompilerGenerated]
	public void method_6()
	{
		Action_4?.Invoke();
	}

	[CompilerGenerated]
	public bool method_7(T x)
	{
		return Predicate_0(x);
	}
}
