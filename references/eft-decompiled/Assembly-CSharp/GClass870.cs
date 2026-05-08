using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GClass870
{
	public static string smethod_0()
	{
		StackFrame stackFrame = new StackFrame(2);
		return stackFrame.GetFileName() + stackFrame.GetMethod().Name;
	}

	public static List<T> FindObjectsOfTypeAll<T>() where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
				}
			}
		}
		return list;
	}

	public static T[] FindUnityObjectsOfType<T>() where T : UnityEngine.Object
	{
		string.Format("WTF!? {0}({1}) called from '{2}'", "FindObjectsOfType", typeof(T), smethod_0());
		return UnityEngine.Object.FindObjectsOfType<T>();
	}

	public static UnityEngine.Object[] FindUnityObjectsOfType(Type type)
	{
		string.Format("WTF!? {0}({1}) called from '{2}'", "FindObjectsOfType", type, smethod_0());
		return UnityEngine.Object.FindObjectsOfType(type);
	}

	public static UnityEngine.Object FindUnityObjectOfType(Type type)
	{
		string.Format("WTF!? {0}({1}) called from '{2}'", "FindObjectOfType", type, smethod_0());
		return UnityEngine.Object.FindObjectOfType(type);
	}

	public static T FindUnityObjectOfType<T>() where T : UnityEngine.Object
	{
		string.Format("WTF!? {0}({1}) called from '{2}'", "FindObjectOfType", typeof(T), smethod_0());
		return UnityEngine.Object.FindObjectOfType<T>();
	}

	[Conditional("DEBUG")]
	public static void smethod_1(string sampleName)
	{
		UnityEngine.Debug.LogWarningFormat("<color=red>{0}</color>", sampleName);
	}

	public static List<T> FindUnityObjectsOfTypeUpsideDownOrder<T>() where T : Component
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				smethod_2(rootGameObjects[j].transform, list);
			}
		}
		return list;
	}

	public static List<T> FindUnityObjectsOnScene<T>() where T : Component
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
			}
		}
		return list;
	}

	public static void smethod_2<T>(Transform transform, List<T> results) where T : Component
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			smethod_2(child, results);
			T[] components = child.GetComponents<T>();
			results.AddRange(components);
		}
	}

	public static List<T> FindAllObjectsOfTypeOnLoadedScenes<T>()
	{
		List<T> list = new List<T>(256);
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
				}
			}
		}
		return list;
	}
}
