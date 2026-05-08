using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass981
{
	[Serializable]
	[CompilerGenerated]
	public class Class660<T>
	{
		public static readonly Class660<T> class660_0 = new Class660<T>();

		public static Func<Type, bool> func_0;

		public static Func<MonoBehaviour, bool> func_1;

		public static Func<MonoBehaviour, MonoBehaviour> func_2;

		public bool method_0(MonoBehaviour a)
		{
			return a.GetType().GetInterfaces().Any((Type k) => k == typeof(T));
		}

		public bool method_1(Type k)
		{
			return k == typeof(T);
		}

		public MonoBehaviour method_2(MonoBehaviour a)
		{
			return a;
		}
	}

	public static void Toggle(ref bool value, string name)
	{
		GUILayout.BeginHorizontal(new GUIStyle("box"));
		GUILayout.Label(name + " : ");
		value = GUILayout.Toggle(value, "");
		GUILayout.EndHorizontal();
	}

	public static T[] FindObjectsOfType<T>()
	{
		return GClass870.FindUnityObjectsOfType(typeof(T)) as T[];
	}

	public static T[] GetInterfaces<T>(this GameObject gameObject)
	{
		if (!typeof(T).IsInterface)
		{
			throw new SystemException("Specified type is not an interface!");
		}
		return (from a in gameObject.GetComponents<MonoBehaviour>()
			where a.GetType().GetInterfaces().Any((Type k) => k == typeof(T))
			select (a)).ToArray() as T[];
	}

	public static List<Vector3> DedupCollectionWithRandom(List<Vector3> input)
	{
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		foreach (Vector3 item in input)
		{
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item + UnityEngine.Random.insideUnitSphere * 0.0001f);
			}
		}
		return new List<Vector3>(hashSet);
	}

	public static List<Vector3> DedupCollectionWithRandom(List<Vector3> input, IEqualityComparer<Vector3> comparer)
	{
		HashSet<Vector3> hashSet = new HashSet<Vector3>(comparer);
		foreach (Vector3 item in input)
		{
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item + UnityEngine.Random.insideUnitSphere * 0.0001f);
			}
		}
		return new List<Vector3>(hashSet);
	}

	public static List<T> DedupCollection<T>(IEnumerable<T> input)
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T item in input)
		{
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
			}
		}
		return new List<T>(hashSet);
	}
}
