using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer;

public class AddRemoveInstances : MonoBehaviour
{
	public GPUInstancerPrefab prefab;

	public GPUInstancerPrefabManager prefabManager;

	private Transform transform_0;

	private int int_0;

	private List<GPUInstancerPrefab> list_0 = new List<GPUInstancerPrefab>();

	private List<GPUInstancerPrefab> list_1 = new List<GPUInstancerPrefab>();

	private Toggle toggle_0;

	private Canvas canvas_0;

	public void Awake()
	{
		transform_0 = GameObject.Find("SphereObjects").transform;
		int_0 = transform_0.childCount;
		canvas_0 = Object.FindObjectOfType<Canvas>();
		toggle_0 = GameObject.Find("AddRemoveInstantlyToggle").GetComponent<Toggle>();
	}

	public void Start()
	{
		for (int i = 0; i < int_0; i++)
		{
			list_0.Add(transform_0.GetChild(i).gameObject.GetComponent<GPUInstancerPrefab>());
		}
	}

	public void AddInstances()
	{
		method_4();
		StartCoroutine(method_0());
	}

	public void RemoveInstances()
	{
		method_4();
		StartCoroutine(method_1());
	}

	public void AddExtraInstances()
	{
		method_4();
		StartCoroutine(method_2());
	}

	public void RemoveExtraInstances()
	{
		method_4();
		StartCoroutine(method_3());
	}

	public IEnumerator method_0()
	{
		for (int i = 0; i < int_0; i++)
		{
			GPUInstancerPrefab gPUInstancerPrefab = Object.Instantiate(prefab);
			gPUInstancerPrefab.transform.localPosition = Random.insideUnitSphere * 10f;
			gPUInstancerPrefab.transform.SetParent(transform_0);
			if (!gPUInstancerPrefab.prefabPrototype.addRuntimeHandlerScript)
			{
				GClass1257.AddPrefabInstance(prefabManager, gPUInstancerPrefab);
			}
			list_0.Add(gPUInstancerPrefab);
			if (!toggle_0.isOn)
			{
				yield return new WaitForSeconds(0.001f);
			}
		}
		method_5("RemoveInstancesButton");
		if (list_1.Count == 0)
		{
			method_5("AddExtraInstancesButton");
		}
		else
		{
			method_5("RemoveExtraInstancesButton");
		}
	}

	public IEnumerator method_1()
	{
		for (int i = 0; i < int_0; i++)
		{
			if (!list_0[list_0.Count - 1].prefabPrototype.addRuntimeHandlerScript)
			{
				GClass1257.RemovePrefabInstance(prefabManager, list_0[list_0.Count - 1]);
			}
			Object.Destroy(list_0[list_0.Count - 1].gameObject);
			list_0.RemoveAt(list_0.Count - 1);
			if (!toggle_0.isOn)
			{
				yield return new WaitForSeconds(0.001f);
			}
		}
		method_5("AddInstancesButton");
		if (list_1.Count == 0)
		{
			method_5("AddExtraInstancesButton");
		}
		else
		{
			method_5("RemoveExtraInstancesButton");
		}
	}

	public IEnumerator method_2()
	{
		for (int i = 0; i < prefab.prefabPrototype.extraBufferSize; i++)
		{
			GPUInstancerPrefab gPUInstancerPrefab = Object.Instantiate(prefab);
			gPUInstancerPrefab.transform.localPosition = Random.insideUnitSphere * 5f;
			gPUInstancerPrefab.transform.localPosition = gPUInstancerPrefab.transform.localPosition.normalized * (gPUInstancerPrefab.transform.localPosition.magnitude + 10f);
			gPUInstancerPrefab.transform.SetParent(transform_0);
			if (!gPUInstancerPrefab.prefabPrototype.addRuntimeHandlerScript)
			{
				GClass1257.AddPrefabInstance(prefabManager, gPUInstancerPrefab);
			}
			list_1.Add(gPUInstancerPrefab);
			if (!toggle_0.isOn)
			{
				yield return new WaitForSeconds(0.001f);
			}
		}
		method_5("RemoveExtraInstancesButton");
		if (list_0.Count == 0)
		{
			method_5("AddInstancesButton");
		}
		else
		{
			method_5("RemoveInstancesButton");
		}
	}

	public IEnumerator method_3()
	{
		for (int i = 0; i < prefab.prefabPrototype.extraBufferSize; i++)
		{
			if (!list_1[list_1.Count - 1].prefabPrototype.addRuntimeHandlerScript)
			{
				GClass1257.RemovePrefabInstance(prefabManager, list_1[list_1.Count - 1]);
			}
			Object.Destroy(list_1[list_1.Count - 1].gameObject);
			list_1.RemoveAt(list_1.Count - 1);
			if (!toggle_0.isOn)
			{
				yield return new WaitForSeconds(0.001f);
			}
		}
		method_5("AddExtraInstancesButton");
		if (list_0.Count == 0)
		{
			method_5("AddInstancesButton");
		}
		else
		{
			method_5("RemoveInstancesButton");
		}
	}

	public void method_4()
	{
		Selectable[] componentsInChildren = canvas_0.transform.GetComponentsInChildren<Selectable>();
		foreach (Selectable selectable in componentsInChildren)
		{
			if (selectable != toggle_0)
			{
				selectable.interactable = false;
			}
		}
	}

	public void method_5(string buttonName)
	{
		canvas_0.transform.GetChild(0).Find(buttonName).GetComponent<Selectable>()
			.interactable = true;
	}
}
