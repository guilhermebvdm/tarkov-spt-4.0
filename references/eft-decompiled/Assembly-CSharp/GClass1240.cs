using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1240
{
	public static Bounds GetLODGroupBounds(this LODGroup lodGroup, bool includeInactive = true)
	{
		Bounds result = default(Bounds);
		MeshRenderer[] componentsInChildren = lodGroup.GetComponentsInChildren<MeshRenderer>(includeInactive);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			result = componentsInChildren[0].bounds;
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (meshRenderer != null)
				{
					result.Encapsulate(meshRenderer.bounds);
				}
			}
			return result;
		}
		return result;
	}

	public static HashSet<Renderer> GetAllRenderers(this LODGroup group)
	{
		HashSet<Renderer> hashSet = new HashSet<Renderer>();
		LOD[] lODs = group.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null) && renderer.enabled && renderer.gameObject.activeInHierarchy)
				{
					hashSet.Add(renderer);
				}
			}
		}
		return hashSet;
	}
}
