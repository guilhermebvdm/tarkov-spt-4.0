using System;
using EFT.BlitDebug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;

namespace GPUInstancer;

public class GPUInstancerHiZOcclusionGenerator : MonoBehaviour
{
	public bool debuggerEnabled;

	public bool debuggerGUIOnTop;

	[Range(0f, 0.1f)]
	public float debuggerOverlay;

	[Range(0f, 16f)]
	public int debuggerMipLevel;

	[Header("For info only, don't change:")]
	public RenderTexture hiZDepthTexture;

	public Texture unityDepthTexture;

	public Vector2 hiZTextureSize;

	[HideInInspector]
	public bool isVREnabled;

	private Camera camera_0;

	private SSAA ssaa_0;

	private int int_0;

	private int int_1;

	private bool bool_0;

	private GPUIOcclusionCullingType gpuiocclusionCullingType_0;

	private Material material_0;

	private int int_2;

	private RenderTexture[] renderTexture_0;

	private RawImage rawImage_0;

	private GameObject gameObject_0;

	private bool bool_1;

	private float float_0;

	public void Awake()
	{
		hiZTextureSize = Vector2.zero;
		GClass1262.SetupComputeTextureUtils();
		gpuiocclusionCullingType_0 = GClass1262.gpuiSettings.occlusionCullingType;
	}

	public void OnEnable()
	{
		if (!GClass1262.gpuiSettings.isLWRP && !GClass1262.gpuiSettings.isHDRP)
		{
			if (gpuiocclusionCullingType_0 == GPUIOcclusionCullingType.Default)
			{
				material_0 = new Material(Shader.Find(GClass1262.SHADER_GPUI_HIZ_OCCLUSION_GENERATOR));
				Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(method_5));
			}
			else
			{
				Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(method_3));
			}
		}
		else
		{
			RenderPipelineManager.endCameraRendering += method_2;
		}
	}

	public void OnDisable()
	{
		if (!GClass1262.gpuiSettings.isLWRP && !GClass1262.gpuiSettings.isHDRP)
		{
			if (gpuiocclusionCullingType_0 == GPUIOcclusionCullingType.Default)
			{
				Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(method_5));
			}
			else
			{
				Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(method_3));
			}
		}
		else
		{
			RenderPipelineManager.endCameraRendering -= method_2;
		}
	}

	public void OnDestroy()
	{
		if (hiZDepthTexture != null)
		{
			hiZDepthTexture.Release();
			UnityEngine.Object.DestroyImmediate(hiZDepthTexture);
			hiZDepthTexture = null;
		}
		if (renderTexture_0 == null)
		{
			return;
		}
		for (int i = 0; i < renderTexture_0.Length; i++)
		{
			if (renderTexture_0[i] != null)
			{
				UnityEngine.Object.DestroyImmediate(renderTexture_0[i]);
			}
		}
		renderTexture_0 = null;
	}

	public void Initialize(Camera occlusionCamera = null)
	{
		bool_0 = false;
		camera_0 = ((occlusionCamera != null) ? occlusionCamera : base.gameObject.GetComponent<Camera>());
		ssaa_0 = camera_0?.gameObject.GetComponent<SSAA>();
		if (camera_0 == null)
		{
			Debug.LogError("GPUI Hi-Z Occlision Culling Generator failed to initialize: camera not found.");
			bool_0 = true;
			return;
		}
		if (XRSettings.enabled)
		{
			isVREnabled = true;
			GClass1262.gpuiSettings.vrRenderingMode = ((XRSettings.stereoRenderingMode != XRSettings.StereoRenderingMode.SinglePass) ? 1 : 0);
			if (isVREnabled && material_0 != null)
			{
				if (GClass1262.gpuiSettings.vrRenderingMode == 1)
				{
					material_0.EnableKeyword("MULTIPASS_VR_ENABLED");
				}
				else
				{
					material_0.EnableKeyword("SINGLEPASS_VR_ENABLED");
				}
				if (GClass1262.gpuiSettings.testBothEyesForVROcclusion)
				{
					material_0.EnableKeyword("HIZ_TEXTURE_FOR_BOTH_EYES");
				}
			}
			int_0 = XRSettings.eyeTextureWidth;
			int_1 = XRSettings.eyeTextureHeight;
			if (camera_0.stereoTargetEye != StereoTargetEyeMask.Both)
			{
				Debug.LogError("GPUI Hi-Z Occlision works only for cameras that render to Both eyes. Disabling Occlusion Culling.");
				bool_0 = true;
				return;
			}
		}
		else
		{
			isVREnabled = false;
			int_0 = method_0();
			int_1 = method_1();
		}
		camera_0.depthTextureMode |= DepthTextureMode.Depth;
		method_6();
	}

	public int method_0()
	{
		int num = (camera_0 ? camera_0.pixelWidth : Screen.width);
		if (ssaa_0 != null)
		{
			num = ((ssaa_0.GetInputWidth() > 0) ? ssaa_0.GetInputWidth() : num);
		}
		return num;
	}

	public int method_1()
	{
		int num = (camera_0 ? camera_0.pixelHeight : Screen.height);
		if (ssaa_0 != null)
		{
			num = ((ssaa_0.GetInputHeight() > 0) ? ssaa_0.GetInputHeight() : num);
		}
		return num;
	}

	public void method_2(ScriptableRenderContext context, Camera camera)
	{
		method_3(camera);
	}

	public void method_3(Camera camera)
	{
		if (bool_0 || camera_0 == null || camera != camera_0)
		{
			return;
		}
		if (hiZDepthTexture == null)
		{
			Debug.LogWarning("GPUI HiZ Depth texture is null where it should not be. Recreating it.");
			method_6();
		}
		method_7();
		if (unityDepthTexture == null)
		{
			unityDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
		}
		if (!(unityDepthTexture != null))
		{
			return;
		}
		if (isVREnabled && GClass1262.gpuiSettings.vrRenderingMode == 1)
		{
			if (camera_0.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
			{
				method_4(0);
			}
			else if (GClass1262.gpuiSettings.testBothEyesForVROcclusion)
			{
				method_4((int)hiZTextureSize.x / 2);
			}
		}
		else
		{
			method_4(0);
		}
	}

	public void method_4(int offset)
	{
		GClass1274.CopyTextureWithComputeShader(unityDepthTexture, hiZDepthTexture, offset);
		for (int i = 0; i < int_2 - 1; i++)
		{
			RenderTexture renderTexture = renderTexture_0[i];
			if (i == 0)
			{
				GClass1274.ReduceTextureWithComputeShader(hiZDepthTexture, renderTexture, offset);
			}
			else
			{
				GClass1274.ReduceTextureWithComputeShader(renderTexture_0[i - 1], renderTexture, offset);
			}
			GClass1274.CopyTextureWithComputeShader(renderTexture, hiZDepthTexture, offset, 0, i + 1, reverseZ: false);
		}
	}

	public void method_5(Camera camera)
	{
		if (bool_0 || camera_0 == null || camera != camera_0)
		{
			return;
		}
		if (hiZDepthTexture == null)
		{
			Debug.LogWarning("GPUI HiZ Depth texture is null where it should not be. Recreating it.");
			method_6();
		}
		method_7();
		DebugGraphics.Blit(null, hiZDepthTexture, material_0, 0);
		for (int i = 0; i < int_2; i++)
		{
			RenderTexture renderTexture = renderTexture_0[i];
			if (i == 0)
			{
				DebugGraphics.Blit(hiZDepthTexture, renderTexture, material_0, 1);
			}
			else
			{
				DebugGraphics.Blit(renderTexture_0[i - 1], renderTexture, material_0, 1);
			}
			Graphics.CopyTexture(renderTexture, 0, 0, hiZDepthTexture, 0, i + 1);
		}
		method_8();
	}

	public void method_6()
	{
		if (isVREnabled)
		{
			hiZTextureSize.x = XRSettings.eyeTextureWidth;
			hiZTextureSize.y = XRSettings.eyeTextureHeight;
			if (GClass1262.gpuiSettings.testBothEyesForVROcclusion)
			{
				hiZTextureSize.x *= 2f;
			}
		}
		else
		{
			hiZTextureSize.x = method_0();
			hiZTextureSize.y = method_1();
		}
		int_2 = (int)Mathf.Floor(Mathf.Log(hiZTextureSize.x, 2f));
		if (!(hiZTextureSize.x <= 0f) && !(hiZTextureSize.y <= 0f) && int_2 != 0)
		{
			if (hiZDepthTexture != null)
			{
				hiZDepthTexture.Release();
				UnityEngine.Object.DestroyImmediate(hiZDepthTexture);
			}
			int num = (int)hiZTextureSize.x;
			int num2 = (int)hiZTextureSize.y;
			hiZDepthTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
			hiZDepthTexture.name = "GPUIHiZDepthTexture";
			hiZDepthTexture.filterMode = FilterMode.Point;
			hiZDepthTexture.useMipMap = true;
			hiZDepthTexture.autoGenerateMips = false;
			hiZDepthTexture.enableRandomWrite = true;
			hiZDepthTexture.Create();
			hiZDepthTexture.hideFlags = HideFlags.HideAndDontSave;
			hiZDepthTexture.GenerateMips();
			if (renderTexture_0 != null)
			{
				for (int i = 0; i < renderTexture_0.Length; i++)
				{
					if (renderTexture_0[i] != null)
					{
						renderTexture_0[i].Release();
						UnityEngine.Object.DestroyImmediate(renderTexture_0[i]);
					}
				}
			}
			renderTexture_0 = new RenderTexture[int_2];
			for (int j = 0; j < int_2; j++)
			{
				num >>= 1;
				num2 >>= 1;
				if (num == 0)
				{
					num = 1;
				}
				if (num2 == 0)
				{
					num2 = 1;
				}
				renderTexture_0[j] = new RenderTexture(num, num2, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
				renderTexture_0[j].name = "GPUIHiZDepthTexture_Mip_" + j;
				renderTexture_0[j].filterMode = FilterMode.Point;
				renderTexture_0[j].useMipMap = false;
				renderTexture_0[j].autoGenerateMips = false;
				renderTexture_0[j].enableRandomWrite = true;
				renderTexture_0[j].Create();
				renderTexture_0[j].hideFlags = HideFlags.HideAndDontSave;
			}
		}
		else
		{
			if (hiZDepthTexture != null)
			{
				hiZDepthTexture.Release();
				UnityEngine.Object.DestroyImmediate(hiZDepthTexture);
			}
			Debug.LogError("Cannot create GPUI HiZ Depth Texture for occlusion culling: Screen size is too small.");
		}
	}

	public void method_7()
	{
		if (isVREnabled)
		{
			if (int_0 != XRSettings.eyeTextureWidth || int_1 != XRSettings.eyeTextureHeight)
			{
				int_0 = XRSettings.eyeTextureWidth;
				int_1 = XRSettings.eyeTextureHeight;
				method_6();
			}
		}
		else if (int_0 != method_0() || int_1 != method_1())
		{
			int_0 = method_0();
			int_1 = method_1();
			unityDepthTexture = null;
			method_6();
		}
	}

	public void method_8()
	{
	}

	public void method_9()
	{
		gameObject_0 = new GameObject("GPUI HiZ Occlusion Culling Debugger");
		bool_1 = debuggerGUIOnTop;
		float_0 = debuggerOverlay;
		Canvas canvas = gameObject_0.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.pixelPerfect = true;
		canvas.sortingOrder = (debuggerGUIOnTop ? 10000 : (-10000));
		canvas.targetDisplay = 0;
		GameObject gameObject = new GameObject("HiZ Depth Texture");
		gameObject.transform.parent = gameObject_0.transform;
		rawImage_0 = gameObject.AddComponent<RawImage>();
		rawImage_0.color = new Color(1f, 1f, 1f, 1f - debuggerOverlay);
		Vector2 vector = new Vector2(0f, 0f);
		rawImage_0.rectTransform.anchorMin = vector;
		rawImage_0.rectTransform.anchorMax = vector;
		rawImage_0.rectTransform.pivot = vector;
		rawImage_0.rectTransform.position = Vector2.zero;
	}
}
