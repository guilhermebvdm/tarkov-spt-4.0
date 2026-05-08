using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public class GClass1655<T, U> : GInterface156<GClass1654<T>>, IEnumerable<GClass1654<T>>, IEnumerable
{
	[CompilerGenerated]
	public class Class1047
	{
		public Func<T, U> keySelector;

		public GClass1655<T, U> gclass1655_0;

		public void method_0(T element)
		{
			U key = keySelector(element);
			if (gclass1655_0.Dictionary_0.TryGetValue(key, out var value))
			{
				value.Count.Value++;
				return;
			}
			Dictionary<U, GClass1654<T>> dictionary_ = gclass1655_0.Dictionary_0;
			GClass1654<T> obj = new GClass1654<T>
			{
				Value = element,
				Count = new global::BindableStateClass<int>(1)
			};
			value = obj;
			dictionary_[key] = obj;
			gclass1655_0.Action_0?.Invoke(value);
		}

		public void method_1(T element)
		{
			U key = keySelector(element);
			if (gclass1655_0.Dictionary_0.TryGetValue(key, out var value))
			{
				int num = value.Count.Value - 1;
				if (num == 0)
				{
					gclass1655_0.Dictionary_0.Remove(key);
					gclass1655_0.Action_1?.Invoke(value);
				}
				else
				{
					value.Count.Value = num;
				}
				return;
			}
			throw new Exception("this should never happen");
		}

		public void method_2(IEnumerable<T> elements)
		{
			foreach (T element in elements)
			{
				method_0(element);
			}
		}

		public void method_3(IEnumerable<T> elements)
		{
			foreach (T element in elements)
			{
				method_1(element);
			}
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public Action<GClass1654<T>> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<GClass1654<T>> Action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Action<IEnumerable<GClass1654<T>>> Action_2;

	[NonSerialized]
	[CompilerGenerated]
	public Action<IEnumerable<GClass1654<T>>> Action_3;

	[NonSerialized]
	[CompilerGenerated]
	public Action Action_4;

	[NonSerialized]
	public Dictionary<U, GClass1654<T>> Dictionary_0;

	public event Action<GClass1654<T>> ItemAdded
	{
		[CompilerGenerated]
		add
		{
			Action<GClass1654<T>> action = Action_0;
			Action<GClass1654<T>> action2;
			do
			{
				action2 = action;
				Action<GClass1654<T>> value2 = (Action<GClass1654<T>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GClass1654<T>> action = Action_0;
			Action<GClass1654<T>> action2;
			do
			{
				action2 = action;
				Action<GClass1654<T>> value2 = (Action<GClass1654<T>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<GClass1654<T>> ItemRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<GClass1654<T>> action = Action_1;
			Action<GClass1654<T>> action2;
			do
			{
				action2 = action;
				Action<GClass1654<T>> value2 = (Action<GClass1654<T>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<GClass1654<T>> action = Action_1;
			Action<GClass1654<T>> action2;
			do
			{
				action2 = action;
				Action<GClass1654<T>> value2 = (Action<GClass1654<T>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEnumerable<GClass1654<T>>> ItemsAdded
	{
		[CompilerGenerated]
		add
		{
			Action<IEnumerable<GClass1654<T>>> action = Action_2;
			Action<IEnumerable<GClass1654<T>>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<GClass1654<T>>> value2 = (Action<IEnumerable<GClass1654<T>>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEnumerable<GClass1654<T>>> action = Action_2;
			Action<IEnumerable<GClass1654<T>>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<GClass1654<T>>> value2 = (Action<IEnumerable<GClass1654<T>>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEnumerable<GClass1654<T>>> ItemsRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<IEnumerable<GClass1654<T>>> action = Action_3;
			Action<IEnumerable<GClass1654<T>>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<GClass1654<T>>> value2 = (Action<IEnumerable<GClass1654<T>>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEnumerable<GClass1654<T>>> action = Action_3;
			Action<IEnumerable<GClass1654<T>>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<GClass1654<T>>> value2 = (Action<IEnumerable<GClass1654<T>>>)Delegate.Remove(action2, value);
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

	public GClass1655(GInterface156<T> source, Func<T, U> keySelector)
	{
		Class1047 CS_0024_003C_003E8__locals13 = new Class1047();
		CS_0024_003C_003E8__locals13.keySelector = keySelector;
		base._002Ector();
		CS_0024_003C_003E8__locals13.gclass1655_0 = this;
		Dictionary_0 = new Dictionary<U, GClass1654<T>>();
		foreach (T item in source)
		{
			U key = CS_0024_003C_003E8__locals13.keySelector(item);
			if (Dictionary_0.TryGetValue(key, out var value))
			{
				value.Count.Value++;
				continue;
			}
			Dictionary_0[key] = new GClass1654<T>
			{
				Value = item,
				Count = new global::BindableStateClass<int>(1)
			};
		}
		source.ItemAdded += delegate(T element)
		{
			U key2 = CS_0024_003C_003E8__locals13.keySelector(element);
			if (CS_0024_003C_003E8__locals13.gclass1655_0.Dictionary_0.TryGetValue(key2, out var value2))
			{
				value2.Count.Value++;
			}
			else
			{
				Dictionary<U, GClass1654<T>> dictionary_ = CS_0024_003C_003E8__locals13.gclass1655_0.Dictionary_0;
				GClass1654<T> obj = new GClass1654<T>
				{
					Value = element,
					Count = new global::BindableStateClass<int>(1)
				};
				value2 = obj;
				dictionary_[key2] = obj;
				CS_0024_003C_003E8__locals13.gclass1655_0.Action_0?.Invoke(value2);
			}
		};
		source.ItemRemoved += delegate(T element)
		{
			U key2 = CS_0024_003C_003E8__locals13.keySelector(element);
			if (CS_0024_003C_003E8__locals13.gclass1655_0.Dictionary_0.TryGetValue(key2, out var value2))
			{
				int num = value2.Count.Value - 1;
				if (num == 0)
				{
					CS_0024_003C_003E8__locals13.gclass1655_0.Dictionary_0.Remove(key2);
					CS_0024_003C_003E8__locals13.gclass1655_0.Action_1?.Invoke(value2);
				}
				else
				{
					value2.Count.Value = num;
				}
				return;
			}
			throw new Exception("this should never happen");
		};
		source.ItemsAdded += delegate(IEnumerable<T> elements)
		{
			foreach (T element in elements)
			{
				CS_0024_003C_003E8__locals13.method_0(element);
			}
		};
		source.ItemsRemoved += delegate(IEnumerable<T> elements)
		{
			foreach (T element2 in elements)
			{
				CS_0024_003C_003E8__locals13.method_1(element2);
			}
		};
	}

	public IEnumerator<GClass1654<T>> GetEnumerator()
	{
		return Dictionary_0.Values.GetEnumerator();
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
}
