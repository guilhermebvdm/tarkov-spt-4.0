using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
	public Material[] Materials;

	public void Awake()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		Material[] materials = Materials;
		foreach (Material material in materials)
		{
			smethod_0(componentsInChildren, base.transform, material);
		}
	}

	public static void smethod_0(Renderer[] renderers, Transform combinedParent, Material material)
	{
		int num = 0;
		Renderer renderer = null;
		List<CombineInstance> list = new List<CombineInstance>();
		foreach (Renderer renderer2 in renderers)
		{
			if (renderer2.enabled && !(renderer2.sharedMaterial != material))
			{
				renderer2.enabled = false;
				if (renderer == null)
				{
					renderer = renderer2;
				}
				MeshFilter component = renderer2.GetComponent<MeshFilter>();
				Mesh sharedMesh = component.sharedMesh;
				num += sharedMesh.vertexCount;
				if (num > 65535)
				{
					smethod_1(list.ToArray(), combinedParent, renderer);
					list.Clear();
					num = sharedMesh.vertexCount;
				}
				list.Add(new CombineInstance
				{
					mesh = sharedMesh,
					transform = component.transform.localToWorldMatrix
				});
			}
		}
		if (list.Count > 0)
		{
			smethod_1(list.ToArray(), combinedParent, renderer);
			list.Clear();
		}
	}

	public static void smethod_1(CombineInstance[] instancesAccumulator, Transform parent, Renderer origin)
	{
		Mesh mesh = new Mesh
		{
			name = "MeshCombiner Combine"
		};
		mesh.CombineMeshes(instancesAccumulator, mergeSubMeshes: true, useMatrices: true);
		GameObject obj = new GameObject("Combined " + origin.sharedMaterial.name);
		obj.AddComponent<MeshFilter>().sharedMesh = mesh;
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = origin.sharedMaterial;
		meshRenderer.probeAnchor = origin.probeAnchor;
		meshRenderer.receiveShadows = origin.receiveShadows;
		meshRenderer.reflectionProbeUsage = origin.reflectionProbeUsage;
		meshRenderer.shadowCastingMode = origin.shadowCastingMode;
		obj.transform.position = Vector3.zero;
		obj.transform.parent = parent;
	}
}
