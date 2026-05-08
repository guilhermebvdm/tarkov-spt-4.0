using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public abstract class GClass842
{
	public abstract class GClass843
	{
		public static Color ToRTTColor(double rtt)
		{
			if (!(rtt < 0.0) && rtt <= 120.0)
			{
				if (!(rtt <= 80.0))
				{
					return YELLOW_COLOR;
				}
				return GREEN_COLOR;
			}
			return RED_COLOR;
		}

		public static Color ToLossColor(int loss)
		{
			switch (loss)
			{
			case 1:
			case 2:
				return YELLOW_COLOR;
			case 0:
				return GREEN_COLOR;
			default:
				return RED_COLOR;
			}
		}

		public static Color ToPlayerRTTColor(double rtt)
		{
			if (!(rtt < 0.0) && rtt <= 120.0)
			{
				if (!(rtt <= 60.0))
				{
					return YELLOW_COLOR;
				}
				return GREEN_COLOR;
			}
			return RED_COLOR;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class459<T> where T : Component
	{
		public static readonly Class459<T> class459_0 = new Class459<T>();

		public static Func<Scene, bool> func_0;

		public static Func<Scene, IEnumerable<GameObject>> func_1;

		public bool method_0(Scene s)
		{
			return s.isLoaded;
		}

		public IEnumerable<GameObject> method_1(Scene s)
		{
			return s.GetRootGameObjects();
		}
	}

	public static bool False = false;

	public static bool True = true;

	public static bool DisabledForNow = true;

	public static readonly Vector2 Vector2Zero = Vector2.zero;

	public static readonly Vector3 Vector3Zero = Vector3.zero;

	public static readonly Vector4 Vector4Zero = Vector4.zero;

	public static readonly Vector3 Vector3Up = Vector3.up;

	public static readonly Vector3 Vector3Down = Vector3.down;

	public static readonly Color WHITE_COLOR = Color.white;

	public static readonly Color GREEN_COLOR = Color.green;

	public static readonly Color YELLOW_COLOR = Color.yellow;

	public static readonly Color RED_COLOR = Color.red;

	public static void ClearLog()
	{
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public static float ClampAngle360Sensitive(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		float num = ((angle > 0f) ? (angle - 360f) : (angle + 360f));
		if (num > min && num < max)
		{
			return angle;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public static float ZipAngle(float angle)
	{
		float num = angle % 360f;
		if (num < 0f)
		{
			return 360f + num;
		}
		return num;
	}

	public static float GetVerticalFovFromHorizontal(float fov, float aspect)
	{
		return Mathf.Atan(Mathf.Tan(fov * (MathF.PI / 180f) * 0.5f) / aspect) * 2f * 57.29578f;
	}

	public static float GetHorizontalFovFromVertical(float fov, float aspect)
	{
		float num = fov * (MathF.PI / 180f);
		float num2 = 2f * (float)Math.Atan(Mathf.Tan(num / 2f) * aspect);
		return 57.29578f * num2;
	}

	public static float GetDuration(this AnimationCurve c)
	{
		if (c.length >= 1)
		{
			return c[c.length - 1].time;
		}
		return 0f;
	}

	public static float GetTime(this AnimationCurve c, float value, float checkFrequency, uint valueOrder = 0u)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < c.length; i++)
		{
			if (c[i].value > value)
			{
				if (num2 == valueOrder)
				{
					break;
				}
				num2++;
			}
			num = i;
		}
		if (num == c.length - 1)
		{
			return c[num].time;
		}
		float num3 = c[num].time;
		float num4 = num3;
		float time = c[num + 1].time;
		int num5 = 100;
		checkFrequency = Math.Max(GetDuration(c) / 100f, checkFrequency);
		while (num3 < time && num5 > 0)
		{
			num4 = num3 + checkFrequency;
			if (c.Evaluate(num4) <= value)
			{
				num3 = num4;
				num5--;
				continue;
			}
			return num3;
		}
		if (num5 == 0)
		{
			Debug.LogError("Didn't find time for appropriate loops count.");
		}
		return time;
	}

	public static float RoundFloatValue(float value, int digits)
	{
		float num = Mathf.Pow(10f, digits);
		return Mathf.Round(value * num) / num;
	}

	public static Vector3 GetDestination(this NavMeshAgent agent)
	{
		Vector3 destination = agent.destination;
		if (!float.IsInfinity(destination.x) && !float.IsInfinity(destination.y) && !float.IsInfinity(destination.z))
		{
			return destination;
		}
		Debug.LogException(new Exception("No NavMesh found!"));
		return Vector3.zero;
	}

	public static T Do<T>(this T t, Action<T> action)
	{
		action(t);
		return t;
	}

	public static IEnumerable<T> DoMap<T>(this IEnumerable<T> t, Action<T> action)
	{
		foreach (T item in t)
		{
			action(item);
			yield return item;
		}
	}

	public static T[] FillWith<T>(this T[] list, T value)
	{
		for (int i = 0; i < list.Length; i++)
		{
			list[i] = value;
		}
		return list;
	}

	public static List<T> FillWith<T>(this List<T> list, T value)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = value;
		}
		return list;
	}

	public static string GetFullPathWithoutExtension(string path)
	{
		return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
	}

	public static void ToggleStaticRecursively(GameObject go, bool isStatic)
	{
		go.isStatic = isStatic;
		foreach (Transform item in go.transform)
		{
			ToggleStaticRecursively(item.gameObject, isStatic);
		}
	}

	public static ICollection<T> GetAllComponentsOfType<T>(bool includeInactive) where T : Component
	{
		List<T> list = new List<T>();
		GameObject[] array = (from s in Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
			where s.isLoaded
			select s).SelectMany((Scene s) => s.GetRootGameObjects()).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			T[] componentsInChildren = array[num].GetComponentsInChildren<T>(includeInactive);
			foreach (T val in componentsInChildren)
			{
				if (includeInactive || IsComponentEnabled<T>(val))
				{
					list.Add(val);
				}
			}
		}
		return list;
	}

	public static bool IsComponentEnabled<T>(Component component) where T : Component
	{
		Renderer renderer = component as Renderer;
		LODGroup lODGroup = component as LODGroup;
		if (renderer != null)
		{
			return renderer.enabled;
		}
		if (lODGroup != null)
		{
			return lODGroup.enabled;
		}
		return true;
	}

	public static Bounds GetColliderBoundsWithoutRotation(Collider collider)
	{
		BoxCollider boxCollider = collider as BoxCollider;
		SphereCollider sphereCollider = collider as SphereCollider;
		MeshCollider meshCollider = collider as MeshCollider;
		CapsuleCollider capsuleCollider = collider as CapsuleCollider;
		Vector3 size;
		if (boxCollider != null)
		{
			size = Vector3.Scale(boxCollider.size, collider.transform.lossyScale);
		}
		else if (sphereCollider != null)
		{
			float num = sphereCollider.radius * 2f;
			size = Vector3.Scale(new Vector3(num, num, num), collider.transform.lossyScale);
		}
		else if (meshCollider != null && meshCollider.sharedMesh != null)
		{
			size = Vector3.Scale(meshCollider.sharedMesh.bounds.size, collider.transform.lossyScale);
		}
		else if (capsuleCollider != null)
		{
			float num2 = capsuleCollider.radius * 2f;
			float value = Mathf.Max(capsuleCollider.height, num2);
			size = new Vector3(num2, num2, num2);
			size[capsuleCollider.direction] = value;
			size = Vector3.Scale(size, collider.transform.lossyScale);
		}
		else
		{
			size = collider.bounds.size;
		}
		return new Bounds(collider.bounds.center, size);
	}

	public static bool HasTagInParent(this GameObject gameObject, string tag)
	{
		Transform transform = gameObject.transform;
		do
		{
			if (transform.gameObject.tag == tag)
			{
				return true;
			}
		}
		while ((transform = transform.parent) != null);
		return false;
	}

	public static bool Contains(this LODGroup lodGroup, Renderer renderer)
	{
		LOD[] lODs = lodGroup.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			for (int j = 0; j < renderers.Length; j++)
			{
				if (renderers[j] == renderer)
				{
					return true;
				}
			}
		}
		return false;
	}
}
