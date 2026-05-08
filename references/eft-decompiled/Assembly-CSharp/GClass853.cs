using System.Collections.Generic;
using JetBrains.Annotations;

public abstract class GClass853
{
	[MustUseReturnValue]
	public static bool ContainsElement<TCollection, TElement>([CanBeNull] this TCollection collection, TElement element) where TCollection : ICollection<TElement>, new()
	{
		return collection?.Contains(element) ?? false;
	}

	[MustUseReturnValue]
	public static TCollection AddElement<TCollection, TElement>([CanBeNull] this TCollection collection, TElement element) where TCollection : ICollection<TElement>, new()
	{
		if (collection == null)
		{
			collection = new TCollection();
		}
		collection.Add(element);
		return collection;
	}

	[MustUseReturnValue]
	public static TDictionary AddElement<TDictionary, TKey, TValue>([CanBeNull] this TDictionary collection, TKey key, TValue value) where TDictionary : IDictionary<TKey, TValue>, new()
	{
		if (collection == null)
		{
			collection = new TDictionary();
		}
		collection[key] = value;
		return collection;
	}

	[NotNull]
	public static IEnumerable<TElement> EmptyIfNull<TElement>([CanBeNull] this IEnumerable<TElement> collection)
	{
		if (collection == null)
		{
			yield break;
		}
		foreach (TElement item in collection)
		{
			yield return item;
		}
	}
}
