using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bsg.GameSettings;
using Comfort.Common;
using EFT.Impostors;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class DistantShadow : MonoBehaviour
{
	public enum ResolutionState
	{
		QUARTER,
		HALF,
		FULL
	}

	public class Class613
	{
		public Vector3 _cachedLightDirection = new Vector3(0f, -1f, 0f);

		public Vector3 _cachedShadowCameraPosition = new Vector3(0f, -1f, 0f);

		public Vector3 _cachedShadowCameraForward = new Vector3(0f, 0f, 0f);

		public Vector3 _cachedShadowCameraRight = new Vector3(0f, 0f, 0f);

		public Vector3 _cachedShadowCameraUp = new Vector3(0f, 0f, 0f);

		public Matrix4x4 _cachedGlobalShadowWorldToScreen = Matrix4x4.identity;

		public Matrix4x4 _cachedGlobalShadowWorldToView = Matrix4x4.identity;

		public Matrix4x4 _cachedGlobalShadowProjGPU = Matrix4x4.identity;

		public Matrix4x4 _cachedGlobalShadowProj = Matrix4x4.identity;

		public float _cachedNearPlane = 1f;

		public float _cachedFarPlane = 1f;

		public bool _buffersDataIsPropagated;

		public Matrix4x4[] _cachedGlobalShadowWorldToScreenData;

		public Matrix4x4[] _cachedGlobalShadowWorldToViewData;

		public Matrix4x4[] _cachedGlobalShadowProjData;

		public Vector4[] _cachedOffsetScaleData;

		public ComputeBuffer _cachedGlobalShadowWorldToScreenBuffer;

		public ComputeBuffer _cachedGlobalShadowWorldToViewBuffer;

		public ComputeBuffer _cachedGlobalShadowProjBuffer;

		public ComputeBuffer _cachedOffsetScaleBuffer;
	}

	public class Class614
	{
		public Mesh mesh;

		public Material material;

		public int submeshIdx;

		public Matrix4x4 drawTransform;
	}

	private static readonly int int_0 = Shader.PropertyToID("GlobalShadowSampled");

	private static readonly int int_1 = Shader.PropertyToID("GlobalShadowWorldToScreen");

	private static readonly int int_2 = Shader.PropertyToID("GlobalShadowWorldToView");

	private static readonly int int_3 = Shader.PropertyToID("GlobalShadowProjection");

	private static readonly int int_4 = Shader.PropertyToID("GlobalShadowSettings");

	private static readonly int int_5 = Shader.PropertyToID("GlobalShadowSampled2");

	private static readonly int int_6 = Shader.PropertyToID("GlobalShadowWorldToScreen2");

	private static readonly int int_7 = Shader.PropertyToID("GlobalShadowWorldToView2");

	private static readonly int int_8 = Shader.PropertyToID("GlobalShadowProjection2");

	private static readonly int int_9 = Shader.PropertyToID("GlobalShadowProj");

	private static readonly int int_10 = Shader.PropertyToID("GlobalShadowTexelSize");

	private static readonly int int_11 = Shader.PropertyToID("DebugParams");

	private static readonly int int_12 = Shader.PropertyToID("GlobalShadowL0");

	private static readonly int int_13 = Shader.PropertyToID("GlobalShadowL1");

	private static readonly int int_14 = Shader.PropertyToID("ParallaxSearchMul");

	private static readonly int int_15 = Shader.PropertyToID("DepthTexture_TexelSize");

	private static readonly int int_16 = Shader.PropertyToID("GlobalShadowL0Old");

	private static readonly int int_17 = Shader.PropertyToID("HaltonOffsets");

	private static readonly int int_18 = Shader.PropertyToID("ShadowMaskLowRes");

	private static readonly int int_19 = Shader.PropertyToID("_BlurMask");

	private static readonly int int_20 = Shader.PropertyToID("_MainTex");

	private static readonly int int_21 = Shader.PropertyToID("PreComputedGlobalShadow");

	private static readonly int int_22 = Shader.PropertyToID("_LowResDepth");

	private static readonly int int_23 = Shader.PropertyToID("DSBlurThreshold");

	public Camera MasterCamera;

	private SSAA ssaa_0;

	public bool EnableShadowsBlend = true;

	public bool EnableAdaptiveBias;

	public float AdaptiveBiasK = 0.0003f;

	public float ConstantBias = 0.001f;

	public float PCFShift = 0.0003f;

	public int ShadowPixelsSize = 4096;

	public bool DisableWind = true;

	public bool ShouldApplySettings = true;

	public float GlobalShadowHalfSize = 500f;

	public float OffsetTowardsLight = 100f;

	public float BlendTime = 0.5f;

	public float LightDirectionExtrapolateFactor = 1.5f;

	public float ObjectSizeFilter = 1f;

	public bool FixGlowEffect = true;

	public bool EnableDistantShadowKeyword = true;

	public bool EnableDistantShadowPCF;

	public bool EnableMultiviewTiles;

	public bool EnablePCFShift;

	public bool ReduceBlurSamples = true;

	public float BlurDepthThreshold = 1f;

	[NonSerialized]
	public bool InvalidateRenderersOnNextUpdate = true;

	private float float_0;

	private float float_1;

	private float float_2;

	private bool bool_0;

	public bool PreComputeMask;

	public bool EnableShadowmaskBlur = true;

	public bool EnableBlurMask = true;

	public bool EnableParallax;

	private float float_3 = 300000f;

	private bool bool_1 = true;

	[Range(0.01f, 1f)]
	private float float_4 = 0.5f;

	public static int ObjectsToRenderInfo = 0;

	public static int ObjectsToRenderPerFrameInfo = 0;

	public static float LastTimeToUpdateInfo = 0f;

	public float LastTimeToUpdate;

	private float float_5;

	public float NearPlane = 0.1f;

	public float FarPlane = 500f;

	public int ObjectsPerFrame = 150;

	private int int_24 = 2;

	private int int_25 = 1;

	private float float_6;

	private const float float_7 = 10f;

	public bool UseGaussDistribution = true;

	public float SamplingRotation = 60f;

	private uint uint_0 = 100u;

	private CommandBuffer commandBuffer_0;

	private CommandBuffer commandBuffer_1;

	private CommandBuffer commandBuffer_2;

	private CommandBuffer commandBuffer_3;

	private CommandBuffer commandBuffer_4;

	private CommandBuffer commandBuffer_5;

	private CommandBuffer commandBuffer_6;

	private CommandBuffer commandBuffer_7;

	private RenderTexture renderTexture_0;

	private RenderTexture renderTexture_1;

	private const GraphicsFormat graphicsFormat_0 = GraphicsFormat.R32_SFloat;

	private RenderTexture renderTexture_2;

	private RenderTexture renderTexture_3;

	private RenderTexture renderTexture_4;

	private RenderTexture renderTexture_5;

	private const GraphicsFormat graphicsFormat_1 = GraphicsFormat.R8_UNorm;

	private RenderTexture renderTexture_6;

	private RenderTexture renderTexture_7;

	private RenderTexture renderTexture_8;

	private RenderTexture renderTexture_9;

	private const int int_26 = 25;

	private ComputeBuffer computeBuffer_0;

	private int int_27;

	private int int_28 = 1;

	private int int_29 = 2;

	private float float_8;

	private int int_30;

	private int int_31;

	private string string_0 = "DISTANT_SHADOWS_BLEND";

	private string string_1 = "ADAPTIVE_DEPTH_BIAS";

	private string string_2 = "ENABLE_DISTANT_SHADOW";

	private string string_3 = "ENABLE_DISTANT_SHADOW_PCF";

	private string string_4 = "DISTANT_SHADOW_MULTIPROJECTION";

	private string string_5 = "ENABLE_PCF_SHIFT";

	private string string_6 = "DISABLE_WIND_EFT";

	private string string_7 = "RENDER_FOR_SCOPE";

	private string string_8 = "USE_GAUSS_DISTRIBUTION";

	private string string_9 = "PRECOMPUTE_MASK";

	private string string_10 = "ENABLE_PARALLAX";

	private string string_11 = "REDUCE_SAMPLES";

	private string string_12 = "ENABLE_BLUR_MASK";

	private string string_13 = "DISTANT_SHADOW_FIX_GLOW";

	private int int_32 = Shader.PropertyToID("GlobalShadow");

	private Camera camera_0;

	public ResolutionState CurrentMaskResolution;

	private int int_33 = 4;

	private List<Class613> list_0 = new List<Class613>();

	private float float_9;

	private float float_10;

	private Vector3 vector3_0 = new Vector3(0f, 1f, 0f);

	private RenderTexture[] renderTexture_10;

	private int int_34 = 3;

	private RenderTexture renderTexture_11;

	private List<Class614> list_1 = new List<Class614>();

	private MaterialPropertyBlock materialPropertyBlock_0;

	private MaterialPropertyBlock materialPropertyBlock_1;

	public Material DownscaleDepthMaterial;

	public Material ComputeShadowMaskMaterial;

	public Material UpscaleShadowMaskMaterial;

	public Material ShadowMaskBlurMaterial;

	private Mesh mesh_0;

	public RenderTexture LowResDepth => renderTexture_0;

	public void Awake()
	{
		ssaa_0 = GetComponent<SSAA>();
		if (ShouldApplySettings)
		{
			method_0();
		}
		method_14();
		renderTexture_11 = renderTexture_10[int_29];
		for (int i = 0; i < int_34; i++)
		{
			list_0.Add(new Class613());
		}
		method_15();
	}

	public void method_0()
	{
		if (Singleton<SharedGameSettingsClass>.Instantiated)
		{
			GameSetting<int> shadowsQuality = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.ShadowsQuality;
			if ((int)shadowsQuality == 0)
			{
				int_34 = 3;
				EnableShadowsBlend = true;
				ObjectsPerFrame = 10;
			}
			else if ((int)shadowsQuality == 1)
			{
				int_34 = 3;
				EnableShadowsBlend = true;
				ObjectsPerFrame = 20;
			}
			else if ((int)shadowsQuality == 2)
			{
				int_34 = 3;
				EnableShadowsBlend = true;
				ObjectsPerFrame = 35;
			}
			else if ((int)shadowsQuality == 3)
			{
				int_34 = 3;
				EnableShadowsBlend = true;
				ObjectsPerFrame = 50;
			}
			else
			{
				Debug.LogError("Unsupported shadows quality level in DistantShadow!");
			}
		}
	}

	public int method_1()
	{
		list_1.Clear();
		foreach (LODGroup item in FindObjectsOfTypeAll<LODGroup>())
		{
			LOD[] lODs = item.GetLODs();
			if (lODs.Length < 1)
			{
				continue;
			}
			int num = lODs.Length - 1;
			Renderer[] renderers = lODs[num].renderers;
			num = Math.Max(0, num - 1);
			List<Class614> list = null;
			while (num >= 0 && list == null)
			{
				renderers = lODs[num].renderers;
				bool flag = false;
				for (int i = 0; i < renderers.Length; i++)
				{
					if (renderers[i] != null && renderers[i].shadowCastingMode != ShadowCastingMode.Off)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list = new List<Class614>();
					for (int j = 0; j < renderers.Length; j++)
					{
						if (!(renderers[j] != null) || renderers[j].shadowCastingMode == ShadowCastingMode.Off || !(renderers[j].sharedMaterial != null) || !renderers[j].gameObject.activeInHierarchy)
						{
							continue;
						}
						MeshFilter component = renderers[j].gameObject.GetComponent<MeshFilter>();
						if (component == null || component.sharedMesh == null || Mathf.Max(renderers[j].bounds.size.x, Mathf.Max(renderers[j].bounds.size.y, renderers[j].bounds.size.z)) < ObjectSizeFilter)
						{
							continue;
						}
						for (int k = 0; k < component.sharedMesh.subMeshCount; k++)
						{
							Material material = renderers[j].sharedMaterial;
							if (renderers[j].sharedMaterials != null && k < renderers[j].sharedMaterials.Length)
							{
								material = renderers[j].sharedMaterials[k];
							}
							if ((bool)material)
							{
								if (material.enableInstancing)
								{
									Matrix4x4[] matrices = new Matrix4x4[1] { renderers[j].gameObject.transform.localToWorldMatrix };
									commandBuffer_0.DrawMeshInstanced(component.sharedMesh, k, material, 0, matrices);
								}
								else
								{
									commandBuffer_0.DrawMesh(component.sharedMesh, renderers[j].gameObject.transform.localToWorldMatrix, material, k, 0);
								}
								Class614 @class = new Class614();
								@class.mesh = component.sharedMesh;
								@class.material = material;
								@class.submeshIdx = k;
								@class.drawTransform = renderers[j].gameObject.transform.localToWorldMatrix;
								list.Add(@class);
							}
						}
					}
				}
				num--;
			}
			if (list != null)
			{
				list_1.AddRange(list);
			}
		}
		return list_1.Count;
	}

	public Matrix4x4 method_2()
	{
		return Matrix4x4.Ortho(0f - GlobalShadowHalfSize, GlobalShadowHalfSize, 0f - GlobalShadowHalfSize, GlobalShadowHalfSize, 0f - NearPlane, 0f - FarPlane);
	}

	public static List<T> FindObjectsOfTypeAll<T>()
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
				}
			}
		}
		return list;
	}

	public float method_3(double angleCosSpeed, double angleCos)
	{
		return (float)(-2428668480.0 * angleCosSpeed + 7136641.3 * angleCos - 1821580210.0 * angleCosSpeed * angleCos + 484576146000.0 * angleCosSpeed * angleCosSpeed + 4623877.04 * angleCos * angleCos + 3052001.66);
	}

	public int method_4()
	{
		if (!(ssaa_0 != null))
		{
			return Screen.width;
		}
		return ssaa_0.GetInputWidth();
	}

	public int method_5()
	{
		if (!(ssaa_0 != null))
		{
			return Screen.height;
		}
		return ssaa_0.GetInputHeight();
	}

	public void method_6(RenderTexture renderTexture)
	{
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(renderTexture);
	}

	public RenderTexture method_7(RenderTexture initialRenderTexture, int width, int height, string name, GraphicsFormat format, bool UAV = false, int depth = 0)
	{
		if (initialRenderTexture != null)
		{
			method_6(initialRenderTexture);
		}
		RenderTexture renderTexture = new RenderTexture(width, height, depth, format);
		renderTexture.name = name;
		if (UAV)
		{
			if (renderTexture.IsCreated())
			{
				renderTexture.Release();
			}
			renderTexture.enableRandomWrite = UAV;
			renderTexture.Create();
		}
		return renderTexture;
	}

	public RenderTexture method_8(RenderTexture initialRenderTexture, int width, int height, string name, RenderTextureFormat format, bool UAV = false, int depth = 0)
	{
		if (initialRenderTexture != null)
		{
			method_6(initialRenderTexture);
		}
		RenderTexture renderTexture = new RenderTexture(width, height, depth, format);
		renderTexture.name = name;
		if (UAV)
		{
			if (renderTexture.IsCreated())
			{
				renderTexture.Release();
			}
			renderTexture.enableRandomWrite = UAV;
			renderTexture.Create();
		}
		return renderTexture;
	}

	public void OnDestroy()
	{
		renderTexture_0?.Release();
		renderTexture_0 = null;
		renderTexture_2?.Release();
		renderTexture_2 = null;
		renderTexture_6?.Release();
		renderTexture_6 = null;
		renderTexture_7?.Release();
		renderTexture_7 = null;
		renderTexture_4?.Release();
		renderTexture_4 = null;
		renderTexture_1?.Release();
		renderTexture_1 = null;
		renderTexture_3?.Release();
		renderTexture_3 = null;
		renderTexture_8?.Release();
		renderTexture_8 = null;
		renderTexture_9?.Release();
		renderTexture_9 = null;
		renderTexture_5?.Release();
		renderTexture_5 = null;
		for (int i = 0; i < renderTexture_10.Length; i++)
		{
			renderTexture_10[i]?.Release();
			renderTexture_10[i] = null;
		}
		commandBuffer_0?.Dispose();
		commandBuffer_0 = null;
		commandBuffer_1?.Dispose();
		commandBuffer_1 = null;
		commandBuffer_2?.Dispose();
		commandBuffer_2 = null;
		commandBuffer_3?.Dispose();
		commandBuffer_3 = null;
		commandBuffer_4?.Dispose();
		commandBuffer_4 = null;
		commandBuffer_5?.Dispose();
		commandBuffer_5 = null;
		commandBuffer_6?.Dispose();
		commandBuffer_6 = null;
		commandBuffer_7?.Dispose();
		commandBuffer_7 = null;
		computeBuffer_0?.Dispose();
		computeBuffer_0 = null;
	}

	public void Update()
	{
		if (!GClass4.IsAvailable || !MasterCamera.enabled)
		{
			return;
		}
		if (ssaa_0 == null)
		{
			ssaa_0 = MasterCamera.GetComponent<SSAA>();
		}
		if (!method_15())
		{
			return;
		}
		if (int_25 == 0)
		{
			float_6 += Time.deltaTime;
		}
		if (computeBuffer_0 == null)
		{
			Vector2[] data = new Vector2[25]
			{
				new Vector2(-0.74528f, 0.10120783f),
				new Vector2(-0.34528f, 0.38692212f),
				new Vector2(0.05472f, 0.6726364f),
				new Vector2(0.45472f, 0.95835066f),
				new Vector2(0.85472f, -0.7151187f),
				new Vector2(-0.66528f, -0.4294044f),
				new Vector2(-0.26528f, -0.14369012f),
				new Vector2(0.13472f, 0.14202416f),
				new Vector2(0.53472f, 0.42773843f),
				new Vector2(0.93472f, 0.71345276f),
				new Vector2(-0.96928f, 0.999167f),
				new Vector2(-0.56928f, -0.999881f),
				new Vector2(-0.16928f, -0.7141667f),
				new Vector2(0.23072f, -0.42845243f),
				new Vector2(0.63072f, -0.14273815f),
				new Vector2(-0.88928f, 0.14297614f),
				new Vector2(-0.48928f, 0.42869043f),
				new Vector2(-0.08928f, 0.7144047f),
				new Vector2(0.31072f, -0.95906466f),
				new Vector2(0.71072f, -0.6733504f),
				new Vector2(-0.80928f, -0.3876361f),
				new Vector2(-0.40928f, -0.10192182f),
				new Vector2(-0.00928f, 0.18379247f),
				new Vector2(0.39072f, 0.46950674f),
				new Vector2(0.79072f, 0.755221f)
			};
			int stride = Marshal.SizeOf(typeof(Vector2));
			computeBuffer_0 = new ComputeBuffer(25, stride, ComputeBufferType.Default);
			computeBuffer_0.SetData(data);
		}
		int num = method_4();
		int num2 = method_5();
		switch (CurrentMaskResolution)
		{
		case ResolutionState.QUARTER:
			int_33 = 4;
			break;
		case ResolutionState.HALF:
			int_33 = 2;
			break;
		case ResolutionState.FULL:
			int_33 = 1;
			break;
		}
		int num3 = num / int_33;
		int num4 = num2 / int_33;
		if (renderTexture_0 == null || num3 != renderTexture_0.width || num4 != renderTexture_0.height)
		{
			renderTexture_0 = method_7(renderTexture_0, num3, num4, "DSLowResDepth", GraphicsFormat.R32_SFloat, UAV: true);
			renderTexture_2 = method_7(renderTexture_2, num3, num4, "DSShadowMaskQuarter", GraphicsFormat.R8_UNorm, UAV: true);
			renderTexture_6 = method_7(renderTexture_6, num, num2, "DSShadowMaskFull", GraphicsFormat.R8_UNorm, UAV: true);
			renderTexture_7 = method_7(renderTexture_7, num, num2, "DSShadowMaskFullBuf", GraphicsFormat.R8_UNorm);
			renderTexture_4 = method_7(renderTexture_4, num, num2, "DSBlurMask", GraphicsFormat.R8_UNorm, UAV: true);
		}
		if (camera_0 == null)
		{
			camera_0 = CameraClass.Instance.OpticCameraManager.Camera;
			if (camera_0 != null)
			{
				camera_0.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer_5);
				camera_0.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer_4);
				camera_0.AddCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_7);
			}
		}
		if (camera_0 != null && camera_0.targetTexture != null && (renderTexture_1 == null || camera_0.targetTexture.width / int_33 != renderTexture_1.width || camera_0.targetTexture.height / int_33 != renderTexture_1.height))
		{
			renderTexture_1 = method_7(renderTexture_1, camera_0.targetTexture.width / int_33, camera_0.targetTexture.height / int_33, "DSLowResDepthOptic", GraphicsFormat.R32_SFloat, UAV: true);
			renderTexture_3 = method_7(renderTexture_3, camera_0.targetTexture.width / int_33, camera_0.targetTexture.height / int_33, "DSShadowMaskQuarterOptic", GraphicsFormat.R8_UNorm, UAV: true);
			renderTexture_8 = method_7(renderTexture_8, camera_0.targetTexture.width, camera_0.targetTexture.height, "DSShadowMaskFullOptic", GraphicsFormat.R8_UNorm, UAV: true);
			renderTexture_9 = method_7(renderTexture_9, camera_0.targetTexture.width, camera_0.targetTexture.height, "DSShadowMaskFullOpticBuf", GraphicsFormat.R8_UNorm);
			renderTexture_5 = method_7(renderTexture_5, camera_0.targetTexture.width, camera_0.targetTexture.height, "DSBlurMaskOptic", GraphicsFormat.R8_UNorm, UAV: true);
		}
		method_14();
		while (int_34 > list_0.Count)
		{
			list_0.Add(new Class613());
		}
		method_13();
		bool flag = false;
		if (int_30 == 0 && int_31 == 0)
		{
			if (InvalidateRenderersOnNextUpdate && method_1() > 0)
			{
				InvalidateRenderersOnNextUpdate = false;
			}
			flag = true;
			float_8 = 0f;
			if (float_9 > 0f)
			{
				float_10 = Time.time - float_9;
				LastTimeToUpdateInfo = float_10;
				LastTimeToUpdate = float_10;
			}
			float_9 = Time.time;
			int_27 = int_28;
			int_28 = int_29;
			int_29 = (int_29 + 1) % renderTexture_10.Length;
			renderTexture_11 = renderTexture_10[int_29];
			method_12(int_29);
			int_30 = list_1.Count;
			int_31 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance?.ImpostorsToRenderCount ?? 0;
		}
		int num5 = ObjectsPerFrame;
		if (int_24 > 0)
		{
			float_5 += Time.deltaTime;
			if (float_5 > GClass4.Instance.UpdateInterval)
			{
				int_24--;
				float_5 = 0f;
				num5 = int_30;
			}
			else
			{
				num5 = 1;
			}
			int_31 = 1;
		}
		ObjectsToRenderInfo = int_30 + int_31;
		ObjectsToRenderPerFrameInfo = num5;
		commandBuffer_0.Clear();
		commandBuffer_0.SetViewMatrix(list_0[int_29]._cachedGlobalShadowWorldToView);
		Matrix4x4 cachedGlobalShadowProj = list_0[int_29]._cachedGlobalShadowProj;
		commandBuffer_0.SetProjectionMatrix(cachedGlobalShadowProj);
		commandBuffer_0.SetRenderTarget(renderTexture_11);
		if (flag)
		{
			commandBuffer_0.ClearRenderTarget(clearDepth: true, clearColor: false, Color.white);
		}
		int num6 = 0;
		while (num6 < num5 && int_30 > 0)
		{
			int_30--;
			Class614 @class = list_1[int_30];
			commandBuffer_0.DrawMesh(@class.mesh, @class.drawTransform, @class.material, @class.submeshIdx, 0);
			num6++;
			if (int_30 == 0)
			{
				int_31 = MonoBehaviourSingleton<ImpostorsRenderer>.Instance?.ImpostorsToRenderCount ?? 0;
			}
		}
		if (int_30 == 0 && int_31 > 0)
		{
			int num7 = num5 - num6;
			if (num7 > 0)
			{
				uint impostorsPerFrame = (uint)(num7 * uint_0);
				MonoBehaviourSingleton<ImpostorsRenderer>.Instance?.method_12(MasterCamera, commandBuffer_0, impostorsPerFrame, out int_31);
			}
		}
		int num8 = int_28;
		int num9 = int_27;
		Vector4 vector = new Vector4(1f / (float)renderTexture_10[0].width, 1f / (float)renderTexture_10[0].height, renderTexture_10[0].width, renderTexture_10[0].height);
		float y = ((QualitySettings.shadowCascades < 4) ? 1f : 0f);
		float_8 += Time.smoothDeltaTime;
		float num10 = Mathf.Min(BlendTime, Mathf.Max(float_10, 0.01f));
		Vector4 vector2 = new Vector4(0.25f, y, float_8 / num10, list_0[num8]._cachedFarPlane);
		if (PreComputeMask)
		{
			if (commandBuffer_6 != null && DownscaleDepthMaterial != null && ComputeShadowMaskMaterial != null && UpscaleShadowMaskMaterial != null)
			{
				commandBuffer_6.Clear();
				method_9(commandBuffer_6, num8, num9, vector2, vector, new Rect(0f, 0f, renderTexture_0.width, renderTexture_0.height), new Rect(0f, 0f, renderTexture_6.width, renderTexture_6.height), CurrentMaskResolution, renderTexture_0, renderTexture_2, renderTexture_4, renderTexture_6, renderTexture_7);
				if (camera_0 != null && camera_0.targetTexture != null)
				{
					commandBuffer_7.Clear();
					method_9(commandBuffer_7, num8, num9, vector2, vector, new Rect(0f, 0f, renderTexture_1.width, renderTexture_1.height), new Rect(0f, 0f, renderTexture_8.width, renderTexture_8.height), CurrentMaskResolution, renderTexture_1, renderTexture_3, renderTexture_5, renderTexture_8, renderTexture_9);
				}
			}
			if (commandBuffer_1 != null)
			{
				commandBuffer_1.Clear();
			}
		}
		else if (commandBuffer_1 != null)
		{
			commandBuffer_1.Clear();
			commandBuffer_1.SetGlobalTexture(int_32, renderTexture_10[num8]);
			commandBuffer_1.SetGlobalTexture(int_0, renderTexture_10[num8]);
			commandBuffer_1.SetGlobalMatrix(int_1, list_0[num8]._cachedGlobalShadowWorldToScreen);
			commandBuffer_1.SetGlobalMatrix(int_2, list_0[num8]._cachedGlobalShadowWorldToView);
			commandBuffer_1.SetGlobalMatrix(int_3, list_0[num8]._cachedGlobalShadowProjGPU);
			float x = (EnableAdaptiveBias ? 1f : 0f);
			float adaptiveBiasK = AdaptiveBiasK;
			Vector4 value = new Vector4(x, ConstantBias, adaptiveBiasK, PCFShift);
			commandBuffer_1.SetGlobalVector(int_4, value);
			if (EnableShadowsBlend)
			{
				commandBuffer_1.SetGlobalTexture(int_5, renderTexture_10[num9]);
				commandBuffer_1.SetGlobalMatrix(int_6, list_0[num9]._cachedGlobalShadowWorldToScreen);
				commandBuffer_1.SetGlobalMatrix(int_7, list_0[num9]._cachedGlobalShadowWorldToView);
				commandBuffer_1.SetGlobalMatrix(int_8, list_0[num9]._cachedGlobalShadowProjGPU);
			}
			commandBuffer_1.SetGlobalVector(int_9, vector2);
			commandBuffer_1.SetGlobalVector(int_10, vector);
			Vector4 value2 = new Vector4(SamplingRotation / 180f, 0f, 0f, 0f);
			commandBuffer_1.SetGlobalVector(int_11, value2);
			commandBuffer_1.SetGlobalVector(int_12, list_0[num8]._cachedLightDirection);
			commandBuffer_1.SetGlobalVector(int_13, -GClass4.Instance.LightObject.transform.forward);
		}
		if (GClass4.IsAvailable)
		{
			float_2 += Time.deltaTime;
			if (!bool_0)
			{
				float_1 = GClass4.Instance.LightObject.transform.forward.y;
				float_0 = 0f;
				bool_0 = true;
			}
			if (float_1 != GClass4.Instance.LightObject.transform.forward.y)
			{
				float_0 = Mathf.Abs(float_1 - GClass4.Instance.LightObject.transform.forward.y) / float_2;
				float_2 = 0f;
			}
			else if (float_2 > GClass4.Instance.UpdateInterval)
			{
				float_0 = 0f;
			}
			float_1 = GClass4.Instance.LightObject.transform.forward.y;
		}
	}

	public void method_9(CommandBuffer cmdBuf, int targetIdx, int targetIdxOld, Vector4 GlobalShadowProj, Vector4 TextureParams, Rect lowResViewport, Rect fullResViewport, ResolutionState resState, RenderTexture lowResDepth, RenderTexture lowResShadowMask, RenderTexture blurMask, RenderTexture hiResShadowMask, RenderTexture hiResShadowMaskBuf)
	{
		cmdBuf.SetGlobalTexture(int_32, renderTexture_10[targetIdx]);
		cmdBuf.SetGlobalTexture(int_0, renderTexture_10[targetIdx]);
		cmdBuf.SetGlobalMatrix(int_1, list_0[targetIdx]._cachedGlobalShadowWorldToScreen);
		cmdBuf.SetGlobalMatrix(int_2, list_0[targetIdx]._cachedGlobalShadowWorldToView);
		cmdBuf.SetGlobalMatrix(int_3, list_0[targetIdx]._cachedGlobalShadowProjGPU);
		float num = float_3;
		if (bool_1)
		{
			float num2 = num;
			if (float_0 != 0f)
			{
				num2 = method_3(float_0, float_1);
				num2 = Mathf.Max(num2, 0f);
			}
			else
			{
				num2 = 0f;
			}
			num2 = Mathf.Min(num, float_3);
			num = Mathf.Lerp(num, num2, float_4);
		}
		num *= Mathf.Clamp01(float_6 / 10f);
		cmdBuf.SetGlobalFloat(int_14, num);
		float x = (EnableAdaptiveBias ? 1f : 0f);
		float adaptiveBiasK = AdaptiveBiasK;
		cmdBuf.SetGlobalVector(value: new Vector4(x, ConstantBias, adaptiveBiasK, PCFShift), nameID: int_4);
		cmdBuf.SetGlobalVector(value: new Vector4(method_4(), method_5(), 1f / (float)method_4(), 1f / (float)method_5()), nameID: int_15);
		if (EnableShadowsBlend)
		{
			cmdBuf.SetGlobalTexture(int_5, renderTexture_10[targetIdxOld]);
			cmdBuf.SetGlobalMatrix(int_6, list_0[targetIdxOld]._cachedGlobalShadowWorldToScreen);
			cmdBuf.SetGlobalMatrix(int_7, list_0[targetIdxOld]._cachedGlobalShadowWorldToView);
			cmdBuf.SetGlobalMatrix(int_8, list_0[targetIdxOld]._cachedGlobalShadowProjGPU);
		}
		cmdBuf.SetGlobalVector(int_9, GlobalShadowProj);
		cmdBuf.SetGlobalVector(int_10, TextureParams);
		cmdBuf.SetGlobalVector(value: new Vector4(SamplingRotation / 180f, 0f, 0f, 0f), nameID: int_11);
		cmdBuf.SetGlobalVector(int_12, list_0[targetIdx]._cachedLightDirection);
		cmdBuf.SetGlobalVector(int_16, list_0[targetIdxOld]._cachedLightDirection);
		if (GClass4.IsAvailable)
		{
			cmdBuf.SetGlobalVector(int_13, -GClass4.Instance.LightObject.transform.forward);
		}
		if (resState == ResolutionState.QUARTER || resState == ResolutionState.HALF)
		{
			cmdBuf.SetRenderTarget(lowResDepth);
			cmdBuf.SetViewport(lowResViewport);
			if (resState == ResolutionState.QUARTER)
			{
				cmdBuf.EnableShaderKeyword("QUARTER_DEPTH");
			}
			else
			{
				cmdBuf.EnableShaderKeyword("HALF_DEPTH");
			}
			cmdBuf.DrawMesh(mesh_0, Matrix4x4.identity, DownscaleDepthMaterial);
		}
		if (resState == ResolutionState.FULL)
		{
			cmdBuf.SetRenderTarget(hiResShadowMask);
			cmdBuf.SetViewport(fullResViewport);
			cmdBuf.DisableShaderKeyword("LOWRES_DEPTH");
		}
		else
		{
			cmdBuf.SetRenderTarget(lowResShadowMask);
			cmdBuf.SetViewport(lowResViewport);
			cmdBuf.SetGlobalTexture(int_22, lowResDepth);
			cmdBuf.EnableShaderKeyword("LOWRES_DEPTH");
		}
		cmdBuf.SetGlobalBuffer(int_17, computeBuffer_0);
		cmdBuf.DrawMesh(mesh_0, Matrix4x4.identity, ComputeShadowMaskMaterial, 0, 0);
		if (resState == ResolutionState.QUARTER || resState == ResolutionState.HALF)
		{
			cmdBuf.SetGlobalTexture(int_18, lowResShadowMask);
			if (EnableShadowmaskBlur && EnableBlurMask && resState == ResolutionState.QUARTER)
			{
				cmdBuf.SetRenderTarget(blurMask);
				cmdBuf.SetViewport(fullResViewport);
				cmdBuf.DrawMesh(mesh_0, Matrix4x4.identity, ComputeShadowMaskMaterial, 0, 1);
				cmdBuf.SetGlobalTexture(int_19, blurMask);
			}
			cmdBuf.SetRenderTarget(hiResShadowMask);
			cmdBuf.SetViewport(fullResViewport);
			if (EnableBlurMask)
			{
				cmdBuf.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			}
			cmdBuf.DrawMesh(mesh_0, Matrix4x4.identity, UpscaleShadowMaskMaterial);
		}
		if (EnableShadowmaskBlur)
		{
			if (FixGlowEffect)
			{
				cmdBuf.EnableShaderKeyword(string_13);
			}
			cmdBuf.SetRenderTarget(hiResShadowMaskBuf);
			cmdBuf.SetGlobalFloat(int_23, BlurDepthThreshold);
			if (EnableBlurMask)
			{
				cmdBuf.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			}
			if (materialPropertyBlock_0 == null)
			{
				materialPropertyBlock_0 = new MaterialPropertyBlock();
			}
			materialPropertyBlock_0.Clear();
			materialPropertyBlock_0.SetTexture(int_20, hiResShadowMask);
			cmdBuf.DrawMesh(mesh_0, Matrix4x4.identity, ShadowMaskBlurMaterial, 0, 0, materialPropertyBlock_0);
			cmdBuf.SetRenderTarget(hiResShadowMask);
			if (EnableBlurMask)
			{
				cmdBuf.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			}
			if (materialPropertyBlock_1 == null)
			{
				materialPropertyBlock_1 = new MaterialPropertyBlock();
			}
			materialPropertyBlock_1.Clear();
			materialPropertyBlock_1.SetTexture(int_20, hiResShadowMaskBuf);
			cmdBuf.DrawMesh(mesh_0, Matrix4x4.identity, ShadowMaskBlurMaterial, 0, 1, materialPropertyBlock_1);
			cmdBuf.DisableShaderKeyword(string_13);
		}
		if (PreComputeMask)
		{
			cmdBuf.SetGlobalTexture(int_21, hiResShadowMask);
		}
	}

	public void WarmupForFrames(int frames)
	{
		int_24 = frames;
	}

	public void method_10(CommandBuffer cmdBuf, bool opticCamera)
	{
		if (EnableShadowsBlend)
		{
			cmdBuf.EnableShaderKeyword(string_0);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_0);
		}
		if (EnableAdaptiveBias)
		{
			cmdBuf.EnableShaderKeyword(string_1);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_1);
		}
		if (EnableDistantShadowKeyword)
		{
			cmdBuf.EnableShaderKeyword(string_2);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_2);
		}
		if (EnableDistantShadowPCF)
		{
			cmdBuf.EnableShaderKeyword(string_3);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_3);
		}
		if (EnableMultiviewTiles)
		{
			cmdBuf.EnableShaderKeyword(string_4);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_4);
		}
		if (EnablePCFShift)
		{
			cmdBuf.EnableShaderKeyword(string_5);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_5);
		}
		if (DisableWind)
		{
			cmdBuf.EnableShaderKeyword(string_6);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_6);
		}
		if (PreComputeMask)
		{
			cmdBuf.EnableShaderKeyword(string_9);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_9);
		}
		if (EnableParallax && int_25 == 0)
		{
			cmdBuf.EnableShaderKeyword(string_10);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_10);
		}
		if (UseGaussDistribution)
		{
			cmdBuf.EnableShaderKeyword(string_8);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_8);
		}
		if (ReduceBlurSamples)
		{
			cmdBuf.EnableShaderKeyword(string_11);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_11);
		}
		if (opticCamera)
		{
			cmdBuf.EnableShaderKeyword(string_7);
		}
		if (EnableShadowmaskBlur && EnableBlurMask && CurrentMaskResolution == ResolutionState.QUARTER)
		{
			cmdBuf.EnableShaderKeyword(string_12);
		}
		else
		{
			cmdBuf.DisableShaderKeyword(string_12);
		}
	}

	public void method_11(CommandBuffer cmdBuf)
	{
		cmdBuf.DisableShaderKeyword(string_0);
		cmdBuf.DisableShaderKeyword(string_1);
		cmdBuf.DisableShaderKeyword(string_2);
		cmdBuf.DisableShaderKeyword(string_3);
		cmdBuf.DisableShaderKeyword(string_4);
		cmdBuf.DisableShaderKeyword(string_5);
		cmdBuf.DisableShaderKeyword(string_6);
		cmdBuf.DisableShaderKeyword(string_9);
		cmdBuf.DisableShaderKeyword(string_10);
		cmdBuf.DisableShaderKeyword(string_7);
		cmdBuf.DisableShaderKeyword(string_12);
		cmdBuf.DisableShaderKeyword(string_11);
	}

	public void method_12(int idx)
	{
		if (int_24 == 0 && int_25 > 0)
		{
			int_25--;
		}
		list_0[idx]._cachedShadowCameraPosition = base.transform.position;
		list_0[idx]._cachedLightDirection = -base.transform.forward;
		Matrix4x4 matrix4x = method_2();
		list_0[idx]._cachedGlobalShadowWorldToScreen = GL.GetGPUProjectionMatrix(matrix4x, renderIntoTexture: false) * base.transform.worldToLocalMatrix;
		list_0[idx]._cachedGlobalShadowWorldToView = base.transform.worldToLocalMatrix;
		list_0[idx]._cachedGlobalShadowProjGPU = GL.GetGPUProjectionMatrix(matrix4x, renderIntoTexture: false);
		list_0[idx]._cachedGlobalShadowProj = matrix4x;
		list_0[idx]._cachedNearPlane = NearPlane;
		list_0[idx]._cachedFarPlane = FarPlane;
		list_0[idx]._cachedShadowCameraForward = base.transform.forward;
		list_0[idx]._cachedShadowCameraRight = base.transform.right;
		list_0[idx]._cachedShadowCameraUp = base.transform.up;
		list_0[idx]._buffersDataIsPropagated = false;
	}

	public void method_13()
	{
		float num = 0f;
		Vector3 position = MasterCamera.transform.position;
		Vector3 forward = MasterCamera.transform.forward;
		if (GClass4.IsAvailable)
		{
			vector3_0 = GClass4.Instance.LightDirectionExtrapolated(float_10 * LightDirectionExtrapolateFactor);
		}
		base.transform.position = position + vector3_0 * OffsetTowardsLight + num * forward;
		base.transform.LookAt(base.transform.position - vector3_0);
	}

	public void method_14()
	{
		if (renderTexture_10 == null || renderTexture_10.Length != int_34)
		{
			renderTexture_10 = new RenderTexture[int_34];
			int_27 = 0;
			int_28 = 1;
			int_29 = 2 % int_34;
			int_30 = 0;
		}
		for (int i = 0; i < int_34; i++)
		{
			if (renderTexture_10[i] == null || renderTexture_10[i].width != ShadowPixelsSize || renderTexture_10[i].height != ShadowPixelsSize)
			{
				string text = "DSDepthRT_" + i;
				renderTexture_10[i] = method_8(renderTexture_10[i], ShadowPixelsSize, ShadowPixelsSize, text, RenderTextureFormat.Depth, UAV: false, 16);
				int_30 = 0;
			}
		}
	}

	public bool method_15()
	{
		if (MasterCamera == null)
		{
			return false;
		}
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer();
			commandBuffer_0.name = "DistantShadowObjects";
			MasterCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer_0);
		}
		if (commandBuffer_1 == null)
		{
			commandBuffer_1 = new CommandBuffer();
			commandBuffer_1.name = "SetupDistantShadow";
			MasterCamera.AddCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_1);
		}
		if (commandBuffer_2 == null)
		{
			commandBuffer_2 = new CommandBuffer();
			commandBuffer_2.name = "DisableDistantShadowKeywords";
			MasterCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer_2);
			method_11(commandBuffer_2);
		}
		if (commandBuffer_3 == null)
		{
			commandBuffer_3 = new CommandBuffer();
			commandBuffer_3.name = "EnableDistantShadowKeywords";
			MasterCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer_3);
		}
		if (commandBuffer_6 == null)
		{
			commandBuffer_6 = new CommandBuffer();
			commandBuffer_6.name = "ComputeDistantShadowMask";
			MasterCamera.AddCommandBuffer(CameraEvent.BeforeLighting, commandBuffer_6);
			commandBuffer_7 = new CommandBuffer();
			commandBuffer_7.name = "ComputeOpticDistantShadowMask";
		}
		if (commandBuffer_4 == null)
		{
			commandBuffer_4 = new CommandBuffer();
			commandBuffer_4.name = "DisableDistantShadowOpticKeywords";
			method_11(commandBuffer_4);
		}
		if (commandBuffer_5 == null)
		{
			commandBuffer_5 = new CommandBuffer();
			commandBuffer_5.name = "EnableDistantShadowOpticKeywords";
		}
		if (mesh_0 == null)
		{
			mesh_0 = new Mesh();
			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f),
				default(Vector3)
			};
			mesh_0.vertices = vertices;
			Vector2[] uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 2f),
				new Vector2(2f, 0f),
				default(Vector2)
			};
			mesh_0.uv = uv;
			int[] triangles = new int[6] { 0, 1, 2, 0, 0, 0 };
			mesh_0.triangles = triangles;
		}
		commandBuffer_3.Clear();
		method_10(commandBuffer_3, opticCamera: false);
		commandBuffer_5.Clear();
		method_10(commandBuffer_5, opticCamera: true);
		return true;
	}

	public void DisableFeature()
	{
		if (commandBuffer_0 != null)
		{
			commandBuffer_0.Clear();
		}
		if (commandBuffer_6 != null)
		{
			commandBuffer_6.Clear();
		}
		if (commandBuffer_7 != null)
		{
			commandBuffer_7.Clear();
		}
		if (commandBuffer_1 != null)
		{
			commandBuffer_1.Clear();
		}
		if (commandBuffer_3 != null)
		{
			method_11(commandBuffer_3);
		}
		if (commandBuffer_5 != null)
		{
			method_11(commandBuffer_5);
		}
		base.gameObject.SetActive(value: false);
	}

	public void EnableFeature()
	{
		if (EnableDistantShadowKeyword)
		{
			if (commandBuffer_3 != null)
			{
				method_10(commandBuffer_3, opticCamera: false);
			}
			if (commandBuffer_5 != null)
			{
				method_10(commandBuffer_5, opticCamera: true);
			}
		}
		base.gameObject.SetActive(value: true);
	}
}
