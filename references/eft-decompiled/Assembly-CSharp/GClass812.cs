using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GClass812
{
	[NonSerialized]
	public static HashSet<int> HashSet_0 = new HashSet<int>();

	public static void LogErrorFormat(string format, params object[] args)
	{
		int hashCode = format.GetHashCode();
		if (!HashSet_0.Contains(hashCode))
		{
			Debug.LogErrorFormat(format, args);
			HashSet_0.Add(hashCode);
		}
	}

	public static void LogError(string message)
	{
		int hashCode = message.GetHashCode();
		if (!HashSet_0.Contains(hashCode))
		{
			Debug.LogError(message);
			HashSet_0.Add(hashCode);
		}
	}

	public static void LogWarningFormat(string format, params object[] args)
	{
		int hashCode = format.GetHashCode();
		if (!HashSet_0.Contains(hashCode))
		{
			Debug.LogWarningFormat(format, args);
			HashSet_0.Add(hashCode);
		}
	}

	public static void LogWarning(string message)
	{
		int hashCode = message.GetHashCode();
		if (!HashSet_0.Contains(hashCode))
		{
			Debug.LogWarning(message);
			HashSet_0.Add(hashCode);
		}
	}
}
