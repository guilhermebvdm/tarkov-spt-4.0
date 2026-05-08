using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerDrawCallColorDebugger : MonoBehaviour
{
	public GPUInstancerManager gPUIManager;

	public List<Color> drawCallColors = new List<Color>
	{
		Color.red,
		Color.blue,
		Color.yellow,
		Color.green,
		Color.magenta
	};

	public bool removeMainTextureWhenColored;

	private Dictionary<Material, Color> dictionary_0;

	private Dictionary<Material, Texture> dictionary_1;

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
		ChangeDrawCallColors();
	}

	public void ChangeDrawCallColors()
	{
		dictionary_0 = new Dictionary<Material, Color>();
		if (removeMainTextureWhenColored)
		{
			dictionary_1 = new Dictionary<Material, Texture>();
		}
		else
		{
			dictionary_1 = null;
		}
		int num = 0;
		foreach (GClass1270 runtimeData in gPUIManager.runtimeDataList)
		{
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
				{
					for (int k = 0; k < runtimeData.instanceLODs[i].renderers[j].materials.Count; k++)
					{
						if (runtimeData.instanceLODs[i].renderers[j].materials[k].HasProperty("_Color"))
						{
							dictionary_0.Add(runtimeData.instanceLODs[i].renderers[j].materials[k], runtimeData.instanceLODs[i].renderers[j].materials[k].color);
							runtimeData.instanceLODs[i].renderers[j].materials[k].color = ((num < drawCallColors.Count) ? drawCallColors[num] : Random.ColorHSV());
						}
						if (dictionary_1 != null && runtimeData.instanceLODs[i].renderers[j].materials[k].HasProperty("_MainTex"))
						{
							dictionary_1.Add(runtimeData.instanceLODs[i].renderers[j].materials[k], runtimeData.instanceLODs[i].renderers[j].materials[k].mainTexture);
							runtimeData.instanceLODs[i].renderers[j].materials[k].mainTexture = null;
						}
						num++;
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
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
				{
					for (int k = 0; k < runtimeData.instanceLODs[i].renderers[j].materials.Count; k++)
					{
						if (runtimeData.instanceLODs[i].renderers[j].materials[k].HasProperty("_Color") && dictionary_0.ContainsKey(runtimeData.instanceLODs[i].renderers[j].materials[k]))
						{
							runtimeData.instanceLODs[i].renderers[j].materials[k].color = dictionary_0[runtimeData.instanceLODs[i].renderers[j].materials[k]];
						}
						if (dictionary_1 != null && runtimeData.instanceLODs[i].renderers[j].materials[k].HasProperty("_MainTex") && dictionary_1.ContainsKey(runtimeData.instanceLODs[i].renderers[j].materials[k]))
						{
							runtimeData.instanceLODs[i].renderers[j].materials[k].mainTexture = dictionary_1[runtimeData.instanceLODs[i].renderers[j].materials[k]];
						}
					}
				}
			}
		}
	}
}
