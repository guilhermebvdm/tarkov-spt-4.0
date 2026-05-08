using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass1518
{
	[Serializable]
	[CompilerGenerated]
	public class Class1013<T>
	{
		public static readonly Class1013<T> class1013_0 = new Class1013<T>();

		public static Func<IEnumerable<T>, IEnumerable<T>> func_0;

		public IEnumerable<T> method_0(IEnumerable<T> i)
		{
			return i;
		}
	}

	[CompilerGenerated]
	public class Class1014<T>
	{
		public Func<T, IEnumerable<T>> f;

		public IEnumerable<T> method_0(T c)
		{
			return Flatten(f(c), f);
		}
	}

	[CompilerGenerated]
	public class Class1015<T>
	{
		public Func<T, IEnumerable<T>> f;

		public IEnumerable<T> method_0(T c)
		{
			return Flatten(f(c), f);
		}
	}

	[CompilerGenerated]
	public class Class1016<T>
	{
		public Func<T, IEnumerable<T>> f;

		public IEnumerable<T> method_0(T c)
		{
			return FlattenDepthFirst(f(c), f);
		}
	}

	public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> e)
	{
		return e.SelectMany((IEnumerable<T> i) => i);
	}

	public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
	{
		T[] source = e.ToArray();
		return e.Concat(source.SelectMany((T c) => Flatten(f(c), f)));
	}

	public static IEnumerable<T> FlattenWithoutArray<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
	{
		return e.Concat(e.SelectMany((T c) => Flatten(f(c), f)));
	}

	public static IEnumerable<T> FlattenDepthFirst<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
	{
		T[] array = e.ToArray();
		return array.SelectMany((T c) => FlattenDepthFirst(f(c), f)).Concat(array);
	}

	public static IEnumerable<T> ToEnumerable<T>(this T val)
	{
		yield return val;
	}

	public static IEnumerable<T> Recurse<T>([CanBeNull] this T value, Func<T, T> recurse, Func<T, bool> condition)
	{
		while (condition(value))
		{
			yield return value;
			value = recurse(value);
		}
	}

	public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
	{
		HashSet<TKey> hashSet = new HashSet<TKey>();
		foreach (T item2 in source)
		{
			TKey item = keySelector(item2);
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
				yield return item2;
			}
		}
	}

	public static T Random<T>(this IEnumerable<T> source)
	{
		T[] array = (source as T[]) ?? source.ToArray();
		return array[UnityEngine.Random.Range(0, array.Length)];
	}
}
