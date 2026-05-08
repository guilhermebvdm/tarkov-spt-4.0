using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Diz.Binding;
using UnityEngine;

public class GClass1628<T> : GInterface155<T>, GInterface156<T>, IEnumerable<T>, IEnumerable, IList<T>, ICollection<T>
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
	[CompilerGenerated]
	public Action<T> Action_5;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public BindableEvent BindableEvent_0 = new BindableEvent();

	public readonly global::BindableStateClass<int> CountChanged = new global::BindableStateClass<int>();

	[NonSerialized]
	public List<T> List_0;

	[NonSerialized]
	public Comparison<T> Comparison_0;

	[NonSerialized]
	public bool Bool_1;

	public bool IsExceptionSafe
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public BindableEvent ItemsChanged
	{
		[CompilerGenerated]
		get
		{
			return BindableEvent_0;
		}
	}

	public int Count => List_0.Count;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			return List_0[index];
		}
		set
		{
			T obj = List_0[index];
			List_0[index] = value;
			method_2(obj);
			method_1(value);
			ItemsChanged.Invoke();
		}
	}

	public T this[T item]
	{
		get
		{
			int num = IndexOf(item);
			if (num == -1)
			{
				return default(T);
			}
			return List_0[num];
		}
		set
		{
			this[IndexOf(value)] = value;
		}
	}

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

	public event Action<T> ItemUpdated
	{
		[CompilerGenerated]
		add
		{
			Action<T> action = Action_5;
			Action<T> action2;
			do
			{
				action2 = action;
				Action<T> value2 = (Action<T>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<T> action = Action_5;
			Action<T> action2;
			do
			{
				action2 = action;
				Action<T> value2 = (Action<T>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass1628()
	{
		List_0 = new List<T>();
	}

	public GClass1628(IEnumerable<T> items)
	{
		List_0 = new List<T>(items);
	}

	public GClass1628(Comparison<T> comparer)
	{
		List_0 = new List<T>();
		Comparison_0 = comparer;
		Bool_1 = comparer != null;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		List_0.CopyTo(array, arrayIndex);
	}

	public void method_0(T item)
	{
		if (Bool_1)
		{
			List_0.Sort(Comparison_0);
			ItemsChanged.Invoke();
			method_6(item);
		}
	}

	public virtual bool Remove(T existingItem)
	{
		if (!List_0.Remove(existingItem))
		{
			return false;
		}
		method_2(existingItem);
		ItemsChanged.Invoke();
		CountChanged.Value = List_0.Count;
		return true;
	}

	public int IndexOf(T item)
	{
		return List_0.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		List_0.Insert(index, item);
		if (Bool_1)
		{
			List_0.Sort(Comparison_0);
		}
		method_1(item);
		ItemsChanged.Invoke();
		CountChanged.Value = List_0.Count;
	}

	public void RemoveAt(int index)
	{
		T obj = List_0[index];
		List_0.RemoveAt(index);
		method_2(obj);
		ItemsChanged.Invoke();
		CountChanged.Value = List_0.Count;
	}

	public virtual void Add(T newItem)
	{
		List_0.Add(newItem);
		if (Bool_1)
		{
			List_0.Sort(Comparison_0);
		}
		method_1(newItem);
		ItemsChanged.Invoke();
		CountChanged.Value = List_0.Count;
	}

	public void AddRange(IEnumerable<T> list)
	{
		T[] array = list.Where((T x) => !List_0.Contains(x)).ToArray();
		List_0.AddRange(array);
		if (Bool_1)
		{
			List_0.Sort(Comparison_0);
		}
		method_3(array);
		ItemsChanged.Invoke();
		CountChanged.Value = List_0.Count;
	}

	public virtual void Clear()
	{
		List<T> list_ = List_0;
		List_0 = new List<T>();
		CountChanged.Value = 0;
		method_4(list_);
		method_5();
		ItemsChanged.Invoke();
	}

	public bool Contains(T item)
	{
		return List_0.Contains(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return List_0.GetEnumerator();
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

	public void method_1(T obj)
	{
		try
		{
			Action_0?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (!IsExceptionSafe)
			{
				throw;
			}
		}
	}

	public void method_2(T obj)
	{
		try
		{
			Action_1?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (!IsExceptionSafe)
			{
				throw;
			}
		}
	}

	public void method_3(IEnumerable<T> obj)
	{
		try
		{
			Action_2?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (!IsExceptionSafe)
			{
				throw;
			}
		}
	}

	public void method_4(IEnumerable<T> obj)
	{
		try
		{
			Action_3?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (!IsExceptionSafe)
			{
				throw;
			}
		}
	}

	public void method_5()
	{
		try
		{
			Action_4?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (!IsExceptionSafe)
			{
				throw;
			}
		}
	}

	public void method_6(T obj)
	{
		try
		{
			Action_5?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if (!IsExceptionSafe)
			{
				throw;
			}
		}
	}

	[CompilerGenerated]
	public bool method_7(T x)
	{
		return !List_0.Contains(x);
	}
}
