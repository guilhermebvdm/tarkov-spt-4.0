using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Diz.Binding;

public class GClass1630<T> : GClass1628<T>, GInterface157<T> where T : IUpdatable<T>
{
	[CompilerGenerated]
	public class Class1048
	{
		public T newItem;

		public bool method_0(T x)
		{
			return x.Compare(newItem);
		}
	}

	[CompilerGenerated]
	public class Class1049
	{
		public T existingItem;

		public bool method_0(T x)
		{
			return x.Compare(existingItem);
		}
	}

	public GClass1630()
	{
	}

	public GClass1630(IEnumerable<T> items)
		: base(items)
	{
	}

	public virtual void UpdateItems(ICollection<T> newItems)
	{
		foreach (T newItem in newItems)
		{
			T val = List_0.Find((T x) => x.Compare(newItem));
			if (val != null)
			{
				val.UpdateFromAnotherItem(newItem);
				method_0(val);
			}
			else
			{
				Add(newItem);
			}
		}
		for (int num = List_0.Count - 1; num >= 0; num--)
		{
			T existingItem = List_0[num];
			if (newItems.FirstOrDefault((T x) => x.Compare(existingItem)) == null)
			{
				Remove(existingItem);
			}
		}
		base.ItemsChanged.Invoke();
	}
}
