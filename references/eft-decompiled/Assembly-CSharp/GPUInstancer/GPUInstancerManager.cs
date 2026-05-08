using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT.Settings.Graphics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace GPUInstancer;

public abstract class GPUInstancerManager : MonoBehaviour
{
	public class GClass1261
	{
		public Thread thread;

		public object parameter;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class875
	{
		public static readonly Class875 class875_0 = new Class875();

		public static Predicate<GPUInstancerPrototype> predicate_0;

		public bool method_0(GPUInstancerPrototype p)
		{
			return p == null;
		}
	}

	public bool IsOptic;

	public List<GPUInstancerPrototype> prototypeList;

	public GPUInstancerCameraData cameraData = new GPUInstancerCameraData();

	public bool useFloatingOriginHandler;

	public Transform floatingOriginTransform;

	public GPUInstancerFloatingOriginHandler floatingOriginHandler;

	public GPUInstancerTerrainSettings terrainSettings;

	public List<GClass1270> runtimeDataList;

	public Bounds instancingBounds;

	public bool isFrustumCulling = true;

	public bool isOcclusionCulling = true;

	public float minCullingDistance;

	protected GClass1273<GClass1258> spData;

	public static List<GPUInstancerManager> activeManagerList;

	public static bool showRenderedAmount;

	protected static ComputeShader _cameraComputeShader;

	protected static int[] _cameraComputeKernelIDs;

	protected static ComputeShader _visibilityComputeShader;

	protected static int[] _instanceVisibilityComputeKernelIDs;

	protected static ComputeShader _bufferToTextureComputeShader;

	protected static int _bufferToTextureComputeKernelID;

	public int maxThreads = 3;

	public readonly List<Thread> activeThreads = new List<Thread>();

	public readonly Queue<GClass1261> threadStartQueue = new Queue<GClass1261>();

	public readonly Queue<Action> threadQueue = new Queue<Action>();

	public static int lastTreePositionUpdate;

	public static GameObject treeProxyParent;

	public static Dictionary<GameObject, Transform> treeProxyList;

	public static int lastDrawCallFrame;

	public static float lastDrawCallTime;

	public static float timeSinceLastDrawCall;

	protected static Vector4 _windVector = Vector4.zero;

	protected bool isInitial = true;

	public bool isInitialized;

	public Dictionary<GPUInstancerPrototype, GClass1270> runtimeDataDictionary;

	public LayerMask layerMask = -1;

	protected static CommandBuffer motionVectorsCb;

	protected static CommandBuffer motionVectorsAfterCb;

	protected static RenderTexture motionVectorsRt;

	protected static RenderTexture motionVectorsRtDepth;

	protected static CommandBuffer motionVectorsCbOptic;

	protected static CommandBuffer motionVectorsAfterCbOptic;

	protected static RenderTexture motionVectorsRtOptic;

	protected static RenderTexture motionVectorsRtDepthOptic;

	protected static RenderTargetIdentifier[] mrt;

	protected static RenderTargetIdentifier[] mrtOptic;

	protected static bool isMotionVectorDataClear;

	protected static bool isMotionVectorDataClearOptic;

	protected static bool isMotionVectorsInit;

	protected readonly CompositeDisposableClass CompositeDisposable = new CompositeDisposableClass();

	public Exception threadException;

	private static readonly int int_0 = Shader.PropertyToID("_Wind");

	private static readonly int int_1 = Shader.PropertyToID("_BlendGrassMv");

	public virtual bool isCulled => false;

	public Camera Camera => cameraData?.mainCamera;

	public SSAA CameraSSAA => cameraData?.mainCameraSSAA;

	public CommandBuffer MotionVectorsCommandBuffer
	{
		get
		{
			if (!IsOptic)
			{
				return motionVectorsCb;
			}
			return motionVectorsCbOptic;
		}
	}

	public CommandBuffer MotionVectorsAfterCommandBuffer
	{
		get
		{
			if (!IsOptic)
			{
				return motionVectorsAfterCb;
			}
			return motionVectorsAfterCbOptic;
		}
	}

	public RenderTexture MotionVectorsTexture
	{
		get
		{
			if (!IsOptic)
			{
				return motionVectorsRt;
			}
			return motionVectorsRtOptic;
		}
	}

	public RenderTexture MotionVectorsDepth
	{
		get
		{
			if (!IsOptic)
			{
				return motionVectorsRtDepth;
			}
			return motionVectorsRtDepthOptic;
		}
	}

	public bool IsMotionVectorDataClear
	{
		get
		{
			if (!IsOptic)
			{
				return isMotionVectorDataClear;
			}
			return isMotionVectorDataClearOptic;
		}
		set
		{
			if (IsOptic)
			{
				isMotionVectorDataClearOptic = value;
			}
			else
			{
				isMotionVectorDataClear = value;
			}
		}
	}

	public RenderTargetIdentifier[] MRT
	{
		get
		{
			if (!IsOptic)
			{
				return mrt;
			}
			return mrtOptic;
		}
	}

	public static bool bGenerateMotionVectors
	{
		get
		{
			if (CameraClass.Exist && CameraClass.Instance.GetSSREnabled())
			{
				return true;
			}
			if (Singleton<SharedGameSettingsClass>.Instantiated)
			{
				GraphicsSettingsClass settings = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings;
				if (!settings.IsTaaEnabled() && !settings.IsDLSSEnabled() && !settings.IsFSR2Enabled() && !settings.IsFSR3Enabled())
				{
					return settings.SSR.Value > ESSRMode.Off;
				}
				return true;
			}
			return false;
		}
	}

	public static bool isTaaEnabled
	{
		get
		{
			if (!Singleton<SharedGameSettingsClass>.Instantiated)
			{
				return false;
			}
			GraphicsSettingsClass settings = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings;
			if (!settings.IsTaaEnabled())
			{
				return settings.IsDLSSEnabled();
			}
			return true;
		}
	}

	public static int OpticResolution => CameraClass.Instance.OpticCameraManager.OpticRenderResolution;

	public virtual void Awake()
	{
		if (!isMotionVectorsInit)
		{
			isMotionVectorsInit = true;
			if (Singleton<SharedGameSettingsClass>.Instantiated)
			{
				CompositeDisposable.BindState(Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.DisplaySettings, OnResolutionChangeStatic);
			}
			CreateMotionVectorsData();
		}
		if (GClass1262.gpuiSettings == null)
		{
			GClass1262.gpuiSettings = GPUInstancerSettings.GetDefaultGPUInstancerSettings();
		}
		GClass1262.gpuiSettings.SetDefultBindings();
		GClass1274.SetPlatformDependentVariables();
		if (Application.isPlaying && activeManagerList == null)
		{
			activeManagerList = new List<GPUInstancerManager>();
		}
		if (_visibilityComputeShader == null)
		{
			_visibilityComputeShader = (ComputeShader)Resources.Load(GClass1262.VISIBILITY_COMPUTE_RESOURCE_PATH);
			switch (GClass1274.matrixHandlingType)
			{
			default:
				_visibilityComputeShader = (ComputeShader)Resources.Load(GClass1262.VISIBILITY_COMPUTE_RESOURCE_PATH);
				break;
			case GPUIMatrixHandlingType.CopyToTexture:
				_visibilityComputeShader = (ComputeShader)Resources.Load(GClass1262.VISIBILITY_COMPUTE_RESOURCE_PATH);
				_bufferToTextureComputeShader = (ComputeShader)Resources.Load(GClass1262.BUFFER_TO_TEXTURE_COMPUTE_RESOURCE_PATH);
				_bufferToTextureComputeKernelID = _bufferToTextureComputeShader.FindKernel(GClass1262.BUFFER_TO_TEXTURE_KERNEL);
				break;
			case GPUIMatrixHandlingType.MatrixAppend:
				_visibilityComputeShader = (ComputeShader)Resources.Load(GClass1262.VISIBILITY_COMPUTE_RESOURCE_PATH_VULKAN);
				GClass1262.DETAIL_STORE_INSTANCE_DATA = true;
				GClass1262.COMPUTE_MAX_LOD_BUFFER = 3;
				break;
			}
			_instanceVisibilityComputeKernelIDs = new int[GClass1262.VISIBILITY_COMPUTE_KERNELS.Length];
			for (int i = 0; i < _instanceVisibilityComputeKernelIDs.Length; i++)
			{
				_instanceVisibilityComputeKernelIDs[i] = _visibilityComputeShader.FindKernel(GClass1262.VISIBILITY_COMPUTE_KERNELS[i]);
			}
			GClass1262.TEXTURE_MAX_SIZE = SystemInfo.maxTextureSize;
		}
		if (_cameraComputeShader == null)
		{
			_cameraComputeShader = (ComputeShader)Resources.Load(GClass1262.CAMERA_COMPUTE_RESOURCE_PATH);
			if (isOcclusionCulling && XRSettings.enabled && GClass1262.gpuiSettings.testBothEyesForVROcclusion)
			{
				_cameraComputeShader = (ComputeShader)Resources.Load(GClass1262.CAMERA_VR_COMPUTE_RESOURCE_PATH);
			}
			_cameraComputeKernelIDs = new int[GClass1262.CAMERA_COMPUTE_KERNELS.Length];
			for (int j = 0; j < _cameraComputeKernelIDs.Length; j++)
			{
				_cameraComputeKernelIDs[j] = _cameraComputeShader.FindKernel(GClass1262.CAMERA_COMPUTE_KERNELS[j]);
			}
		}
		GClass1262.SetupComputeRuntimeModification();
		GClass1262.SetupComputeSetDataPartial();
		showRenderedAmount = false;
		InitializeCameraData();
		SetupOcclusionCulling();
	}

	public static void OnResolutionChangeStatic(GStruct293 displaySettings)
	{
		smethod_0(displaySettings.Resolution);
		smethod_1();
	}

	public static void smethod_0(EftResolution resolution)
	{
		if (motionVectorsRt != null)
		{
			motionVectorsRt.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRt);
		}
		motionVectorsRt = new RenderTexture(resolution.Width, resolution.Height, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear)
		{
			name = "GrassMotionVectorsRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		if ((bool)motionVectorsRtDepth)
		{
			motionVectorsRtDepth.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtDepth);
		}
		motionVectorsRtDepth = new RenderTexture(resolution.Width, resolution.Height, 0, RenderTextureFormat.RFloat)
		{
			name = "GrassMotionVectorsDepthRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		mrt = new RenderTargetIdentifier[2] { motionVectorsRt, motionVectorsRtDepth };
	}

	public static void smethod_1()
	{
		if (motionVectorsRtOptic != null)
		{
			motionVectorsRtOptic.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtOptic);
		}
		motionVectorsRtOptic = new RenderTexture(OpticResolution, OpticResolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear)
		{
			name = "GrassMotionVectorsOpticRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		if (motionVectorsRtDepthOptic != null)
		{
			motionVectorsRtDepthOptic.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtDepthOptic);
		}
		motionVectorsRtDepthOptic = new RenderTexture(OpticResolution, OpticResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
		{
			name = "GrassMotionVectorsDepthOpticRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		mrtOptic = new RenderTargetIdentifier[2] { motionVectorsRtOptic, motionVectorsRtDepthOptic };
	}

	public void CreateMotionVectorsData()
	{
		DisposeMotionVectorsData();
		motionVectorsCb = new CommandBuffer
		{
			name = "MotionVectorsPASS"
		};
		motionVectorsAfterCb = new CommandBuffer
		{
			name = "MotionVectorsAfterPASS"
		};
		if (motionVectorsRt != null)
		{
			motionVectorsRt.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRt);
		}
		motionVectorsRt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear)
		{
			name = "GrassMotionVectorsRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		if (motionVectorsRtDepth != null)
		{
			motionVectorsRtDepth.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtDepth);
		}
		motionVectorsRtDepth = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat)
		{
			name = "GrassMotionVectorsDepthRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		mrt = new RenderTargetIdentifier[2] { motionVectorsRt, motionVectorsRtDepth };
		motionVectorsCbOptic = new CommandBuffer
		{
			name = "MotionVectorsOpticPASS"
		};
		motionVectorsAfterCbOptic = new CommandBuffer
		{
			name = "MotionVectorsAfterOpticPASS"
		};
		if (motionVectorsRtOptic != null)
		{
			motionVectorsRtOptic.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtOptic);
		}
		motionVectorsRtOptic = new RenderTexture(OpticResolution, OpticResolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear)
		{
			name = "GrassMotionVectorsOpticRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		if (motionVectorsRtDepthOptic != null)
		{
			motionVectorsRtDepthOptic.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtDepthOptic);
		}
		motionVectorsRtDepthOptic = new RenderTexture(OpticResolution, OpticResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
		{
			name = "GrassMotionVectorsDepthOpticRT",
			useMipMap = false,
			antiAliasing = 1,
			wrapMode = TextureWrapMode.Clamp
		};
		mrtOptic = new RenderTargetIdentifier[2] { motionVectorsRtOptic, motionVectorsRtDepthOptic };
		Shader.SetGlobalFloat(int_1, 1f);
	}

	public virtual void OnEnable()
	{
		if (activeManagerList != null && !activeManagerList.Contains(this))
		{
			activeManagerList.Add(this);
		}
		if (GClass1262.gpuiSettings == null || GClass1262.gpuiSettings.shaderBindings == null)
		{
			Debug.LogWarning("No shader bindings file was supplied. Instancing will terminate!");
		}
		if (runtimeDataList == null || runtimeDataList.Count == 0)
		{
			InitializeRuntimeDataAndBuffers();
		}
		isInitial = true;
		if (!useFloatingOriginHandler || !(floatingOriginTransform != null))
		{
			return;
		}
		if (floatingOriginHandler == null)
		{
			floatingOriginHandler = floatingOriginTransform.gameObject.GetComponent<GPUInstancerFloatingOriginHandler>();
			if (floatingOriginHandler == null)
			{
				floatingOriginHandler = floatingOriginTransform.gameObject.AddComponent<GPUInstancerFloatingOriginHandler>();
			}
		}
		if (floatingOriginHandler.gPUIManagers == null)
		{
			floatingOriginHandler.gPUIManagers = new List<GPUInstancerManager>();
		}
		if (!floatingOriginHandler.gPUIManagers.Contains(this))
		{
			floatingOriginHandler.gPUIManagers.Add(this);
		}
	}

	public void ClearMotionVectorData()
	{
		if (!IsMotionVectorDataClear && !(Camera == null) && MotionVectorsCommandBuffer != null)
		{
			MotionVectorsCommandBuffer.Clear();
			MotionVectorsAfterCommandBuffer.Clear();
			if (bGenerateMotionVectors)
			{
				Matrix4x4 value = GL.GetGPUProjectionMatrix(Camera.projectionMatrix, renderIntoTexture: true) * Camera.worldToCameraMatrix;
				Matrix4x4 previousViewProjectionMatrix = Camera.previousViewProjectionMatrix;
				MotionVectorsCommandBuffer.SetGlobalMatrix("_PreviousVP", previousViewProjectionMatrix);
				MotionVectorsCommandBuffer.SetGlobalMatrix("_NonJitteredVP", value);
				MotionVectorsCommandBuffer.SetRenderTarget(MRT, BuiltinRenderTextureType.CameraTarget);
				MotionVectorsCommandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
				MotionVectorsAfterCommandBuffer.SetGlobalTexture("_CameraMotionVectorsAddition", MotionVectorsTexture);
				MotionVectorsAfterCommandBuffer.SetGlobalTexture("_CameraMotionVectorsDepth", MotionVectorsDepth);
			}
			else
			{
				MotionVectorsCommandBuffer.SetRenderTarget(MotionVectorsDepth, BuiltinRenderTextureType.CameraTarget);
				MotionVectorsCommandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
				MotionVectorsCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
			}
			IsMotionVectorDataClear = true;
		}
	}

	public virtual void Update()
	{
		if (Camera != null && !IsOptic && motionVectorsRt != null && CameraSSAA != null)
		{
			int inputWidth = CameraSSAA.GetInputWidth();
			int inputHeight = CameraSSAA.GetInputHeight();
			if (inputWidth != MotionVectorsTexture.width || inputHeight != MotionVectorsTexture.height)
			{
				smethod_0(new EftResolution(inputWidth, inputHeight));
			}
		}
		int opticRenderResolution;
		if (IsOptic && motionVectorsRtOptic != null && motionVectorsRtDepthOptic != null && ((opticRenderResolution = CameraClass.Instance.OpticCameraManager.OpticRenderResolution) != motionVectorsRtOptic.width || opticRenderResolution != motionVectorsRtOptic.height))
		{
			smethod_1();
		}
		ClearCompletedThreads();
		ClearMotionVectorData();
		while (threadStartQueue.Count > 0 && activeThreads.Count < maxThreads)
		{
			GClass1261 gClass = threadStartQueue.Dequeue();
			gClass.thread.Start(gClass.parameter);
			activeThreads.Add(gClass.thread);
		}
		if (threadQueue.Count > 0)
		{
			threadQueue.Dequeue()?.Invoke();
		}
		if (Application.isPlaying && (bool)treeProxyParent && lastTreePositionUpdate != Time.frameCount && cameraData.mainCamera != null)
		{
			treeProxyParent.transform.position = cameraData.mainCamera.transform.position;
			treeProxyParent.transform.rotation = cameraData.mainCamera.transform.rotation;
			lastTreePositionUpdate = Time.frameCount;
		}
		if (Camera == null)
		{
			InitializeCameraData();
		}
		if (!(Camera == null) && !isCulled && Camera.isActiveAndEnabled)
		{
			UpdateTreeMPB();
			UpdateBuffers();
			GClass1274.DrawPrePass(MotionVectorsCommandBuffer, runtimeDataList, instancingBounds, cameraData, bGenerateMotionVectors);
			GClass1274.GPUIDrawMeshInstancedIndirect(runtimeDataList, instancingBounds, cameraData);
		}
	}

	public void LateUpdate()
	{
		IsMotionVectorDataClear = false;
	}

	public virtual void OnDestroy()
	{
		CompositeDisposable.Dispose();
		DisposeMotionVectorsData();
	}

	public void DisposeMotionVectorsData()
	{
		Shader.SetGlobalFloat(int_1, 0f);
		isMotionVectorsInit = false;
		if (motionVectorsCb != null)
		{
			motionVectorsCb.Release();
		}
		if (motionVectorsCbOptic != null)
		{
			motionVectorsCbOptic.Release();
		}
		if (motionVectorsRt != null)
		{
			motionVectorsRt.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRt);
		}
		if (motionVectorsRtOptic != null)
		{
			motionVectorsRtOptic.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtOptic);
		}
		if (motionVectorsAfterCb != null)
		{
			motionVectorsAfterCb.Release();
		}
		if (motionVectorsAfterCbOptic != null)
		{
			motionVectorsAfterCbOptic.Release();
		}
		if (motionVectorsRtDepth != null)
		{
			motionVectorsRtDepth.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtDepth);
		}
		if (motionVectorsRtDepthOptic != null)
		{
			motionVectorsRtDepthOptic.Release();
			UnityEngine.Object.DestroyImmediate(motionVectorsRtDepthOptic);
		}
		motionVectorsCb = null;
		motionVectorsCbOptic = null;
		motionVectorsRt = null;
		motionVectorsRtOptic = null;
		motionVectorsRtDepth = null;
		motionVectorsRtDepthOptic = null;
		motionVectorsAfterCb = null;
		motionVectorsAfterCbOptic = null;
	}

	public virtual void Reset()
	{
		GClass1262.gpuiSettings.SetDefultBindings();
	}

	public virtual void OnDisable()
	{
		activeManagerList?.Remove(this);
		ClearInstancingData();
		if (floatingOriginHandler?.gPUIManagers != null && floatingOriginHandler.gPUIManagers.Contains(this))
		{
			floatingOriginHandler.gPUIManagers.Remove(this);
		}
	}

	public virtual void ClearInstancingData()
	{
		GClass1274.ReleaseInstanceBuffers(runtimeDataList);
		GClass1274.ReleaseSPBuffers(spData);
		runtimeDataList?.Clear();
		runtimeDataDictionary?.Clear();
		spData = null;
		threadStartQueue.Clear();
		threadQueue.Clear();
		isInitialized = false;
	}

	public virtual void GeneratePrototypes(bool forceNew = false)
	{
		ClearInstancingData();
		if (!forceNew && prototypeList != null)
		{
			prototypeList.RemoveAll((GPUInstancerPrototype p) => p == null);
		}
		else
		{
			prototypeList = new List<GPUInstancerPrototype>();
		}
		if (GClass1262.gpuiSettings == null)
		{
			GClass1262.gpuiSettings = GPUInstancerSettings.GetDefaultGPUInstancerSettings();
		}
		GClass1262.gpuiSettings.SetDefultBindings();
	}

	public virtual void InitializeRuntimeDataAndBuffers(bool forceNew = true)
	{
		GClass1274.SetPlatformDependentVariables();
		if (forceNew || !isInitialized)
		{
			instancingBounds = new Bounds(Vector3.zero, Vector3.one * GClass1262.gpuiSettings.instancingBoundsSize);
			GClass1274.ReleaseInstanceBuffers(runtimeDataList);
			GClass1274.ReleaseSPBuffers(spData);
			if (runtimeDataList != null)
			{
				runtimeDataList.Clear();
			}
			else
			{
				runtimeDataList = new List<GClass1270>();
			}
			if (runtimeDataDictionary != null)
			{
				runtimeDataDictionary.Clear();
			}
			else
			{
				runtimeDataDictionary = new Dictionary<GPUInstancerPrototype, GClass1270>();
			}
			if (prototypeList == null)
			{
				prototypeList = new List<GPUInstancerPrototype>();
			}
		}
	}

	public virtual void InitializeSpatialPartitioning()
	{
	}

	public virtual void UpdateSpatialPartitioningCells(GPUInstancerCameraData renderingCameraData)
	{
	}

	public virtual void DeletePrototype(GPUInstancerPrototype prototype, bool removeSO = true)
	{
		prototypeList.Remove(prototype);
		if (removeSO && prototype.useGeneratedBillboard && prototype.billboard != null && GClass1262.gpuiSettings.billboardAtlasBindings.DeleteBillboardTextures(prototype))
		{
			prototype.billboard = null;
		}
	}

	public virtual void RemoveInstancesInsideBounds(Bounds bounds, float offset, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		if (runtimeDataList == null)
		{
			return;
		}
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			if (prototypeFilter == null || prototypeFilter.Contains(runtimeData.prototype))
			{
				GClass1274.RemoveInstancesInsideBounds(runtimeData.transformationMatrixVisibilityBuffer, bounds.center, bounds.extents, offset);
			}
		}
	}

	public virtual void RemoveInstancesInsideCollider(Collider collider, float offset, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		if (runtimeDataList == null)
		{
			return;
		}
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			if (prototypeFilter == null || prototypeFilter.Contains(runtimeData.prototype))
			{
				if (collider is BoxCollider)
				{
					GClass1274.RemoveInstancesInsideBoxCollider(runtimeData.transformationMatrixVisibilityBuffer, (BoxCollider)collider, offset);
				}
				else if (collider is SphereCollider)
				{
					GClass1274.RemoveInstancesInsideSphereCollider(runtimeData.transformationMatrixVisibilityBuffer, (SphereCollider)collider, offset);
				}
				else if (collider is CapsuleCollider)
				{
					GClass1274.RemoveInstancesInsideCapsuleCollider(runtimeData.transformationMatrixVisibilityBuffer, (CapsuleCollider)collider, offset);
				}
				else
				{
					GClass1274.RemoveInstancesInsideBounds(runtimeData.transformationMatrixVisibilityBuffer, collider.bounds.center, collider.bounds.extents, offset);
				}
			}
		}
	}

	public virtual void SetGlobalPositionOffset(Vector3 offsetPosition)
	{
	}

	public void ClearCompletedThreads()
	{
		if (activeThreads.Count <= 0)
		{
			return;
		}
		for (int num = activeThreads.Count - 1; num >= 0; num--)
		{
			if (!activeThreads[num].IsAlive)
			{
				activeThreads.RemoveAt(num);
			}
		}
	}

	public void InitializeCameraData()
	{
		Camera camera = (IsOptic ? CameraClass.Instance.OpticCameraManager.Camera : CameraClass.Instance.Camera);
		cameraData.SetCamera(camera);
		if (!(Camera == null) && MotionVectorsCommandBuffer != null)
		{
			Camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, MotionVectorsCommandBuffer);
			Camera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, MotionVectorsAfterCommandBuffer);
			Camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, MotionVectorsCommandBuffer);
			Camera.AddCommandBuffer(CameraEvent.AfterGBuffer, MotionVectorsAfterCommandBuffer);
		}
	}

	public void SetupOcclusionCulling()
	{
		if (!(Camera == null) && isOcclusionCulling && !(cameraData.hiZOcclusionGenerator != null))
		{
			GPUInstancerHiZOcclusionGenerator orAddComponent = GClass6.GetOrAddComponent<GPUInstancerHiZOcclusionGenerator>(cameraData.mainCamera.gameObject);
			cameraData.hiZOcclusionGenerator = orAddComponent;
			cameraData.hiZOcclusionGenerator.Initialize(cameraData.mainCamera);
		}
	}

	public void UpdateBuffers()
	{
		if (!(Camera == null))
		{
			if (isOcclusionCulling && cameraData.hiZOcclusionGenerator == null)
			{
				SetupOcclusionCulling();
			}
			cameraData.CalculateCameraData();
			instancingBounds.center = cameraData.mainCamera.transform.position;
			if (lastDrawCallFrame != Time.frameCount)
			{
				lastDrawCallFrame = Time.frameCount;
				timeSinceLastDrawCall = Time.realtimeSinceStartup - lastDrawCallTime;
				lastDrawCallTime = Time.realtimeSinceStartup;
			}
			UpdateSpatialPartitioningCells(cameraData);
			GClass1274.UpdateGPUBuffers(_cameraComputeShader, _cameraComputeKernelIDs, _visibilityComputeShader, _instanceVisibilityComputeKernelIDs, runtimeDataList, terrainSettings, cameraData, isFrustumCulling, isOcclusionCulling, showRenderedAmount, isInitial);
			if (GClass1274.matrixHandlingType == GPUIMatrixHandlingType.CopyToTexture)
			{
				GClass1274.DispatchBufferToTexture(runtimeDataList, _bufferToTextureComputeShader, _bufferToTextureComputeKernelID);
			}
			isInitial = false;
		}
	}

	public void SetCamera(Camera camera)
	{
		if (cameraData == null)
		{
			cameraData = new GPUInstancerCameraData(camera);
		}
		else
		{
			cameraData.SetCamera(camera);
		}
		if (cameraData.hiZOcclusionGenerator != null)
		{
			UnityEngine.Object.DestroyImmediate(cameraData.hiZOcclusionGenerator);
		}
		SetupOcclusionCulling();
	}

	public ComputeBuffer GetTransformDataBuffer(GPUInstancerPrototype prototype)
	{
		return GetRuntimeData(prototype)?.transformationMatrixVisibilityBuffer;
	}

	public void SetLODBias(float newLODBias, List<GPUInstancerPrototype> prototypeFilter)
	{
		if (runtimeDataList == null || !(newLODBias > 0f))
		{
			return;
		}
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			if (runtimeData == null || runtimeData.lodBiasApplied <= 0f || runtimeData.lodBiasApplied == newLODBias || runtimeData.instanceLODs == null || runtimeData.instanceLODs.Count == 0 || (prototypeFilter != null && !prototypeFilter.Contains(runtimeData.prototype)))
			{
				continue;
			}
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				if (i < 4)
				{
					runtimeData.lodSizes[i * 4] *= runtimeData.lodBiasApplied / newLODBias;
				}
				else
				{
					runtimeData.lodSizes[(i - 4) * 4 + 1] *= runtimeData.lodBiasApplied / newLODBias;
				}
			}
			runtimeData.lodBiasApplied = newLODBias;
		}
	}

	public void UpdateTreeMPB()
	{
		if (treeProxyList == null || treeProxyList.Count <= 0)
		{
			return;
		}
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			if (runtimeData.bufferSize == 0 || (runtimeData.prototype.treeType != GPUInstancerTreeType.SpeedTree && runtimeData.prototype.treeType != GPUInstancerTreeType.SpeedTree8))
			{
				continue;
			}
			Transform transform = treeProxyList[runtimeData.prototype.prefabObject];
			if (!transform)
			{
				continue;
			}
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				if (transform.childCount <= i)
				{
					continue;
				}
				GClass1271 gClass = runtimeData.instanceLODs[i];
				MeshRenderer component = transform.GetChild(i).GetComponent<MeshRenderer>();
				for (int j = 0; j < gClass.renderers.Count; j++)
				{
					GClass1272 gClass2 = gClass.renderers[j];
					component.GetPropertyBlock(gClass2.mpb);
					if (gClass2.shadowMPB != null)
					{
						component.GetPropertyBlock(gClass2.shadowMPB);
					}
				}
			}
			GClass1274.SetAppendBuffers(runtimeData);
		}
	}

	public static void AddTreeProxy(GPUInstancerPrototype treePrototype, GClass1270 runtimeData)
	{
		switch (treePrototype.treeType)
		{
		case GPUInstancerTreeType.SpeedTree:
		case GPUInstancerTreeType.SpeedTree8:
		{
			if (treeProxyParent == null)
			{
				treeProxyParent = new GameObject("GPUI Tree Manager Proxy");
				if (treeProxyList != null)
				{
					treeProxyList.Clear();
				}
			}
			if (treeProxyList == null)
			{
				treeProxyList = new Dictionary<GameObject, Transform>();
			}
			else if (treeProxyList.ContainsKey(treePrototype.prefabObject))
			{
				if (!(treeProxyList[treePrototype.prefabObject] == null))
				{
					break;
				}
				treeProxyList.Remove(treePrototype.prefabObject);
			}
			Mesh mesh = new Mesh();
			mesh.name = "TreeProxyMesh";
			GameObject gameObject = new GameObject(treeProxyList.Count + "_" + treePrototype.name);
			gameObject.transform.SetParent(treeProxyParent.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			treeProxyList.Add(treePrototype.prefabObject, gameObject.transform);
			if (treePrototype.prefabObject.GetComponent<LODGroup>() != null)
			{
				LOD[] lODs = treePrototype.prefabObject.GetComponent<LODGroup>().GetLODs();
				for (int i = 0; i < lODs.Length; i++)
				{
					if (!lODs[i].renderers[0].GetComponent<BillboardRenderer>())
					{
						Material[] proxyMaterials = new Material[1]
						{
							new Material(Shader.Find(GClass1262.SHADER_GPUI_TREE_PROXY))
						};
						InstantiateTreeProxyObject(lODs[i].renderers[0].gameObject, gameObject, proxyMaterials, mesh, i == 0);
					}
				}
			}
			else
			{
				Material[] proxyMaterials2 = new Material[1]
				{
					new Material(Shader.Find(GClass1262.SHADER_GPUI_TREE_PROXY))
				};
				InstantiateTreeProxyObject(treePrototype.prefabObject, gameObject, proxyMaterials2, mesh, setBounds: true);
			}
			break;
		}
		case GPUInstancerTreeType.TreeCreatorTree:
			Shader.SetGlobalVector(int_0, GetWindVector());
			break;
		}
	}

	public static void InstantiateTreeProxyObject(GameObject treePrefab, GameObject proxyObjectParent, Material[] proxyMaterials, Mesh proxyMesh, bool setBounds)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(treePrefab, proxyObjectParent.transform);
		gameObject.name = treePrefab.name;
		if (setBounds)
		{
			proxyMesh.bounds = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds;
		}
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		component.shadowCastingMode = ShadowCastingMode.Off;
		component.receiveShadows = false;
		component.lightProbeUsage = LightProbeUsage.Off;
		for (int i = 0; i < proxyMaterials.Length; i++)
		{
			proxyMaterials[i].CopyPropertiesFromMaterial(component.materials[i]);
			proxyMaterials[i].enableInstancing = true;
		}
		component.sharedMaterials = proxyMaterials;
		component.GetComponent<MeshFilter>().sharedMesh = proxyMesh;
		Component[] components = gameObject.GetComponents(typeof(Component));
		for (int j = 0; j < components.Length; j++)
		{
			if (!(components[j] is Transform) && !(components[j] is MeshFilter) && !(components[j] is MeshRenderer) && !(components[j] is Tree))
			{
				UnityEngine.Object.Destroy(components[j]);
			}
		}
	}

	public static Vector4 GetWindVector()
	{
		if (_windVector != Vector4.zero)
		{
			return _windVector;
		}
		UpdateSceneWind();
		return _windVector;
	}

	public static void UpdateSceneWind()
	{
		WindZone[] array = UnityEngine.Object.FindObjectsOfType<WindZone>();
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (array[num].mode == WindZoneMode.Directional)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		_windVector = new Vector4(array[num].windTurbulence, array[num].windPulseMagnitude, array[num].windPulseFrequency, array[num].windMain);
	}

	public void LogThreadException()
	{
		Debug.LogException(threadException);
	}

	public GClass1270 GetRuntimeData(GPUInstancerPrototype prototype, bool logError = false)
	{
		GClass1270 value = null;
		if (!runtimeDataDictionary.TryGetValue(prototype, out value) && logError)
		{
			Debug.LogError("Can not find runtime data for prototype: " + prototype?.ToString() + ". Please check if the prototype was added to the Manager and the initialize method was called.");
		}
		return value;
	}

	public GPUInstancerManager()
	{
	}
}
