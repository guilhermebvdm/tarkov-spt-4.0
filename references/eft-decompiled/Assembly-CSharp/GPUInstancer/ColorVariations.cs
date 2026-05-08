using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class ColorVariations : MonoBehaviour
{
	public GPUInstancerPrefab prefab;

	public GPUInstancerPrefabManager prefabManager;

	public int instances = 1000;

	private string string_0 = "colorBuffer";

	private List<GPUInstancerPrefab> list_0;

	public void Start()
	{
		list_0 = new List<GPUInstancerPrefab>();
		if (prefabManager != null && prefabManager.isActiveAndEnabled)
		{
			GClass1257.DefinePrototypeVariationBuffer<Vector4>(prefabManager, prefab.prefabPrototype, string_0);
		}
		for (int i = 0; i < instances; i++)
		{
			GPUInstancerPrefab gPUInstancerPrefab = Object.Instantiate(prefab);
			gPUInstancerPrefab.transform.localPosition = Random.insideUnitSphere * 20f;
			gPUInstancerPrefab.transform.SetParent(base.transform);
			list_0.Add(gPUInstancerPrefab);
			gPUInstancerPrefab.AddVariation(string_0, (Vector4)Random.ColorHSV());
		}
		if (prefabManager != null && prefabManager.isActiveAndEnabled)
		{
			GClass1257.RegisterPrefabInstanceList(prefabManager, list_0);
			GClass1257.InitializeGPUInstancer(prefabManager);
		}
	}

	public void Update()
	{
		GClass1257.UpdateVariation(prefabManager, list_0[Random.Range(0, list_0.Count)], string_0, (Vector4)Random.ColorHSV());
	}
}
