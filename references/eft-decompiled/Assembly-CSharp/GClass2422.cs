using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public abstract class GClass2422
{
	[Serializable]
	[CompilerGenerated]
	public class Class1905
	{
		public static readonly Class1905 class1905_0 = new Class1905();

		public void method_0(GInterface246 x, BaseEventData y)
		{
			x.HideoutAwake();
		}
	}

	[NonSerialized]
	public static ExecuteEvents.EventFunction<GInterface246> EventFunction_0 = delegate(GInterface246 x, BaseEventData y)
	{
		x.HideoutAwake();
	};

	public static void SendAwakeEvents()
	{
		Scene sceneByName = SceneManager.GetSceneByName("bunker_2");
		if (sceneByName.IsValid())
		{
			GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				ExecuteEvents.ExecuteHierarchy(rootGameObjects[i], null, EventFunction_0);
			}
		}
	}
}
