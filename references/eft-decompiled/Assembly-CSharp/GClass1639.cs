using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public class GClass1639<T> : GInterface156<T>, IEnumerable<T>, IEnumerable
{
	[Serializable]
	[CompilerGenerated]
	public class Class1038
	{
		public static readonly Class1038 class1038_0 = new Class1038();

		public static Func<GInterface156<T>, IEnumerable<T>> func_0;

		public IEnumerable<T> method_0(GInterface156<T> x)
		{
			return x;
		}
	}

	[NonSerialized]
	public IEnumerable<T> Ienumerable_0;

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

	public GClass1639(IEnumerable<GInterface156<T>> lists)
	{
		Ienumerable_0 = lists.SelectMany((GInterface156<T> x) => x);
		foreach (GInterface156<T> list in lists)
		{
			list.ItemAdded += delegate(T item)
			{
				Action_0?.Invoke(item);
			};
			list.ItemRemoved += delegate(T item)
			{
				Action_1?.Invoke(item);
			};
			list.ItemsAdded += delegate(IEnumerable<T> items)
			{
				Action_2?.Invoke(items);
			};
			list.ItemsRemoved += delegate(IEnumerable<T> items)
			{
				Action_3?.Invoke(items);
			};
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Ienumerable_0.GetEnumerator();
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
		Action_0?.Invoke(item);
	}

	[CompilerGenerated]
	public void method_1(T item)
	{
		Action_1?.Invoke(item);
	}

	[CompilerGenerated]
	public void method_2(IEnumerable<T> items)
	{
		Action_2?.Invoke(items);
	}

	[CompilerGenerated]
	public void method_3(IEnumerable<T> items)
	{
		Action_3?.Invoke(items);
	}
}
