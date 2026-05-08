using System.Collections.Generic;

public abstract class GClass1663
{
	public static void Release<T>(this IEnumerable<global::DependencyGraphClass<T>.GClass1661> tokens) where T : GInterface161
	{
		foreach (global::DependencyGraphClass<T>.GClass1661 token in tokens)
		{
			token.Release();
		}
	}
}
