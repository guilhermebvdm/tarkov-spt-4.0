using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public abstract class GClass1666<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : class, GInterface163
{
	[NonSerialized]
	public Dictionary<string, T> Dictionary_0 = new Dictionary<string, T>();

	[NonSerialized]
	public List<T> List_0 = new List<T>();

	[NonSerialized]
	[CompilerGenerated]
	public Action<string, GInterface163> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public EventHandler EventHandler_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<T> Action_1;

	public abstract string ItemTypeName { get; }

	public T this[int index] => List_0[index];

	public T this[string name]
	{
		get
		{
			if (!Dictionary_0.TryGetValue(name, out var value))
			{
				throw new Exception2($"A {ItemTypeName} by the name {name} does not exist in the collection");
			}
			return value;
		}
	}

	public int Count => List_0.Count;

	public bool IsReadOnly => false;

	public event Action<string, GInterface163> NameChanged
	{
		[CompilerGenerated]
		add
		{
			Action<string, GInterface163> action = Action_0;
			Action<string, GInterface163> action2;
			do
			{
				action2 = action;
				Action<string, GInterface163> value2 = (Action<string, GInterface163>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string, GInterface163> action = Action_0;
			Action<string, GInterface163> action2;
			do
			{
				action2 = action;
				Action<string, GInterface163> value2 = (Action<string, GInterface163>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event EventHandler OrderChanged
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = EventHandler_0;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref EventHandler_0, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = EventHandler_0;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref EventHandler_0, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
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

	public void Add(T item)
	{
		Insert(List_0.Count, item);
	}

	public bool TryGetIndexByName(string name, out int result)
	{
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				if (List_0[num].Name == name)
				{
					break;
				}
				num++;
				continue;
			}
			result = -1;
			return false;
		}
		result = num;
		return true;
	}

	public void Insert(int index, T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item argument can't be null");
		}
		if (item.Name != null && Dictionary_0.ContainsKey(item.Name))
		{
			throw new Exception1($"A {ItemTypeName} by that name already exists in the data source");
		}
		if (List_0.Contains(item))
		{
			throw new Exception1($"The {ItemTypeName} is already added to the collection");
		}
		List_0.Insert(index, item);
		item.NameChanged += method_0;
		if (item.Name != null)
		{
			Dictionary_0.Add(item.Name, item);
		}
		if (EventHandler_0 != null)
		{
			EventHandler_0(this, EventArgs.Empty);
		}
	}

	public void method_0(string prevName, GInterface163 item)
	{
		if (!(prevName == item.Name))
		{
			if (Dictionary_0.ContainsKey(item.Name))
			{
				item.CancelNameChange();
				throw new Exception1($"A {ItemTypeName} by that name already exists in the data source");
			}
			if (prevName != null && !Dictionary_0.Remove(prevName))
			{
				item.CancelNameChange();
				throw new Exception0($"Renaming {ItemTypeName} failed");
			}
			if (item.Name != null)
			{
				Dictionary_0.Add(item.Name, (T)item);
			}
			if (Action_0 != null)
			{
				Action_0(prevName, item);
			}
		}
	}

	public void Clear()
	{
		foreach (T item in List_0)
		{
			item.NameChanged -= method_0;
			if (Action_1 != null)
			{
				Action_1(item);
			}
		}
		List_0.Clear();
		Dictionary_0.Clear();
		if (EventHandler_0 != null)
		{
			EventHandler_0(this, EventArgs.Empty);
		}
	}

	public bool Contains(T item)
	{
		return List_0.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		List_0.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		if (List_0.Remove(item))
		{
			if (item.Name != null)
			{
				Dictionary_0.Remove(item.Name);
			}
			item.NameChanged -= method_0;
			if (Action_1 != null)
			{
				Action_1(item);
			}
			if (EventHandler_0 != null)
			{
				EventHandler_0(this, EventArgs.Empty);
			}
			return true;
		}
		return false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return List_0.GetEnumerator();
	}

	public IEnumerator GetEnumerator_1()
	{
		return List_0.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}

	public GClass1666()
	{
	}
}
