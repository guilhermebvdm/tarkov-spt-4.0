using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerInstanceRemover : MonoBehaviour
{
	public bool useBounds;

	public List<Collider> selectedColliders;

	public bool removeFromDetailManagers = true;

	public bool removeFromTreeManagers = true;

	public bool removeFromPrefabManagers = true;

	public bool removeAtUpdate;

	public float offset;

	public bool onlyRemoveSelectedPrototypes;

	public List<GPUInstancerPrototype> selectedPrototypes;

	public void Reset()
	{
		selectedColliders = new List<Collider>(GetComponentsInChildren<Collider>());
	}

	public void Start()
	{
		foreach (GPUInstancerManager activeManager in GPUInstancerManager.activeManagerList)
		{
			if ((!removeFromDetailManagers && activeManager is GPUInstancerDetailManager) || (!removeFromTreeManagers && activeManager is GPUInstancerTreeManager) || (!removeFromPrefabManagers && activeManager is GPUInstancerPrefabManager))
			{
				continue;
			}
			foreach (Collider selectedCollider in selectedColliders)
			{
				if (useBounds)
				{
					GClass1257.RemoveInstancesInsideBounds(activeManager, selectedCollider.bounds, offset, onlyRemoveSelectedPrototypes ? selectedPrototypes : null);
				}
				else
				{
					GClass1257.RemoveInstancesInsideCollider(activeManager, selectedCollider, offset, onlyRemoveSelectedPrototypes ? selectedPrototypes : null);
				}
			}
		}
	}

	public void Update()
	{
		if (removeAtUpdate)
		{
			Start();
		}
	}
}
