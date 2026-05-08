using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MultiFlare;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GClass6
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct2
	{
		public List<Transform> objectsChildren;

		public Action<Transform> onTick;
	}

	[CompilerGenerated]
	public class Class91
	{
		public string part;

		public Func<GameObject, bool> func_0;

		public bool method_0(GameObject o)
		{
			return o.name == part;
		}
	}

	public static GameObject InstantiatePrefab(this Transform parent, GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays: false);
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public static T InstantiatePrefab<T>(this Transform parent, GameObject prefab) where T : MonoBehaviour
	{
		return InstantiatePrefab(parent, prefab).GetComponent<T>();
	}

	public static T InstantiatePrefab<T>(this GameObject parent, GameObject prefab) where T : MonoBehaviour
	{
		return InstantiatePrefab<T>(parent.transform, prefab);
	}

	public static void smethod_0(this Transform parent, bool onlyActive = false)
	{
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			Transform child = parent.GetChild(num);
			if (!onlyActive || child.gameObject.activeSelf)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(child.gameObject);
				}
			}
		}
	}

	public static void DestroyAllChildren(this GameObject parent, bool onlyActive = false)
	{
		smethod_0(parent.transform, onlyActive);
	}

	public static void ParentFake(this Transform child, Transform parent)
	{
	}

	public static void PreventMaterialChangeInEditor(this Renderer renderer)
	{
	}

	public static Material CopyToPreventMaterialChangeInEditor(this Material material)
	{
		return material;
	}

	public static void SmartEnable(this GameObject @object)
	{
		smethod_2(@object, value: true);
		smethod_1(@object, value: true);
	}

	public static void SmartEnableWithoutHierarchy(this GameObject @object)
	{
		@object.gameObject.SetActive(value: true);
		smethod_1(@object, value: true);
	}

	public static async Task SmartEnableAsync(this GameObject @object, float delay, Action<Transform> onTick = null)
	{
		smethod_1(@object, value: true);
		await smethod_3(delay, @object, onTick);
	}

	public static void SmartDisable(this GameObject @object)
	{
		smethod_1(@object, value: false);
		smethod_2(@object, value: false);
	}

	public static void SmartDisableWithoutHierarchy(this GameObject @object)
	{
		smethod_1(@object, value: false);
		@object.gameObject.SetActive(value: false);
	}

	public static void smethod_1(GameObject @object, bool value)
	{
		FlareLight[] componentsInChildren = @object.GetComponentsInChildren<FlareLight>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = value;
		}
	}

	public static void smethod_2(GameObject @object, bool value)
	{
		int childCount = @object.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			@object.transform.GetChild(i).gameObject.SetActive(value);
		}
		@object.gameObject.SetActive(value);
	}

	public static async Task smethod_3(float delay, GameObject @object, Action<Transform> onTick)
	{
		Struct2 struct2_ = default(Struct2);
		struct2_.onTick = onTick;
		struct2_.objectsChildren = @object.transform.Cast<Transform>().ToList();
		if (Application.isPlaying && delay > 0f)
		{
			float time = Time.time;
			float num = time + delay * (float)struct2_.objectsChildren.Count;
			int num2 = 0;
			do
			{
				int num3 = (int)((Time.time - time) / delay);
				for (int i = num2; i <= num3 && i < struct2_.objectsChildren.Count; i++)
				{
					smethod_6(@object.transform.GetChild(i), ref struct2_);
				}
				num2 = num3;
				await Task.Yield();
			}
			while (Time.time < num);
			for (int num4 = struct2_.objectsChildren.Count - 1; num4 >= 0; num4--)
			{
				smethod_6(struct2_.objectsChildren[num4], ref struct2_);
			}
		}
		else
		{
			smethod_5(ref struct2_);
		}
	}

	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		T val = gameObject.GetComponent<T>();
		if (val == null)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	public static Component GetOrAddComponent(this GameObject gameObject, Type type)
	{
		Component component = gameObject.GetComponent(type);
		if (component == null)
		{
			component = gameObject.AddComponent(type);
		}
		return component;
	}

	public static T GetOrAddComponent<T>(this MonoBehaviour component) where T : Component
	{
		return GetOrAddComponent<T>(component.gameObject);
	}

	public static string GetFullPath(this Transform transform, bool withSceneName = false)
	{
		Transform transform2 = transform;
		StringBuilder stringBuilder = new StringBuilder();
		do
		{
			stringBuilder.Insert(0, "/" + transform2.name);
			if (withSceneName && transform2.parent == null)
			{
				stringBuilder.Insert(0, transform2.gameObject.scene.name + ":");
			}
		}
		while ((transform2 = transform2.parent) != null);
		return stringBuilder.ToString();
	}

	public static Transform FindObjectByFullPath(string path)
	{
		string[] array = path.Split('/');
		if (array.Length == 0)
		{
			return null;
		}
		Transform transform = null;
		string text = array[0];
		if (text.EndsWith(":"))
		{
			text = text.Remove(text.Length - 1);
		}
		foreach (string part in array.Skip(1))
		{
			if (transform == null)
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene sceneAt = SceneManager.GetSceneAt(i);
					if (sceneAt.isLoaded && (!(text != "") || !(sceneAt.name != text)))
					{
						GameObject gameObject = (sceneAt.GetRootGameObjects() ?? Array.Empty<GameObject>()).FirstOrDefault((GameObject o) => o.name == part);
						if (gameObject != null)
						{
							transform = gameObject.transform;
							break;
						}
					}
				}
			}
			else
			{
				transform = transform.transform.Find(part);
			}
			if (transform == null)
			{
				break;
			}
		}
		return transform;
	}

	public static List<T> GetComponentsInChildrenActiveIgnoreFirstLevel<T>(this Transform transform) where T : Component
	{
		T[] componentsInChildren = transform.GetComponentsInChildren<T>(includeInactive: true);
		List<T> list = new List<T>();
		T[] array = componentsInChildren;
		foreach (T val in array)
		{
			if (smethod_4(val, transform))
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static T GetComponentInChildrenActiveIgnoreFirstLevel<T>(this Transform transform) where T : Component
	{
		T componentInChildren = transform.GetComponentInChildren<T>(includeInactive: true);
		if (componentInChildren != null && smethod_4(componentInChildren, transform))
		{
			return componentInChildren;
		}
		return null;
	}

	public static bool smethod_4(Component component, Transform firstLevel)
	{
		Transform transform = component.transform;
		if (transform == firstLevel)
		{
			return true;
		}
		do
		{
			if (!transform.gameObject.activeSelf)
			{
				return false;
			}
		}
		while ((transform = transform.parent) != firstLevel);
		return true;
	}

	public static T AddComponentCopy<T>(this GameObject go, T source) where T : Component
	{
		T val = go.AddComponent<T>();
		PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite)
			{
				try
				{
					propertyInfo.SetValue(val, propertyInfo.GetValue(source, null), null);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			fieldInfo.SetValue(val, fieldInfo.GetValue(source));
		}
		return val;
	}

	public static List<Transform> GetChildren(this Transform transform)
	{
		List<Transform> list = new List<Transform>(transform.childCount);
		for (int i = 0; i < transform.childCount; i++)
		{
			list.Add(transform.GetChild(i));
		}
		return list;
	}

	public static void SetActiveWithCheck(this GameObject go, bool active)
	{
		if (go.activeSelf != active)
		{
			go.SetActive(active);
		}
	}

	public static bool TryGetComponentsInChildren<T>(this GameObject go, bool includeInactive, out T[] components) where T : Component
	{
		components = go.GetComponentsInChildren<T>(includeInactive);
		if (components != null)
		{
			return components.Length != 0;
		}
		return false;
	}

	[CompilerGenerated]
	public static void smethod_5(ref Struct2 struct2_0)
	{
		for (int num = struct2_0.objectsChildren.Count - 1; num >= 0; num--)
		{
			smethod_6(struct2_0.objectsChildren[num], ref struct2_0);
		}
	}

	[CompilerGenerated]
	public static void smethod_6(Transform transform, ref Struct2 struct2_0)
	{
		struct2_0.onTick(transform);
		struct2_0.objectsChildren.Remove(transform);
	}
}
