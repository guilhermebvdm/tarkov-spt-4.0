using System;
using UnityEngine;

public class GClass1461
{
	public static bool HierarchyIsValid(Transform[] bones)
	{
		int num = 1;
		while (true)
		{
			if (num < bones.Length)
			{
				if (!IsAncestor(bones[num], bones[num - 1]))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public static UnityEngine.Object ContainsDuplicate(UnityEngine.Object[] objects)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			for (int j = 0; j < objects.Length; j++)
			{
				if (i != j && objects[i] == objects[j])
				{
					return objects[i];
				}
			}
		}
		return null;
	}

	public static bool IsAncestor(Transform transform, Transform ancestor)
	{
		if (transform == null)
		{
			return true;
		}
		if (ancestor == null)
		{
			return true;
		}
		if (transform.parent == null)
		{
			return false;
		}
		if (transform.parent == ancestor)
		{
			return true;
		}
		return IsAncestor(transform.parent, ancestor);
	}

	public static bool ContainsChild(Transform transform, Transform child)
	{
		if (transform == child)
		{
			return true;
		}
		Transform[] componentsInChildren = transform.GetComponentsInChildren<Transform>();
		int num = 0;
		while (true)
		{
			if (num < componentsInChildren.Length)
			{
				if (componentsInChildren[num] == child)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static void AddAncestors(Transform transform, Transform blocker, ref Transform[] array)
	{
		if (transform.parent != null && transform.parent != blocker)
		{
			if (transform.parent.position != transform.position && transform.parent.position != blocker.position)
			{
				Array.Resize(ref array, array.Length + 1);
				array[array.Length - 1] = transform.parent;
			}
			AddAncestors(transform.parent, blocker, ref array);
		}
	}

	public static Transform GetAncestor(Transform transform, int minChildCount)
	{
		if (transform == null)
		{
			return null;
		}
		if (transform.parent != null)
		{
			if (transform.parent.childCount >= minChildCount)
			{
				return transform.parent;
			}
			return GetAncestor(transform.parent, minChildCount);
		}
		return null;
	}

	public static Transform GetFirstCommonAncestor(Transform t1, Transform t2)
	{
		if (t1 == null)
		{
			return null;
		}
		if (t2 == null)
		{
			return null;
		}
		if (t1.parent == null)
		{
			return null;
		}
		if (t2.parent == null)
		{
			return null;
		}
		if (IsAncestor(t2, t1.parent))
		{
			return t1.parent;
		}
		return GetFirstCommonAncestor(t1.parent, t2);
	}

	public static Transform GetFirstCommonAncestor(Transform[] transforms)
	{
		if (transforms == null)
		{
			Debug.LogWarning("Transforms is null.");
			return null;
		}
		if (transforms.Length == 0)
		{
			Debug.LogWarning("Transforms.Length is 0.");
			return null;
		}
		int num = 0;
		while (true)
		{
			if (num < transforms.Length)
			{
				if (!(transforms[num] == null))
				{
					if (IsCommonAncestor(transforms[num], transforms))
					{
						break;
					}
					num++;
					continue;
				}
				return null;
			}
			return GetFirstCommonAncestorRecursive(transforms[0], transforms);
		}
		return transforms[num];
	}

	public static Transform GetFirstCommonAncestorRecursive(Transform transform, Transform[] transforms)
	{
		if (transform == null)
		{
			Debug.LogWarning("Transform is null.");
			return null;
		}
		if (transforms == null)
		{
			Debug.LogWarning("Transforms is null.");
			return null;
		}
		if (transforms.Length == 0)
		{
			Debug.LogWarning("Transforms.Length is 0.");
			return null;
		}
		if (IsCommonAncestor(transform, transforms))
		{
			return transform;
		}
		if (transform.parent == null)
		{
			return null;
		}
		return GetFirstCommonAncestorRecursive(transform.parent, transforms);
	}

	public static bool IsCommonAncestor(Transform transform, Transform[] transforms)
	{
		if (transform == null)
		{
			Debug.LogWarning("Transform is null.");
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < transforms.Length)
			{
				if (!(transforms[num] == null))
				{
					if (!IsAncestor(transforms[num], transform) && transforms[num] != transform)
					{
						break;
					}
					num++;
					continue;
				}
				Debug.Log("Transforms[" + num + "] is null.");
				return false;
			}
			return true;
		}
		return false;
	}
}
