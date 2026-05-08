using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class GClass1239
{
	[CompilerGenerated]
	public class Class843
	{
		public Renderer rend;

		public Predicate<Renderer> predicate_0;

		public bool method_0(Renderer x)
		{
			return x == rend;
		}
	}

	[NonSerialized]
	public const string String_0 = "SharedOccluder";

	[NonSerialized]
	public const string String_1 = "LowPolyCollider";

	[NonSerialized]
	public static HashSet<Renderer> HashSet_0 = new HashSet<Renderer>();

	public static Renderer[] smethod_0(LOD[] lods, ESharedOccluderLODMode mode)
	{
		if (mode == ESharedOccluderLODMode.Last)
		{
			return lods[^1].renderers;
		}
		if ((int)mode < lods.Length)
		{
			return lods[(int)mode].renderers;
		}
		return lods[^1].renderers;
	}

	public static bool smethod_1(Renderer rend, IReadOnlyCollection<PerfectCullingBakeGroup> bakeGroups)
	{
		foreach (PerfectCullingBakeGroup bakeGroup in bakeGroups)
		{
			if (bakeGroup.renderers != null && Array.Find(bakeGroup.renderers, (Renderer x) => x == rend) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool smethod_2(string sourceGroupName, Renderer inputRenderer, GameObject occluderRoot, ETransparencyMode transparencyMode, IReadOnlyCollection<PerfectCullingBakeGroup> optionalBakeGroups = null, LODGroup optionalLodGroup = null)
	{
		if (!(inputRenderer == null) && inputRenderer.enabled && inputRenderer.gameObject.activeInHierarchy)
		{
			MeshRenderer meshRenderer = inputRenderer as MeshRenderer;
			if (meshRenderer == null)
			{
				return false;
			}
			if (GClass1228.IsShadowRenderer(inputRenderer))
			{
				return false;
			}
			if (GClass1228.IsDecalRenderer(meshRenderer))
			{
				return false;
			}
			if (optionalLodGroup != null && GClass1228.ContainsComponentInHierarchy(meshRenderer.transform, GClass1228.SHARED_OCCLUDER_IGNORE_TYPES, optionalLodGroup.transform))
			{
				return false;
			}
			if (optionalBakeGroups != null && smethod_1(meshRenderer, optionalBakeGroups))
			{
				return false;
			}
			if (HashSet_0.Contains(meshRenderer))
			{
				return false;
			}
			GameObject gameObject = smethod_3(meshRenderer, transparencyMode);
			if (gameObject == null)
			{
				return false;
			}
			HashSet_0.Add(meshRenderer);
			gameObject.transform.SetParent(occluderRoot.transform, worldPositionStays: true);
			gameObject.layer = LayerMask.NameToLayer("LowPolyCollider");
			gameObject.name = sourceGroupName + "_" + gameObject.name;
			MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddComponent<MeshCollider>();
			}
			if ((bool)meshRenderer.GetComponent<PerfectCullingTerrain>())
			{
				gameObject.gameObject.AddComponent<PerfectCullingTerrain>();
			}
			return true;
		}
		return false;
	}

	public static GameObject smethod_3(MeshRenderer rend, ETransparencyMode transparencyMode, ICollection<Mesh> optionalOutMeshCopies = null)
	{
		MeshFilter component = rend.GetComponent<MeshFilter>();
		if (!(component == null) && !(component.sharedMesh == null))
		{
			GameObject gameObject = null;
			Mesh sharedMesh = component.sharedMesh;
			int subMeshCount = sharedMesh.subMeshCount;
			Material[] sharedMaterials = rend.sharedMaterials;
			Material sharedOccluderMaterial = PerfectCullingSettings.Instance.sharedOccluderMaterial;
			if (subMeshCount == 1)
			{
				if (transparencyMode == ETransparencyMode.Default && GClass1228.IsMaterialTransparent(sharedMaterials[0]))
				{
					return null;
				}
				gameObject = new GameObject();
				MeshRenderer meshRenderer = GClass1228.CloneRenderer(rend, sharedOccluderMaterial);
				meshRenderer.gameObject.transform.SetParent(gameObject.transform, worldPositionStays: true);
				meshRenderer.gameObject.layer = LayerMask.NameToLayer("LowPolyCollider");
				gameObject.layer = LayerMask.NameToLayer("LowPolyCollider");
			}
			else
			{
				gameObject = new GameObject();
				gameObject.transform.position = rend.transform.position;
				gameObject.transform.rotation = rend.transform.rotation;
				gameObject.transform.localScale = rend.transform.lossyScale;
				gameObject.layer = LayerMask.NameToLayer("LowPolyCollider");
				Vector3[] vertices = sharedMesh.vertices;
				int num = Mathf.Min(sharedMesh.subMeshCount, sharedMaterials.Length);
				for (int i = 0; i < num; i++)
				{
					if (transparencyMode != ETransparencyMode.Default || !GClass1228.IsMaterialTransparent(sharedMaterials[i]))
					{
						GameObject obj = new GameObject
						{
							name = $"{rend.gameObject.name}_{i}"
						};
						Mesh mesh = new Mesh();
						mesh.indexFormat = IndexFormat.UInt32;
						mesh.vertices = vertices;
						mesh.triangles = sharedMesh.GetTriangles(i);
						mesh.RecalculateNormals();
						mesh.RecalculateBounds();
						obj.AddComponent<MeshFilter>().sharedMesh = mesh;
						obj.AddComponent<MeshRenderer>().sharedMaterial = sharedOccluderMaterial;
						obj.transform.SetParent(gameObject.transform, worldPositionStays: false);
						obj.transform.localPosition = Vector3.zero;
						obj.transform.localRotation = Quaternion.identity;
						obj.layer = LayerMask.NameToLayer("LowPolyCollider");
						optionalOutMeshCopies?.Add(mesh);
					}
				}
			}
			if (gameObject != null)
			{
				gameObject.name = rend.gameObject.name;
			}
			return gameObject;
		}
		Debug.LogWarning($"Mesh renderer has null mesh {rend.gameObject}", rend.gameObject);
		return null;
	}

	public static SharedOccluder GenerateSharedOccluder(PerfectCullingCrossSceneVolume volume, PerfectCullingCrossSceneGroup group, IReadOnlyCollection<PerfectCullingBakeGroup> optionalBakeGroups = null)
	{
		HashSet_0.Clear();
		GameObject gameObject = new GameObject();
		gameObject.name = "SharedOccluder_" + group?.gameObject.name;
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.layer = LayerMask.NameToLayer("LowPolyCollider");
		PerfectCullingCrossSceneGroup[] array = UnityEngine.Object.FindObjectsOfType<PerfectCullingCrossSceneGroup>();
		foreach (PerfectCullingCrossSceneGroup perfectCullingCrossSceneGroup in array)
		{
			GuidComponent component = perfectCullingCrossSceneGroup.GetComponent<GuidComponent>();
			if (perfectCullingCrossSceneGroup.GetComponent<CrossSceneCullingTreePreProcess>() != null || perfectCullingCrossSceneGroup.GetComponent<PerfectCullingTreePreProcess>() != null || !(perfectCullingCrossSceneGroup != group))
			{
				continue;
			}
			if (group != null)
			{
				CrossSceneCullingGroupPreProcess component2 = group.GetComponent<CrossSceneCullingGroupPreProcess>();
				if (component2 != null && component2.Contains(new GuidReference(component)))
				{
					continue;
				}
			}
			EditorGenerateSharedOccluderUsingGroupContent(perfectCullingCrossSceneGroup, gameObject);
		}
		SharedOccluder sharedOccluder = gameObject.AddComponent<SharedOccluder>();
		if (group != null)
		{
			group.sharedOccluder = sharedOccluder;
		}
		return sharedOccluder;
	}

	public static void EditorGenerateSharedOccluderUsingGroupContent(PerfectCullingCrossSceneGroup group, GameObject occluderRoot)
	{
		group.method_4();
		PerfectCullingBakeGroup[] bakeGroups = group.bakeGroups;
		foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in bakeGroups)
		{
			if (!perfectCullingBakeGroup.ContainsRenderers() || (perfectCullingBakeGroup.occludeMode != EOccludeMode.SharedOccludeeOccluder && perfectCullingBakeGroup.occludeMode != EOccludeMode.SharedOccluder))
			{
				continue;
			}
			MultisceneSharedOccluder multisceneSharedOccluder = GClass1228.ContainsComponentOnParentInHierarchy<MultisceneSharedOccluder>(perfectCullingBakeGroup.renderers[0].transform);
			ESharedOccluderLODMode mode = ((multisceneSharedOccluder != null) ? multisceneSharedOccluder.SharedOccluderLODMode : ESharedOccluderLODMode.LOD_0);
			LODGroup lODGroup = perfectCullingBakeGroup.method_1();
			Renderer[] array = ((lODGroup != null) ? smethod_0(lODGroup.GetLODs(), mode) : null);
			Renderer[] renderers = perfectCullingBakeGroup.renderers;
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null) && !GClass1228.ContainsComponentInHierarchy(renderer.transform, GClass1228.SHARED_OCCLUDER_IGNORE_TYPES, group.transform) && (array == null || array.Contains(renderer)))
				{
					smethod_2(group.gameObject.name, renderer, occluderRoot, ETransparencyMode.Default);
				}
			}
		}
	}
}
