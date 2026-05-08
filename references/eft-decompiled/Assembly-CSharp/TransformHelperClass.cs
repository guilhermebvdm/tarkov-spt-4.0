using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public abstract class TransformHelperClass
{
	public static void Render(Transform T, Vector3 position, Quaternion rotation, int layer, Camera camera, bool fast = true)
	{
		Renderer[] componentsInChildren = T.GetComponentsInChildren<Renderer>();
		HashSet<Transform> hashSet = new HashSet<Transform>(new Transform[1] { T });
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Transform transform = componentsInChildren[i].transform;
			while (hashSet.Add(transform))
			{
				transform = transform.parent;
			}
		}
		if (fast)
		{
			smethod_1(T, hashSet, position, rotation, layer, camera);
		}
		else
		{
			smethod_0(T, hashSet, Matrix4x4.TRS(position, rotation, Vector3.one), layer, camera);
		}
	}

	public static void smethod_0(Transform t, HashSet<Transform> transforms, Matrix4x4 matrix, int layer, Camera camera)
	{
		if (!transforms.Contains(t))
		{
			return;
		}
		matrix *= Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
		Renderer component = t.GetComponent<Renderer>();
		if ((bool)component)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = component as SkinnedMeshRenderer;
			Mesh sharedMesh;
			Material[] sharedMaterials;
			if ((bool)skinnedMeshRenderer)
			{
				sharedMesh = skinnedMeshRenderer.sharedMesh;
				sharedMaterials = skinnedMeshRenderer.sharedMaterials;
			}
			else
			{
				sharedMesh = t.GetComponent<MeshFilter>().sharedMesh;
				sharedMaterials = component.sharedMaterials;
			}
			int subMeshCount = sharedMesh.subMeshCount;
			for (int i = 0; i < subMeshCount; i++)
			{
				Graphics.DrawMesh(sharedMesh, matrix, sharedMaterials[i], layer, camera, i);
			}
		}
		int childCount = t.childCount;
		for (int j = 0; j < childCount; j++)
		{
			smethod_0(t.GetChild(j), transforms, matrix, layer, camera);
		}
	}

	public static void smethod_1(Transform t, HashSet<Transform> transforms, Vector3 position, Quaternion rotation, int layer, Camera camera)
	{
		if (!transforms.Contains(t))
		{
			return;
		}
		position += rotation * t.localPosition;
		rotation *= t.localRotation;
		Renderer component = t.GetComponent<Renderer>();
		if ((bool)component)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = component as SkinnedMeshRenderer;
			Mesh sharedMesh;
			Material[] sharedMaterials;
			if ((bool)skinnedMeshRenderer)
			{
				sharedMesh = skinnedMeshRenderer.sharedMesh;
				sharedMaterials = skinnedMeshRenderer.sharedMaterials;
			}
			else
			{
				sharedMesh = t.GetComponent<MeshFilter>().sharedMesh;
				sharedMaterials = component.sharedMaterials;
			}
			int subMeshCount = sharedMesh.subMeshCount;
			for (int i = 0; i < subMeshCount; i++)
			{
				Graphics.DrawMesh(sharedMesh, position, rotation, sharedMaterials[i], layer, camera, i);
			}
		}
		int childCount = t.childCount;
		for (int j = 0; j < childCount; j++)
		{
			smethod_1(t.GetChild(j), transforms, position, rotation, layer, camera);
		}
	}

	public static Bounds GetAllBounds(GameObject itemGameObject)
	{
		Renderer[] componentsInChildren = itemGameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		List<Bounds> list = null;
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Bounds bounds = array[i].bounds;
			if (!(bounds.extents == Vector3.zero))
			{
				if (list == null)
				{
					list = new List<Bounds>(componentsInChildren.Length);
				}
				list.Add(bounds);
			}
		}
		if (list != null && list.Count != 0)
		{
			for (int j = 1; j < list.Count; j++)
			{
				list[0].Encapsulate(list[j]);
			}
			return list[0];
		}
		return default(Bounds);
	}

	public static int NumParents(Transform child, Transform parent, int index = 0)
	{
		while (!(child == null) && !(child == parent))
		{
			child = child.parent;
			index = ++index;
		}
		return index;
	}

	public static void LocalRotateAround(this Transform t, Vector3 center, Vector3 eulerRotation)
	{
		Vector3 direction = t.parent.TransformDirection(eulerRotation);
		direction = t.InverseTransformDirection(direction);
		Quaternion quaternion = Quaternion.Euler(direction);
		Vector3 vector = t.localRotation * center;
		Vector3 localPosition = t.localPosition + vector + quaternion * -center;
		t.localPosition = localPosition;
		t.localRotation *= quaternion;
	}

	public static void RotateAround(this Transform t, Vector3 center, Vector3 eulerRotation)
	{
		Quaternion quaternion = Quaternion.Euler(t.TransformDirection(eulerRotation));
		Vector3 vector = t.position - center;
		vector = quaternion * vector;
		Quaternion rotation = t.rotation;
		t.SetPositionAndRotation(center + vector, quaternion * rotation);
	}

	public static void RotateAround(this Transform t, Vector3 center, Vector3 eulerRotation, Quaternion q)
	{
		Quaternion quaternion = Quaternion.Euler(t.TransformDirection(eulerRotation));
		Vector3 vector = t.position - center;
		vector = quaternion * vector;
		Quaternion rotation = t.rotation;
		t.SetPositionAndRotation(center + vector, quaternion * rotation * q);
	}

	public static void LerpPositionAndRotation(this Transform transform, Vector3 newPos, Quaternion newRot, float t)
	{
		if (!(t <= 0f))
		{
			transform.SetPositionAndRotation(Vector3.Lerp(transform.position, newPos, t), Quaternion.Slerp(transform.rotation, newRot, t));
		}
	}

	public static void RotateAround(this Transform t, Vector3 center, Quaternion rot)
	{
		Vector3 vector = t.position - center;
		vector = rot * vector;
		t.position = center + vector;
		Quaternion rotation = t.rotation;
		t.rotation *= Quaternion.Inverse(rotation) * rot * rotation;
	}

	public static void LookAtAround(this Transform t, Vector3 center, Quaternion rot)
	{
		Vector3 vector = -t.InverseTransformPoint(center);
		t.position = center + rot * vector;
		t.rotation = rot;
	}

	public static Vector3 GetPosition(Matrix4x4 m)
	{
		return m.MultiplyPoint(Vector3.zero);
	}

	public static float Random(this Vector2 v, bool randomizeSign = false)
	{
		if (!randomizeSign)
		{
			return UnityEngine.Random.Range(v.x, v.y);
		}
		return UnityEngine.Random.Range(v.x, v.y) * (float)((!(UnityEngine.Random.Range(0f, 1f) > 0.5f)) ? 1 : (-1));
	}

	public static int Random(this Vector2Int v, bool randomizeSign = false)
	{
		if (!randomizeSign)
		{
			return UnityEngine.Random.Range(v.x, v.y);
		}
		return UnityEngine.Random.Range(v.x, v.y) * ((!(UnityEngine.Random.Range(0f, 1f) > 0.5f)) ? 1 : (-1));
	}

	public static float RandomInt(this Vector2 v, bool randomizeSign = false)
	{
		return Mathf.FloorToInt(Random(v, randomizeSign));
	}

	public static Quaternion GetRotation(Matrix4x4 m)
	{
		return Quaternion.LookRotation(m.MultiplyVector(Vector3.forward), m.MultiplyVector(Vector3.up));
	}

	public static void GetPosRotRelativeToParent(Transform parent, Transform child, out Vector3 position, out Quaternion rotation)
	{
		Matrix4x4 matrixRelativeToParent = GetMatrixRelativeToParent(parent, child);
		if (matrixRelativeToParent == Matrix4x4.zero)
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
		}
		else
		{
			rotation = GetRotation(matrixRelativeToParent);
			position = GetPosition(matrixRelativeToParent);
		}
	}

	public static Vector3 GetPositionRelativeToParent(Transform parent, Transform child)
	{
		GetPosRotRelativeToParent(parent, child, out var position, out var _);
		return position;
	}

	public static Quaternion GetRotationRelativeToParent(Transform parent, Transform child)
	{
		GetPosRotRelativeToParent(parent, child, out var _, out var rotation);
		return rotation;
	}

	public static Matrix4x4 GetMatrixRelativeToTransform(Transform parent, Transform my, Transform another)
	{
		Matrix4x4 matrixRelativeToParent = GetMatrixRelativeToParent(parent, another);
		return Matrix4x4.Inverse(GetMatrixRelativeToParent(parent, my)) * matrixRelativeToParent;
	}

	public static Matrix4x4 GetMatrixRelativeToParent(Transform parent, Transform child)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale);
		int num = 0;
		while (true)
		{
			if (num < 100)
			{
				if (!(child.parent == null))
				{
					child = child.parent;
					if (child == parent)
					{
						break;
					}
					matrix4x = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale) * matrix4x;
					num++;
					continue;
				}
				return Matrix4x4.zero;
			}
			return Matrix4x4.zero;
		}
		return matrix4x;
	}

	public static Quaternion LocalIdenity(Transform parent, Transform child)
	{
		Quaternion identity = Quaternion.identity;
		Transform transform = child;
		do
		{
			transform = transform.parent;
			if (!(transform == null))
			{
				identity *= Quaternion.Inverse(transform.localRotation);
				continue;
			}
			Debug.LogError(child?.ToString() + " is not a child of " + parent);
			break;
		}
		while (transform != parent);
		return identity;
	}

	[CanBeNull]
	public static Transform GetMutualTransform(Transform a, Transform b)
	{
		if (a == b)
		{
			return a;
		}
		HashSet<Transform> hashSet = new HashSet<Transform>(new Transform[2] { a, b });
		int num = 0;
		while (true)
		{
			if (num < 100)
			{
				if (a != null)
				{
					if (!hashSet.Add(a.parent))
					{
						return a.parent;
					}
					a = a.parent;
				}
				if (b != null)
				{
					if (!hashSet.Add(b.parent))
					{
						return b.parent;
					}
					b = b.parent;
				}
				else if (a == null)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return null;
	}

	[CanBeNull]
	public static Transform FindTransformRecursive(Transform root, string name, bool ignoreCase = false)
	{
		int childCount = root.childCount;
		int num = 0;
		Transform transform;
		while (true)
		{
			if (num < childCount)
			{
				Transform child = root.GetChild(num);
				transform = ((ignoreCase ? child.name.Equals(name, StringComparison.InvariantCultureIgnoreCase) : (child.name == name)) ? child : FindTransformRecursive(child, name, ignoreCase));
				if ((bool)transform)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return transform;
	}

	[CanBeNull]
	public static Transform FindTransformRecursiveWidthFirst(Transform root, string name)
	{
		int childCount = root.childCount;
		int num = 0;
		Transform child;
		while (true)
		{
			if (num < childCount)
			{
				child = root.GetChild(num);
				if (child.name == name)
				{
					break;
				}
				num++;
				continue;
			}
			int num2 = 0;
			Transform transform;
			while (true)
			{
				if (num2 < childCount)
				{
					transform = FindTransformRecursiveWidthFirst(root.GetChild(num2), name);
					if (transform != null)
					{
						break;
					}
					num2++;
					continue;
				}
				return null;
			}
			return transform;
		}
		return child;
	}

	[CanBeNull]
	public static Transform FindTransformRecursiveContains(Transform root, string name, bool activeInHierarchyOnly = false)
	{
		int childCount = root.childCount;
		int num = 0;
		Transform transform;
		while (true)
		{
			if (num < childCount)
			{
				Transform child = root.GetChild(num);
				if (!activeInHierarchyOnly || child.gameObject.activeInHierarchy)
				{
					transform = (child.name.Contains(name) ? child : FindTransformRecursiveContains(child, name));
					if ((bool)transform)
					{
						break;
					}
				}
				num++;
				continue;
			}
			return null;
		}
		return transform;
	}

	public static List<Transform> FindChildsByNameRecNoneAlloc(Transform root, string name, List<Transform> childs, bool activeInHierarchyOnly = false)
	{
		int childCount = root.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = root.GetChild(i);
			if (!activeInHierarchyOnly || child.gameObject.activeInHierarchy)
			{
				if (child.name.Contains(name))
				{
					childs.Add(child);
				}
				else
				{
					FindChildsByNameRecNoneAlloc(child, name, childs);
				}
			}
		}
		return childs;
	}

	[CanBeNull]
	public static Transform FindTransform(this Transform root, string name)
	{
		return FindTransformRecursive(root, name);
	}

	[CanBeNull]
	public static Transform FindTransformWhere(this Transform root, Func<Transform, bool> where)
	{
		int childCount = root.childCount;
		int num = 0;
		Transform transform;
		while (true)
		{
			if (num < childCount)
			{
				Transform child = root.GetChild(num);
				transform = (where(child) ? child : FindTransformWhere(child, where));
				if ((bool)transform)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return transform;
	}

	[CanBeNull]
	public static Transform FindActiveTransformRecursive(Transform root, string name)
	{
		int childCount = root.childCount;
		int num = 0;
		Transform transform;
		while (true)
		{
			if (num < childCount)
			{
				Transform child = root.GetChild(num);
				transform = ((!(child.name == name) || !child.gameObject.activeInHierarchy) ? FindTransformRecursive(child, name) : child);
				if ((bool)transform)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return transform;
	}

	public static void smethod_2(Transform root, string searchName, List<Transform> aimList)
	{
		int childCount = root.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = root.GetChild(i);
			if (child.name == searchName)
			{
				aimList.Add(child);
			}
			smethod_2(child, searchName, aimList);
		}
	}

	public static int smethod_3(Transform root, string searchName, List<Transform> yourList, bool activeOnly, params string[] ignore)
	{
		int num = 0;
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(root);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			if (transform.name.Contains(searchName) && (!activeOnly || transform.gameObject.activeInHierarchy))
			{
				num++;
				yourList.Add(transform);
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (ignore == null || !ignore.Contains(child.name))
				{
					queue.Enqueue(child);
				}
			}
		}
		return num;
	}

	public static List<Transform> smethod_4(Transform root, string searchName, bool activeOnly, params string[] ignore)
	{
		List<Transform> list = new List<Transform>();
		smethod_3(root, searchName, list, activeOnly, ignore);
		return list;
	}

	public static int smethod_5(Transform root, string searchName, List<Transform> yourList, bool activeOnly, params string[] ignore)
	{
		int num = 0;
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(root);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			if (transform.name.StartsWith(searchName) && (!activeOnly || transform.gameObject.activeInHierarchy))
			{
				num++;
				yourList.Add(transform);
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (ignore == null || !ignore.Contains(child.name))
				{
					queue.Enqueue(child);
				}
			}
		}
		return num;
	}

	public static List<Transform> smethod_6(Transform root, string searchName, bool activeOnly, params string[] ignore)
	{
		List<Transform> list = new List<Transform>();
		smethod_5(root, searchName, list, activeOnly, ignore);
		return list;
	}

	public static void ParentToInitial(Transform transform, Transform parentTransform)
	{
		transform.parent = parentTransform;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;
	}

	public static void SetLayersRecursively(GameObject gameObject, int layer)
	{
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
		gameObject.layer = layer;
	}

	public static void SetLayersRecursively(GameObject gameObject, int layer, params string[] ignore)
	{
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		int mask = LayerMask.GetMask(ignore);
		foreach (Transform transform in componentsInChildren)
		{
			if ((mask & (1 << transform.gameObject.layer)) == 0)
			{
				transform.gameObject.layer = layer;
			}
		}
		gameObject.layer = layer;
	}

	public static void SetLayersRecursively<T>(GameObject gameObject, int layer) where T : Component
	{
		T[] componentsInChildren = gameObject.GetComponentsInChildren<T>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
		gameObject.layer = layer;
	}

	public static void SetLayersRecursively<T>(GameObject gameObject, int layer, params string[] ignore) where T : Component
	{
		T[] componentsInChildren = gameObject.GetComponentsInChildren<T>(includeInactive: true);
		int mask = LayerMask.GetMask(ignore);
		foreach (T val in componentsInChildren)
		{
			if ((mask & (1 << val.gameObject.layer)) == 0)
			{
				val.gameObject.layer = layer;
			}
		}
		gameObject.layer = layer;
	}
}
