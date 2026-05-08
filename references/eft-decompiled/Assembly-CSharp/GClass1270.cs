using System.Collections.Generic;
using System.Linq;
using GPUInstancer;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1270
{
	public GPUInstancerPrototype prototype;

	public List<GClass1271> instanceLODs;

	public Bounds instanceBounds;

	public float[] lodSizes = new float[16]
	{
		1000f, 1000f, 1000f, 1000f, 1000f, 1000f, 1000f, 1000f, 1000f, 1000f,
		1000f, 1000f, 1000f, 1000f, 1000f, 1000f
	};

	public float lodBiasApplied = -1f;

	[HideInInspector]
	public Matrix4x4[] instanceDataArray;

	public int instanceCount;

	public int bufferSize;

	public ComputeBuffer transformationMatrixVisibilityBuffer;

	public ComputeBuffer argsBuffer;

	public ComputeBuffer instanceLODDataBuffer;

	public uint[] args;

	public bool hasShadowCasterBuffer;

	public Material shadowCasterMaterial;

	public ComputeBuffer shadowArgsBuffer;

	public uint[] shadowArgs;

	public bool transformDataModified;

	public GClass1270(GPUInstancerPrototype prototype)
	{
		this.prototype = prototype;
	}

	public virtual void InitializeData()
	{
	}

	public virtual void ReleaseBuffers()
	{
	}

	public virtual void AddLodAndRenderer(Mesh mesh, List<Material> materials, MaterialPropertyBlock mpb, bool castShadows, float lodSize = -1f, MaterialPropertyBlock shadowMPB = null, bool excludeBounds = false, int layer = 0)
	{
		AddLod(lodSize, excludeBounds);
		AddRenderer(instanceLODs.Count - 1, mesh, materials, Matrix4x4.identity, mpb, castShadows, layer, shadowMPB);
	}

	public virtual void AddLod(float screenRelativeTransitionHeight = -1f, bool excludeBounds = false)
	{
		if (instanceLODs == null)
		{
			instanceLODs = new List<GClass1271>();
		}
		GClass1271 gClass = new GClass1271();
		gClass.excludeBounds = excludeBounds;
		instanceLODs.Add(gClass);
		if (instanceLODs.Count == 1 && screenRelativeTransitionHeight < 0f)
		{
			lodSizes[0] = 0f;
		}
		if (screenRelativeTransitionHeight < 0f)
		{
			return;
		}
		if (lodBiasApplied == -1f)
		{
			lodBiasApplied = QualitySettings.lodBias;
		}
		int num = (instanceLODs.Count - 1) * 4;
		if (instanceLODs.Count > 4)
		{
			num = (instanceLODs.Count - 5) * 4 + 1;
		}
		float num2 = screenRelativeTransitionHeight / ((!(prototype != null) || prototype.lodBiasAdjustment <= 0f) ? 1f : prototype.lodBiasAdjustment) / lodBiasApplied;
		lodSizes[num] = num2;
		if (GClass1274.matrixHandlingType != GPUIMatrixHandlingType.Default)
		{
			prototype.isLODCrossFade = false;
		}
		if (prototype.isLODCrossFade && !prototype.isLODCrossFadeAnimate)
		{
			float num3 = 1f;
			if (instanceLODs.Count > 1)
			{
				num3 = ((instanceLODs.Count < 5) ? lodSizes[(instanceLODs.Count - 2) * 4] : ((instanceLODs.Count != 5) ? lodSizes[(instanceLODs.Count - 6) * 4] : lodSizes[12]));
			}
			float num4 = num3 - num2;
			float num5 = num2 + num4 * prototype.lodFadeTransitionWidth;
			lodSizes[num + 2] = num5;
		}
	}

	public virtual void AddRenderer(int lod, Mesh mesh, List<Material> materials, Matrix4x4 transformOffset, MaterialPropertyBlock mpb, bool castShadows, int layer = 0, MaterialPropertyBlock shadowMPB = null, Renderer rendererRef = null)
	{
		if (instanceLODs != null && instanceLODs.Count > lod && instanceLODs[lod] != null)
		{
			if (mesh == null)
			{
				Debug.LogError("Can't add renderer: mesh is null. Make sure that all the MeshFilters on the objects has a mesh assigned.");
			}
			else if (materials != null && materials.Count != 0)
			{
				foreach (Material material in materials)
				{
					material.SetFloat("_ZWrite", 0f);
					material.SetFloat("_ZTest", 3f);
					material.SetFloat("AditionClip_ON", 0f);
					material.DisableKeyword("AditionClip_ON");
				}
				if (instanceLODs[lod].renderers == null)
				{
					instanceLODs[lod].renderers = new List<GClass1272>();
				}
				GClass1272 item = new GClass1272
				{
					mesh = mesh,
					materials = materials,
					transformOffset = transformOffset,
					mpb = mpb,
					shadowMPB = shadowMPB,
					layer = layer,
					castShadows = castShadows,
					rendererRef = rendererRef
				};
				instanceLODs[lod].renderers.Add(item);
				CalculateBounds();
			}
			else
			{
				Debug.LogError("Can't add renderer: no materials. Make sure that all the MeshRenderers have their materials assigned.");
			}
		}
		else
		{
			Debug.LogError("Can't add renderer: Invalid LOD");
		}
	}

	public virtual void CalculateBounds()
	{
		if (instanceLODs == null || instanceLODs.Count == 0 || instanceLODs[0].renderers == null || instanceLODs[0].renderers.Count == 0)
		{
			return;
		}
		for (int i = 0; i < instanceLODs.Count; i++)
		{
			if (instanceLODs[i].excludeBounds)
			{
				continue;
			}
			for (int j = 0; j < instanceLODs[i].renderers.Count; j++)
			{
				Bounds bounds = new Bounds(instanceLODs[i].renderers[j].mesh.bounds.center + (Vector3)instanceLODs[i].renderers[j].transformOffset.GetColumn(3), new Vector3(instanceLODs[i].renderers[j].mesh.bounds.size.x * instanceLODs[i].renderers[j].transformOffset.GetRow(0).magnitude, instanceLODs[i].renderers[j].mesh.bounds.size.y * instanceLODs[i].renderers[j].transformOffset.GetRow(1).magnitude, instanceLODs[i].renderers[j].mesh.bounds.size.z * instanceLODs[i].renderers[j].transformOffset.GetRow(2).magnitude));
				if (i == 0 && j == 0)
				{
					instanceBounds = bounds;
				}
				else
				{
					instanceBounds.Encapsulate(bounds);
				}
			}
		}
		instanceBounds.size += prototype.boundsOffset;
	}

	public virtual bool CreateRenderersFromGameObject(GPUInstancerPrototype prototype)
	{
		if (prototype.prefabObject == null)
		{
			return false;
		}
		if (prototype.isShadowCasting && (prototype.shadowLODMap == null || prototype.shadowLODMap.Length != 16))
		{
			prototype.shadowLODMap = new float[16]
			{
				0f, 4f, 0f, 0f, 1f, 5f, 0f, 0f, 2f, 6f,
				0f, 0f, 3f, 7f, 0f, 0f
			};
		}
		if (prototype.prefabObject.GetComponent<LODGroup>() != null)
		{
			return GenerateLODsFromLODGroup(prototype);
		}
		if (instanceLODs == null || instanceLODs.Count == 0)
		{
			AddLod();
		}
		return CreateRenderersFromMeshRenderers(0, prototype);
	}

	public virtual bool GenerateLODsFromLODGroup(GPUInstancerPrototype prototype)
	{
		LODGroup component = prototype.prefabObject.GetComponent<LODGroup>();
		if (instanceLODs == null)
		{
			instanceLODs = new List<GClass1271>();
		}
		else
		{
			instanceLODs.Clear();
		}
		for (int i = 0; i < component.GetLODs().Length; i++)
		{
			bool flag = false;
			List<Renderer> list = new List<Renderer>();
			if (component.GetLODs()[i].renderers != null)
			{
				Renderer[] renderers = component.GetLODs()[i].renderers;
				foreach (Renderer renderer in renderers)
				{
					if (renderer != null && renderer is MeshRenderer && renderer.GetComponent<MeshFilter>() != null)
					{
						if (prototype.useGeneratedBillboard && prototype.treeType == GPUInstancerTreeType.SpeedTree8 && renderer.sharedMaterials[0].IsKeywordEnabled("EFFECT_BILLBOARD"))
						{
							flag = true;
						}
						else
						{
							list.Add(renderer);
						}
					}
					else if (renderer != null && renderer is BillboardRenderer)
					{
						flag = true;
					}
				}
			}
			if (!list.Any())
			{
				if (!flag)
				{
					Debug.LogWarning("LODGroup has no mesh renderers. Prefab: " + component.gameObject.name + " LODIndex: " + i);
				}
				continue;
			}
			AddLod(component.GetLODs()[i].screenRelativeTransitionHeight);
			for (int k = 0; k < list.Count; k++)
			{
				List<Material> list2 = new List<Material>();
				for (int l = 0; l < list[k].sharedMaterials.Length; l++)
				{
					list2.Add(GClass1262.gpuiSettings.shaderBindings.GetInstancedMaterial(list[k].sharedMaterials[l]));
					if (prototype.isLODCrossFade)
					{
						list2[l].EnableKeyword("LOD_FADE_CROSSFADE");
					}
				}
				Matrix4x4 matrix4x = Matrix4x4.identity;
				Transform transform = list[k].gameObject.transform;
				while (transform != component.gameObject.transform)
				{
					matrix4x = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale) * matrix4x;
					transform = transform.parent;
				}
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				list[k].GetPropertyBlock(materialPropertyBlock);
				MaterialPropertyBlock materialPropertyBlock2 = null;
				if (prototype.isShadowCasting)
				{
					materialPropertyBlock2 = new MaterialPropertyBlock();
					list[k].GetPropertyBlock(materialPropertyBlock2);
				}
				AddRenderer(i, list[k].GetComponent<MeshFilter>().sharedMesh, list2, matrix4x, materialPropertyBlock, list[k].shadowCastingMode != ShadowCastingMode.Off, list[k].gameObject.layer, materialPropertyBlock2);
			}
		}
		return true;
	}

	public virtual bool CreateRenderersFromMeshRenderers(int lod, GPUInstancerPrototype prototype)
	{
		if (instanceLODs != null && instanceLODs.Count > lod && instanceLODs[lod] != null)
		{
			if (!prototype.prefabObject)
			{
				Debug.LogError("Can't create renderer(s): reference GameObject is null");
				return false;
			}
			List<MeshRenderer> list = new List<MeshRenderer>();
			GetMeshRenderersOfTransform(prototype.prefabObject.transform, list);
			if (list != null && list.Count != 0)
			{
				foreach (MeshRenderer item in list)
				{
					if (item.GetComponent<MeshFilter>() == null)
					{
						Debug.LogWarning("MeshRenderer with no MeshFilter found on GameObject <" + prototype.prefabObject.name + "> (Child: <" + item.gameObject?.ToString() + ">). Are you missing a component?");
						continue;
					}
					List<Material> list2 = new List<Material>();
					for (int i = 0; i < item.sharedMaterials.Length; i++)
					{
						list2.Add(GClass1262.gpuiSettings.shaderBindings.GetInstancedMaterial(item.sharedMaterials[i]));
					}
					Matrix4x4 matrix4x = Matrix4x4.identity;
					Transform transform = item.gameObject.transform;
					while (transform != prototype.prefabObject.transform)
					{
						matrix4x = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale) * matrix4x;
						transform = transform.parent;
					}
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					item.GetPropertyBlock(materialPropertyBlock);
					MaterialPropertyBlock materialPropertyBlock2 = null;
					if (prototype.isShadowCasting)
					{
						materialPropertyBlock2 = new MaterialPropertyBlock();
						item.GetPropertyBlock(materialPropertyBlock2);
					}
					AddRenderer(lod, item.GetComponent<MeshFilter>().sharedMesh, list2, matrix4x, materialPropertyBlock, item.shadowCastingMode != ShadowCastingMode.Off, item.gameObject.layer, materialPropertyBlock2, item);
				}
				return true;
			}
			Debug.LogError("Can't create renderer(s): no MeshRenderers found in the reference GameObject <" + prototype.prefabObject.name + "> or any of its children");
			return false;
		}
		Debug.LogError("Can't create renderer(s): Invalid LOD");
		return false;
	}

	public virtual void GetMeshRenderersOfTransform(Transform objectTransform, List<MeshRenderer> meshRenderers)
	{
		MeshRenderer component = objectTransform.GetComponent<MeshRenderer>();
		if (component != null)
		{
			meshRenderers.Add(component);
		}
		for (int i = 0; i < objectTransform.childCount; i++)
		{
			Transform child = objectTransform.GetChild(i);
			if (!(child.GetComponent<GPUInstancerPrefab>() != null))
			{
				GetMeshRenderersOfTransform(child, meshRenderers);
			}
		}
	}
}
