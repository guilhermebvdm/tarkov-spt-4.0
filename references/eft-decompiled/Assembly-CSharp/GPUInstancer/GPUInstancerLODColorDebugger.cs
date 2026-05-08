using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerLODColorDebugger : MonoBehaviour
{
	public GPUInstancerManager gPUIManager;

	public List<Color> lODColors = new List<Color>
	{
		Color.red,
		Color.blue,
		Color.yellow
	};

	private Dictionary<Material, Color> dictionary_0;

	public void OnEnable()
	{
		if (gPUIManager != null)
		{
			StartCoroutine(method_0());
		}
	}

	public void OnDisable()
	{
		if (gPUIManager != null)
		{
			method_1();
		}
	}

	public void Reset()
	{
		if (GetComponent<GPUInstancerManager>() != null)
		{
			gPUIManager = GetComponent<GPUInstancerManager>();
		}
	}

	public IEnumerator method_0()
	{
		while (!gPUIManager.isInitialized)
		{
			yield return null;
		}
		ChangeLODColors();
	}

	public void ChangeLODColors()
	{
		dictionary_0 = new Dictionary<Material, Color>();
		foreach (GClass1270 runtimeData in gPUIManager.runtimeDataList)
		{
			for (int i = 1; i < runtimeData.instanceLODs.Count && i <= lODColors.Count; i++)
			{
				for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
				{
					for (int k = 0; k < runtimeData.instanceLODs[i].renderers[j].materials.Count; k++)
					{
						if (runtimeData.instanceLODs[i].renderers[j].materials[k].HasProperty("_Color"))
						{
							dictionary_0.Add(runtimeData.instanceLODs[i].renderers[j].materials[k], runtimeData.instanceLODs[i].renderers[j].materials[k].color);
							runtimeData.instanceLODs[i].renderers[j].materials[k].color = lODColors[i - 1];
						}
					}
				}
			}
		}
	}

	public void method_1()
	{
		if (dictionary_0 == null)
		{
			return;
		}
		foreach (GClass1270 runtimeData in gPUIManager.runtimeDataList)
		{
			for (int i = 1; i < runtimeData.instanceLODs.Count && i <= lODColors.Count; i++)
			{
				for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
				{
					for (int k = 0; k < runtimeData.instanceLODs[i].renderers[j].materials.Count; k++)
					{
						if (runtimeData.instanceLODs[i].renderers[j].materials[k].HasProperty("_Color") && dictionary_0.ContainsKey(runtimeData.instanceLODs[i].renderers[j].materials[k]))
						{
							runtimeData.instanceLODs[i].renderers[j].materials[k].color = dictionary_0[runtimeData.instanceLODs[i].renderers[j].materials[k]];
						}
					}
				}
			}
		}
	}
}
