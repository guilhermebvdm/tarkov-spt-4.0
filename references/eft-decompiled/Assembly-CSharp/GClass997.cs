using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class GClass997
{
	public static void SetComponentsEnabled(GameObject gameObject, MonoBehaviour component, ref IEnumerator setComponentsEnabledWorker, List<Component> componentsToTurnOff, List<GameObject> gameObjectsToTurnOff, int componentsSwitchPerFrameOnEnable, int objectsSwitchPerFrameOnEnable, int componentsSwitchPerFrameOnDisable, int objectsSwitchPerFrameOnDisable, bool isEnabled, Stopwatch stopwatch)
	{
		if ((componentsToTurnOff != null || gameObjectsToTurnOff != null) && (componentsToTurnOff == null || componentsToTurnOff.Count != 0 || gameObjectsToTurnOff == null || gameObjectsToTurnOff.Count != 0))
		{
			if (setComponentsEnabledWorker != null && gameObject.activeInHierarchy)
			{
				component.StopCoroutine(setComponentsEnabledWorker);
			}
			setComponentsEnabledWorker = SetComponentsEnabledWorker(gameObject, componentsToTurnOff, gameObjectsToTurnOff, componentsSwitchPerFrameOnEnable, objectsSwitchPerFrameOnEnable, componentsSwitchPerFrameOnDisable, objectsSwitchPerFrameOnDisable, isEnabled, stopwatch);
			if (gameObject.activeInHierarchy)
			{
				component.StartCoroutine(setComponentsEnabledWorker);
			}
		}
	}

	public static IEnumerator SetComponentsEnabledWorker(GameObject gameObject, List<Component> componentsToTurnOff, List<GameObject> gameObjectsToTurnOff, int componentsSwitchPerFrameOnEnable, int objectsSwitchPerFrameOnEnable, int componentsSwitchPerFrameOnDisable, int objectsSwitchPerFrameOnDisable, bool isEnabled, Stopwatch stopwatch)
	{
		int num;
		int num2;
		if (isEnabled)
		{
			num = componentsSwitchPerFrameOnEnable;
			num2 = objectsSwitchPerFrameOnEnable;
		}
		else
		{
			num = componentsSwitchPerFrameOnDisable;
			num2 = objectsSwitchPerFrameOnDisable;
		}
		int num3;
		if (componentsToTurnOff != null)
		{
			num3 = 0;
			for (int i = 0; i < componentsToTurnOff.Count; i++)
			{
				int index = ((!isEnabled) ? (componentsToTurnOff.Count - 1 - i) : i);
				Component component = componentsToTurnOff[index];
				if (!(component == null))
				{
					if (GClass809.SetEnabledUniversal(component, isEnabled))
					{
						num3++;
					}
					if (num3 >= num)
					{
						num3 = 0;
						yield return null;
					}
				}
			}
		}
		if (gameObjectsToTurnOff == null)
		{
			yield break;
		}
		num3 = 0;
		for (int i = 0; i < gameObjectsToTurnOff.Count; i++)
		{
			int index2 = ((!isEnabled) ? i : (gameObjectsToTurnOff.Count - 1 - i));
			GameObject gameObject2 = gameObjectsToTurnOff[index2];
			if (!(gameObject2 == null))
			{
				if (gameObject2.activeSelf != isEnabled)
				{
					gameObject2.SetActive(isEnabled);
					num3++;
				}
				if (num3 >= num2)
				{
					num3 = 0;
					yield return null;
				}
			}
		}
	}
}
