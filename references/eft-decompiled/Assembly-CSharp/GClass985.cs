using System.Collections.Generic;
using EFT;
using UnityEngine;

public abstract class GClass985
{
	public static List<RainCondensator> CreateRainCondensators(GameObject prefab, bool enabled = false, bool cleanOld = true)
	{
		if (cleanOld)
		{
			RainCondensator[] componentsInChildren = prefab.GetComponentsInChildren<RainCondensator>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.DestroyImmediate(componentsInChildren[i], allowDestroyingAssets: true);
			}
		}
		List<RainCondensator> list = new List<RainCondensator>();
		LODGroup[] componentsInChildren2 = prefab.GetComponentsInChildren<LODGroup>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Renderer[] renderers = componentsInChildren2[j].GetLODs()[0].renderers;
			for (int k = 0; k < renderers.Length; k++)
			{
				if (renderers[k] != null && renderers[k].transform.parent.GetComponent<Shell>() == null)
				{
					RainCondensator orAddComponent = GClass6.GetOrAddComponent<RainCondensator>(renderers[k].gameObject);
					orAddComponent.enabled = enabled;
					list.Add(orAddComponent);
				}
			}
		}
		Renderer[] componentsInChildren3 = prefab.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int l = 0; l < componentsInChildren3.Length; l++)
		{
			if (componentsInChildren3[l].name.Contains("LOD0") && !(componentsInChildren3[l].transform.parent.GetComponent<Shell>() != null))
			{
				RainCondensator orAddComponent2 = GClass6.GetOrAddComponent<RainCondensator>(componentsInChildren3[l].gameObject);
				orAddComponent2.enabled = enabled;
				if (!list.Contains(orAddComponent2))
				{
					list.Add(orAddComponent2);
				}
			}
		}
		return list;
	}

	public static void SetEnabled(this IEnumerable<RainCondensator> rainCondensators, bool enabled)
	{
		foreach (RainCondensator rainCondensator in rainCondensators)
		{
			rainCondensator.enabled = enabled;
		}
	}
}
