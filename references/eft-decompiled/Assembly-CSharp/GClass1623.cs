using System;
using System.Collections;
using System.Collections.Generic;

public class GClass1623<T, U> : GInterface156<U>, IEnumerable<U>, IEnumerable
{
	[NonSerialized]
	public GClass1624<T, U> Gclass1624_0;

	public event Action<U> ItemAdded
	{
		add
		{
			Gclass1624_0.ItemAdded += value;
		}
		remove
		{
			Gclass1624_0.ItemAdded -= value;
		}
	}

	public event Action<U> ItemRemoved
	{
		add
		{
			Gclass1624_0.ItemRemoved += value;
		}
		remove
		{
			Gclass1624_0.ItemRemoved -= value;
		}
	}

	public event Action<IEnumerable<U>> ItemsAdded
	{
		add
		{
			Gclass1624_0.ItemsAdded += value;
		}
		remove
		{
			Gclass1624_0.ItemsAdded -= value;
		}
	}

	public event Action<IEnumerable<U>> ItemsRemoved
	{
		add
		{
			Gclass1624_0.ItemsRemoved += value;
		}
		remove
		{
			Gclass1624_0.ItemsRemoved -= value;
		}
	}

	public event Action AllItemsRemoved
	{
		add
		{
			Gclass1624_0.AllItemsRemoved += value;
		}
		remove
		{
			Gclass1624_0.AllItemsRemoved -= value;
		}
	}

	public GClass1623(GClass1624<T, U> dictionary)
	{
		Gclass1624_0 = dictionary;
	}

	public IEnumerator<U> GetEnumerator()
	{
		return Gclass1624_0.Values.GetEnumerator();
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
