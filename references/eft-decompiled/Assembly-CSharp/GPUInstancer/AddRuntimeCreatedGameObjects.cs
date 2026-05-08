using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class AddRuntimeCreatedGameObjects : MonoBehaviour
{
	public GPUInstancerPrefabManager prefabManager;

	public Material material;

	private List<GameObject> list_0;

	private GPUInstancerPrefabPrototype gpuinstancerPrefabPrototype_0;

	private GameObject gameObject_0;

	public void Start()
	{
		gameObject_0 = GameObject.CreatePrimitive(PrimitiveType.Quad);
		SetMaterial();
		list_0 = new List<GameObject>();
		list_0.Add(gameObject_0);
		for (int i = 0; i < 1000; i++)
		{
			list_0.Add(UnityEngine.Object.Instantiate(gameObject_0, UnityEngine.Random.insideUnitSphere * 20f, Quaternion.identity));
		}
		gpuinstancerPrefabPrototype_0 = GClass1257.DefineGameObjectAsPrefabPrototypeAtRuntime(prefabManager, gameObject_0);
		gpuinstancerPrefabPrototype_0.enableRuntimeModifications = true;
		gpuinstancerPrefabPrototype_0.autoUpdateTransformData = true;
		GClass1257.AddInstancesToPrefabPrototypeAtRuntime(prefabManager, gpuinstancerPrefabPrototype_0, list_0);
		StartCoroutine(method_0());
	}

	public IEnumerator method_0()
	{
		while (true)
		{
			foreach (PrimitiveType value in Enum.GetValues(typeof(PrimitiveType)))
			{
				yield return new WaitForSeconds(3f);
				GClass1257.RemovePrototypeAtRuntime(prefabManager, gpuinstancerPrefabPrototype_0);
				ClearInstances();
				yield return new WaitForSeconds(1f);
				gameObject_0 = GameObject.CreatePrimitive(value);
				SetMaterial();
				list_0.Add(gameObject_0);
				gpuinstancerPrefabPrototype_0 = GClass1257.DefineGameObjectAsPrefabPrototypeAtRuntime(prefabManager, gameObject_0);
				for (int i = 0; i < 1000; i++)
				{
					list_0.Add(UnityEngine.Object.Instantiate(gameObject_0, UnityEngine.Random.insideUnitSphere * 20f, Quaternion.identity));
				}
				GClass1257.AddInstancesToPrefabPrototypeAtRuntime(prefabManager, gpuinstancerPrefabPrototype_0, list_0);
				yield return new WaitForSeconds(1f);
			}
		}
	}

	public void ClearInstances()
	{
		foreach (GameObject item in list_0)
		{
			UnityEngine.Object.Destroy(item);
		}
		list_0.Clear();
	}

	public void SetMaterial()
	{
		gameObject_0.GetComponent<MeshRenderer>().sharedMaterial = material;
	}
}
