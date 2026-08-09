using System;
using System.Collections;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Dismemberment.Classes;

public class Utils
{
	public static T GetComponentInParentRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		Transform val = obj.transform;
		while ((Object)(object)val != (Object)null)
		{
			T component = ((Component)val).GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
			val = val.parent;
		}
		return default(T);
	}

	public static List<T> GetComponentsInParentRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		Transform val = obj.transform;
		while ((Object)(object)val != (Object)null)
		{
			T[] components = ((Component)val).GetComponents<T>();
			if (components.Length != 0)
			{
				list.AddRange(components);
			}
			val = val.parent;
		}
		return list;
	}

	public static T GetComponentInChildRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		Transform transform = obj.transform;
		T component = ((Component)transform).GetComponent<T>();
		if ((Object)(object)component != (Object)null)
		{
			return component;
		}
		foreach (Transform item in transform)
		{
			Transform val = item;
			component = GetComponentInChildRecursive<T>(((Component)val).gameObject);
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return default(T);
	}

	public static List<T> GetComponentsInChildRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		Transform transform = obj.transform;
		T[] components = ((Component)transform).GetComponents<T>();
		if (components.Length != 0)
		{
			list.AddRange(components);
		}
		foreach (Transform item in transform)
		{
			Transform val = item;
			list.AddRange(GetComponentsInChildRecursive<T>(((Component)val).gameObject));
		}
		return list;
	}

	public static List<Collider> GetCollidersInChildRecursive(GameObject obj)
	{
		List<Collider> list = new List<Collider>();
		Transform transform = obj.transform;
		Collider[] components = ((Component)transform).GetComponents<Collider>();
		if (components.Length != 0)
		{
			list.AddRange(components);
		}
		foreach (Transform item in transform)
		{
			Transform val = item;
			list.AddRange(GetCollidersInChildRecursive(((Component)val).gameObject));
		}
		return list;
	}

	public static bool CheckNameInHierarchyRecursive(GameObject obj, string word)
	{
		if (((Object)obj).name.Contains(word))
		{
			return true;
		}
		Transform val = obj.transform;
		while ((Object)(object)val != (Object)null)
		{
			if (((Object)val).name.Contains(word))
			{
				return true;
			}
			val = val.parent;
		}
		foreach (Transform item in obj.transform)
		{
			Transform val2 = item;
			if (CheckNameInHierarchyRecursive(((Component)val2).gameObject, word))
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
	{
		if (root == null) yield break;
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(root);
		while (queue.Count > 0)
		{
			Transform current = queue.Dequeue();
			if (current == null) continue;
			for (int i = 0; i < current.childCount; i++)
			{
				queue.Enqueue(current.GetChild(i));
			}
			yield return current;
		}
	}

	public static bool ParentContains(Transform t, string name)
	{
		while ((Object)(object)t.parent != (Object)null)
		{
			t = t.parent;
			if (((Object)t).name.Contains(name))
			{
				return true;
			}
		}
		return false;
	}

	public static GameObject GetRootGameObject(GameObject obj)
	{
		while ((Object)(object)obj.transform.parent != (Object)null)
		{
			obj = ((Component)obj.transform.parent).gameObject;
		}
		return obj;
	}

	public static Player GetPlayerFromRootGameObject(GameObject obj)
	{
		return GetRootGameObject(obj).GetComponentInChildren<Player>();
	}
}
