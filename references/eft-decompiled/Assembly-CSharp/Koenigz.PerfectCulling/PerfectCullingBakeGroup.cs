using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling;

[Serializable]
public class PerfectCullingBakeGroup
{
	public readonly struct RuntimeGroupContent(Renderer renderer, ShadowCastingMode shadowCastingMode)
	{
		public readonly Renderer Renderer = renderer;

		public readonly ShadowCastingMode ShadowCastingMode = shadowCastingMode;

		public readonly GameObject RendererObj = renderer.gameObject;

		public readonly string RendererObjName = renderer.gameObject.name;
	}

	public enum GroupType
	{
		Other,
		LOD,
		User,
		Light,
		CullingLightObject,
		Door,
		Portal,
		CullingCell,
		BakedLod
	}

	[CompilerGenerated]
	public class Class825
	{
		public Renderer rend;

		public bool method_0(Renderer x)
		{
			return rend == x;
		}
	}

	public GroupType groupType;

	public Renderer[] renderers;

	public ScreenDistanceSwitcher screenDistanceSwitcher;

	public EOccludeMode occludeMode;

	public int serializedGroupId;

	public float lightBakeSize;

	public Vector3 lightBakePosition;

	public Vector3 lightDir;

	public float lightAngle;

	public LightType lightType;

	public CullingObject[] cullingLightObjects;

	public AnalyticSource[] analyticSources;

	[NonSerialized]
	public int vertexCount;

	[NonSerialized]
	public ushort runtimeGroupIndex;

	[NonSerialized]
	public int updateCounter;

	[NonSerialized]
	public Renderer[] runtimeProxies;

	[NonSerialized]
	public LODGroup runtimeLodGroup;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public RuntimeGroupContent[] RuntimeGroupContent_0;

	[NonSerialized]
	public bool isGroupEnabled = true;

	public const float PERSISTENT_SHADOW_OBJ_SIZE = 2.5f;

	public static int numPersistentShadowLods;

	[NonSerialized]
	public Light[] runtimeLights;

	public IEnumerator<Vector3> EnumerateCenters
	{
		get
		{
			if (Application.isPlaying)
			{
				if (RuntimeGroupContent_0 != null)
				{
					RuntimeGroupContent[] runtimeGroupContent_ = RuntimeGroupContent_0;
					for (int i = 0; i < runtimeGroupContent_.Length; i++)
					{
						RuntimeGroupContent runtimeGroupContent = runtimeGroupContent_[i];
						if (runtimeGroupContent.Renderer != null)
						{
							yield return runtimeGroupContent.Renderer.transform.position;
						}
					}
				}
				if (runtimeLights != null)
				{
					Light[] array = runtimeLights;
					foreach (Light light in array)
					{
						yield return light.transform.position;
					}
				}
				yield break;
			}
			if (renderers != null)
			{
				Renderer[] array2 = renderers;
				foreach (Renderer renderer in array2)
				{
					yield return renderer.transform.position;
				}
			}
			if (cullingLightObjects != null)
			{
				CullingObject[] array3 = cullingLightObjects;
				foreach (CullingObject cullingObject in array3)
				{
					yield return cullingObject.transform.position;
				}
			}
		}
	}

	public RuntimeGroupContent[] RuntimeGroupData => RuntimeGroupContent_0;

	public bool IsEnabled
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return isGroupEnabled;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			isGroupEnabled = value;
			Toggle(value);
		}
	}

	public float Volume
	{
		get
		{
			Vector3 lhs = Vector3.zero;
			if (renderers != null && renderers.Length != 0)
			{
				Renderer[] array = renderers;
				foreach (Renderer renderer in array)
				{
					if (renderer != null)
					{
						lhs = Vector3.Max(lhs, renderer.bounds.size);
					}
				}
				return Mathf.Max(lhs.x, Mathf.Max(lhs.y, lhs.z));
			}
			return 0f;
		}
	}

	public bool ContainsRenderers()
	{
		if (renderers != null)
		{
			return renderers.Length != 0;
		}
		return false;
	}

	public LODGroup TryGetLODGroupFromRenderersHierarchyUpwards()
	{
		if (renderers == null)
		{
			return null;
		}
		Renderer[] array = renderers;
		int num = 0;
		LODGroup lODGroup;
		while (true)
		{
			if (num < array.Length)
			{
				Renderer renderer = array[num];
				if (!(renderer == null))
				{
					lODGroup = GClass1228.TryGetParentLODGroup(renderer.transform);
					if (lODGroup != null)
					{
						break;
					}
				}
				num++;
				continue;
			}
			return null;
		}
		return lODGroup;
	}

	public float GetVisualVerticalSize()
	{
		if (renderers != null && renderers.Length != 0)
		{
			return renderers[0].bounds.size.y;
		}
		return 1f;
	}

	public Vector3 GetCenter()
	{
		Vector3 zero = Vector3.zero;
		if (renderers != null && renderers.Length != 0)
		{
			float num = 0f;
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				if (!(renderer == null))
				{
					zero += renderer.bounds.center;
					num += 1f;
				}
			}
			if (num > 0f)
			{
				return zero / num;
			}
			return Vector3.zero;
		}
		return zero;
	}

	public bool ContainsRenderer(Renderer rend)
	{
		if (renderers != null && renderers.Length != 0)
		{
			if (rend == null)
			{
				return false;
			}
			return Array.Find(renderers, (Renderer x) => rend == x) != null;
		}
		return false;
	}

	public void DeleteRuntimeProxies()
	{
		Renderer[] array = runtimeProxies;
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(array[i].gameObject);
		}
		runtimeProxies = null;
	}

	public void CreateRuntimeProxies()
	{
		if (runtimeProxies != null)
		{
			return;
		}
		List<Renderer> list = new List<Renderer>();
		Material rainbowProxyMaterial = PerfectCullingResourcesLocator.Instance.RainbowProxyMaterial;
		RuntimeGroupContent[] runtimeGroupContent_ = RuntimeGroupContent_0;
		for (int i = 0; i < runtimeGroupContent_.Length; i++)
		{
			RuntimeGroupContent runtimeGroupContent = runtimeGroupContent_[i];
			if (runtimeGroupContent.Renderer is MeshRenderer)
			{
				MeshRenderer meshRenderer = GClass1228.CloneRenderer(runtimeGroupContent.Renderer as MeshRenderer, rainbowProxyMaterial);
				meshRenderer.enabled = !isGroupEnabled;
				list.Add(meshRenderer);
			}
		}
		runtimeProxies = list.ToArray();
	}

	public void method_0()
	{
		if (!Application.isPlaying)
		{
			throw new InvalidOperationException("Playmode only allowed for autotesting");
		}
		Material autotestRendererMaterialOpaque = PerfectCullingResourcesLocator.Instance.AutotestRendererMaterialOpaque;
		Material autotestRendererMaterialTransparent = PerfectCullingResourcesLocator.Instance.AutotestRendererMaterialTransparent;
		Material autotestProxyMaterialOpaque = PerfectCullingResourcesLocator.Instance.AutotestProxyMaterialOpaque;
		Material autotestProxyMaterialTransparent = PerfectCullingResourcesLocator.Instance.AutotestProxyMaterialTransparent;
		if (groupType != GroupType.LOD)
		{
			return;
		}
		LODGroup original = method_1();
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			if (renderer != null)
			{
				renderer.enabled = false;
			}
		}
		renderers = GClass1228.smethod_1(original, autotestRendererMaterialOpaque, autotestRendererMaterialTransparent).ToArray();
		Init();
		runtimeProxies = GClass1228.smethod_1(original, autotestProxyMaterialOpaque, autotestProxyMaterialTransparent).ToArray();
	}

	public LODGroup method_1()
	{
		Renderer[] array = renderers;
		int num = 0;
		LODGroup lODGroup;
		while (true)
		{
			if (num < array.Length)
			{
				Renderer renderer = array[num];
				if (!(renderer == null))
				{
					lODGroup = GClass1228.ContainsComponentOnParentInHierarchy<LODGroup>(renderer.transform);
					if (lODGroup != null)
					{
						return lODGroup;
					}
					lODGroup = renderer.GetComponent<LODGroup>();
					if (lODGroup != null)
					{
						break;
					}
				}
				num++;
				continue;
			}
			return null;
		}
		return lODGroup;
	}

	public void ValidateRuntime()
	{
		if (RuntimeGroupContent_0 == null)
		{
			return;
		}
		for (int i = 0; i < Int_0; i++)
		{
			RuntimeGroupContent runtimeGroupContent = RuntimeGroupContent_0[i];
			if (runtimeGroupContent.Renderer == null)
			{
				if (runtimeGroupContent.RendererObj != null)
				{
					Debug.LogError("Null runtime renderer " + runtimeGroupContent.RendererObjName, runtimeGroupContent.RendererObj);
				}
				else
				{
					Debug.LogError("Null runtime renderer and GameObject " + runtimeGroupContent.RendererObjName);
				}
			}
		}
	}

	public Bounds method_2()
	{
		Renderer[] array = renderers;
		int num = 0;
		Renderer renderer;
		while (true)
		{
			if (num < array.Length)
			{
				renderer = array[num];
				if (renderer != null && (!(renderer is MeshRenderer) || !((renderer as MeshRenderer).GetComponent<MeshFilter>().sharedMesh == null)))
				{
					break;
				}
				num++;
				continue;
			}
			Debug.LogError("No valid bounds or renderers in group");
			return default(Bounds);
		}
		return renderer.bounds;
	}

	public (bool, Bounds) GetGroupBounds()
	{
		if (renderers != null && renderers.Length != 0)
		{
			Bounds item = method_2();
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				if (renderer != null && (!(renderer is MeshRenderer) || !((renderer as MeshRenderer).GetComponent<MeshFilter>().sharedMesh == null)))
				{
					item.Encapsulate(renderer.bounds);
				}
			}
			return (true, item);
		}
		return (false, default(Bounds));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RuntimeSetForceRenderingOff(bool forceRenderingOff)
	{
		for (int i = 0; i < Int_0; i++)
		{
			RuntimeGroupContent_0[i].Renderer.forceRenderingOff = forceRenderingOff;
		}
	}

	public int Init()
	{
		int num = 0;
		RuntimeGroupContent_0 = new RuntimeGroupContent[renderers.Length];
		Renderer[] array;
		if (groupType == GroupType.LOD && renderers != null)
		{
			array = renderers;
			foreach (Renderer renderer in array)
			{
				if (renderer != null)
				{
					runtimeLodGroup = renderer.transform.parent.GetComponent<LODGroup>();
					break;
				}
			}
		}
		if (cullingLightObjects != null && cullingLightObjects.Length != 0)
		{
			List<Light> list = new List<Light>();
			CullingObject[] array2 = cullingLightObjects;
			foreach (CullingObject cullingObject in array2)
			{
				if (!(cullingObject == null))
				{
					Light component = cullingObject.GetComponent<Light>();
					if (component != null)
					{
						list.Add(component);
					}
				}
			}
			runtimeLights = list.ToArray();
		}
		array = renderers;
		foreach (Renderer renderer2 in array)
		{
			if (renderer2 == null)
			{
				num++;
			}
			else if (renderer2.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
			{
				_ = renderer2.bounds.size;
				numPersistentShadowLods++;
			}
			else
			{
				PushRuntimeRenderer(renderer2);
			}
		}
		return num;
	}

	public void PushRuntimeRenderer(Renderer renderer)
	{
		method_3(new RuntimeGroupContent(renderer, renderer.shadowCastingMode));
	}

	public bool PopRuntimeRenderer(Renderer renderer)
	{
		int num = -1;
		for (int i = 0; i < Int_0; i++)
		{
			if (RuntimeGroupContent_0[i].Renderer == renderer)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return false;
		}
		if (Int_0 >= 2)
		{
			ref RuntimeGroupContent reference = ref RuntimeGroupContent_0[num];
			ref RuntimeGroupContent reference2 = ref RuntimeGroupContent_0[Int_0 - 1];
			RuntimeGroupContent runtimeGroupContent = RuntimeGroupContent_0[Int_0 - 1];
			RuntimeGroupContent runtimeGroupContent2 = RuntimeGroupContent_0[num];
			reference = runtimeGroupContent;
			reference2 = runtimeGroupContent2;
		}
		method_4();
		return true;
	}

	public void method_3(RuntimeGroupContent groupContent)
	{
		if (Int_0 >= RuntimeGroupContent_0.Length)
		{
			Array.Resize(ref RuntimeGroupContent_0, Mathf.Max(1, Int_0) * 2);
		}
		RuntimeGroupContent_0[Int_0] = groupContent;
		Int_0++;
	}

	public RuntimeGroupContent method_4()
	{
		if (Int_0 <= 0)
		{
			return default(RuntimeGroupContent);
		}
		Int_0--;
		return RuntimeGroupContent_0[Int_0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Toggle(bool rendererEnabled)
	{
		if (runtimeProxies != null)
		{
			Renderer[] array = runtimeProxies;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = !rendererEnabled;
			}
		}
		if (cullingLightObjects != null)
		{
			CullingObject[] array2 = cullingLightObjects;
			foreach (CullingObject cullingObject in array2)
			{
				if (cullingObject != null)
				{
					cullingObject.SetAutocullVisibility(rendererEnabled);
				}
			}
		}
		if (analyticSources != null)
		{
			AnalyticSource[] array3 = analyticSources;
			foreach (AnalyticSource analyticSource in array3)
			{
				if (analyticSource != null)
				{
					analyticSource.IsAutocullVisible = rendererEnabled;
				}
			}
		}
		if (screenDistanceSwitcher != null)
		{
			screenDistanceSwitcher.IsBakedAutocullVisible = rendererEnabled;
		}
		for (int j = 0; j < Int_0; j++)
		{
			RuntimeGroupContent_0[j].Renderer.enabled = rendererEnabled;
		}
	}

	public void ForeachRenderer(Action<Renderer> actionForRenderer)
	{
		Renderer[] array = renderers;
		foreach (Renderer obj in array)
		{
			actionForRenderer(obj);
		}
	}
}
