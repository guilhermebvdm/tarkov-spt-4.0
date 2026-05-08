using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerModificationCollider : MonoBehaviour
{
	public GPUInstancerPrefabManager prefabManager;

	private List<GPUInstancerPrefab> list_0;

	private Collider collider_0;

	public void Awake()
	{
		list_0 = new List<GPUInstancerPrefab>();
		collider_0 = GetComponent<Collider>();
		if (prefabManager == null)
		{
			prefabManager = Object.FindObjectOfType<GPUInstancerPrefabManager>();
		}
		if (prefabManager != null)
		{
			prefabManager.AddModificationCollider(this);
		}
		else
		{
			Debug.LogWarning("GPUInstancerModificationCollider does not have a GPUInstancerPrefabManager defined.");
		}
	}

	public void Update()
	{
		if (!(prefabManager != null) || !prefabManager.isActiveAndEnabled)
		{
			return;
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			if (!IsInsideCollider(list_0[i]))
			{
				Rigidbody component = list_0[i].GetComponent<Rigidbody>();
				if (!(component != null) || component.IsSleeping())
				{
					GClass1257.EnableInstancingForInstance(prefabManager, list_0[i]);
					list_0.Remove(list_0[i]);
					i--;
				}
			}
			else if (list_0[i].state != PrefabInstancingState.Disabled)
			{
				prefabManager.DisableIntancingForInstance(list_0[i]);
			}
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (prefabManager != null && prefabManager.isActiveAndEnabled && (bool)collider.gameObject)
		{
			GPUInstancerPrefab component = collider.gameObject.GetComponent<GPUInstancerPrefab>();
			if (component != null && component.prefabPrototype.enableRuntimeModifications && component.state != PrefabInstancingState.Disabled)
			{
				prefabManager.DisableIntancingForInstance(component);
				list_0.Add(component);
			}
		}
	}

	public bool IsInsideCollider(GPUInstancerPrefab prefabInstance)
	{
		Collider component = prefabInstance.GetComponent<Collider>();
		if (component == null)
		{
			return false;
		}
		return collider_0.bounds.Intersects(component.bounds);
	}

	public void AddEnteredInstance(GPUInstancerPrefab prefabInstance)
	{
		list_0.Add(prefabInstance);
	}

	public int GetEnteredInstanceCount()
	{
		return list_0.Count;
	}
}
