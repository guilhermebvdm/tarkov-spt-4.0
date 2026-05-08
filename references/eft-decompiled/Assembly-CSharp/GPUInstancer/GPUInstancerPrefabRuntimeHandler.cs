using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerPrefabRuntimeHandler : MonoBehaviour
{
	[HideInInspector]
	public GPUInstancerPrefab gpuiPrefab;

	private GPUInstancerPrefabManager gpuinstancerPrefabManager_0;

	private static Dictionary<GPUInstancerPrefabPrototype, GPUInstancerPrefabManager> dictionary_0;

	public void Awake()
	{
		gpuiPrefab = GetComponent<GPUInstancerPrefab>();
		if (dictionary_0 != null)
		{
			return;
		}
		dictionary_0 = new Dictionary<GPUInstancerPrefabPrototype, GPUInstancerPrefabManager>();
		GPUInstancerPrefabManager[] array = Object.FindObjectsOfType<GPUInstancerPrefabManager>();
		if (array == null || array.Length == 0)
		{
			return;
		}
		GPUInstancerPrefabManager[] array2 = array;
		foreach (GPUInstancerPrefabManager gPUInstancerPrefabManager in array2)
		{
			foreach (GPUInstancerPrefabPrototype prototype in gPUInstancerPrefabManager.prototypeList)
			{
				if (!dictionary_0.ContainsKey(prototype))
				{
					dictionary_0.Add(prototype, gPUInstancerPrefabManager);
				}
			}
		}
	}

	public void Start()
	{
		if (gpuiPrefab.state != PrefabInstancingState.None)
		{
			return;
		}
		if (gpuinstancerPrefabManager_0 == null)
		{
			gpuinstancerPrefabManager_0 = method_0();
		}
		if (gpuinstancerPrefabManager_0 != null)
		{
			if (!gpuinstancerPrefabManager_0.isInitialized)
			{
				gpuinstancerPrefabManager_0.InitializeRuntimeDataAndBuffers();
			}
			gpuinstancerPrefabManager_0.AddPrefabInstance(gpuiPrefab, automaticallyIncreaseBufferSize: true);
		}
	}

	public void OnDisable()
	{
		if (gpuiPrefab.state == PrefabInstancingState.Instanced)
		{
			if (gpuinstancerPrefabManager_0 == null)
			{
				gpuinstancerPrefabManager_0 = method_0();
			}
			if (gpuinstancerPrefabManager_0 != null)
			{
				gpuinstancerPrefabManager_0.RemovePrefabInstance(gpuiPrefab, setRenderersEnabled: false);
			}
		}
	}

	public GPUInstancerPrefabManager method_0()
	{
		GPUInstancerPrefabManager value = null;
		if (GPUInstancerManager.activeManagerList != null)
		{
			if (!dictionary_0.TryGetValue(gpuiPrefab.prefabPrototype, out value))
			{
				value = (GPUInstancerPrefabManager)GPUInstancerManager.activeManagerList.Find((GPUInstancerManager manager) => manager.prototypeList.Contains(gpuiPrefab.prefabPrototype));
				if (value == null)
				{
					Debug.LogWarning("Can not find GPUI Prefab Manager for prototype: " + gpuiPrefab.prefabPrototype);
					return null;
				}
				dictionary_0.Add(gpuiPrefab.prefabPrototype, value);
			}
			if (value == null)
			{
				value = (GPUInstancerPrefabManager)GPUInstancerManager.activeManagerList.Find((GPUInstancerManager manager) => manager.prototypeList.Contains(gpuiPrefab.prefabPrototype));
				if (value == null)
				{
					return null;
				}
				dictionary_0[gpuiPrefab.prefabPrototype] = value;
			}
		}
		return value;
	}

	[CompilerGenerated]
	public bool method_1(GPUInstancerManager manager)
	{
		return manager.prototypeList.Contains(gpuiPrefab.prefabPrototype);
	}

	[CompilerGenerated]
	public bool method_2(GPUInstancerManager manager)
	{
		return manager.prototypeList.Contains(gpuiPrefab.prefabPrototype);
	}
}
