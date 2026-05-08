using System.Collections.Generic;

public abstract class GClass1517
{
	public static int IndexOf<T>(this IEnumerable<T> enumerable, T value)
	{
		int num = 0;
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		foreach (T item in enumerable)
		{
			if (!equalityComparer.Equals(item, value))
			{
				num++;
				continue;
			}
			return num;
		}
		return -1;
	}
}
