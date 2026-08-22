using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class Hierarchy
{
	public static bool HierarchyIsValid(Transform[] bones)
	{
		for (int i = 1; i < bones.Length; i++)
		{
			if (!IsAncestor(bones[i], bones[i - 1]))
			{
				return false;
			}
		}
		return true;
	}

	public static Object ContainsDuplicate(Object[] objects)
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
		if ((Object)(object)transform == (Object)null)
		{
			return true;
		}
		if ((Object)(object)ancestor == (Object)null)
		{
			return true;
		}
		if ((Object)(object)transform.parent == (Object)null)
		{
			return false;
		}
		if ((Object)(object)transform.parent == (Object)(object)ancestor)
		{
			return true;
		}
		return IsAncestor(transform.parent, ancestor);
	}

	public static bool ContainsChild(Transform transform, Transform child)
	{
		if ((Object)(object)transform == (Object)(object)child)
		{
			return true;
		}
		Transform[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform val in array)
		{
			if ((Object)(object)val == (Object)(object)child)
			{
				return true;
			}
		}
		return false;
	}

	public static void AddAncestors(Transform transform, Transform blocker, ref Transform[] array)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform.parent != (Object)null && (Object)(object)transform.parent != (Object)(object)blocker)
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
		if ((Object)(object)transform == (Object)null)
		{
			return null;
		}
		if ((Object)(object)transform.parent != (Object)null)
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
		if ((Object)(object)t1 == (Object)null)
		{
			return null;
		}
		if ((Object)(object)t2 == (Object)null)
		{
			return null;
		}
		if ((Object)(object)t1.parent == (Object)null)
		{
			return null;
		}
		if ((Object)(object)t2.parent == (Object)null)
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
			return null;
		}
		if (transforms.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < transforms.Length; i++)
		{
			if ((Object)(object)transforms[i] == (Object)null)
			{
				return null;
			}
			if (IsCommonAncestor(transforms[i], transforms))
			{
				return transforms[i];
			}
		}
		return GetFirstCommonAncestorRecursive(transforms[0], transforms);
	}

	public static Transform GetFirstCommonAncestorRecursive(Transform transform, Transform[] transforms)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return null;
		}
		if (transforms == null)
		{
			return null;
		}
		if (transforms.Length == 0)
		{
			return null;
		}
		if (IsCommonAncestor(transform, transforms))
		{
			return transform;
		}
		if ((Object)(object)transform.parent == (Object)null)
		{
			return null;
		}
		return GetFirstCommonAncestorRecursive(transform.parent, transforms);
	}

	public static bool IsCommonAncestor(Transform transform, Transform[] transforms)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return false;
		}
		for (int i = 0; i < transforms.Length; i++)
		{
			if ((Object)(object)transforms[i] == (Object)null)
			{
				return false;
			}
			if (!IsAncestor(transforms[i], transform) && (Object)(object)transforms[i] != (Object)(object)transform)
			{
				return false;
			}
		}
		return true;
	}
}
