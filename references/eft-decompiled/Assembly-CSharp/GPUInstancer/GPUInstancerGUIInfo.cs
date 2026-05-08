using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerGUIInfo : MonoBehaviour
{
	public bool showRenderedAmount;

	public bool showPrototypesSeparate;

	public bool showPrefabManagers = true;

	public bool showDetailManagers = true;

	public bool showTreeManagers = true;

	private static List<GClass1270> list_0 = new List<GClass1270> { null };

	public void OnGUI()
	{
		if (GPUInstancerManager.activeManagerList == null)
		{
			return;
		}
		if (GPUInstancerManager.showRenderedAmount != showRenderedAmount)
		{
			GPUInstancerManager.showRenderedAmount = showRenderedAmount;
		}
		int num = 0;
		int num2 = 0;
		string text = "";
		Color color = GUI.color;
		GUI.color = Color.red;
		if (showRenderedAmount)
		{
			GUI.Label(new Rect(10f, Screen.height - 30, 700f, 30f), "SHOWING RENDERED AMOUNTS! FRAME RATE WILL DROP!");
			num += 30;
		}
		foreach (GPUInstancerManager activeManager in GPUInstancerManager.activeManagerList)
		{
			num2 = 0;
			if (activeManager is GPUInstancerPrefabManager)
			{
				if (!showPrefabManagers)
				{
					continue;
				}
				text = "prefab";
				num2 = ((GPUInstancerPrefabManager)activeManager).GetEnabledPrefabCount();
			}
			else if (activeManager is GPUInstancerTreeManager)
			{
				if (!showTreeManagers)
				{
					continue;
				}
				text = "tree";
			}
			else if (activeManager is GPUInstancerDetailManager)
			{
				if (!showDetailManagers)
				{
					continue;
				}
				text = "detail";
			}
			if (showPrototypesSeparate)
			{
				foreach (GClass1270 runtimeData in activeManager.runtimeDataList)
				{
					list_0[0] = runtimeData;
					smethod_0(runtimeData.prototype.ToString(), list_0, showRenderedAmount, num, num2);
					num += GClass1262.DEBUG_INFO_SIZE;
				}
			}
			else
			{
				smethod_0(text, activeManager.runtimeDataList, showRenderedAmount, num, num2);
				num += GClass1262.DEBUG_INFO_SIZE;
			}
		}
		GUI.color = color;
	}

	public void OnDisable()
	{
		if (showRenderedAmount)
		{
			GPUInstancerManager.showRenderedAmount = false;
		}
	}

	public static void smethod_0(string name, List<GClass1270> runtimeDataList, bool showRenderedAmount, int startPos, int enabledCount)
	{
		if (runtimeDataList != null && runtimeDataList.Count != 0)
		{
			int num = 0;
			for (int i = 0; i < runtimeDataList.Count; i++)
			{
				num += runtimeDataList[i].instanceCount;
			}
			GUI.Label(new Rect(10f, Screen.height - startPos - 45, 700f, 30f), "Total " + name + " prototype count: " + runtimeDataList.Count);
			GUI.Label(new Rect(10f, Screen.height - startPos - 25, 700f, 30f), "Total " + name + " instance count: " + num);
			if (showRenderedAmount)
			{
				GUI.Label(new Rect(10f, Screen.height - startPos - 65, 700f, 30f), "Rendered " + name + " instance count: " + smethod_1(runtimeDataList));
				GUI.Label(new Rect(10f, Screen.height - startPos - 85, 700f, 30f), "Rendered Shadow " + name + " instance count: " + smethod_1(runtimeDataList, isShadow: true));
			}
			if (enabledCount > 0)
			{
				GUI.Label(new Rect(10f, Screen.height - startPos - 105, 700f, 30f), "Instancing disabled " + name + " instance count: " + enabledCount);
			}
		}
		else
		{
			GUI.Label(new Rect(10f, Screen.height - startPos - 25, 700f, 30f), "There are no " + name + " instance prototypes to render!");
		}
	}

	public static string smethod_1<T>(List<T> runtimeData, bool isShadow = false) where T : GClass1270
	{
		int num = 0;
		int num2 = 1;
		for (int i = 0; i < runtimeData.Count; i++)
		{
			if (num2 < runtimeData[i].instanceLODs.Count)
			{
				num2 = runtimeData[i].instanceLODs.Count;
			}
		}
		int[] array = new int[num2];
		for (int j = 0; j < runtimeData.Count; j++)
		{
			if (isShadow)
			{
				if (runtimeData[j].shadowArgs != null && runtimeData[j].shadowArgs.Length != 0)
				{
					for (int k = 0; k < runtimeData[j].instanceLODs.Count; k++)
					{
						array[k] += (int)runtimeData[j].shadowArgs[runtimeData[j].instanceLODs[k].argsBufferOffset + 1];
					}
				}
			}
			else if (runtimeData[j].args != null && runtimeData[j].args.Length != 0)
			{
				for (int l = 0; l < runtimeData[j].instanceLODs.Count; l++)
				{
					array[l] += (int)runtimeData[j].args[runtimeData[j].instanceLODs[l].argsBufferOffset + 1];
				}
			}
		}
		string text = "";
		for (int m = 0; m < array.Length; m++)
		{
			num += array[m];
			text = text + "LOD" + m + ": " + array[m] + ((m == array.Length - 1) ? "" : ", ");
		}
		return num + " (" + text + ")";
	}
}
