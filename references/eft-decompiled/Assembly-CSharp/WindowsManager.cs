using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Comfort.Common;
using EFT.BlitDebug;
using EFT.Interactive;
using GPUInstancer;
using Koenigz.PerfectCulling.EFT;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using WindowsManagerUtilities;

[ExecuteAlways]
public class WindowsManager : MonoBehaviour
{
	[Serializable]
	public class MyKeyValuePair<KeyType, ValueType>
	{
		public KeyType key;

		public ValueType value;
	}

	[Serializable]
	public class BreakerIdInstanceIdPair : MyKeyValuePair<string, int>
	{
		public static List<BreakerIdInstanceIdPair> ListFromDictionary(Dictionary<string, int> dict)
		{
			List<BreakerIdInstanceIdPair> list = new List<BreakerIdInstanceIdPair>();
			foreach (KeyValuePair<string, int> item in dict)
			{
				list.Add(new BreakerIdInstanceIdPair
				{
					key = item.Key,
					value = item.Value
				});
			}
			return list;
		}

		public static Dictionary<string, int> DictFromList(List<BreakerIdInstanceIdPair> list)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (BreakerIdInstanceIdPair item in list)
			{
				dictionary[item.key] = item.value;
			}
			return dictionary;
		}
	}

	[Serializable]
	public class InteractableIdInstancesIdsPair : MyKeyValuePair<string, List<int>>
	{
		public static List<InteractableIdInstancesIdsPair> ListFromDictionary(Dictionary<string, List<int>> dict)
		{
			List<InteractableIdInstancesIdsPair> list = new List<InteractableIdInstancesIdsPair>();
			foreach (KeyValuePair<string, List<int>> item in dict)
			{
				list.Add(new InteractableIdInstancesIdsPair
				{
					key = item.Key,
					value = item.Value
				});
			}
			return list;
		}

		public static Dictionary<string, List<int>> DictFromList(List<InteractableIdInstancesIdsPair> list)
		{
			Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
			foreach (InteractableIdInstancesIdsPair item in list)
			{
				dictionary[item.key] = item.value;
			}
			return dictionary;
		}
	}

	[Serializable]
	public class BreakerTextureOffsetsPair : MyKeyValuePair<int, TexturesOffsets>
	{
		public static List<BreakerTextureOffsetsPair> ListFromDictionary(Dictionary<int, TexturesOffsets> dict)
		{
			List<BreakerTextureOffsetsPair> list = new List<BreakerTextureOffsetsPair>();
			foreach (KeyValuePair<int, TexturesOffsets> item in dict)
			{
				list.Add(new BreakerTextureOffsetsPair
				{
					key = item.Key,
					value = item.Value
				});
			}
			return list;
		}

		public static Dictionary<int, TexturesOffsets> DictFromList(List<BreakerTextureOffsetsPair> list)
		{
			Dictionary<int, TexturesOffsets> dictionary = new Dictionary<int, TexturesOffsets>();
			foreach (BreakerTextureOffsetsPair item in list)
			{
				dictionary[item.key] = item.value;
			}
			return dictionary;
		}
	}

	public struct Struct176
	{
		public string breakerId;

		public string pieceId;

		public MeshRenderer renderer;

		public Mesh mesh;

		public Material material;
	}

	private static WindowsManager windowsManager_0 = null;

	private Dictionary<int, Class706> dictionary_0 = new Dictionary<int, Class706>();

	private Dictionary<int, Camera> dictionary_1 = new Dictionary<int, Camera>();

	public Material WindowsMaterial;

	public Material CopyCubemapMaterial;

	public Material MBOITBlitOpaque;

	public Material QuarterMomentsUpscaler;

	public ComputeShader FrustumCullShader;

	public ComputeShader OcclusionCullShader;

	public ComputeShader ParticlesIndirectArgsCleaner;

	public ComputeShader ParticlesTexturesCopyShader;

	public ComputeShader ParticlesRenderDataCopyShader;

	public ComputeShader MBOITFogBlurShader;

	public int OcclusionAccuracy = 1;

	public float OcclusionOffset;

	public bool IsActive = true;

	public bool ScatterBlur;

	public int FogBlurSamplesCount = 3;

	public float FogBlurRadiusScale = 1f;

	public float BackgroundSplitDistance = 30f;

	private Renderer[] renderer_0;

	private static bool bool_0 = false;

	private static Dictionary<int, int> dictionary_2 = new Dictionary<int, int>();

	private static ParticleSystemRenderer[] particleSystemRenderer_0;

	private static HashSet<ParticleSystem> hashSet_0 = new HashSet<ParticleSystem>();

	private static int int_0 = -1;

	private HashSet<ParticleSystem> hashSet_1 = new HashSet<ParticleSystem>();

	public static bool EnableIsManageFunction = true;

	public static bool UseUnityMathCullingForParticles = false;

	public static bool EnableParticlesCulling = true;

	private bool bool_1;

	private static int int_1 = Shader.PropertyToID("MBOITForeground");

	private static int int_2 = Shader.PropertyToID("CameraParameters");

	private static int int_3 = Shader.PropertyToID("WindowsVerticesIDs");

	private static int int_4 = Shader.PropertyToID("WindowsVerticesInstances");

	private static int int_5 = Shader.PropertyToID("WindowsVerticesLocalOffsets");

	private static int int_6 = Shader.PropertyToID("WindowsInstancesMeshesIndices");

	private static int int_7 = Shader.PropertyToID("WindowsInstancesMeshesOffsets");

	private static int int_8 = Shader.PropertyToID("WindowsMeshesVerticesCount");

	private static int int_9 = Shader.PropertyToID("WindowsVertices");

	private static int int_10 = Shader.PropertyToID("WindowsBounds");

	private static int int_11 = Shader.PropertyToID("WindowsLocalToWorld");

	private static int int_12 = Shader.PropertyToID("WindowsTriangles");

	private static int int_13 = Shader.PropertyToID("WindowsTexturesOffsets");

	private static int int_14 = Shader.PropertyToID("_InstanceIdToStartVertexId");

	private static int int_15 = Shader.PropertyToID("MaterialsProperties");

	private static int int_16 = Shader.PropertyToID("LightsParameters");

	private static int int_17 = Shader.PropertyToID("_GlobalReflectionStrength");

	private static int int_18 = Shader.PropertyToID("_CustomFogStrength");

	private static int int_19 = Shader.PropertyToID("_ScatterStrength");

	private static int int_20 = Shader.PropertyToID("WindowsSpecTex");

	private static int int_21 = Shader.PropertyToID("WindowsCubeTex");

	private static int int_22 = Shader.PropertyToID("moments");

	private static int int_23 = Shader.PropertyToID("zeroth_moment");

	private static int int_24 = Shader.PropertyToID("zeroth_moment_foreground");

	private static int int_25 = Shader.PropertyToID("_MainTex");

	private static int int_26 = Shader.PropertyToID("_DepthTex");

	private static int int_27 = Shader.PropertyToID("_WindowsInstancesCount");

	private static int int_28 = Shader.PropertyToID("FrustumPlanes");

	private static int int_29 = Shader.PropertyToID("FrustumPlanesPNFactors");

	private static int int_30 = Shader.PropertyToID("_InstancesBounds");

	private static int int_31 = Shader.PropertyToID("_VerticesCountForeground");

	private static int int_32 = Shader.PropertyToID("_VerticesIDsForeground");

	private static int int_33 = Shader.PropertyToID("_VerticesCountBackground");

	private static int int_34 = Shader.PropertyToID("_VerticesIDsBackground");

	private static int int_35 = Shader.PropertyToID("_WindowsInstancesCountForeground");

	private static int int_36 = Shader.PropertyToID("OcclusionGroupsCountForeground");

	private static int int_37 = Shader.PropertyToID("_WindowsInstancesCountBackground");

	private static int int_38 = Shader.PropertyToID("OcclusionGroupsCountBackground");

	private static int int_39 = Shader.PropertyToID("_VerticesCount");

	private static int int_40 = Shader.PropertyToID("_VerticesIDs");

	private static int int_41 = Shader.PropertyToID("InstancesId");

	private static int int_42 = Shader.PropertyToID("ProjViewMatrix");

	private static int int_43 = Shader.PropertyToID("_WindowsInstancesEnabled");

	private static int int_44 = Shader.PropertyToID("occlusionAccuracy");

	private static int int_45 = Shader.PropertyToID("occlusionOffset");

	private static int int_46 = Shader.PropertyToID("hiZMap");

	private static int int_47 = Shader.PropertyToID("hiZTxtrSize");

	private static int int_48 = Shader.PropertyToID("momentsLowRes");

	private static int int_49 = Shader.PropertyToID("zeroth_momentLowRes");

	private static int int_50 = Shader.PropertyToID("transmittance_sum_rcp");

	private static int int_51 = Shader.PropertyToID("UnpackedVertexOffsetInstanceId");

	private static int int_52 = Shader.PropertyToID("_FrustumCornersWS");

	private Dictionary<Mesh, MeshOffsets> dictionary_3 = new Dictionary<Mesh, MeshOffsets>();

	private List<Texture2D> list_0 = new List<Texture2D>();

	private List<Cubemap> list_1 = new List<Cubemap>();

	[SerializeField]
	private List<LightProperties> _lightsProperties = new List<LightProperties>();

	[SerializeField]
	private List<MaterialParameters> _materialsParameters = new List<MaterialParameters>();

	private Vector4[] vector4_0 = new Vector4[12];

	private Vector3 vector3_0;

	public static int PiecesInstancesRingSize = 2000;

	public static int PiecesVerticesRingSize = 20000;

	public static int PiecesIndicesRingSize = 100000;

	private Dictionary<int, Class708> dictionary_4 = new Dictionary<int, Class708>();

	private Queue<Class708> queue_0 = new Queue<Class708>();

	private Class708 class708_0;

	private Class708 class708_1;

	private Class708 class708_2;

	[SerializeField]
	private InstancesBuffers _instancesGeometry;

	[SerializeField]
	private GeometryBuffers _geometryData;

	[SerializeField]
	private SceneDataContainer _sceneData;

	private Class707 class707_0 = new Class707();

	private GClass1000 gclass1000_0;

	private bool bool_2;

	private Light light_0;

	[SerializeField]
	private Texture2DArray _texture2DArray;

	[SerializeField]
	private CubemapArray _cubemapArray;

	private List<int> list_2 = new List<int>();

	private Dictionary<string, int> dictionary_5 = new Dictionary<string, int>();

	[SerializeField]
	private List<BreakerIdInstanceIdPair> _breakerIdInstanceIdList = new List<BreakerIdInstanceIdPair>();

	private Dictionary<string, List<int>> dictionary_6 = new Dictionary<string, List<int>>();

	[SerializeField]
	private List<InteractableIdInstancesIdsPair> _interactableIdInstanceIdList = new List<InteractableIdInstancesIdsPair>();

	[SerializeField]
	private List<BreakerTextureOffsetsPair> _breakersTexturesOffsetsList = new List<BreakerTextureOffsetsPair>();

	private Dictionary<int, TexturesOffsets> dictionary_7 = new Dictionary<int, TexturesOffsets>();

	private static float4[] float4_0 = new float4[6];

	private Plane[] plane_0;

	private Plane[] plane_1;

	private Dictionary<int, Struct176> dictionary_8 = new Dictionary<int, Struct176>();

	private List<string> list_3 = new List<string>();

	private List<MaterialParameters> list_4 = new List<MaterialParameters>();

	private List<MeshOffsets> list_5 = new List<MeshOffsets>();

	private List<int> list_6 = new List<int>();

	private List<WindowVertex> list_7 = new List<WindowVertex>();

	private List<int> list_8 = new List<int>();

	private WindowVertex windowVertex_0;

	private List<int> list_9 = new List<int>();

	private List<int> list_10 = new List<int>();

	private List<int> list_11 = new List<int>();

	private Class708 class708_3 = new Class708();

	private List<Matrix4x4> list_12 = new List<Matrix4x4>();

	private List<Vector3> list_13 = new List<Vector3>();

	private List<int> list_14 = new List<int>();

	private List<TexturesOffsets> list_15 = new List<TexturesOffsets>();

	private List<int> list_16 = new List<int>();

	private Dictionary<string, Matrix4x4> dictionary_9 = new Dictionary<string, Matrix4x4>();

	private static Vector3 vector3_1 = new Vector3(-1f, 1f, 1f).normalized;

	private static Vector3 vector3_2 = new Vector3(-1f, 1f, -1f).normalized;

	private static Vector3 vector3_3 = new Vector3(1f, 1f, 1f).normalized;

	private static Vector3 vector3_4 = new Vector3(1f, -1f, 1f).normalized;

	private static Vector3[] vector3_5 = new Vector3[4] { vector3_1, vector3_2, vector3_3, vector3_4 };

	private static XYCellSizeStruct[] gstruct29_0 = new XYCellSizeStruct[4]
	{
		new XYCellSizeStruct(0, 7),
		new XYCellSizeStruct(1, 6),
		new XYCellSizeStruct(4, 3),
		new XYCellSizeStruct(5, 2)
	};

	private static Vector3[] vector3_6 = new Vector3[8]
	{
		new Vector3(1f, 0f, 0f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 1f, 0f),
		new Vector3(1f, 1f, 1f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 1f, 1f)
	};

	private static float[] float_0 = new float[20];

	private static readonly float[] float_1 = new float[10];

	private static readonly int int_53 = Shader.PropertyToID("_CameraLogarithmicDepthSliceData");

	private List<int> list_17 = new List<int>();

	private static HashSet<string> hashSet_2 = new HashSet<string> { "Particles/Smoke Distorted", "Legacy Shaders/Particles/Additive", "CustomFX/HDRFireParticle", "Particles/Smoke Distorted Grenade", "Particles/Smoke Distorted Grenade Custom Billboard" };

	public static WindowsManager Instance => windowsManager_0;

	public GClass1000 ParticlesManager => gclass1000_0;

	public static bool InstanceIsActive()
	{
		if (windowsManager_0 == null)
		{
			return false;
		}
		return windowsManager_0.IsActive;
	}

	public void Awake()
	{
		windowsManager_0 = this;
		dictionary_5 = BreakerIdInstanceIdPair.DictFromList(_breakerIdInstanceIdList);
		dictionary_7 = BreakerTextureOffsetsPair.DictFromList(_breakersTexturesOffsetsList);
		dictionary_6 = InteractableIdInstancesIdsPair.DictFromList(_interactableIdInstanceIdList);
		method_12(ref _lightsProperties);
		if (!Application.isPlaying)
		{
			bool_1 = false;
		}
	}

	public void Start()
	{
		light_0 = GClass4.Instance.LightObject;
		if (Application.isPlaying)
		{
			CollectAndDisableMeshRenderers(collect: true);
			PopulateRenderersForOITMomentsLists();
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_4));
		}
	}

	public void ProcessAll()
	{
	}

	public void EnableMeshRenderers()
	{
		method_1(enable: true);
	}

	public void DisableMeshRenderers()
	{
		method_1(enable: false);
	}

	public void CheckInteractablesIDs()
	{
		HashSet<string> hashSet = new HashSet<string>();
		Trunk[] array = UnityEngine.Object.FindObjectsOfType<Trunk>();
		Door[] array2 = UnityEngine.Object.FindObjectsOfType<Door>();
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i].mboitRenderersCount();
			for (int j = 0; j < num; j++)
			{
				string text = array[i].mboitRendererId(j);
				if (hashSet.Contains(text))
				{
					Debug.LogError("DUPLICATE!!!: " + text);
				}
				else
				{
					hashSet.Add(text);
				}
			}
		}
		for (int k = 0; k < array2.Length; k++)
		{
			int num2 = array2[k].mboitRenderersCount();
			for (int l = 0; l < num2; l++)
			{
				string text2 = array2[k].mboitRendererId(l);
				if (hashSet.Contains(text2))
				{
					Debug.LogError("DUPLICATE!!!: " + text2);
				}
				else
				{
					hashSet.Add(text2);
				}
			}
		}
		Debug.LogError("Check is finished");
	}

	public void RunAwakeForSelected()
	{
	}

	public void OnDestroy()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_4));
		method_9();
		method_10();
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_4));
	}

	public void method_0()
	{
		CrossSceneCullingGroupPreProcess[] array = UnityEngine.Object.FindObjectsOfType<CrossSceneCullingGroupPreProcess>();
		for (int i = 0; i < array.Length; i++)
		{
			GuidReference[] cullingGroups = array[i].CullingGroups;
			foreach (GuidReference guidReference in cullingGroups)
			{
				if (!(guidReference.gameObject == null))
				{
					PerfectCullingCrossSceneContentMeshes component = guidReference.GetComponent<PerfectCullingCrossSceneContentMeshes>();
					if (!(component == null))
					{
						component.FillWindows();
					}
				}
			}
		}
	}

	public void CollectAndDisableMeshRenderers(bool collect)
	{
		if (collect)
		{
			method_0();
		}
		method_1(enable: false);
	}

	public void method_1(bool enable)
	{
		CrossSceneCullingGroupPreProcess[] array = UnityEngine.Object.FindObjectsOfType<CrossSceneCullingGroupPreProcess>();
		for (int i = 0; i < array.Length; i++)
		{
			GuidReference[] cullingGroups = array[i].CullingGroups;
			foreach (GuidReference guidReference in cullingGroups)
			{
				if (guidReference.gameObject == null)
				{
					continue;
				}
				PerfectCullingCrossSceneContentMeshes component = guidReference.GetComponent<PerfectCullingCrossSceneContentMeshes>();
				if (component == null || component.Windows == null)
				{
					continue;
				}
				Renderer[] windows = component.Windows;
				foreach (Renderer renderer in windows)
				{
					if (renderer != null)
					{
						renderer.enabled = enable;
					}
				}
			}
		}
	}

	public void CollectAndEnableMeshRenderers(bool collect)
	{
		if (collect)
		{
			method_0();
		}
		method_1(enable: true);
	}

	public void method_2()
	{
		ClearRenderData(releaseTextures: false);
	}

	public void ClearRenderData(bool releaseTextures)
	{
		method_9();
		method_10(releaseTextures);
	}

	public void Discard()
	{
		windowsManager_0 = null;
		method_1(enable: true);
		method_8();
		method_9();
		method_10();
	}

	public GClass1000 method_3()
	{
		GClass1000 gClass = new GClass1000(1000, 150, ParticlesIndirectArgsCleaner, ParticlesTexturesCopyShader, ParticlesRenderDataCopyShader);
		if (renderer_0 != null)
		{
			for (int i = 0; i < renderer_0.Length; i++)
			{
				ParticleSystem component = renderer_0[i].GetComponent<ParticleSystem>();
				gClass.AddParticleSystem(component);
			}
		}
		return gClass;
	}

	public void method_4(Camera camera)
	{
		if (!IsActive)
		{
			return;
		}
		if (bool_1)
		{
			if (gclass1000_0 == null)
			{
				if (!bool_2)
				{
					PopulateRenderersForOITMomentsLists();
					bool_2 = true;
				}
				gclass1000_0 = method_3();
			}
			if (bool_0)
			{
				foreach (ParticleSystem item in hashSet_1)
				{
					if (item == null || !hashSet_0.Contains(item))
					{
						gclass1000_0.RemoveParticleSystem(item);
					}
				}
				foreach (ParticleSystem item2 in hashSet_0)
				{
					if (item2 != null && !hashSet_1.Contains(item2))
					{
						gclass1000_0.AddParticleSystem(item2);
					}
				}
				hashSet_1 = new HashSet<ParticleSystem>(hashSet_0);
				bool_0 = false;
			}
		}
		CameraClass instance = CameraClass.Instance;
		bool flag = false;
		if (instance != null)
		{
			flag = (instance.Camera != null && instance.Camera.GetInstanceID() == camera.GetInstanceID()) | (instance.OpticCameraManager != null && instance.OpticCameraManager.Camera != null && instance.OpticCameraManager.Camera.GetInstanceID() == camera.GetInstanceID());
		}
		if (!flag || WindowsMaterial == null || _instancesGeometry._allTransforms.Count == 0)
		{
			return;
		}
		if (!dictionary_0.ContainsKey(camera.GetInstanceID()))
		{
			Class706 crd = new Class706();
			crd.CmdBufAfterTransparent = new CommandBuffer();
			crd.CmdBufAfterTransparent.name = "Windows_" + camera.name + "+00";
			crd.CmdBufBeforeTransparent = new CommandBuffer();
			crd.CmdBufBeforeTransparent.name = "WindowsPre_" + camera.name;
			crd.CmdBufCulling = new CommandBuffer();
			crd.CmdBufCulling.name = "WindowsCull_" + camera.name;
			crd._ssaa = camera.GetComponent<SSAA>();
			crd.ProjViewMatrix.Add(camera.projectionMatrix * camera.worldToCameraMatrix);
			crd.HiZGenerator = camera.gameObject.GetComponent<GPUInstancerHiZOcclusionGenerator>();
			crd.MBOITScattering = camera.gameObject.GetComponent<MBOIT_Scattering>();
			crd.DistantShadowComponent = camera.gameObject.GetComponentInChildren<DistantShadow>();
			if (crd.HiZGenerator == null)
			{
				if (GClass1262.gpuiSettings == null)
				{
					GClass1262.gpuiSettings = GPUInstancerSettings.GetDefaultGPUInstancerSettings();
				}
				GClass1262.gpuiSettings.SetDefultBindings();
				crd.HiZGenerator = camera.gameObject.AddComponent<GPUInstancerHiZOcclusionGenerator>();
				crd.HiZGenerator.Initialize(camera);
			}
			camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, crd.CmdBufCulling);
			camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, crd.CmdBufBeforeTransparent);
			camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, crd.CmdBufAfterTransparent);
			GClass1002.SortCommandBuffers(camera, CameraEvent.AfterForwardAlpha);
			dictionary_1.Add(camera.GetInstanceID(), camera);
			method_30(ref crd, camera);
			dictionary_0.Add(camera.GetInstanceID(), crd);
		}
		else
		{
			int instanceID = camera.GetInstanceID();
			Class706 crd2 = dictionary_0[instanceID];
			bool num = crd2.ResolutionMismatched(camera);
			bool flag2 = !(crd2._ssaa == null) && crd2._ssaa.GetRTIdentifier() != crd2._finalOutputTarget;
			bool flag3 = crd2.MBOITScatter();
			bool flag4 = crd2.DistantShadowComponent != null && crd2.DistantShadowComponent.LowResDepth != crd2._distantShadowLowResDepth;
			if (num || flag2 || (flag3 && (flag4 || crd2.MBOITScattering.isActiveAndEnabled != crd2._todScatteringEnabled || crd2.MBOITScattering.IsDataUpdated)) || crd2.IsMBOITScatterToggled())
			{
				crd2.ReleaseRenderTextures();
			}
			method_5(ref crd2, camera);
			method_30(ref crd2, camera);
			method_6(ref crd2, camera);
			method_19();
		}
	}

	public void method_5(ref Class706 crd, Camera cam)
	{
		int num = Math.Max(int_0, 0);
		int num2 = ((renderer_0 != null) ? renderer_0.Length : 0);
		if (crd._particlesRenderers == null || crd._particlesRenderers.Length < num2 + num)
		{
			crd._particlesRenderers = new Renderer[num2 + num + 1];
		}
		if (!EnableParticlesCulling)
		{
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				Renderer renderer = renderer_0[i];
				crd._particlesRenderers[i] = renderer;
				num3++;
			}
			for (int j = 0; j < int_0; j++)
			{
				ParticleSystemRenderer particleSystemRenderer = particleSystemRenderer_0[j];
				crd._particlesRenderers[num3] = particleSystemRenderer;
				num3++;
			}
			crd._particlesRenderers[num3] = null;
			return;
		}
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(cam);
		int num4 = 0;
		if (!UseUnityMathCullingForParticles)
		{
			for (int k = 0; k < num2; k++)
			{
				Renderer renderer2 = renderer_0[k];
				if (renderer2 != null)
				{
					_ = renderer2.gameObject.GetComponent<ParticleSystem>().main;
					if (GeometryUtility.TestPlanesAABB(array, renderer2.bounds))
					{
						crd._particlesRenderers[num4] = renderer2;
						num4++;
					}
				}
			}
			for (int l = 0; l < int_0; l++)
			{
				ParticleSystemRenderer particleSystemRenderer2 = particleSystemRenderer_0[l];
				if (particleSystemRenderer2 != null)
				{
					_ = particleSystemRenderer2.gameObject.GetComponent<ParticleSystem>().main;
					if (GeometryUtility.TestPlanesAABB(array, particleSystemRenderer2.bounds))
					{
						crd._particlesRenderers[num4] = particleSystemRenderer2;
						num4++;
					}
				}
			}
		}
		else
		{
			for (int m = 0; m < 6; m++)
			{
				float4_0[m] = new float4(array[m].normal, array[m].distance);
			}
			for (int n = 0; n < num2; n++)
			{
				Renderer renderer3 = renderer_0[n];
				float3 start = renderer3.bounds.min;
				float3 end = renderer3.bounds.max;
				bool flag = true;
				for (int num5 = 0; num5 < 6; num5++)
				{
					float3 t = crd.PNFactorsBuffer[num5 * 2];
					float3 t2 = crd.PNFactorsBuffer[num5 * 2 + 1];
					float3 xyz = math.lerp(start, end, t);
					if ((double)math.dot(new float4(math.lerp(start, end, t2), 1f), float4_0[num5]) >= 0.0)
					{
						if (!((double)math.dot(new float4(xyz, 1f), float4_0[num5]) >= 0.0))
						{
							flag = true;
							break;
						}
						continue;
					}
					flag = false;
					break;
				}
				if (flag)
				{
					crd._particlesRenderers[num4] = renderer3;
					num4++;
				}
			}
			for (int num6 = 0; num6 < int_0; num6++)
			{
				ParticleSystemRenderer particleSystemRenderer3 = particleSystemRenderer_0[num6];
				float3 start2 = particleSystemRenderer3.bounds.min;
				float3 end2 = particleSystemRenderer3.bounds.max;
				bool flag2 = true;
				for (int num7 = 0; num7 < 6; num7++)
				{
					float3 t3 = crd.PNFactorsBuffer[num7 * 2];
					float3 t4 = crd.PNFactorsBuffer[num7 * 2 + 1];
					float3 xyz2 = math.lerp(start2, end2, t3);
					if ((double)math.dot(new float4(math.lerp(start2, end2, t4), 1f), float4_0[num7]) >= 0.0)
					{
						if (!((double)math.dot(new float4(xyz2, 1f), float4_0[num7]) >= 0.0))
						{
							flag2 = true;
							break;
						}
						continue;
					}
					flag2 = false;
					break;
				}
				if (flag2)
				{
					crd._particlesRenderers[num4] = particleSystemRenderer3;
					num4++;
				}
			}
		}
		crd._particlesRenderers[num4] = null;
	}

	public void method_6(ref Class706 crd, Camera camera)
	{
		method_18();
		if (crd.FrustumPlanes != null)
		{
			_ = camera.farClipPlane;
			float nearClipPlane = camera.nearClipPlane;
			if (plane_0 == null)
			{
				plane_0 = GeometryUtility.CalculateFrustumPlanes(camera);
				plane_1 = GeometryUtility.CalculateFrustumPlanes(camera);
			}
			else
			{
				GeometryUtility.CalculateFrustumPlanes(camera, plane_0);
				Array.Copy(plane_0, plane_1, 6);
			}
			plane_0[5] = Plane.Translate(plane_0[4], plane_0[4].normal * (0f - BackgroundSplitDistance + nearClipPlane));
			plane_0[5].distance *= -1f;
			plane_0[5].normal *= -1f;
			for (int i = 0; i < 6; i++)
			{
				vector4_0[i] = new Vector4(plane_0[i].normal.x, plane_0[i].normal.y, plane_0[i].normal.z, plane_0[i].distance);
			}
			plane_1[4] = Plane.Translate(plane_1[4], plane_1[4].normal * (0f - BackgroundSplitDistance + nearClipPlane));
			for (int j = 0; j < 6; j++)
			{
				vector4_0[j + 6] = new Vector4(plane_1[j].normal.x, plane_1[j].normal.y, plane_1[j].normal.z, plane_1[j].distance);
			}
			crd.FrustumPlanes.SetData(vector4_0);
		}
		if (crd.FrustumPNFactors != null)
		{
			smethod_4(camera, ref crd.PNFactorsBuffer);
			crd.FrustumPNFactors.SetData(crd.PNFactorsBuffer);
		}
		crd.ProjViewMatrix[0] = camera.projectionMatrix * camera.worldToCameraMatrix;
		if (crd.CameraProjViewMatrixBuffer != null)
		{
			crd.CameraProjViewMatrixBuffer.SetData(crd.ProjViewMatrix);
		}
		if (crd.CameraParameters != null)
		{
			method_7(ref float_0, camera);
			crd.CameraParameters.SetData(float_0);
		}
		method_13(ref _lightsProperties);
		class707_0._lightsProperties.SetData(_lightsProperties);
	}

	public void method_7(ref float[] cameraParameters, Camera cam)
	{
		float_0[0] = cam.nearClipPlane;
		float_0[1] = cam.farClipPlane;
		float_0[2] = cam.farClipPlane - cam.nearClipPlane;
		float_0[3] = BackgroundSplitDistance;
		float farClipPlane = cam.farClipPlane;
		float fieldOfView = cam.fieldOfView;
		float aspect = cam.aspect;
		float num = fieldOfView * 0.5f;
		Vector3 vector = new Vector3(0f, 0f, -1f) * farClipPlane;
		Vector3 vector2 = new Vector3(1f, 0f, 0f);
		vector2 = vector2 * Mathf.Tan(num * (MathF.PI / 180f)) * aspect * farClipPlane;
		Vector3 vector3 = new Vector3(0f, 1f, 0f);
		vector3 = vector3 * Mathf.Tan(num * (MathF.PI / 180f)) * farClipPlane;
		Vector3 vector4 = vector - vector2 + vector3;
		Vector3 vector5 = vector + vector2 + vector3;
		Vector3 vector6 = vector + vector2 - vector3;
		Vector3 vector7 = vector - vector2 - vector3;
		cameraParameters[4] = vector4.x;
		cameraParameters[5] = vector4.y;
		cameraParameters[6] = vector4.z;
		cameraParameters[7] = 1f;
		cameraParameters[8] = vector5.x;
		cameraParameters[9] = vector5.y;
		cameraParameters[10] = vector5.z;
		cameraParameters[11] = 1f;
		cameraParameters[12] = vector6.x;
		cameraParameters[13] = vector6.y;
		cameraParameters[14] = vector6.z;
		cameraParameters[15] = 1f;
		cameraParameters[16] = vector7.x;
		cameraParameters[17] = vector7.y;
		cameraParameters[18] = vector7.z;
		cameraParameters[19] = 1f;
	}

	public void method_8()
	{
		dictionary_4.Clear();
		queue_0.Clear();
		class708_0 = null;
		class708_1 = null;
		_instancesGeometry.Clear();
		_geometryData.Clear();
		dictionary_3.Clear();
		_sceneData._meshesOffsetsList.Clear();
		_sceneData._meshesVerticesCount.Clear();
		_sceneData._instanceIdToStartVertexId.Clear();
		_sceneData._verticesInstances.Clear();
		_sceneData._verticesLocalOffsets.Clear();
		list_0.Clear();
		list_1.Clear();
		_lightsProperties.Clear();
		_materialsParameters.Clear();
		dictionary_5.Clear();
		_breakerIdInstanceIdList.Clear();
		dictionary_7.Clear();
		_breakersTexturesOffsetsList.Clear();
	}

	public void method_9()
	{
		foreach (KeyValuePair<int, Class706> item in dictionary_0)
		{
			Camera camera = dictionary_1[item.Key];
			if ((bool)camera)
			{
				camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, item.Value.CmdBufAfterTransparent);
				camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, item.Value.CmdBufCulling);
				camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, item.Value.CmdBufBeforeTransparent);
			}
			item.Value.Release();
		}
		dictionary_0.Clear();
		dictionary_1.Clear();
	}

	public void method_10(bool releaseTextures = true)
	{
		class707_0.Release();
		class708_0 = null;
		class708_1 = null;
		if (releaseTextures)
		{
			if (_texture2DArray != null)
			{
				_texture2DArray = null;
			}
			if (_cubemapArray != null)
			{
				_cubemapArray = null;
			}
		}
	}

	public static bool IsManagedByWindowsManager(Renderer renderer)
	{
		if (!EnableIsManageFunction)
		{
			return false;
		}
		MeshRenderer meshRenderer = renderer as MeshRenderer;
		if (meshRenderer == null)
		{
			return false;
		}
		return smethod_0(meshRenderer);
	}

	public static bool smethod_0(MeshRenderer renderer)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		if (sharedMaterials.Length != 1)
		{
			return false;
		}
		if (!smethod_1(sharedMaterials[0]))
		{
			return false;
		}
		if (renderer.GetComponent<MeshFilter>().sharedMesh == null)
		{
			return false;
		}
		return true;
	}

	public List<MeshRenderer> method_11(MeshRenderer[] meshRenderers)
	{
		List<MeshRenderer> list = new List<MeshRenderer>();
		int num = 0;
		_ = Application.isPlaying;
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			num++;
			if (smethod_0(meshRenderer))
			{
				list.Add(meshRenderer);
			}
		}
		return list;
	}

	public void method_12(ref List<LightProperties> props)
	{
		props.Clear();
		props.Add(default(LightProperties));
		method_13(ref props);
	}

	public void method_13(ref List<LightProperties> props)
	{
		if (!(light_0 == null))
		{
			LightProperties value = props[0];
			value.pos = light_0.transform.position;
			value.pos.w = 1f;
			value.dir = light_0.transform.forward;
			vector3_0.x = light_0.color.r;
			vector3_0.y = light_0.color.g;
			vector3_0.z = light_0.color.b;
			value.color = light_0.intensity * vector3_0;
			value.worldToLight = light_0.transform.worldToLocalMatrix;
			props[0] = value;
		}
	}

	public bool method_14(Class708 last, Class708 desiredSize)
	{
		int num = last.InstanceOffset + 1 + desiredSize.InstanceOffset;
		if (num > class707_0._texturesOffsetsBuffer.count)
		{
			return false;
		}
		if (num > class707_0._allTransformsBuffer.count)
		{
			return false;
		}
		if (num * 2 > class707_0._allBoundsBuffer.count)
		{
			return false;
		}
		if (num > class707_0._materialsProperties.count)
		{
			return false;
		}
		if (num > class707_0._instancesEnable.count)
		{
			return false;
		}
		if (last.VerticesOffset + last.VerticesCount + desiredSize.VerticesOffset > class707_0._allVerticesBuffer.count)
		{
			return false;
		}
		if (last.VerticesInstancesOffset + last.VerticesInstancesCount + desiredSize.VerticesInstancesOffset > class707_0._verticesInstances.count)
		{
			return false;
		}
		if (last.TrianglesOffset + last.TrianglesCount + desiredSize.TrianglesOffset > class707_0._allTrianglesBuffer.count)
		{
			return false;
		}
		if (last.VerticesLocalOffsetsOffset + last.VerticesLocalOffsetsCount + desiredSize.VerticesLocalOffsetsOffset > class707_0._verticesLocalOffsets.count)
		{
			return false;
		}
		if (last.InstanceMeshIndexOffset + 1 + desiredSize.InstanceMeshIndexOffset > class707_0._instancesMeshesIndices.count)
		{
			return false;
		}
		if (last.MeshOffsetOffset + 1 + desiredSize.MeshOffsetOffset > class707_0._meshesOffsets.count)
		{
			return false;
		}
		if (last.InstanceIdToStartVertexIdOffset + 1 + desiredSize.InstanceIdToStartVertexIdOffset > class707_0._instanceIdToStartVertexId.count)
		{
			return false;
		}
		if (last.MeshesVerticesCountOffset + 1 + desiredSize.MeshesVerticesCountOffset > class707_0._meshesVerticesCounts.count)
		{
			return false;
		}
		return true;
	}

	public bool method_15(Class708 bufferHead, Class708 first, Class708 desiredSize)
	{
		if (bufferHead.InstanceOffset + desiredSize.InstanceOffset > first.InstanceOffset)
		{
			return false;
		}
		if (bufferHead.VerticesOffset + desiredSize.VerticesOffset > first.VerticesOffset)
		{
			return false;
		}
		if (bufferHead.VerticesInstancesOffset + desiredSize.VerticesInstancesOffset > first.VerticesInstancesOffset)
		{
			return false;
		}
		if (bufferHead.TrianglesOffset + desiredSize.TrianglesOffset > first.TrianglesOffset)
		{
			return false;
		}
		if (bufferHead.VerticesLocalOffsetsOffset + desiredSize.VerticesLocalOffsetsOffset > first.VerticesLocalOffsetsOffset)
		{
			return false;
		}
		if (bufferHead.InstanceMeshIndexOffset + desiredSize.InstanceMeshIndexOffset > first.InstanceMeshIndexOffset)
		{
			return false;
		}
		if (bufferHead.MeshOffsetOffset + desiredSize.MeshOffsetOffset > first.MeshOffsetOffset)
		{
			return false;
		}
		if (bufferHead.InstanceIdToStartVertexIdOffset + desiredSize.InstanceIdToStartVertexIdOffset > first.InstanceIdToStartVertexIdOffset)
		{
			return false;
		}
		if (bufferHead.MeshesVerticesCountOffset + desiredSize.MeshesVerticesCountOffset > first.MeshesVerticesCountOffset)
		{
			return false;
		}
		return true;
	}

	public bool method_16(Class708 left, Class708 right, Class708 desiredSize)
	{
		if (left.InstanceOffset + 1 + desiredSize.InstanceOffset > right.InstanceOffset)
		{
			return false;
		}
		if (left.VerticesOffset + left.VerticesCount + desiredSize.VerticesOffset > right.VerticesOffset)
		{
			return false;
		}
		if (left.VerticesInstancesOffset + left.VerticesInstancesCount + desiredSize.VerticesInstancesOffset > right.VerticesInstancesOffset)
		{
			return false;
		}
		if (left.TrianglesOffset + left.TrianglesCount + desiredSize.TrianglesOffset > right.TrianglesOffset)
		{
			return false;
		}
		if (left.VerticesLocalOffsetsOffset + left.VerticesLocalOffsetsCount + desiredSize.VerticesLocalOffsetsOffset > right.VerticesLocalOffsetsOffset)
		{
			return false;
		}
		if (left.InstanceMeshIndexOffset + 1 + desiredSize.InstanceMeshIndexOffset > right.InstanceMeshIndexOffset)
		{
			return false;
		}
		if (left.MeshOffsetOffset + 1 + desiredSize.MeshOffsetOffset > right.MeshOffsetOffset)
		{
			return false;
		}
		if (left.InstanceIdToStartVertexIdOffset + 1 + desiredSize.InstanceIdToStartVertexIdOffset > right.InstanceIdToStartVertexIdOffset)
		{
			return false;
		}
		if (left.MeshesVerticesCountOffset + 1 + desiredSize.MeshesVerticesCountOffset > right.MeshesVerticesCountOffset)
		{
			return false;
		}
		return true;
	}

	public Class708 method_17(string pieceId, int pieceIdHash, Class708 desiredSizes)
	{
		if (class708_0 == null)
		{
			class708_1 = new Class708
			{
				PieceId = pieceId,
				PieceIdHash = pieceIdHash,
				InstanceOffset = _instancesGeometry.TexturesOffsetsCount,
				VerticesOffset = _geometryData._allVertices.Count,
				VerticesInstancesOffset = _sceneData._verticesInstances.Count,
				TrianglesOffset = _geometryData._allTriangles.Count,
				VerticesLocalOffsetsOffset = _sceneData._verticesLocalOffsets.Count,
				InstanceMeshIndexOffset = _instancesGeometry._meshIndex.Count,
				MeshOffsetOffset = _sceneData._meshesOffsetsList.Count,
				InstanceIdToStartVertexIdOffset = _sceneData._instanceIdToStartVertexId.Count,
				MeshesVerticesCountOffset = _sceneData._meshesVerticesCount.Count
			};
			class708_0 = class708_1;
			class708_2 = new Class708(class708_0);
			return class708_0;
		}
		if (class708_0.InstanceOffset > class708_1.InstanceOffset)
		{
			if (method_14(class708_0, desiredSizes))
			{
				return class708_0 = new Class708
				{
					PieceId = pieceId,
					PieceIdHash = pieceIdHash,
					InstanceOffset = class708_0.InstanceOffset + 1,
					VerticesOffset = class708_0.VerticesOffset + class708_0.VerticesCount,
					VerticesInstancesOffset = class708_0.VerticesInstancesOffset + class708_0.VerticesInstancesCount,
					TrianglesOffset = class708_0.TrianglesOffset + class708_0.TrianglesCount,
					VerticesLocalOffsetsOffset = class708_0.VerticesLocalOffsetsOffset + class708_0.VerticesLocalOffsetsCount,
					InstanceMeshIndexOffset = class708_0.InstanceMeshIndexOffset + 1,
					MeshOffsetOffset = class708_0.MeshOffsetOffset + 1,
					InstanceIdToStartVertexIdOffset = class708_0.InstanceIdToStartVertexIdOffset + 1,
					MeshesVerticesCountOffset = class708_0.MeshesVerticesCountOffset + 1
				};
			}
			if (method_15(class708_2, class708_1, desiredSizes))
			{
				return class708_0 = new Class708(class708_2, pieceId, pieceIdHash);
			}
			while (class708_1 != null && !method_15(class708_2, class708_1, desiredSizes))
			{
				Class708 @class = queue_0.Dequeue();
				dictionary_4.Remove(@class.PieceIdHash);
				if (queue_0.Count == 0)
				{
					class708_1 = null;
					class708_0 = null;
				}
				else
				{
					class708_1 = queue_0.Peek();
				}
			}
			Class708 class2 = new Class708(class708_2, pieceId, pieceIdHash);
			if (class708_1 == null)
			{
				if (!method_14(class708_2, desiredSizes))
				{
					return null;
				}
				class708_1 = class2;
			}
			class708_0 = class2;
			class708_2 = new Class708(class708_0);
			return class708_0;
		}
		if (method_16(class708_0, class708_1, desiredSizes))
		{
			return class708_0 = new Class708
			{
				PieceId = pieceId,
				PieceIdHash = pieceIdHash,
				InstanceOffset = class708_0.InstanceOffset + 1,
				VerticesOffset = class708_0.VerticesOffset + class708_0.VerticesCount,
				VerticesInstancesOffset = class708_0.VerticesInstancesOffset + class708_0.VerticesInstancesCount,
				TrianglesOffset = class708_0.TrianglesOffset + class708_0.TrianglesCount,
				VerticesLocalOffsetsOffset = class708_0.VerticesLocalOffsetsOffset + class708_0.VerticesLocalOffsetsCount,
				InstanceMeshIndexOffset = class708_0.InstanceMeshIndexOffset + 1,
				MeshOffsetOffset = class708_0.MeshOffsetOffset + 1,
				InstanceIdToStartVertexIdOffset = class708_0.InstanceIdToStartVertexIdOffset + 1,
				MeshesVerticesCountOffset = class708_0.MeshesVerticesCountOffset + 1
			};
		}
		while (!method_16(class708_0, class708_1, desiredSizes) && class708_0.InstanceOffset < class708_1.InstanceOffset)
		{
			Class708 class3 = queue_0.Dequeue();
			dictionary_4.Remove(class3.PieceIdHash);
			if (queue_0.Count == 0)
			{
				class708_1 = null;
				class708_0 = null;
			}
			else
			{
				class708_1 = queue_0.Peek();
			}
		}
		if (class708_0.InstanceOffset > class708_1.InstanceOffset)
		{
			return method_17(pieceId, pieceIdHash, desiredSizes);
		}
		return class708_0 = new Class708
		{
			PieceId = pieceId,
			PieceIdHash = pieceIdHash,
			InstanceOffset = class708_0.InstanceOffset + 1,
			VerticesOffset = class708_0.VerticesOffset + class708_0.VerticesCount,
			VerticesInstancesOffset = class708_0.VerticesInstancesOffset + class708_0.VerticesInstancesCount,
			TrianglesOffset = class708_0.TrianglesOffset + class708_0.TrianglesCount,
			VerticesLocalOffsetsOffset = class708_0.VerticesLocalOffsetsOffset + class708_0.VerticesLocalOffsetsCount,
			InstanceMeshIndexOffset = class708_0.InstanceMeshIndexOffset + 1,
			MeshOffsetOffset = class708_0.MeshOffsetOffset + 1,
			InstanceIdToStartVertexIdOffset = class708_0.InstanceIdToStartVertexIdOffset + 1,
			MeshesVerticesCountOffset = class708_0.MeshesVerticesCountOffset + 1
		};
	}

	public static bool smethod_1(Material material)
	{
		if (material == null)
		{
			return false;
		}
		if (!(material != null) || !(material.shader != null) || material.shader.name == null)
		{
			return false;
		}
		if (material.shader.name != "Global Fog/Transparent Reflective Specular")
		{
			return false;
		}
		return true;
	}

	public bool AddPiece(string breakerId, string pieceId, MeshRenderer renderer, Mesh mesh, Material material)
	{
		if (!IsActive)
		{
			return false;
		}
		if (!smethod_1(material))
		{
			return false;
		}
		int hashCode = pieceId.GetHashCode();
		if (class707_0._allBoundsBuffer == null)
		{
			dictionary_8.Add(hashCode, new Struct176
			{
				breakerId = breakerId,
				pieceId = pieceId,
				renderer = renderer,
				mesh = mesh,
				material = material
			});
			return true;
		}
		if (dictionary_4.ContainsKey(hashCode))
		{
			Debug.LogError("Already have idHash " + hashCode + " with id " + dictionary_4[hashCode].PieceIdHash + " while trying to add piece with id " + pieceId);
			return true;
		}
		class708_3.Reset();
		if (list_4.Count == 1)
		{
			MaterialParameters value = list_4[0];
			value.Init(material, "_MainTex");
			list_4[0] = value;
		}
		else
		{
			list_4.Add(new MaterialParameters(material, "_MainTex"));
		}
		class708_3.InstanceOffset = 1;
		class708_3.MeshOffsetOffset = 1;
		int[] triangles = mesh.triangles;
		if (list_6.Count == 1)
		{
			list_6[0] = triangles.Length;
		}
		else
		{
			list_6.Add(triangles.Length);
		}
		class708_3.MeshesVerticesCountOffset = 1;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		list_7.Clear();
		list_8.Clear();
		for (int i = 0; i < vertices.Length; i++)
		{
			windowVertex_0.vertex = vertices[i];
			windowVertex_0.normal = normals[i];
			windowVertex_0.uv = uv[i];
			list_7.Add(windowVertex_0);
		}
		list_8.AddRange(triangles);
		class708_3.VerticesOffset = vertices.Length;
		class708_3.TrianglesOffset = triangles.Length;
		class708_3.InstanceIdToStartVertexIdOffset = 1;
		class708_3.VerticesInstancesOffset = triangles.Length;
		class708_3.VerticesLocalOffsetsOffset = triangles.Length;
		list_12.Clear();
		list_13.Clear();
		list_14.Clear();
		renderer.enabled = false;
		int hashCode2 = breakerId.GetHashCode();
		TexturesOffsets item = (dictionary_7.ContainsKey(hashCode2) ? dictionary_7[hashCode2] : new TexturesOffsets
		{
			MainTexOffset = 0,
			SpecTexOffset = 0,
			ReflectionCubeOffset = 0
		});
		list_15.Clear();
		list_15.Add(item);
		list_16.Clear();
		list_16.Add(1);
		Class708 @class = method_17(pieceId, hashCode, class708_3);
		if (@class == null)
		{
			Debug.LogError("Couldn't allocate enough space for piece!");
			return false;
		}
		class707_0._materialsProperties.SetData(list_4, 0, @class.InstanceOffset, 1);
		if (list_5.Count == 0)
		{
			list_5.Add(new MeshOffsets
			{
				VerticesOffset = @class.VerticesOffset,
				IndicesOffset = @class.TrianglesOffset,
				MeshIndex = @class.InstanceMeshIndexOffset
			});
		}
		else
		{
			MeshOffsets value2 = list_5[0];
			value2.VerticesOffset = @class.VerticesOffset;
			value2.IndicesOffset = @class.TrianglesOffset;
			value2.MeshIndex = @class.InstanceMeshIndexOffset;
			list_5[0] = value2;
		}
		class707_0._meshesOffsets.SetData(list_5, 0, @class.MeshOffsetOffset, 1);
		class707_0._meshesVerticesCounts.SetData(list_6, 0, @class.MeshesVerticesCountOffset, 1);
		class707_0._allVerticesBuffer.SetData(list_7, 0, @class.VerticesOffset, vertices.Length);
		@class.VerticesCount = vertices.Length;
		class707_0._allTrianglesBuffer.SetData(list_8, 0, @class.TrianglesOffset, triangles.Length);
		@class.TrianglesCount = triangles.Length;
		if (list_9.Count == 0)
		{
			list_9.Add(@class.VerticesInstancesOffset);
		}
		else
		{
			list_9[0] = @class.VerticesInstancesOffset;
		}
		class707_0._instanceIdToStartVertexId.SetData(list_9, 0, @class.InstanceIdToStartVertexIdOffset, 1);
		list_10.Clear();
		list_11.Clear();
		int instanceOffset = @class.InstanceOffset;
		for (int j = 0; j < triangles.Length; j++)
		{
			list_10.Add(instanceOffset);
			list_11.Add(j);
		}
		class707_0._verticesInstances.SetData(list_10, 0, @class.VerticesInstancesOffset, triangles.Length);
		@class.VerticesInstancesCount = triangles.Length;
		class707_0._verticesLocalOffsets.SetData(list_11, 0, @class.VerticesLocalOffsetsOffset, triangles.Length);
		@class.VerticesLocalOffsetsCount = triangles.Length;
		list_12.Add(renderer.localToWorldMatrix);
		class707_0._allTransformsBuffer.SetData(list_12, 0, @class.InstanceOffset, 1);
		list_14.Add(@class.MeshOffsetOffset);
		class707_0._instancesMeshesIndices.SetData(list_14, 0, @class.InstanceMeshIndexOffset, 1);
		list_13.Add(renderer.bounds.min);
		list_13.Add(renderer.bounds.max);
		class707_0._allBoundsBuffer.SetData(list_13, 0, @class.InstanceOffset * 2, 2);
		class707_0._texturesOffsetsBuffer.SetData(list_15, 0, @class.InstanceOffset, 1);
		class707_0._instancesEnable.SetData(list_16, 0, @class.InstanceOffset, 1);
		class708_0.VerticesCount = @class.VerticesCount;
		class708_0.VerticesInstancesCount = @class.VerticesInstancesCount;
		class708_0.TrianglesCount = @class.TrianglesCount;
		class708_0.VerticesLocalOffsetsCount = @class.VerticesLocalOffsetsCount;
		dictionary_4.Add(hashCode, class708_0);
		queue_0.Enqueue(class708_0);
		return true;
	}

	public void method_18()
	{
		if (dictionary_8.Count > 0)
		{
			foreach (KeyValuePair<int, Struct176> item in dictionary_8)
			{
				Struct176 value = item.Value;
				AddPiece(value.breakerId, value.pieceId, value.renderer, value.mesh, value.material);
			}
			dictionary_8.Clear();
		}
		if (list_3.Count <= 0)
		{
			return;
		}
		foreach (string item2 in list_3)
		{
			BreakWindow(item2);
		}
		list_3.Clear();
	}

	public void UpdatePieceTransform(string pieceId, Transform transform)
	{
		int hashCode = pieceId.GetHashCode();
		if (dictionary_4.ContainsKey(hashCode))
		{
			Class708 @class = dictionary_4[hashCode];
			list_12.Clear();
			list_12.Add(transform.localToWorldMatrix);
			class707_0._allTransformsBuffer.SetData(list_12, 0, @class.InstanceOffset, 1);
		}
	}

	public void method_19()
	{
		if (dictionary_9.Count == 0)
		{
			return;
		}
		int num = 100;
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Matrix4x4> item in dictionary_9)
		{
			list.Add(item.Key);
			if (dictionary_6.TryGetValue(item.Key, out var value))
			{
				method_20(value, item.Value);
			}
			num--;
			if (num < 1)
			{
				break;
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			dictionary_9.Remove(list[i]);
		}
		list.Clear();
	}

	public void UpdateInteractableTransform(string id, Transform transform)
	{
		if (dictionary_6.TryGetValue(id, out var value))
		{
			if (class707_0._allTransformsBuffer == null)
			{
				dictionary_9[id] = transform.localToWorldMatrix;
			}
			else
			{
				method_20(value, transform.localToWorldMatrix);
			}
		}
	}

	public void method_20(List<int> offsets, Matrix4x4 transform)
	{
		for (int i = 0; i < offsets.Count; i++)
		{
			int computeBufferStartIndex = offsets[i];
			list_12.Clear();
			list_12.Add(transform);
			class707_0._allTransformsBuffer.SetData(list_12, 0, computeBufferStartIndex, 1);
		}
	}

	public void method_21()
	{
		if (queue_0.Count != 0)
		{
			do
			{
				Class708 @class = queue_0.Dequeue();
				dictionary_4.Remove(@class.PieceIdHash);
			}
			while (queue_0.Count > 0 && !queue_0.Peek().Enabled);
			if (queue_0.Count == 0)
			{
				class708_1 = null;
				class708_0 = null;
			}
			else
			{
				class708_1 = queue_0.Peek();
			}
		}
	}

	public void RemovePiece(string pieceId)
	{
		if (!IsActive)
		{
			return;
		}
		int hashCode = pieceId.GetHashCode();
		if (dictionary_8.Count > 0 && dictionary_8.ContainsKey(hashCode))
		{
			dictionary_8.Remove(hashCode);
		}
		else if (dictionary_4.ContainsKey(hashCode))
		{
			Class708 @class = dictionary_4[hashCode];
			@class.Enabled = false;
			list_16.Clear();
			list_16.Add(0);
			class707_0._instancesEnable.SetData(list_16, 0, @class.InstanceOffset, 1);
			if (@class == class708_1)
			{
				method_21();
			}
		}
	}

	public ComputeBuffer method_22<T>(List<T> data, int tailReserveCount, string name = "", ComputeBufferType bufferType = ComputeBufferType.Default)
	{
		if (data.Count == 0)
		{
			Debug.LogError("Zero data!");
			return null;
		}
		int stride = Marshal.SizeOf<T>();
		ComputeBuffer computeBuffer = new ComputeBuffer(data.Count + tailReserveCount, stride, bufferType);
		if (name.Length > 0)
		{
			computeBuffer.name = name;
		}
		computeBuffer.SetData(data.ToArray(), 0, 0, data.Count);
		return computeBuffer;
	}

	public ComputeBuffer method_23<T>(T[] data, int tailReserveCount, string name = "", ComputeBufferType bufferType = ComputeBufferType.Default)
	{
		if (data.Length == 0)
		{
			Debug.LogError("Zero data!");
			return null;
		}
		ComputeBuffer computeBuffer = new ComputeBuffer(data.Length + tailReserveCount, Marshal.SizeOf<T>(), bufferType);
		computeBuffer.SetData(data, 0, 0, data.Length);
		computeBuffer.name = name;
		return computeBuffer;
	}

	public static Texture2DArray smethod_2(List<Texture2D> textures, string name, int firstMipLevel)
	{
		Texture2D texture2D = ((textures.Count > 1) ? textures[1] : textures[0]);
		int num = texture2D.width >> firstMipLevel;
		int num2 = texture2D.height >> firstMipLevel;
		Texture2DArray texture2DArray = new Texture2DArray(num, num2, textures.Count, TextureFormat.DXT5, mipChain: true, linear: false)
		{
			name = name,
			filterMode = FilterMode.Bilinear,
			wrapMode = TextureWrapMode.Repeat,
			anisoLevel = 1
		};
		int mipmapCount = texture2DArray.mipmapCount;
		RenderTexture[] array = new RenderTexture[texture2D.mipmapCount];
		for (int i = 0; i < texture2D.mipmapCount; i++)
		{
			int width = Math.Max(1, num >> i);
			int height = Math.Max(1, num2 >> i);
			array[i] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
			array[i].antiAliasing = 1;
			array[i].Create();
		}
		RenderTexture active = RenderTexture.active;
		for (int j = 1; j < textures.Count; j++)
		{
			Texture2D texture2D2 = textures[j];
			if (!(texture2D2 == null))
			{
				for (int k = firstMipLevel; k < mipmapCount; k++)
				{
					Graphics.Blit(texture2D2, array[k - firstMipLevel]);
					Graphics.SetRenderTarget(array[k - firstMipLevel], 0);
					RenderTexture.active = array[k - firstMipLevel];
					int width2 = array[k - firstMipLevel].width;
					int height2 = array[k - firstMipLevel].height;
					Texture2D texture2D3 = new Texture2D(width2, height2, TextureFormat.ARGB32, mipChain: false);
					texture2D3.ReadPixels(new Rect(0f, 0f, width2, height2), 0, 0);
					texture2D3.Apply();
					texture2D3.Compress(highQuality: false);
					Graphics.CopyTexture(texture2D3, 0, 0, texture2DArray, j, k - firstMipLevel);
					RenderTexture.active = null;
				}
				continue;
			}
			Debug.LogError($"Texture[{j}] is null");
			UnityEngine.Object.DestroyImmediate(texture2DArray);
			texture2DArray = null;
			break;
		}
		RenderTexture.active = active;
		RenderTexture[] array2 = array;
		for (int l = 0; l < array2.Length; l++)
		{
			array2[l].Release();
		}
		return texture2DArray;
	}

	public static CubemapArray smethod_3(List<Cubemap> textures, string name, int firstMipLevel, Material copyMaterial)
	{
		if (copyMaterial == null)
		{
			return null;
		}
		Cubemap cubemap = textures[1];
		int num = cubemap.width >> firstMipLevel;
		int num2 = cubemap.height >> firstMipLevel;
		CubemapArray cubemapArray = new CubemapArray(num, textures.Count, TextureFormat.DXT5, mipChain: true, linear: false)
		{
			name = name,
			filterMode = FilterMode.Bilinear,
			wrapMode = TextureWrapMode.Repeat,
			anisoLevel = 1
		};
		int mipmapCount = cubemapArray.mipmapCount;
		RenderTexture[] array = new RenderTexture[cubemap.mipmapCount];
		for (int i = 0; i < mipmapCount; i++)
		{
			array[i] = new RenderTexture(num >> i, num2 >> i, 0, RenderTextureFormat.ARGB32);
			array[i].antiAliasing = 1;
			array[i].Create();
		}
		RenderTexture active = RenderTexture.active;
		for (int j = 1; j < textures.Count; j++)
		{
			Cubemap cubemap2 = textures[j];
			if (!(cubemap2 == null))
			{
				for (int k = firstMipLevel; k < mipmapCount; k++)
				{
					for (int l = 0; l < 6; l++)
					{
						copyMaterial.SetInt("_Face", l);
						DebugGraphics.Blit(cubemap2, array[k - firstMipLevel], copyMaterial, 0);
						Graphics.SetRenderTarget(array[k - firstMipLevel], 0);
						RenderTexture.active = array[k - firstMipLevel];
						Texture2D texture2D = new Texture2D(array[k - firstMipLevel].width, array[k - firstMipLevel].height, TextureFormat.ARGB32, mipChain: false);
						texture2D.ReadPixels(new Rect(0f, 0f, array[k - firstMipLevel].width, array[k - firstMipLevel].height), 0, 0);
						texture2D.Apply();
						texture2D.Compress(highQuality: false);
						Graphics.CopyTexture(texture2D, 0, 0, cubemapArray, j * 6 + l, k - firstMipLevel);
						RenderTexture.active = null;
					}
				}
				continue;
			}
			Debug.LogError($"Texture[{j}] is null");
			UnityEngine.Object.Destroy(cubemapArray);
			cubemapArray = null;
			break;
		}
		RenderTexture.active = active;
		RenderTexture[] array2 = array;
		for (int m = 0; m < array2.Length; m++)
		{
			array2[m].Release();
		}
		return cubemapArray;
	}

	public static void smethod_4(Camera camera, ref Vector3[] result)
	{
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
		for (int i = 0; i < 6; i++)
		{
			int num = 0;
			float num2 = Vector3.Dot(vector3_5[0], array[i].normal);
			float num3 = Mathf.Abs(num2);
			for (int j = 1; j < 4; j++)
			{
				float num4 = Vector3.Dot(vector3_5[j], array[i].normal);
				float num5 = Mathf.Abs(num4);
				if (num5 > num3)
				{
					num2 = num4;
					num3 = num5;
					num = j;
				}
			}
			XYCellSizeStruct xYCellSizeStruct = gstruct29_0[num];
			if (num2 < 0f)
			{
				xYCellSizeStruct = new XYCellSizeStruct(xYCellSizeStruct.Y, xYCellSizeStruct.X);
			}
			result[i * 2] = vector3_6[xYCellSizeStruct.X];
			result[i * 2 + 1] = vector3_6[xYCellSizeStruct.Y];
		}
	}

	public void method_24(RenderTexture renderTexture)
	{
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(renderTexture);
	}

	public RenderTexture method_25(RenderTexture initialRenderTexture, int width, int height, string name, GraphicsFormat format, bool UAV = false, int depth = 0)
	{
		if (initialRenderTexture != null)
		{
			method_24(initialRenderTexture);
		}
		RenderTexture renderTexture = new RenderTexture(width, height, depth, format);
		renderTexture.name = name;
		if (renderTexture.IsCreated())
		{
			renderTexture.Release();
		}
		renderTexture.enableRandomWrite = UAV;
		renderTexture.Create();
		return renderTexture;
	}

	public RenderTexture method_26(RenderTexture initialRenderTexture, int width, int height, string name, RenderTextureFormat format, bool UAV = false, int depth = 0)
	{
		if (initialRenderTexture != null)
		{
			method_24(initialRenderTexture);
		}
		RenderTexture renderTexture = new RenderTexture(width, height, depth, format);
		renderTexture.name = name;
		if (renderTexture.IsCreated())
		{
			renderTexture.Release();
		}
		renderTexture.enableRandomWrite = UAV;
		renderTexture.Create();
		return renderTexture;
	}

	public void method_27(CommandBuffer cmdBuf, RenderTexture quarterOIT)
	{
		cmdBuf.BeginSample("BlitMBOITFog");
		cmdBuf.SetGlobalTexture("QuarterOIT", quarterOIT);
		cmdBuf.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, QuarterMomentsUpscaler, 0, 5);
		cmdBuf.EndSample("BlitMBOITFog");
	}

	public void method_28(ref Class706 crd, Camera camera, int w, int h, GraphicsFormat momentsFormat, GraphicsFormat momentsRcpFormat)
	{
		Vector4 value = new Vector4(w, h, 1f / (float)w, 1f / (float)h);
		crd.CmdBufAfterTransparent.SetGlobalVector("MBOITResolution", value);
		if (crd.MomentsRT == null)
		{
			crd.MomentsRT = method_25(null, w, h, "MomentsRT", momentsFormat, UAV: true);
			crd.MomentsRTBackground = method_25(null, w, h, "MomentsRTBackground", momentsFormat, UAV: true);
			crd.FogAlphaRT = method_25(null, w, h, "FogAlpha", GraphicsFormat.R16_SFloat, UAV: true);
			crd.FogAlphaRTBackground = method_25(null, w, h, "FogAlphaBackground", GraphicsFormat.R16_SFloat, UAV: true);
			crd._oitRTIs0[0] = new RenderTargetIdentifier(crd.MomentsRT);
			crd._rtisToClear[0] = crd._oitRTIs0[0];
			crd._rtisToClear[1] = new RenderTargetIdentifier(crd.FogAlphaRT);
			crd._oitRTIsBackground[0] = new RenderTargetIdentifier(crd.MomentsRTBackground);
			crd._rtisToClearBackground[0] = crd._oitRTIsBackground[0];
			crd._rtisToClearBackground[1] = new RenderTargetIdentifier(crd.FogAlphaRTBackground);
		}
		if (crd.MomentsRT_Quarter == null)
		{
			crd.MomentsRT_Quarter = method_25(null, w / 4, h / 4, "MomentsRT_Q", momentsFormat, UAV: true);
			crd._oitQuarterRTIs[0] = new RenderTargetIdentifier(crd.MomentsRT_Quarter);
			crd._rtisQuarterToClear[0] = crd._oitQuarterRTIs[0];
		}
		if (crd.MomentsRT1 == null)
		{
			crd.MomentsRT1 = method_25(null, w, h, "MomentsRT1", momentsRcpFormat, UAV: true);
			crd.MomentsRT1Background = method_25(null, w, h, "MomentsRT1Background", momentsRcpFormat, UAV: true);
			crd._oitRTIs0[1] = new RenderTargetIdentifier(crd.MomentsRT1);
			crd._rtisToClear[2] = crd._oitRTIs0[1];
			crd._oitRTIsBackground[1] = new RenderTargetIdentifier(crd.MomentsRT1Background);
			crd._rtisToClearBackground[2] = crd._oitRTIsBackground[1];
		}
		if (crd.MomentsRT1_Quarter == null)
		{
			crd.MomentsRT1_Quarter = method_25(null, w / 2, h / 2, "MomentsRT1_Q", momentsFormat, UAV: true);
			crd._oitQuarterRTIs[1] = new RenderTargetIdentifier(crd.MomentsRT1_Quarter);
			crd._rtisQuarterToClear[1] = crd._oitQuarterRTIs[1];
		}
		if (crd.TransmittanceSumRcp == null)
		{
			crd.TransmittanceSumRcp = method_25(null, w, h, "TransmittanceSumRcp", momentsRcpFormat, UAV: true);
			crd.TransmittanceSumRcpBackground = method_25(null, w, h, "TransmittanceSumRcpBackground", momentsRcpFormat, UAV: true);
			crd._oitSumRcp = new RenderTargetIdentifier(crd.TransmittanceSumRcp);
			crd._oitSumRcpBackground = new RenderTargetIdentifier(crd.TransmittanceSumRcpBackground);
			crd._rtisToClear[3] = crd._oitSumRcp;
			crd._rtisToClearBackground[3] = crd._oitSumRcpBackground;
		}
		if (crd.TransmittanceSumRcpQuarter == null)
		{
			crd.TransmittanceSumRcpQuarter = method_25(null, w / 4, h / 4, "TransmittanceSumRcpQ", momentsRcpFormat, UAV: true);
			crd._oitSumRcpQuarter = new RenderTargetIdentifier(crd.TransmittanceSumRcpQuarter);
			crd._rtisQuarterToClear[2] = crd._oitSumRcpQuarter;
		}
		if (crd.OpaqueRT == null)
		{
			crd.OpaqueRT = method_25(null, w, h, "MBOITOpaque", momentsFormat, UAV: true);
			crd._opaqueRTI = new RenderTargetIdentifier(crd.OpaqueRT);
			crd.RenderTexturesWidth = w;
			crd.RenderTexturesHeight = h;
			crd.TransparentsRT = method_25(null, w, h, "MBOITTransparents", momentsFormat, UAV: true);
			crd._transparentsRTI = new RenderTargetIdentifier(crd.TransparentsRT);
		}
		if (crd.ForegroundRT == null)
		{
			crd.ForegroundRT = method_25(null, w, h, "ForegroundResult", momentsFormat, UAV: true);
			crd._foregroundMBOITResult = new RenderTargetIdentifier(crd.ForegroundRT);
		}
		if (crd.TransparentOutput_Quarter == null)
		{
			crd.TransparentOutput_Quarter = method_26(null, w / 4, h / 4, "MBOITOutputQuarter", RuntimeUtilities.defaultHDRRenderTextureFormat, UAV: true);
			crd._transparentOutputQuarterRTI = new RenderTargetIdentifier(crd.TransparentOutput_Quarter);
		}
		if (crd.TransparentOutput_QuarterBlurBuf == null)
		{
			crd.TransparentOutput_QuarterBlurBuf = method_26(null, w / 4, h / 4, "MBOITOutputQuarterBlurBuf", RuntimeUtilities.defaultHDRRenderTextureFormat, UAV: true);
			crd._transparentOutputQuarterBlurBufRTI = new RenderTargetIdentifier(crd.TransparentOutput_QuarterBlurBuf);
		}
		if (crd.TransparentOutput_HiRes == null)
		{
			crd.TransparentOutput_HiRes = method_26(null, w, h, "MBOITOutputBlurBuf", RuntimeUtilities.defaultHDRRenderTextureFormat, UAV: true);
		}
		if (crd.TransparentOutput_HiResBlurBuf == null)
		{
			crd.TransparentOutput_HiResBlurBuf = method_26(null, w, h, "MBOITOutputBlurBuf2", RuntimeUtilities.defaultHDRRenderTextureFormat, UAV: true);
		}
		if (crd.StubDepth == null)
		{
			crd.StubDepth = method_26(null, w, h, "MomentsDepth", RenderTextureFormat.Depth);
			crd._stubDepthRTI = new RenderTargetIdentifier(crd.StubDepth);
		}
		if (crd.StubDepth_Quarter == null)
		{
			crd.StubDepth_Quarter = method_26(null, w / 4, h / 4, "MomentsDepthQ", RenderTextureFormat.Depth);
			crd._stubDepthQuarterRTI = new RenderTargetIdentifier(crd.StubDepth_Quarter);
		}
		if (crd.ScatterBlurBuffer0 == null)
		{
			crd.ScatterBlurBuffer0 = method_26(null, w, h, "ScatterBlurBuffer0", RenderTextureFormat.RInt, UAV: true);
			crd.ScatterBlurBuffer1 = method_26(null, w, h, "ScatterBlurBuffer1", RenderTextureFormat.RInt, UAV: true);
		}
		crd._finalOutputTarget = ((crd._ssaa != null) ? crd._ssaa.GetRTIdentifier() : new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));
	}

	public void method_29(Class706 crd, Camera camera)
	{
		Transform transform = camera.transform;
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float fieldOfView = camera.fieldOfView;
		float aspect = camera.aspect;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = fieldOfView * 0.5f;
		Vector3 vector = transform.right * nearClipPlane * Mathf.Tan(num * (MathF.PI / 180f)) * aspect;
		Vector3 vector2 = transform.up * nearClipPlane * Mathf.Tan(num * (MathF.PI / 180f));
		Vector3 vector3 = transform.forward * nearClipPlane - vector + vector2;
		float num2 = vector3.magnitude * farClipPlane / nearClipPlane;
		vector3.Normalize();
		vector3 *= num2;
		Vector3 vector4 = transform.forward * nearClipPlane + vector + vector2;
		vector4.Normalize();
		vector4 *= num2;
		Vector3 vector5 = transform.forward * nearClipPlane + vector - vector2;
		vector5.Normalize();
		vector5 *= num2;
		Vector3 vector6 = transform.forward * nearClipPlane - vector - vector2;
		vector6.Normalize();
		vector6 *= num2;
		identity.SetRow(0, vector3);
		identity.SetRow(1, vector4);
		identity.SetRow(2, vector5);
		identity.SetRow(3, vector6);
		crd.CmdBufCulling.SetGlobalMatrix(int_52, identity);
	}

	public void method_30(ref Class706 crd, Camera camera)
	{
		crd.CmdBufCulling.Clear();
		crd.CmdBufBeforeTransparent.Clear();
		crd.CmdBufAfterTransparent.Clear();
		method_29(crd, camera);
		crd._mboitScattering = crd.MBOITScattering != null && crd.MBOITScattering.isActiveAndEnabled;
		crd._isScatterThermalVisionIsEnabled = crd.MBOITScattering != null && crd.MBOITScattering.ThermalVisionIsEnabled;
		crd._todScatteringEnabled = crd.MBOITScattering != null && crd.MBOITScattering.isActiveAndEnabled;
		if (class707_0._verticesInstances == null)
		{
			class707_0._verticesInstances = method_22(_sceneData._verticesInstances, PiecesVerticesRingSize);
		}
		if (class707_0._verticesLocalOffsets == null)
		{
			class707_0._verticesLocalOffsets = method_22(_sceneData._verticesLocalOffsets, PiecesVerticesRingSize);
		}
		if (class707_0._instancesMeshesIndices == null)
		{
			class707_0._instancesMeshesIndices = method_22(_instancesGeometry._meshIndex, PiecesInstancesRingSize);
		}
		if (class707_0._allVerticesBuffer == null)
		{
			class707_0._allVerticesBuffer = method_22(_geometryData._allVertices, PiecesVerticesRingSize);
		}
		if (class707_0._verticesIDsForeground == null)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < _sceneData._verticesInstances.Count + PiecesVerticesRingSize * 3; i++)
			{
				list.Add(i);
			}
			class707_0._verticesIDsForeground = method_22(list, 0, "WindowsIDsForeground");
			class707_0._verticesIDsBackground = method_22(list, 0, "WindowsIDsBackground");
		}
		if (class707_0._allTransformsBuffer == null)
		{
			class707_0._allTransformsBuffer = method_22(_instancesGeometry._allTransforms, PiecesInstancesRingSize);
		}
		if (class707_0._allTrianglesBuffer == null)
		{
			class707_0._allTrianglesBuffer = method_22(_geometryData._allTriangles, PiecesIndicesRingSize);
		}
		if (class707_0._allBoundsBuffer == null)
		{
			class707_0._allBoundsBuffer = method_22(_instancesGeometry._allBounds, PiecesInstancesRingSize * 2);
		}
		if (class707_0._meshesOffsets == null)
		{
			class707_0._meshesOffsets = method_22(_sceneData._meshesOffsetsList, PiecesInstancesRingSize);
		}
		if (class707_0._meshesVerticesCounts == null)
		{
			class707_0._meshesVerticesCounts = method_22(_sceneData._meshesVerticesCount, PiecesVerticesRingSize);
		}
		if (class707_0._instanceIdToStartVertexId == null)
		{
			class707_0._instanceIdToStartVertexId = method_22(_sceneData._instanceIdToStartVertexId, PiecesInstancesRingSize);
		}
		if (class707_0._texturesOffsetsBuffer == null)
		{
			class707_0._texturesOffsetsBuffer = method_22(_instancesGeometry.TexturesOffsetsList, PiecesInstancesRingSize);
		}
		if (class707_0._materialsProperties == null)
		{
			class707_0._materialsProperties = method_22(_materialsParameters, PiecesInstancesRingSize);
		}
		if (class707_0._lightsProperties == null)
		{
			class707_0._lightsProperties = method_22(_lightsProperties, 0);
		}
		if (class707_0._instancesEnable == null)
		{
			if (list_2.Count != _materialsParameters.Count)
			{
				list_2.Clear();
				for (int j = 0; j < _materialsParameters.Count; j++)
				{
					list_2.Add(1);
				}
				for (int k = _materialsParameters.Count; k < _materialsParameters.Count + PiecesInstancesRingSize; k++)
				{
					list_2.Add(0);
				}
			}
			class707_0._instancesEnable = method_22(list_2, 0);
		}
		if (crd.DrawArgsBufferForeground == null)
		{
			int[] data = new int[4]
			{
				_geometryData._allTriangles.Count,
				1,
				0,
				0
			};
			crd.DrawArgsBufferForeground = method_23(data, 0, "Windows_" + camera.name + "_drawArgsFrgrnd", ComputeBufferType.DrawIndirect);
			crd.DrawArgsBufferBackground = method_23(data, 0, "Windows_" + camera.name + "_drawArgsBckgrnd", ComputeBufferType.DrawIndirect);
		}
		if (crd.FrustumPlanes == null)
		{
			float farClipPlane = camera.farClipPlane;
			float nearClipPlane = camera.nearClipPlane;
			camera.farClipPlane = BackgroundSplitDistance;
			Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
			for (int l = 0; l < 6; l++)
			{
				vector4_0[l] = new Vector4(array[l].normal.x, array[l].normal.y, array[l].normal.z, array[l].distance);
			}
			camera.nearClipPlane = BackgroundSplitDistance;
			camera.farClipPlane = farClipPlane;
			array = GeometryUtility.CalculateFrustumPlanes(camera);
			for (int m = 0; m < 6; m++)
			{
				vector4_0[m + 6] = new Vector4(array[m].normal.x, array[m].normal.y, array[m].normal.z, array[m].distance);
			}
			camera.nearClipPlane = nearClipPlane;
			camera.farClipPlane = farClipPlane;
			crd.FrustumPlanes = method_23(vector4_0, 0);
		}
		if (crd.FrustumPNFactors == null)
		{
			smethod_4(camera, ref crd.PNFactorsBuffer);
			crd.FrustumPNFactors = method_23(crd.PNFactorsBuffer, 0);
		}
		if (crd.InstancesIDsAfterFrustumCullingForeground == null)
		{
			int[] array2 = new int[class707_0._allTransformsBuffer.count + PiecesInstancesRingSize];
			for (int n = 0; n < array2.Length; n++)
			{
				array2[n] = n;
			}
			crd.InstancesIDsAfterFrustumCullingForeground = method_23(array2, 0, "InstancesIDsAfterFrustumCullingForeground");
			crd.InstancesIDsAfterFrustumCullingBackground = method_23(array2, 0, "InstancesIDsAfterFrustumCullingBackground");
		}
		if (crd.InstancesIDsCountForeground == null)
		{
			int[] data2 = new int[3] { 0, 1, 1 };
			crd.InstancesIDsCountForeground = method_23(data2, 0, "Windows_" + camera.name + "_instancesIDsCountFrgrnd", ComputeBufferType.DrawIndirect);
			crd.InstancesIDsCountBackground = method_23(data2, 0, "Windows_" + camera.name + "_instancesIDsCountBckgrnd", ComputeBufferType.DrawIndirect);
		}
		if (crd.OcclusionComputeGroupsCountForegroundBuffer == null)
		{
			int[] data3 = new int[3] { 1, 1, 1 };
			crd.OcclusionComputeGroupsCountForegroundBuffer = method_23(data3, 0, "Windows_" + camera.name + "_occlusionGroupsCountFrgrnd", ComputeBufferType.DrawIndirect);
			crd.OcclusionComputeGroupsCountBackgroundBuffer = method_23(data3, 0, "Windows_" + camera.name + "_occlusionGroupsCountBckgrnd", ComputeBufferType.DrawIndirect);
		}
		if (crd.CameraProjViewMatrixBuffer == null)
		{
			crd.CameraProjViewMatrixBuffer = method_22(crd.ProjViewMatrix, 0);
		}
		if (crd.CameraParameters == null)
		{
			float_0[0] = camera.nearClipPlane;
			float_0[1] = camera.farClipPlane;
			float_0[2] = camera.farClipPlane - camera.nearClipPlane;
			float_0[3] = BackgroundSplitDistance;
			crd.CameraParameters = method_23(float_0, 0);
		}
		if (crd.WindowsMaterial == null)
		{
			crd.WindowsMaterial = new Material(WindowsMaterial);
		}
		if (crd.UnpackedOffsetsBuffer == null)
		{
			List<GeometryBuffers.Struct178> list2 = new List<GeometryBuffers.Struct178>();
			for (uint num = 0u; num < (uint)_sceneData._verticesInstances.Count; num++)
			{
				list2.Add(new GeometryBuffers.Struct178(num, num + 1));
			}
			crd.UnpackedOffsetsBuffer = method_22(list2, 0, "UnpackedOffsets", ComputeBufferType.Structured);
		}
		if (bool_1 && gclass1000_0 != null)
		{
			gclass1000_0.ProcessJobs();
		}
		crd.CmdBufAfterTransparent.Blit(BuiltinRenderTextureType.CameraTarget, crd._transparentsRTI);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_4, class707_0._verticesInstances);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_5, class707_0._verticesLocalOffsets);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_6, class707_0._instancesMeshesIndices);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_7, class707_0._meshesOffsets);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_8, class707_0._meshesVerticesCounts);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_9, class707_0._allVerticesBuffer);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_10, class707_0._allBoundsBuffer);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_11, class707_0._allTransformsBuffer);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_12, class707_0._allTrianglesBuffer);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_13, class707_0._texturesOffsetsBuffer);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_14, class707_0._instanceIdToStartVertexId);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_15, class707_0._materialsProperties);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_2, crd.CameraParameters);
		crd.CmdBufAfterTransparent.SetGlobalBuffer(int_16, class707_0._lightsProperties);
		crd.CmdBufAfterTransparent.SetGlobalFloat(int_17, 0.2f);
		crd.CmdBufAfterTransparent.SetGlobalFloat(int_18, 1f);
		crd.CmdBufAfterTransparent.SetGlobalFloat(int_19, 1f);
		crd.CmdBufAfterTransparent.SetGlobalTexture(int_20, _texture2DArray);
		crd.CmdBufAfterTransparent.SetGlobalTexture(int_21, _cubemapArray);
		crd.CmdBufAfterTransparent.SetGlobalFloat(int_1, 1f);
		GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat;
		int cameraWidth = crd.GetCameraWidth(camera);
		int cameraHeight = crd.GetCameraHeight(camera);
		method_28(ref crd, camera, cameraWidth, cameraHeight, GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormat.R16_SFloat);
		if (crd._ssaa != null)
		{
			crd._ssaa.SetAfterTransparentRT(crd.TransparentsRT);
		}
		if (crd._intermediateRT == null)
		{
			crd._intermediateRT = method_25(null, cameraWidth, cameraHeight, "IntermediateRT", format, UAV: true);
			crd._intermediateRTI = new RenderTargetIdentifier(crd._intermediateRT);
			crd._intermediateRTOut = method_25(null, cameraWidth, cameraHeight, "IntermediateRTOut", format, UAV: true);
			crd._rtisToClear[4] = crd._intermediateRTI;
			crd._rtisToClearBackground[4] = crd._intermediateRTI;
		}
		crd.CmdBufAfterTransparent.SetRenderTarget(crd._intermediateRTI, BuiltinRenderTextureType.CameraTarget);
		crd.CmdBufAfterTransparent.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		crd.CmdBufBeforeTransparent.SetGlobalFloat(int_1, 1f);
		crd.CmdBufBeforeTransparent.SetGlobalBuffer(int_2, crd.CameraParameters);
		crd.CmdBufBeforeTransparent.Blit(BuiltinRenderTextureType.CameraTarget, crd._opaqueRTI);
		crd.CmdBufAfterTransparent.ClearRandomWriteTargets();
		if (bool_1 && gclass1000_0 != null)
		{
			gclass1000_0.BeforeRendering(crd.CmdBufAfterTransparent);
		}
		if (crd.MBOITScatter())
		{
			float farClipPlane2 = camera.farClipPlane;
			float nearClipPlane2 = camera.nearClipPlane;
			for (int num2 = 0; num2 < float_1.Length; num2++)
			{
				float num3 = Mathf.Pow((float)num2 / 9f, crd.MBOITScattering.SlicesDistributionExponent) * crd.MBOITScattering.SlicesFarDistance;
				float f = (farClipPlane2 - nearClipPlane2) * num3 + nearClipPlane2;
				f = (Mathf.Log(f) - Mathf.Log(nearClipPlane2)) / (Mathf.Log(farClipPlane2) - Mathf.Log(nearClipPlane2)) * 2f - 1f;
				float_1[num2] = f;
			}
			crd.CmdBufBeforeTransparent.SetGlobalFloatArray(int_53, float_1);
			crd.CmdBufAfterTransparent.SetGlobalFloatArray(int_53, float_1);
		}
		for (int num4 = 0; num4 < 2; num4++)
		{
			string text = "Layer" + num4;
			crd.CmdBufAfterTransparent.BeginSample(text);
			ComputeBuffer value = ((num4 == 0) ? class707_0._verticesIDsForeground : class707_0._verticesIDsBackground);
			crd.CmdBufAfterTransparent.SetGlobalBuffer(int_3, value);
			crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOITOUT");
			bool flag = true;
			if (crd.MBOITScatter())
			{
				crd._distantShadowLowResDepth = (crd.DistantShadowComponent ? crd.DistantShadowComponent.LowResDepth : null);
				RenderTexture moments0UAV = ((num4 == 0) ? crd.MomentsRT : crd.MomentsRTBackground);
				RenderTexture moments1UAV = ((num4 == 0) ? crd.MomentsRT1 : crd.MomentsRT1Background);
				RenderTexture fogAlpha = ((num4 == 0) ? crd.FogAlphaRT : crd.FogAlphaRTBackground);
				crd.MBOITScattering.RenderMoments(crd.CmdBufAfterTransparent, crd._distantShadowLowResDepth, moments0UAV, moments1UAV, fogAlpha);
				flag = false;
			}
			if (flag)
			{
				RenderTargetIdentifier[] colors = ((num4 == 0) ? crd._rtisToClear : crd._rtisToClearBackground);
				crd.CmdBufAfterTransparent.SetRenderTarget(colors, BuiltinRenderTextureType.CameraTarget);
				crd.CmdBufAfterTransparent.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
			}
			RenderTargetIdentifier[] colors2 = ((num4 == 0) ? crd._oitRTIs0 : crd._oitRTIsBackground);
			crd.CmdBufAfterTransparent.SetRenderTarget(colors2, BuiltinRenderTextureType.CameraTarget);
			if (bool_1 && gclass1000_0 != null)
			{
				if (crd._particlesRenderers != null)
				{
					crd.CmdBufAfterTransparent.BeginSample("Particles_Moments");
					method_31(crd.CmdBufAfterTransparent, "MBOIT_PARTICLES_MOMENTS");
					gclass1000_0.RenderParticles(crd.CmdBufAfterTransparent, camera);
					crd.CmdBufBeforeTransparent.SetRandomWriteTarget(2, gclass1000_0.ParticlesVertices);
					crd.CmdBufBeforeTransparent.SetRandomWriteTarget(3, gclass1000_0.IndirectDataBuffer);
					crd.CmdBufBeforeTransparent.SetRandomWriteTarget(4, gclass1000_0.ParticlesOffsets);
					crd.CmdBufAfterTransparent.EndSample("Particles_Moments");
				}
				method_31(crd.CmdBufBeforeTransparent, "MBOIT_PARTICLES_ON");
			}
			else
			{
				method_31(crd.CmdBufBeforeTransparent, "MBOIT_PARTICLES_OFF");
			}
			crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOIT_OUT");
			if (num4 == 1)
			{
				crd.CmdBufAfterTransparent.EnableShaderKeyword("UNPACK_OFFSETS");
				crd.WindowsMaterial.SetBuffer(int_51, crd.UnpackedOffsetsBuffer);
				crd.CmdBufAfterTransparent.SetRandomWriteTarget(2, crd.UnpackedOffsetsBuffer);
			}
			ComputeBuffer bufferWithArgs = ((num4 == 0) ? crd.DrawArgsBufferForeground : crd.DrawArgsBufferBackground);
			crd.CmdBufAfterTransparent.DrawProceduralIndirect(Matrix4x4.identity, crd.WindowsMaterial, 0, MeshTopology.Triangles, bufferWithArgs);
			crd.CmdBufAfterTransparent.DisableShaderKeyword("UNPACK_OFFSETS");
			crd.CmdBufAfterTransparent.DisableShaderKeyword("MBOIT_OUT");
			crd.CmdBufAfterTransparent.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_22, crd.MomentsRT);
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_23, crd.MomentsRT1);
			if (num4 == 0)
			{
				crd.CmdBufAfterTransparent.SetComputeTextureParam(FrustumCullShader, 3, "Moments1", crd.MomentsRT1);
				crd.CmdBufAfterTransparent.SetComputeTextureParam(FrustumCullShader, 3, "Moments1LowRes", crd.MomentsRT1_Quarter);
				crd.CmdBufAfterTransparent.SetComputeIntParams(FrustumCullShader, "MomentsLowResResolution", crd.MomentsRT1.width / 2, crd.MomentsRT1.height / 2);
				crd.CmdBufAfterTransparent.DispatchCompute(FrustumCullShader, 3, crd.MomentsRT1.width / 2 / 8 + 1, crd.MomentsRT1.height / 2 / 8 + 1, 1);
				crd.CmdBufAfterTransparent.SetGlobalTexture("zeroth_moment_foreground_lowres", crd.MomentsRT1_Quarter);
			}
			else
			{
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_23, crd.MomentsRT1Background);
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_24, crd.MomentsRT1);
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_25, crd._opaqueRTI);
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_26, BuiltinRenderTextureType.ResolvedDepth);
				crd.CmdBufAfterTransparent.SetGlobalTexture("_MBOITTransparents", crd.TransparentsRT);
				crd.CmdBufAfterTransparent.SetGlobalTexture("_MBOITScatterAlphaForeground", crd.FogAlphaRT);
				crd.CmdBufAfterTransparent.SetGlobalTexture("_MBOITScatterAlphaBackground", crd.FogAlphaRTBackground);
				crd.CmdBufAfterTransparent.SetGlobalInt("FogBlurScamplesCountRoot", FogBlurSamplesCount);
				crd.CmdBufAfterTransparent.SetGlobalFloat("FogBlurRadiusScale", FogBlurRadiusScale);
				crd.CmdBufAfterTransparent.Blit(crd._opaqueRTI, crd._intermediateRTI, MBOITBlitOpaque, 0);
			}
			crd.CmdBufAfterTransparent.SetGlobalFloat(int_1, -1f);
			crd.CmdBufAfterTransparent.EndSample(text);
		}
		crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOIT");
		crd.CmdBufAfterTransparent.ClearRandomWriteTargets();
		crd.CmdBufCulling.SetComputeIntParam(FrustumCullShader, int_27, class707_0._allTransformsBuffer.count);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_28, crd.FrustumPlanes);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_29, crd.FrustumPNFactors);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_6, class707_0._instancesMeshesIndices);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_8, class707_0._meshesVerticesCounts);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_30, class707_0._allBoundsBuffer);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_14, class707_0._instanceIdToStartVertexId);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_31, crd.InstancesIDsCountForeground);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_32, crd.InstancesIDsAfterFrustumCullingForeground);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_33, crd.InstancesIDsCountBackground);
		crd.CmdBufCulling.SetComputeBufferParam(FrustumCullShader, 2, int_34, crd.InstancesIDsAfterFrustumCullingBackground);
		crd.CmdBufCulling.DispatchCompute(FrustumCullShader, 2, (class707_0._allTransformsBuffer.count >> 6) + 1, 1, 1);
		crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 1, int_35, crd.InstancesIDsCountForeground);
		crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 1, int_36, crd.OcclusionComputeGroupsCountForegroundBuffer);
		crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 1, int_37, crd.InstancesIDsCountBackground);
		crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 1, int_38, crd.OcclusionComputeGroupsCountBackgroundBuffer);
		crd.CmdBufCulling.DispatchCompute(OcclusionCullShader, 1, 1, 1, 1);
		for (int num5 = 0; num5 < 2; num5++)
		{
			ComputeBuffer buffer = ((num5 == 0) ? crd.InstancesIDsCountForeground : crd.InstancesIDsCountBackground);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_27, buffer);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_6, class707_0._instancesMeshesIndices);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_8, class707_0._meshesVerticesCounts);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_30, class707_0._allBoundsBuffer);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_14, class707_0._instanceIdToStartVertexId);
			ComputeBuffer buffer2 = ((num5 == 0) ? crd.DrawArgsBufferForeground : crd.DrawArgsBufferBackground);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_39, buffer2);
			ComputeBuffer buffer3 = ((num5 == 0) ? class707_0._verticesIDsForeground : class707_0._verticesIDsBackground);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_40, buffer3);
			ComputeBuffer buffer4 = ((num5 == 0) ? crd.InstancesIDsAfterFrustumCullingForeground : crd.InstancesIDsAfterFrustumCullingBackground);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_41, buffer4);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_42, crd.CameraProjViewMatrixBuffer);
			crd.CmdBufCulling.SetComputeBufferParam(OcclusionCullShader, 0, int_43, class707_0._instancesEnable);
			crd.CmdBufCulling.SetComputeIntParam(OcclusionCullShader, int_44, OcclusionAccuracy);
			crd.CmdBufCulling.SetComputeFloatParam(OcclusionCullShader, int_45, OcclusionOffset);
			crd.CmdBufCulling.SetComputeTextureParam(OcclusionCullShader, 0, int_46, crd.HiZGenerator.hiZDepthTexture);
			crd.CmdBufCulling.SetComputeVectorParam(OcclusionCullShader, int_47, crd.HiZGenerator.hiZTextureSize);
			ComputeBuffer indirectBuffer = ((num5 == 0) ? crd.OcclusionComputeGroupsCountForegroundBuffer : crd.OcclusionComputeGroupsCountBackgroundBuffer);
			crd.CmdBufCulling.DispatchCompute(OcclusionCullShader, 0, indirectBuffer, 0u);
		}
		crd.CmdBufAfterTransparent.SetGlobalFloat("MBOITForeground", 1f);
		for (int num6 = 0; num6 < 2; num6++)
		{
			string text2 = "Layer" + num6;
			crd.CmdBufAfterTransparent.BeginSample(text2);
			crd.CmdBufAfterTransparent.ClearRandomWriteTargets();
			crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOIT");
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_22, crd.MomentsRT);
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_23, crd.MomentsRT1);
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_48, crd.MomentsRT_Quarter);
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_49, crd.MomentsRT1_Quarter);
			ComputeBuffer value2 = ((num6 == 0) ? class707_0._verticesIDsForeground : class707_0._verticesIDsBackground);
			crd.CmdBufAfterTransparent.SetGlobalBuffer(int_3, value2);
			crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOITOUT");
			if (num6 == 1)
			{
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_22, crd.MomentsRTBackground);
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_23, crd.MomentsRT1Background);
				crd.CmdBufAfterTransparent.SetGlobalTexture(int_24, crd.MomentsRT1);
			}
			crd.CmdBufAfterTransparent.EnableShaderKeyword("WINDOWS_CULLING");
			crd.CmdBufAfterTransparent.BeginSample("CONSERVATIVE_OUT");
			crd.CmdBufAfterTransparent.DisableShaderKeyword("MBOIT");
			crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOIT_ENERGY_CONSERVATIVE_OUT");
			if (crd.MBOITScatter())
			{
				RenderTexture moments0UAV2 = ((num6 == 0) ? crd.MomentsRT : crd.MomentsRTBackground);
				RenderTexture moments1UAV2 = ((num6 == 0) ? crd.MomentsRT1 : crd.MomentsRT1Background);
				RenderTexture momentsRcp = ((num6 == 0) ? crd.TransmittanceSumRcp : crd.TransmittanceSumRcpBackground);
				crd.MBOITScattering.RenderNormalizationFactor(crd.CmdBufAfterTransparent, moments0UAV2, moments1UAV2, momentsRcp);
			}
			RenderTargetIdentifier color = ((num6 == 0) ? crd._oitSumRcp : crd._oitSumRcpBackground);
			crd.CmdBufAfterTransparent.SetRenderTarget(color, BuiltinRenderTextureType.CameraTarget);
			if (num6 == 1)
			{
				crd.CmdBufAfterTransparent.EnableShaderKeyword("USE_UNPACKED_OFFSETS1");
				crd.CmdBufAfterTransparent.SetGlobalBuffer(int_51, crd.UnpackedOffsetsBuffer);
			}
			ComputeBuffer bufferWithArgs2 = ((num6 == 0) ? crd.DrawArgsBufferForeground : crd.DrawArgsBufferBackground);
			crd.CmdBufAfterTransparent.DrawProceduralIndirect(Matrix4x4.identity, crd.WindowsMaterial, 0, MeshTopology.Triangles, bufferWithArgs2);
			crd.CmdBufAfterTransparent.ClearRandomWriteTargets();
			crd.CmdBufAfterTransparent.DisableShaderKeyword("USE_UNPACKED_OFFSETS1");
			if (bool_1 && gclass1000_0 != null && crd._particlesRenderers != null)
			{
				crd.CmdBufAfterTransparent.BeginSample("Particles_MomentsNorm");
				method_31(crd.CmdBufAfterTransparent, "MBOIT_PARTICLES_NORM");
				gclass1000_0.RenderParticles(crd.CmdBufAfterTransparent, camera);
				crd.CmdBufAfterTransparent.EndSample("Particles_MomentsNorm");
			}
			crd.CmdBufAfterTransparent.DisableShaderKeyword("MBOIT_ENERGY_CONSERVATIVE_OUT");
			crd.CmdBufAfterTransparent.EnableShaderKeyword("MBOIT_ENERGY_CONSERVATIVE_IN");
			if (crd.MBOITScatter())
			{
				RenderTexture moments0UAV3 = ((num6 == 0) ? crd.MomentsRT : crd.MomentsRTBackground);
				RenderTexture moments1UAV3 = ((num6 == 0) ? crd.MomentsRT1 : crd.MomentsRT1Background);
				RenderTexture momentsRcp2 = ((num6 == 0) ? crd.TransmittanceSumRcp : crd.TransmittanceSumRcpBackground);
				crd.MBOITScattering.RenderWithMBOIT(crd.CmdBufAfterTransparent, moments0UAV3, moments1UAV3, momentsRcp2, crd.TransparentOutput_HiRes);
			}
			crd.CmdBufAfterTransparent.SetRenderTarget(crd._intermediateRTI, BuiltinRenderTextureType.CameraTarget);
			RenderTexture renderTexture = ((num6 == 0) ? crd.TransmittanceSumRcp : crd.TransmittanceSumRcpBackground);
			crd.CmdBufAfterTransparent.SetGlobalTexture(int_50, renderTexture);
			method_27(crd.CmdBufAfterTransparent, crd.TransparentOutput_HiRes);
			crd.CmdBufAfterTransparent.EndSample("CONSERVATIVE_OUT");
			crd.CmdBufAfterTransparent.BeginSample("MBOIT_FINAL_OUTPUT");
			if (bool_1 && gclass1000_0 != null)
			{
				if (crd._particlesRenderers != null)
				{
					crd.CmdBufAfterTransparent.BeginSample("Particles_MBOITFinal");
					method_31(crd.CmdBufAfterTransparent, "MBOIT_PARTICLES_OUT");
					gclass1000_0.RenderParticles(crd.CmdBufAfterTransparent, camera);
					crd.CmdBufAfterTransparent.EndSample("Particles_MBOITFinal");
				}
				method_31(crd.CmdBufAfterTransparent, "MBOIT_PARTICLES_OFF");
			}
			if (num6 == 1)
			{
				crd.CmdBufAfterTransparent.EnableShaderKeyword("USE_UNPACKED_OFFSETS2");
				crd.CmdBufAfterTransparent.SetGlobalBuffer(int_51, crd.UnpackedOffsetsBuffer);
			}
			ComputeBuffer bufferWithArgs3 = ((num6 == 0) ? crd.DrawArgsBufferForeground : crd.DrawArgsBufferBackground);
			crd.CmdBufAfterTransparent.DrawProceduralIndirect(Matrix4x4.identity, crd.WindowsMaterial, 0, MeshTopology.Triangles, bufferWithArgs3);
			crd.CmdBufAfterTransparent.DisableShaderKeyword("USE_UNPACKED_OFFSETS2");
			crd.CmdBufAfterTransparent.DisableShaderKeyword("MBOIT_ENERGY_CONSERVATIVE_IN");
			crd.CmdBufAfterTransparent.EndSample("MBOIT_FINAL_OUTPUT");
			crd.CmdBufAfterTransparent.SetGlobalFloat("MBOITForeground", -1f);
			crd.CmdBufAfterTransparent.EndSample(text2);
		}
		SharedGameSettingsClass instance = Singleton<SharedGameSettingsClass>.Instance;
		if (instance != null)
		{
			ScatterBlur = instance.Graphics.Settings.HighQualityFog;
		}
		if (ScatterBlur)
		{
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 0, "FogAlphaBackground", crd.FogAlphaRT);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 0, "FogAlphaForeground", crd.FogAlphaRTBackground);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 0, "Source", crd._intermediateRT);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 0, "rwBuffer0", crd.ScatterBlurBuffer0);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 0, "rwBuffer1", crd.ScatterBlurBuffer1);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 0, "Result", crd._intermediateRTOut);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 1, "FogAlphaBackground", crd.FogAlphaRT);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 1, "FogAlphaForeground", crd.FogAlphaRTBackground);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 1, "Source", crd._intermediateRT);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 1, "rwBuffer0", crd.ScatterBlurBuffer0);
			crd.CmdBufAfterTransparent.SetComputeTextureParam(MBOITFogBlurShader, 1, "rwBuffer1", crd.ScatterBlurBuffer1);
			int cameraWidth2 = crd.GetCameraWidth(camera);
			int cameraHeight2 = crd.GetCameraHeight(camera);
			crd.CmdBufAfterTransparent.SetComputeVectorParam(MBOITFogBlurShader, "Resolution", new Vector4(cameraWidth2, cameraHeight2, 1f / (float)cameraWidth2, 1f / (float)cameraHeight2));
			crd.CmdBufAfterTransparent.DispatchCompute(MBOITFogBlurShader, 1, cameraWidth2, cameraHeight2, 1);
			crd.CmdBufAfterTransparent.DispatchCompute(MBOITFogBlurShader, 0, cameraWidth2, cameraHeight2, 1);
			crd.CmdBufAfterTransparent.SetGlobalTexture("intBuffer0", crd.ScatterBlurBuffer0);
			crd.CmdBufAfterTransparent.SetGlobalTexture("intBuffer1", crd.ScatterBlurBuffer1);
			crd.CmdBufAfterTransparent.SetGlobalVector("intBufferResolution", new Vector4(crd.ScatterBlurBuffer0.width, crd.ScatterBlurBuffer0.height, 1f / (float)crd.ScatterBlurBuffer0.width, 1f / (float)crd.ScatterBlurBuffer0.height));
			crd.CmdBufAfterTransparent.Blit(crd._intermediateRTOut, crd._finalOutputTarget, MBOITBlitOpaque, 1);
		}
		else
		{
			crd.CmdBufAfterTransparent.Blit(crd._intermediateRT, crd._finalOutputTarget);
		}
		crd.CmdBufAfterTransparent.SetComputeBufferParam(FrustumCullShader, 1, int_31, crd.DrawArgsBufferForeground);
		crd.CmdBufAfterTransparent.SetComputeBufferParam(FrustumCullShader, 1, int_33, crd.DrawArgsBufferBackground);
		crd.CmdBufAfterTransparent.SetComputeBufferParam(FrustumCullShader, 1, int_32, crd.InstancesIDsCountForeground);
		crd.CmdBufAfterTransparent.SetComputeBufferParam(FrustumCullShader, 1, int_34, crd.InstancesIDsCountBackground);
		crd.CmdBufAfterTransparent.DispatchCompute(FrustumCullShader, 1, 1, 1, 1);
		if (bool_1 && gclass1000_0 != null)
		{
			gclass1000_0.ClearIndirectDrawData(crd.CmdBufAfterTransparent);
		}
		crd.CmdBufAfterTransparent.DisableShaderKeyword("MBOIT");
		crd.CmdBufAfterTransparent.ClearRandomWriteTargets();
	}

	public void EnableMBOITParticles(bool enable)
	{
		bool_1 = enable;
		ClearRenderData(releaseTextures: false);
		PopulateRenderersForOITMomentsLists();
	}

	public bool TryGetCameraParameters(Camera cam, out ComputeBuffer parameters)
	{
		Class706 value;
		bool flag = dictionary_0.TryGetValue(cam.GetInstanceID(), out value);
		parameters = (flag ? value.CameraParameters : null);
		return flag;
	}

	public void method_31(CommandBuffer cmdBuf, string keyword)
	{
		cmdBuf.DisableShaderKeyword("MBOIT_PARTICLES_OFF");
		cmdBuf.DisableShaderKeyword("MBOIT_PARTICLES_ON");
		cmdBuf.DisableShaderKeyword("MBOIT_PARTICLES_MOMENTS");
		cmdBuf.DisableShaderKeyword("MBOIT_PARTICLES_NORM");
		cmdBuf.DisableShaderKeyword("MBOIT_PARTICLES_OUT");
		cmdBuf.EnableShaderKeyword(keyword);
	}

	public void BreakWindow(string breakerId)
	{
		if (!IsActive || !dictionary_5.ContainsKey(breakerId))
		{
			return;
		}
		if (class707_0._instancesEnable != null)
		{
			int computeBufferStartIndex = dictionary_5[breakerId];
			if (list_17.Count != 0)
			{
				list_17[0] = 0;
			}
			else
			{
				list_17.Add(0);
			}
			class707_0._instancesEnable.SetData(list_17, 0, computeBufferStartIndex, 1);
		}
		else
		{
			list_3.Add(breakerId);
		}
	}

	public void PopulateRenderersForOITMomentsLists()
	{
		List<Renderer> list = new List<Renderer>();
		for (int i = 0; i < int_0; i++)
		{
			particleSystemRenderer_0[i] = null;
		}
		dictionary_2.Clear();
		hashSet_0.Clear();
		hashSet_1.Clear();
		bool_0 = true;
		if (bool_1)
		{
			List<ParticleSystemRenderer> list2 = FindSuitableParticlesSources();
			if (list2 != null)
			{
				foreach (ParticleSystemRenderer item in list2)
				{
					list.Add(item);
				}
			}
		}
		renderer_0 = list.ToArray();
		if (gclass1000_0 != null)
		{
			gclass1000_0.Cleanup();
		}
		gclass1000_0 = null;
		method_2();
	}

	public List<ParticleSystemRenderer> FindSuitableParticlesSources()
	{
		List<ParticleSystemRenderer> list = new List<ParticleSystemRenderer>();
		ParticleSystemRenderer[] array = UnityEngine.Object.FindObjectsOfType<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer particleSystemRenderer in array)
		{
			if (IsParticleSystemRendererSuitableForMBOIT(particleSystemRenderer))
			{
				list.Add(particleSystemRenderer);
			}
		}
		return list;
	}

	public static bool IsParticleSystemRendererSuitableForMBOIT(ParticleSystemRenderer psr)
	{
		if (psr == null)
		{
			return false;
		}
		Material sharedMaterial = psr.sharedMaterial;
		if (sharedMaterial == null)
		{
			return false;
		}
		return hashSet_2.Contains(sharedMaterial.shader.name);
	}

	public static void smethod_5(int count)
	{
		ParticleSystemRenderer[] destinationArray = new ParticleSystemRenderer[particleSystemRenderer_0.Length + count];
		Array.Copy(particleSystemRenderer_0, destinationArray, particleSystemRenderer_0.Length);
		particleSystemRenderer_0 = destinationArray;
	}

	public static void AddParticleSystem(ParticleSystem ps)
	{
		if (dictionary_2.Count == 0)
		{
			ParticleSystemRenderer component = ps.GetComponent<ParticleSystemRenderer>();
			dictionary_2.Add(component.GetInstanceID(), 0);
			particleSystemRenderer_0 = new ParticleSystemRenderer[10];
			particleSystemRenderer_0[0] = component;
			int_0 = 1;
		}
		else
		{
			if (int_0 == particleSystemRenderer_0.Length)
			{
				smethod_5(10);
			}
			ParticleSystemRenderer component2 = ps.GetComponent<ParticleSystemRenderer>();
			int instanceID = component2.GetInstanceID();
			if (!dictionary_2.ContainsKey(instanceID))
			{
				dictionary_2.Add(instanceID, int_0);
				particleSystemRenderer_0[int_0] = component2;
				int_0++;
			}
		}
		if (!hashSet_0.Contains(ps))
		{
			hashSet_0.Add(ps);
			bool_0 = true;
		}
	}

	public static void AddParticleSystemsList(List<ParticleSystem> psList)
	{
		if (dictionary_2.Count == 0)
		{
			particleSystemRenderer_0 = new ParticleSystemRenderer[psList.Count + 10];
			int_0 = 0;
		}
		if (psList.Count > particleSystemRenderer_0.Length - int_0)
		{
			smethod_5(psList.Count + 10);
		}
		for (int i = 0; i < psList.Count; i++)
		{
			ParticleSystemRenderer component = psList[i].GetComponent<ParticleSystemRenderer>();
			int instanceID = component.GetInstanceID();
			if (!dictionary_2.ContainsKey(instanceID) && IsParticleSystemRendererSuitableForMBOIT(component))
			{
				dictionary_2.Add(instanceID, int_0);
				particleSystemRenderer_0[int_0] = component;
				int_0++;
			}
		}
		for (int j = 0; j < psList.Count; j++)
		{
			if (!hashSet_0.Contains(psList[j]))
			{
				ParticleSystem particleSystem = psList[j];
				if (IsParticleSystemRendererSuitableForMBOIT(particleSystem.GetComponent<ParticleSystemRenderer>()))
				{
					hashSet_0.Add(particleSystem);
					bool_0 = true;
				}
			}
		}
	}

	public static void RemoveParticleSystemsList(List<ParticleSystem> psList)
	{
		for (int i = 0; i < psList.Count; i++)
		{
			ParticleSystemRenderer component = psList[i].GetComponent<ParticleSystemRenderer>();
			int instanceID = component.GetInstanceID();
			if (!dictionary_2.ContainsKey(instanceID) && IsParticleSystemRendererSuitableForMBOIT(component))
			{
				int num = dictionary_2[instanceID];
				int num2 = Math.Max(int_0 - 1, 0);
				particleSystemRenderer_0[num] = particleSystemRenderer_0[num2];
				particleSystemRenderer_0[num2] = null;
				dictionary_2[particleSystemRenderer_0[num].GetInstanceID()] = num;
				int_0--;
			}
		}
		for (int j = 0; j < psList.Count; j++)
		{
			if (hashSet_0.Contains(psList[j]))
			{
				hashSet_0.Remove(psList[j]);
				bool_0 = true;
			}
		}
	}

	public static void RemoveParticleSystem(ParticleSystem ps)
	{
		int instanceID = ps.GetComponent<ParticleSystemRenderer>().GetInstanceID();
		if (dictionary_2.ContainsKey(instanceID))
		{
			int num = dictionary_2[instanceID];
			dictionary_2.Remove(instanceID);
			int num2 = Math.Max(int_0 - 1, 0);
			particleSystemRenderer_0[num] = particleSystemRenderer_0[num2];
			particleSystemRenderer_0[num2] = null;
			if (particleSystemRenderer_0[num] != null)
			{
				dictionary_2[particleSystemRenderer_0[num].GetInstanceID()] = num;
			}
			int_0--;
		}
		if (hashSet_0.Contains(ps))
		{
			hashSet_0.Remove(ps);
			bool_0 = true;
		}
	}
}
