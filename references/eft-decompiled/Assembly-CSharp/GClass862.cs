using System;
using System.Collections.Generic;
using Comfort.Common;
using UnityEngine;

public abstract class GClass862
{
	[NonSerialized]
	public static HashSet<Type> HashSet_0 = new HashSet<Type>();

	public static void RegisterInSystem<T>(T component) where T : GInterface31
	{
		if (!Singleton<GInterface32<T>>.Instantiated)
		{
			Type typeFromHandle = typeof(T);
			if (!HashSet_0.Contains(typeFromHandle))
			{
				HashSet_0.Add(typeFromHandle);
				Debug.LogError("System " + typeof(T).Name + " NOT instantiated!!!");
				return;
			}
		}
		Singleton<GInterface32<T>>.Instance.Register(component);
	}

	public static void UnregisterInSystem<T>(T component) where T : GInterface31
	{
		if (Singleton<GInterface32<T>>.Instantiated)
		{
			Singleton<GInterface32<T>>.Instance.Unregister(component);
		}
	}
}
