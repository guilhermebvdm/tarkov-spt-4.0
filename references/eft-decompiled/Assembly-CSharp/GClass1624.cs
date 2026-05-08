using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Diz.Binding;
using JetBrains.Annotations;

public class GClass1624<T, U> : IDictionary<T, U>, ICollection<KeyValuePair<T, U>>, IEnumerable<KeyValuePair<T, U>>, IEnumerable
{
	[NonSerialized]
	[CompilerGenerated]
	public Action<U> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<U> Action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Action<IEnumerable<U>> Action_2;

	[NonSerialized]
	[CompilerGenerated]
	public Action<IEnumerable<U>> Action_3;

	[NonSerialized]
	[CompilerGenerated]
	public Action Action_4;

	[NonSerialized]
	public BindableEvent BindableEvent_0 = new BindableEvent();

	[NonSerialized]
	public Dictionary<T, U> Dictionary_0;

	[NonSerialized]
	public GClass1623<T, U> Gclass1623_0;

	public ICollection<T> Keys => Dictionary_0.Keys;

	public ICollection<U> Values => Dictionary_0.Values;

	public int Count => Dictionary_0.Count;

	public bool IsReadOnly => false;

	public virtual U this[T key]
	{
		get
		{
			return Dictionary_0[key];
		}
		set
		{
			U value2;
			bool num = Dictionary_0.TryGetValue(key, out value2);
			Dictionary_0[key] = value;
			if (num)
			{
				Action_1?.Invoke(value2);
			}
			Action_0?.Invoke(value);
			BindableEvent_0.Invoke();
		}
	}

	public event Action<U> ItemAdded
	{
		[CompilerGenerated]
		add
		{
			Action<U> action = Action_0;
			Action<U> action2;
			do
			{
				action2 = action;
				Action<U> value2 = (Action<U>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<U> action = Action_0;
			Action<U> action2;
			do
			{
				action2 = action;
				Action<U> value2 = (Action<U>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<U> ItemRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<U> action = Action_1;
			Action<U> action2;
			do
			{
				action2 = action;
				Action<U> value2 = (Action<U>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<U> action = Action_1;
			Action<U> action2;
			do
			{
				action2 = action;
				Action<U> value2 = (Action<U>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEnumerable<U>> ItemsAdded
	{
		[CompilerGenerated]
		add
		{
			Action<IEnumerable<U>> action = Action_2;
			Action<IEnumerable<U>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<U>> value2 = (Action<IEnumerable<U>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEnumerable<U>> action = Action_2;
			Action<IEnumerable<U>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<U>> value2 = (Action<IEnumerable<U>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IEnumerable<U>> ItemsRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<IEnumerable<U>> action = Action_3;
			Action<IEnumerable<U>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<U>> value2 = (Action<IEnumerable<U>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IEnumerable<U>> action = Action_3;
			Action<IEnumerable<U>> action2;
			do
			{
				action2 = action;
				Action<IEnumerable<U>> value2 = (Action<IEnumerable<U>>)Delegate.Remove(action2, value);
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

	public GClass1624()
	{
		Dictionary_0 = new Dictionary<T, U>();
	}

	public GInterface156<U> AsBindableList()
	{
		return Gclass1623_0 ?? (Gclass1623_0 = new GClass1623<T, U>(this));
	}

	public virtual bool Remove(T existingItem)
	{
		if (!Dictionary_0.TryGetValue(existingItem, out var value))
		{
			return false;
		}
		Action_1?.Invoke(value);
		if (!Dictionary_0.Remove(existingItem))
		{
			return false;
		}
		BindableEvent_0.Invoke();
		return true;
	}

	public bool TryGetValue(T key, out U value)
	{
		return Dictionary_0.TryGetValue(key, out value);
	}

	public bool ContainsKey(T key)
	{
		return Dictionary_0.ContainsKey(key);
	}

	public virtual void Add(T key, [CanBeNull] U value)
	{
		Dictionary_0.Add(key, value);
		Action_0?.Invoke(value);
		BindableEvent_0.Invoke();
	}

	public virtual void AddRange(IDictionary<T, U> dict)
	{
		foreach (KeyValuePair<T, U> item in dict.Where((KeyValuePair<T, U> item) => !ContainsKey(item.Key)))
		{
			Dictionary_0.Add(item.Key, item.Value);
		}
		Action_2?.Invoke(dict.Values);
		BindableEvent_0.Invoke();
	}

	public void Add(KeyValuePair<T, U> item)
	{
		Add(item.Key, item.Value);
	}

	public virtual void Clear()
	{
		Action_3?.Invoke(Dictionary_0.Values);
		Action_4?.Invoke();
		BindableEvent_0.Invoke();
		Dictionary_0.Clear();
	}

	public bool Contains(KeyValuePair<T, U> item)
	{
		return Dictionary_0.Contains(item);
	}

	public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<T, U>>)Dictionary_0).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<T, U> item)
	{
		return Remove(item.Key);
	}

	public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
	{
		return Dictionary_0.GetEnumerator();
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
	public bool method_0(KeyValuePair<T, U> item)
	{
		return !ContainsKey(item.Key);
	}
}
