using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public class GClass1629<T, U> : GClass1628<U>, IDisposable
{
	public class Class1045
	{
		public U ResultItem;

		public GClass1628<T> Source;
	}

	public delegate bool GDelegate63(T sourceItem, out U resultItem);

	[CompilerGenerated]
	public class Class1046
	{
		public GClass1629<T, U> gclass1629_0;

		public GClass1628<T> source;

		public void method_0()
		{
			gclass1629_0.method_12(source);
		}

		public void method_1(IEnumerable<T> items)
		{
			gclass1629_0.method_11(source, items);
		}

		public void method_2(T item)
		{
			gclass1629_0.method_8(source, item);
		}

		public void method_3(T item)
		{
			gclass1629_0.method_9(source, item);
		}

		public void method_4()
		{
			source.ItemAdded -= delegate(T item)
			{
				gclass1629_0.method_8(source, item);
			};
			source.ItemRemoved -= gclass1629_0.method_10;
			source.ItemsAdded -= delegate(IEnumerable<T> items)
			{
				gclass1629_0.method_11(source, items);
			};
			source.AllItemsRemoved -= delegate
			{
				gclass1629_0.method_12(source);
			};
			source.ItemUpdated -= delegate(T item)
			{
				gclass1629_0.method_9(source, item);
			};
		}
	}

	[NonSerialized]
	public Dictionary<GClass1628<T>, Action> Dictionary_0 = new Dictionary<GClass1628<T>, Action>();

	[NonSerialized]
	public GDelegate63 Gdelegate63_0;

	[NonSerialized]
	public Dictionary<T, Class1045> Dictionary_1 = new Dictionary<T, Class1045>();

	public GClass1629(GClass1628<T> source, GDelegate63 converter, [CanBeNull] Comparison<U> comparer)
		: base(comparer)
	{
		Gdelegate63_0 = converter;
		AddSource(source);
	}

	public void AddSource(GClass1628<T> source)
	{
		Class1046 CS_0024_003C_003E8__locals32 = new Class1046();
		CS_0024_003C_003E8__locals32.gclass1629_0 = this;
		CS_0024_003C_003E8__locals32.source = source;
		Dictionary_0.Add(CS_0024_003C_003E8__locals32.source, delegate
		{
			CS_0024_003C_003E8__locals32.source.ItemAdded -= delegate(T item)
			{
				CS_0024_003C_003E8__locals32.gclass1629_0.method_8(CS_0024_003C_003E8__locals32.source, item);
			};
			CS_0024_003C_003E8__locals32.source.ItemRemoved -= CS_0024_003C_003E8__locals32.gclass1629_0.method_10;
			CS_0024_003C_003E8__locals32.source.ItemsAdded -= delegate(IEnumerable<T> items)
			{
				CS_0024_003C_003E8__locals32.gclass1629_0.method_11(CS_0024_003C_003E8__locals32.source, items);
			};
			CS_0024_003C_003E8__locals32.source.AllItemsRemoved -= delegate
			{
				CS_0024_003C_003E8__locals32.gclass1629_0.method_12(CS_0024_003C_003E8__locals32.source);
			};
			CS_0024_003C_003E8__locals32.source.ItemUpdated -= delegate(T item)
			{
				CS_0024_003C_003E8__locals32.gclass1629_0.method_9(CS_0024_003C_003E8__locals32.source, item);
			};
		});
		CS_0024_003C_003E8__locals32.method_1(CS_0024_003C_003E8__locals32.source.ToList());
		CS_0024_003C_003E8__locals32.source.ItemAdded += delegate(T item)
		{
			CS_0024_003C_003E8__locals32.gclass1629_0.method_8(CS_0024_003C_003E8__locals32.source, item);
		};
		CS_0024_003C_003E8__locals32.source.ItemRemoved += method_10;
		CS_0024_003C_003E8__locals32.source.ItemsAdded += delegate(IEnumerable<T> items)
		{
			CS_0024_003C_003E8__locals32.gclass1629_0.method_11(CS_0024_003C_003E8__locals32.source, items);
		};
		CS_0024_003C_003E8__locals32.source.AllItemsRemoved += delegate
		{
			CS_0024_003C_003E8__locals32.gclass1629_0.method_12(CS_0024_003C_003E8__locals32.source);
		};
		CS_0024_003C_003E8__locals32.source.ItemUpdated += delegate(T item)
		{
			CS_0024_003C_003E8__locals32.gclass1629_0.method_9(CS_0024_003C_003E8__locals32.source, item);
		};
	}

	public void RemoveSource(GClass1628<T> source)
	{
		if (Dictionary_0.TryGetValue(source, out var value))
		{
			value();
			Dictionary_0.Remove(source);
			method_12(source);
		}
	}

	public void method_8(GClass1628<T> source, T item)
	{
		if (method_13(source, item, out var resultItem))
		{
			Add(resultItem);
		}
	}

	public void method_9(GClass1628<T> source, T item)
	{
		if (method_13(source, item, out var resultItem))
		{
			method_0(resultItem);
		}
	}

	public void method_10(T item)
	{
		U resultItem = Dictionary_1[item].ResultItem;
		Remove(resultItem);
		if (IndexOf(resultItem) < 0)
		{
			Dictionary_1.Remove(item);
		}
	}

	public void method_11(GClass1628<T> source, IEnumerable<T> items)
	{
		List<U> list = new List<U>();
		foreach (T item in items)
		{
			if (method_13(source, item, out var resultItem))
			{
				list.Add(resultItem);
			}
		}
		AddRange(list);
	}

	public void method_12(GClass1628<T> source)
	{
		if (Dictionary_0.Count <= 1 && base.Count == Dictionary_1.Count)
		{
			Dictionary_1.Clear();
			base.Clear();
			return;
		}
		for (int num = Dictionary_1.Count - 1; num >= 0; num--)
		{
			KeyValuePair<T, Class1045> keyValuePair = Dictionary_1.ElementAt(num);
			if (keyValuePair.Value.Source == source)
			{
				method_10(keyValuePair.Key);
			}
		}
	}

	public bool method_13(GClass1628<T> source, T item, out U resultItem)
	{
		if (Dictionary_1.TryGetValue(item, out var value))
		{
			resultItem = value.ResultItem;
			return true;
		}
		if (!Gdelegate63_0(item, out resultItem))
		{
			return false;
		}
		Dictionary_1.Add(item, new Class1045
		{
			Source = source,
			ResultItem = resultItem
		});
		return true;
	}

	public override void Clear()
	{
		foreach (KeyValuePair<GClass1628<T>, Action> item in Dictionary_0)
		{
			item.Value();
		}
		Dictionary_0.Clear();
		Dictionary_1.Clear();
		base.Clear();
	}

	public void Dispose()
	{
		Clear();
	}
}
