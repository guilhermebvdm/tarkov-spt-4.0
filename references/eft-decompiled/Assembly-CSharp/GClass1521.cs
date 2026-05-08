using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public abstract class GClass1521
{
	[CompilerGenerated]
	public class Class1021<T, U>
	{
		public IProgress<U> progress;

		public Func<T, U> selector;

		public void method_0(T x)
		{
			progress.Report(selector(x));
		}
	}

	[CanBeNull]
	public static IProgress<TResult> Select<TResult, TSource>([CanBeNull] this IProgress<TSource> progress, Func<TResult, TSource> selector)
	{
		if (progress != null)
		{
			return new GClass1519<TResult>(delegate(TResult x)
			{
				progress.Report(selector(x));
			});
		}
		return null;
	}
}
