using System;
using System.Collections.Generic;

public abstract class GClass1627
{
	public static GInterface156<TSource> BindWhere<TSource>(this GInterface156<TSource> source, Predicate<TSource> predicate)
	{
		return new GClass1656<TSource>(source, predicate);
	}

	public static GInterface156<GClass1654<TSource>> Counted<TSource, TKey>(this GInterface156<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new GClass1655<TSource, TKey>(source, keySelector);
	}

	public static GInterface156<TSource> Merge<TSource>(this IEnumerable<GInterface156<TSource>> lists)
	{
		return new GClass1639<TSource>(lists);
	}
}
