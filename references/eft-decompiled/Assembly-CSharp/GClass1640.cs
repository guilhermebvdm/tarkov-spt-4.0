using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Diz.Binding;

public class GClass1640<T> : GInterface153<int, T>, GInterface157<T> where T : IUpdatable<T>
{
	[CompilerGenerated]
	public class Class1039
	{
		public T newItem;

		public bool method_0(KeyValuePair<int, T> x)
		{
			return x.Value.Compare(newItem);
		}
	}

	[CompilerGenerated]
	public class Class1040
	{
		public KeyValuePair<int, T> existingItem;

		public bool method_0(T x)
		{
			return x.Compare(existingItem.Value);
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public Action<int, T> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<int> Action_1;

	[NonSerialized]
	public BindableEvent OnItemsChanged = new BindableEvent();

	[NonSerialized]
	public SortedList<int, T> SortedList_0 = new SortedList<int, T>();

	public IEnumerable<KeyValuePair<int, T>> Items
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<int, T> item in SortedList_0)
			{
				yield return new KeyValuePair<int, T>(num++, item.Value);
			}
		}
	}

	public event Action<int, T> ItemAdded
	{
		[CompilerGenerated]
		add
		{
			Action<int, T> action = Action_0;
			Action<int, T> action2;
			do
			{
				action2 = action;
				Action<int, T> value2 = (Action<int, T>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, T> action = Action_0;
			Action<int, T> action2;
			do
			{
				action2 = action;
				Action<int, T> value2 = (Action<int, T>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int> ItemRemoved
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = Action_1;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = Action_1;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Add(T value)
	{
		int hashCode = value.GetHashCode();
		SortedList_0.Add(hashCode, value);
		Action_0?.Invoke(SortedList_0.IndexOfKey(hashCode), value);
	}

	public bool Remove(T value)
	{
		int hashCode = value.GetHashCode();
		int obj = SortedList_0.IndexOfKey(hashCode);
		if (!SortedList_0.Remove(hashCode))
		{
			return false;
		}
		Action_1?.Invoke(obj);
		return true;
	}

	public void UpdateItems(ICollection<T> newItems)
	{
		foreach (T newItem in newItems)
		{
			KeyValuePair<int, T> keyValuePair = SortedList_0.FirstOrDefault((KeyValuePair<int, T> x) => x.Value.Compare(newItem));
			if (keyValuePair.Value != null)
			{
				keyValuePair.Value.UpdateFromAnotherItem(newItem);
			}
			else
			{
				Add(newItem);
			}
		}
		foreach (KeyValuePair<int, T> existingItem in SortedList_0)
		{
			if (newItems.FirstOrDefault((T x) => x.Compare(existingItem.Value)) == null)
			{
				Remove(existingItem.Value);
			}
		}
		OnItemsChanged.Invoke();
	}

	public T First()
	{
		return SortedList_0.First().Value;
	}

	public T Last()
	{
		return SortedList_0.Last().Value;
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (KeyValuePair<int, T> item in SortedList_0)
		{
			yield return item.Value;
		}
	}

	public bool Contains(T item)
	{
		return SortedList_0.ContainsKey(item.GetHashCode());
	}
}
