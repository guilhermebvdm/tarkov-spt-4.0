using System;
using System.Collections.Generic;

public abstract class GClass1163
{
	[NonSerialized]
	public static Dictionary<Type, object> Dictionary_0 = new Dictionary<Type, object>();

	public static GClass835<TCalculator> GetOrCreatePool<TCalculator>(int size, Func<TCalculator> constructor, Action<TCalculator> onReturnInPool = null, Action<TCalculator> destructor = null)
	{
		Type typeFromHandle = typeof(TCalculator);
		if (Dictionary_0.TryGetValue(typeFromHandle, out var value))
		{
			return (GClass835<TCalculator>)value;
		}
		GClass835<TCalculator> gClass = new GClass835<TCalculator>(size, constructor, onReturnInPool, destructor);
		Dictionary_0[typeFromHandle] = gClass;
		return gClass;
	}

	public static TCalculator Withdraw<TCalculator>(int size, Func<TCalculator> constructor, Action<TCalculator> onReturnInPool = null, Action<TCalculator> destructor = null)
	{
		return GetOrCreatePool(size, constructor, onReturnInPool, destructor).Withdraw();
	}

	public static void Return<TCalculator>(TCalculator calculator)
	{
		Type typeFromHandle = typeof(TCalculator);
		if (Dictionary_0.TryGetValue(typeFromHandle, out var value))
		{
			((GClass835<TCalculator>)value).Return(calculator);
		}
	}

	public static void PreCreatePool<TCalculator>(int size, Func<TCalculator> constructor, Action<TCalculator> onReturnInPool = null, Action<TCalculator> destructor = null)
	{
		Type typeFromHandle = typeof(TCalculator);
		if (!Dictionary_0.ContainsKey(typeFromHandle))
		{
			GClass835<TCalculator> value = new GClass835<TCalculator>(size, constructor, onReturnInPool, destructor);
			Dictionary_0[typeFromHandle] = value;
		}
	}

	public static void DisposePool<TCalculator>()
	{
		Type typeFromHandle = typeof(TCalculator);
		if (Dictionary_0.TryGetValue(typeFromHandle, out var value))
		{
			((GClass835<TCalculator>)value).Dispose();
			Dictionary_0.Remove(typeFromHandle);
		}
	}

	public static void ClearPools()
	{
		Dictionary_0.Clear();
	}
}
