using System;
using System.Collections.Generic;
using System.Linq;
using AmplifyMotion;
using EFT.BlitDebug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("")]
public class AmplifyMotionEffectBase : MonoBehaviour
{
	[Header("Motion Blur")]
	public Quality QualityLevel = Quality.Standard;

	public int QualitySteps = 1;

	public float MotionScale = 3f;

	public float CameraMotionMult = 1f;

	public float MinVelocity = 1f;

	public float MaxVelocity = 10f;

	public float DepthThreshold = 0.01f;

	public bool Noise;

	[Header("Camera")]
	public Camera[] OverlayCameras = new Camera[0];

	public LayerMask CullingMask = -1;

	[Header("Objects")]
	public bool AutoRegisterObjs = true;

	public float MinResetDeltaDist = 1000f;

	[NonSerialized]
	public float MinResetDeltaDistSqr;

	public int ResetFrameDelay = 1;

	[Header("Low-Level")]
	[FormerlySerializedAs("workerThreads")]
	public int WorkerThreads;

	public bool SystemThreadPool;

	public bool ForceCPUOnly;

	public bool DebugMode;

	private Camera camera_0;

	private bool bool_0 = true;

	private int int_0;

	private int int_1;

	private RenderTexture renderTexture_0;

	private Material material_0;

	private Material material_1;

	private Material material_2;

	private Material material_3;

	private Material material_4;

	private Material material_5;

	private Material material_6;

	private Material material_7;

	private Material material_8;

	private Dictionary<Camera, AmplifyMotionCamera> dictionary_0 = new Dictionary<Camera, AmplifyMotionCamera>();

	internal Camera[] camera_1;

	internal AmplifyMotionCamera[] amplifyMotionCamera_0;

	internal bool bool_1 = true;

	private AmplifyMotionPostProcess amplifyMotionPostProcess_0;

	private int int_2 = 1;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private Quality quality_0;

	private AmplifyMotionCamera amplifyMotionCamera_1;

	private Class3642 class3642_0;

	public static Dictionary<GameObject, AmplifyMotionObjectBase> m_activeObjects = new Dictionary<GameObject, AmplifyMotionObjectBase>();

	public static Dictionary<Camera, AmplifyMotionCamera> m_activeCameras = new Dictionary<Camera, AmplifyMotionCamera>();

	private static bool bool_2 = false;

	private bool bool_3;

	private const CameraEvent cameraEvent_0 = CameraEvent.BeforeImageEffectsOpaque;

	private CommandBuffer commandBuffer_0;

	private const CameraEvent cameraEvent_1 = CameraEvent.BeforeImageEffectsOpaque;

	private CommandBuffer commandBuffer_1;

	private static bool bool_4 = false;

	private static AmplifyMotionEffectBase amplifyMotionEffectBase_0 = null;

	[Obsolete("workerThreads is deprecated, please use WorkerThreads instead.")]
	public int workerThreads
	{
		get
		{
			return WorkerThreads;
		}
		set
		{
			WorkerThreads = value;
		}
	}

	public Material Material_0 => material_4;

	public Material Material_1 => material_1;

	public Material Material_2 => material_2;

	public Material Material_3 => material_3;

	public RenderTexture RenderTexture_0 => renderTexture_0;

	public Dictionary<Camera, AmplifyMotionCamera> LinkedCameras => dictionary_0;

	public float Single_0 => float_2;

	public float Single_1 => float_3;

	public AmplifyMotionCamera BaseCamera => amplifyMotionCamera_1;

	public Class3642 Class3642_0 => class3642_0;

	public static bool IsD3D => bool_2;

	public bool CanUseGPU => bool_3;

	public static bool IgnoreMotionScaleWarning => bool_4;

	public static AmplifyMotionEffectBase FirstInstance => amplifyMotionEffectBase_0;

	public static AmplifyMotionEffectBase Instance => amplifyMotionEffectBase_0;

	public void Awake()
	{
		if (amplifyMotionEffectBase_0 == null)
		{
			amplifyMotionEffectBase_0 = this;
		}
		bool_2 = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D");
		int_2 = 1;
		int_1 = 0;
		int_0 = 0;
		if (ForceCPUOnly)
		{
			bool_3 = false;
			return;
		}
		bool flag = SystemInfo.graphicsShaderLevel >= 30;
		bool flag2 = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf);
		bool flag3 = SystemInfo.SupportsTextureFormat(TextureFormat.RGHalf);
		bool flag4 = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf);
		bool flag5 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
		bool_3 = flag && flag2 && flag3 && flag4 && flag5;
	}

	public void method_0()
	{
		int_2 = 1;
	}

	public int method_1(GameObject obj)
	{
		if (obj.isStatic)
		{
			return 0;
		}
		int_2++;
		if (int_2 > 254)
		{
			int_2 = 1;
		}
		return int_2;
	}

	public void method_2(ref Material mat)
	{
		if (mat != null)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	public bool method_3(Material material, string name)
	{
		bool result = true;
		if (!(material == null) && !(material.shader == null))
		{
			if (!material.shader.isSupported)
			{
				Debug.LogWarning("[AmplifyMotion] " + name + " shader not supported on this platform");
				result = false;
			}
		}
		else
		{
			Debug.LogWarning("[AmplifyMotion] Error creating " + name + " material");
			result = false;
		}
		return result;
	}

	public void method_4()
	{
		method_2(ref material_0);
		method_2(ref material_1);
		method_2(ref material_2);
		method_2(ref material_3);
		method_2(ref material_4);
		method_2(ref material_5);
		method_2(ref material_6);
		method_2(ref material_7);
		method_2(ref material_8);
	}

	public bool method_5()
	{
		method_4();
		string text = "Hidden/Amplify Motion/MotionBlurSM" + ((SystemInfo.graphicsShaderLevel >= 30) ? 3 : 2);
		string text2 = "Hidden/Amplify Motion/SolidVectors";
		string text3 = "Hidden/Amplify Motion/SkinnedVectors";
		string text4 = "Hidden/Amplify Motion/ClothVectors";
		string text5 = "Hidden/Amplify Motion/ReprojectionVectors";
		string text6 = "Hidden/Amplify Motion/Combine";
		string text7 = "Hidden/Amplify Motion/Dilation";
		string text8 = "Hidden/Amplify Motion/Depth";
		string text9 = "Hidden/Amplify Motion/Debug";
		try
		{
			material_0 = new Material(GClass872.Find(text))
			{
				hideFlags = HideFlags.DontSave
			};
			material_1 = new Material(GClass872.Find(text2))
			{
				hideFlags = HideFlags.DontSave
			};
			material_2 = new Material(GClass872.Find(text3))
			{
				hideFlags = HideFlags.DontSave
			};
			material_3 = new Material(GClass872.Find(text4))
			{
				hideFlags = HideFlags.DontSave
			};
			material_4 = new Material(GClass872.Find(text5))
			{
				hideFlags = HideFlags.DontSave
			};
			material_5 = new Material(GClass872.Find(text6))
			{
				hideFlags = HideFlags.DontSave
			};
			material_6 = new Material(GClass872.Find(text7))
			{
				hideFlags = HideFlags.DontSave
			};
			material_7 = new Material(GClass872.Find(text8))
			{
				hideFlags = HideFlags.DontSave
			};
			material_8 = new Material(GClass872.Find(text9))
			{
				hideFlags = HideFlags.DontSave
			};
		}
		catch (Exception)
		{
		}
		if (method_3(material_0, text) && method_3(material_1, text2) && method_3(material_2, text3) && method_3(material_3, text4) && method_3(material_4, text5) && method_3(material_5, text6) && method_3(material_6, text7) && method_3(material_7, text8))
		{
			return method_3(material_8, text9);
		}
		return false;
	}

	public RenderTexture method_6(string name, int depth, RenderTextureFormat fmt, RenderTextureReadWrite rw, FilterMode fm)
	{
		RenderTexture renderTexture = new RenderTexture(int_0, int_1, depth, fmt, rw);
		renderTexture.hideFlags = HideFlags.DontSave;
		renderTexture.name = name;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.filterMode = fm;
		renderTexture.Create();
		return renderTexture;
	}

	public void method_7(ref RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.active = null;
			rt.Release();
			UnityEngine.Object.DestroyImmediate(rt);
			rt = null;
		}
	}

	public void method_8(ref Texture tex)
	{
		if (tex != null)
		{
			UnityEngine.Object.DestroyImmediate(tex);
			tex = null;
		}
	}

	public void method_9()
	{
		RenderTexture.active = null;
		method_7(ref renderTexture_0);
	}

	public void method_10(bool qualityChanged)
	{
		int num = Mathf.Max(Mathf.FloorToInt((float)camera_0.pixelWidth + 0.5f), 1);
		int num2 = Mathf.Max(Mathf.FloorToInt((float)camera_0.pixelHeight + 0.5f), 1);
		if (QualityLevel == Quality.Mobile)
		{
			num /= 2;
			num2 /= 2;
		}
		if (int_0 != num || int_1 != num2)
		{
			int_0 = num;
			int_1 = num2;
			method_9();
		}
		if (renderTexture_0 == null)
		{
			renderTexture_0 = method_6("AM-MotionVectors", 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, FilterMode.Point);
		}
	}

	public bool CheckSupport()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.LogError("[AmplifyMotion] Initialization failed. This plugin requires support for Image Effects and Render Textures.");
			return false;
		}
		return true;
	}

	public void method_11()
	{
		if (WorkerThreads <= 0)
		{
			WorkerThreads = Mathf.Max(Environment.ProcessorCount / 2, 1);
		}
		class3642_0 = new Class3642();
		class3642_0.method_0(WorkerThreads, SystemThreadPool);
	}

	public void method_12()
	{
		if (class3642_0 != null)
		{
			class3642_0.method_1();
			class3642_0 = null;
		}
	}

	public void method_13()
	{
		method_14();
		commandBuffer_0 = new CommandBuffer();
		commandBuffer_0.name = "AmplifyMotion.Update";
		camera_0.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
		commandBuffer_1 = new CommandBuffer();
		commandBuffer_1.name = "AmplifyMotion.FixedUpdate";
		camera_0.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_1);
	}

	public void method_14()
	{
		if (commandBuffer_0 != null)
		{
			camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
			commandBuffer_0.Release();
			commandBuffer_0 = null;
		}
		if (commandBuffer_1 != null)
		{
			camera_0.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_1);
			commandBuffer_1.Release();
			commandBuffer_1 = null;
		}
	}

	public void OnEnable()
	{
		camera_0 = GetComponent<Camera>();
		if (!CheckSupport())
		{
			base.enabled = false;
			return;
		}
		method_11();
		bool_0 = true;
		if (!method_5())
		{
			Debug.LogError("[AmplifyMotion] Failed loading or compiling necessary shaders. Please try reinstalling Amplify Motion or contact support@amplify.pt");
			base.enabled = false;
			return;
		}
		if (AutoRegisterObjs)
		{
			UpdateActiveObjects();
		}
		method_17();
		method_13();
		method_10(qualityChanged: true);
		dictionary_0.TryGetValue(camera_0, out amplifyMotionCamera_1);
		if (amplifyMotionCamera_1 == null)
		{
			Debug.LogError("[AmplifyMotion] Failed setting up Base Camera. Please contact support@amplify.pt");
			base.enabled = false;
			return;
		}
		if (amplifyMotionPostProcess_0 != null)
		{
			amplifyMotionPostProcess_0.enabled = true;
		}
		quality_0 = QualityLevel;
	}

	public void OnDisable()
	{
		if (amplifyMotionPostProcess_0 != null)
		{
			amplifyMotionPostProcess_0.enabled = false;
		}
		method_14();
		method_12();
	}

	public void Start()
	{
		method_18();
	}

	public void method_15(Camera reference)
	{
		dictionary_0.Remove(reference);
	}

	public void OnDestroy()
	{
		AmplifyMotionCamera[] array = dictionary_0.Values.ToArray();
		foreach (AmplifyMotionCamera amplifyMotionCamera in array)
		{
			if (amplifyMotionCamera != null && amplifyMotionCamera.gameObject != base.gameObject)
			{
				Camera component = amplifyMotionCamera.GetComponent<Camera>();
				if (component != null)
				{
					component.targetTexture = null;
				}
				UnityEngine.Object.DestroyImmediate(amplifyMotionCamera);
			}
		}
		method_9();
		method_4();
	}

	public GameObject method_16(GameObject obj, string auxCameraName)
	{
		GameObject gameObject = null;
		if (obj.name == auxCameraName)
		{
			gameObject = obj;
		}
		else
		{
			foreach (Transform item in obj.transform)
			{
				gameObject = method_16(item.gameObject, auxCameraName);
				if (gameObject != null)
				{
					break;
				}
			}
		}
		return gameObject;
	}

	public void method_17()
	{
		List<Camera> list = new List<Camera>(OverlayCameras.Length);
		for (int i = 0; i < OverlayCameras.Length; i++)
		{
			if (OverlayCameras[i] != null)
			{
				list.Add(OverlayCameras[i]);
			}
		}
		Camera[] array = new Camera[list.Count + 1];
		array[0] = camera_0;
		for (int j = 0; j < list.Count; j++)
		{
			array[j + 1] = list[j];
		}
		dictionary_0.Clear();
		for (int k = 0; k < array.Length; k++)
		{
			Camera camera = array[k];
			if (!dictionary_0.ContainsKey(camera))
			{
				AmplifyMotionCamera amplifyMotionCamera = camera.gameObject.GetComponent<AmplifyMotionCamera>();
				if (amplifyMotionCamera != null)
				{
					amplifyMotionCamera.enabled = false;
					amplifyMotionCamera.enabled = true;
				}
				else
				{
					amplifyMotionCamera = camera.gameObject.AddComponent<AmplifyMotionCamera>();
				}
				amplifyMotionCamera.LinkTo(this, k > 0);
				dictionary_0.Add(camera, amplifyMotionCamera);
				bool_1 = true;
			}
		}
	}

	public void UpdateActiveCameras()
	{
		method_17();
	}

	public static void smethod_0(AmplifyMotionCamera cam)
	{
		if (!m_activeCameras.ContainsValue(cam))
		{
			m_activeCameras.Add(cam.GetComponent<Camera>(), cam);
		}
		foreach (AmplifyMotionObjectBase value in m_activeObjects.Values)
		{
			value.method_0(cam);
		}
	}

	public static void smethod_1(AmplifyMotionCamera cam)
	{
		foreach (AmplifyMotionObjectBase value in m_activeObjects.Values)
		{
			value.method_1(cam);
		}
		m_activeCameras.Remove(cam.GetComponent<Camera>());
	}

	public void UpdateActiveObjects()
	{
		GameObject[] array = GClass870.FindUnityObjectsOfType(typeof(GameObject)) as GameObject[];
		for (int i = 0; i < array.Length; i++)
		{
			if (!m_activeObjects.ContainsKey(array[i]))
			{
				smethod_6(array[i], autoReg: true);
			}
		}
	}

	public static void smethod_2(AmplifyMotionObjectBase obj)
	{
		m_activeObjects.Add(obj.gameObject, obj);
		foreach (AmplifyMotionCamera value in m_activeCameras.Values)
		{
			obj.method_0(value);
		}
	}

	public static void smethod_3(AmplifyMotionObjectBase obj)
	{
		foreach (AmplifyMotionCamera value in m_activeCameras.Values)
		{
			obj.method_1(value);
		}
		m_activeObjects.Remove(obj.gameObject);
	}

	public static bool smethod_4(Material[] materials)
	{
		int num = 0;
		Material material;
		while (true)
		{
			if (num < materials.Length)
			{
				material = materials[num];
				if (material != null)
				{
					string text = material.GetTag("RenderType", searchFallbacks: false);
					if (text == "Opaque" || text == "TransparentCutout")
					{
						break;
					}
				}
				num++;
				continue;
			}
			return false;
		}
		if (!material.IsKeywordEnabled("_ALPHABLEND_ON"))
		{
			return !material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
		}
		return false;
	}

	public static bool smethod_5(GameObject gameObj, bool autoReg)
	{
		if (gameObj.isStatic)
		{
			return false;
		}
		Renderer component = gameObj.GetComponent<Renderer>();
		if (!(component == null) && component.sharedMaterials != null && !component.isPartOfStaticBatch)
		{
			if (!component.enabled)
			{
				return false;
			}
			if (component.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
			{
				return false;
			}
			if (component.GetType() == typeof(SpriteRenderer))
			{
				return false;
			}
			if (!smethod_4(component.sharedMaterials))
			{
				return false;
			}
			Type type = component.GetType();
			if (!(type == typeof(MeshRenderer)) && !(type == typeof(SkinnedMeshRenderer)))
			{
				if (type == typeof(ParticleSystemRenderer) && !autoReg)
				{
					ParticleSystemRenderMode renderMode = (component as ParticleSystemRenderer).renderMode;
					if (renderMode != ParticleSystemRenderMode.Mesh)
					{
						return renderMode == ParticleSystemRenderMode.Billboard;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static void smethod_6(GameObject gameObj, bool autoReg)
	{
		if (smethod_5(gameObj, autoReg) && gameObj.GetComponent<AmplifyMotionObjectBase>() == null)
		{
			AmplifyMotionObjectBase.bool_0 = false;
			gameObj.AddComponent<AmplifyMotionObjectBase>();
			AmplifyMotionObjectBase.bool_0 = true;
		}
	}

	public static void smethod_7(GameObject gameObj)
	{
		AmplifyMotionObjectBase component = gameObj.GetComponent<AmplifyMotionObjectBase>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	public void Register(GameObject gameObj)
	{
		if (!m_activeObjects.ContainsKey(gameObj))
		{
			smethod_6(gameObj, autoReg: false);
		}
	}

	public static void RegisterS(GameObject gameObj)
	{
		if (!m_activeObjects.ContainsKey(gameObj))
		{
			smethod_6(gameObj, autoReg: false);
		}
	}

	public void RegisterRecursively(GameObject gameObj)
	{
		if (!m_activeObjects.ContainsKey(gameObj))
		{
			smethod_6(gameObj, autoReg: false);
		}
		foreach (Transform item in gameObj.transform)
		{
			RegisterRecursively(item.gameObject);
		}
	}

	public static void RegisterRecursivelyS(GameObject gameObj)
	{
		if (!m_activeObjects.ContainsKey(gameObj))
		{
			smethod_6(gameObj, autoReg: false);
		}
		foreach (Transform item in gameObj.transform)
		{
			RegisterRecursivelyS(item.gameObject);
		}
	}

	public void Unregister(GameObject gameObj)
	{
		if (m_activeObjects.ContainsKey(gameObj))
		{
			smethod_7(gameObj);
		}
	}

	public static void UnregisterS(GameObject gameObj)
	{
		if (m_activeObjects.ContainsKey(gameObj))
		{
			smethod_7(gameObj);
		}
	}

	public void UnregisterRecursively(GameObject gameObj)
	{
		if (m_activeObjects.ContainsKey(gameObj))
		{
			smethod_7(gameObj);
		}
		foreach (Transform item in gameObj.transform)
		{
			UnregisterRecursively(item.gameObject);
		}
	}

	public static void UnregisterRecursivelyS(GameObject gameObj)
	{
		if (m_activeObjects.ContainsKey(gameObj))
		{
			smethod_7(gameObj);
		}
		foreach (Transform item in gameObj.transform)
		{
			UnregisterRecursivelyS(item.gameObject);
		}
	}

	public void method_18()
	{
		Camera camera = null;
		float num = float.MinValue;
		if (bool_1)
		{
			method_19();
		}
		for (int i = 0; i < camera_1.Length; i++)
		{
			if (camera_1[i] != null && camera_1[i].isActiveAndEnabled && camera_1[i].depth > num)
			{
				camera = camera_1[i];
				num = camera_1[i].depth;
			}
		}
		if (amplifyMotionPostProcess_0 != null && amplifyMotionPostProcess_0.gameObject != camera.gameObject)
		{
			UnityEngine.Object.DestroyImmediate(amplifyMotionPostProcess_0);
			amplifyMotionPostProcess_0 = null;
		}
		if (!(amplifyMotionPostProcess_0 == null) || !(camera != null) || !(camera != camera_0))
		{
			return;
		}
		AmplifyMotionPostProcess[] components = base.gameObject.GetComponents<AmplifyMotionPostProcess>();
		if (components != null && components.Length != 0)
		{
			for (int j = 0; j < components.Length; j++)
			{
				UnityEngine.Object.DestroyImmediate(components[j]);
			}
		}
		amplifyMotionPostProcess_0 = camera.gameObject.AddComponent<AmplifyMotionPostProcess>();
		amplifyMotionPostProcess_0.Instance = this;
	}

	public void LateUpdate()
	{
		if (amplifyMotionCamera_1.AutoStep)
		{
			float num = (Application.isPlaying ? Time.unscaledDeltaTime : Time.fixedDeltaTime);
			float fixedDeltaTime = Time.fixedDeltaTime;
			float_0 = ((num >= float.Epsilon) ? num : float_0);
			float_1 = ((num >= float.Epsilon) ? fixedDeltaTime : float_1);
		}
		QualitySteps = Mathf.Clamp(QualitySteps, 0, 16);
		MotionScale = Mathf.Max(MotionScale, 0f);
		MinVelocity = Mathf.Min(MinVelocity, MaxVelocity);
		DepthThreshold = Mathf.Max(DepthThreshold, 0f);
		MinResetDeltaDist = Mathf.Max(MinResetDeltaDist, 0f);
		MinResetDeltaDistSqr = MinResetDeltaDist * MinResetDeltaDist;
		ResetFrameDelay = Mathf.Max(ResetFrameDelay, 0);
		method_18();
	}

	public void StopAutoStep()
	{
		foreach (AmplifyMotionCamera value in dictionary_0.Values)
		{
			value.StopAutoStep();
		}
	}

	public void StartAutoStep()
	{
		foreach (AmplifyMotionCamera value in dictionary_0.Values)
		{
			value.StartAutoStep();
		}
	}

	public void Step(float delta)
	{
		float_0 = delta;
		float_1 = delta;
		foreach (AmplifyMotionCamera value in dictionary_0.Values)
		{
			value.Step();
		}
	}

	public void method_19()
	{
		Dictionary<Camera, AmplifyMotionCamera>.KeyCollection keys = dictionary_0.Keys;
		Dictionary<Camera, AmplifyMotionCamera>.ValueCollection values = dictionary_0.Values;
		if (camera_1 == null || keys.Count != camera_1.Length)
		{
			camera_1 = new Camera[keys.Count];
		}
		if (amplifyMotionCamera_0 == null || values.Count != amplifyMotionCamera_0.Length)
		{
			amplifyMotionCamera_0 = new AmplifyMotionCamera[values.Count];
		}
		keys.CopyTo(camera_1, 0);
		values.CopyTo(amplifyMotionCamera_0, 0);
		bool_1 = false;
	}

	public void FixedUpdate()
	{
		if (!camera_0.enabled)
		{
			return;
		}
		if (bool_1)
		{
			method_19();
		}
		commandBuffer_1.Clear();
		for (int i = 0; i < amplifyMotionCamera_0.Length; i++)
		{
			if (amplifyMotionCamera_0[i] != null && amplifyMotionCamera_0[i].isActiveAndEnabled)
			{
				amplifyMotionCamera_0[i].FixedUpdateTransform(this, commandBuffer_1);
			}
		}
	}

	public void OnPreRender()
	{
		if (!camera_0.enabled || (Time.frameCount != 1 && !(Mathf.Abs(Time.unscaledDeltaTime) >= float.Epsilon)))
		{
			return;
		}
		if (bool_1)
		{
			method_19();
		}
		commandBuffer_0.Clear();
		for (int i = 0; i < amplifyMotionCamera_0.Length; i++)
		{
			if (amplifyMotionCamera_0[i] != null && amplifyMotionCamera_0[i].isActiveAndEnabled)
			{
				amplifyMotionCamera_0[i].UpdateTransform(this, commandBuffer_0);
			}
		}
	}

	public void OnPostRender()
	{
		bool qualityChanged = QualityLevel != quality_0;
		quality_0 = QualityLevel;
		method_10(qualityChanged);
		method_0();
		bool flag;
		bool clearColor = !(flag = CameraMotionMult >= float.Epsilon) || bool_0;
		float num = ((DepthThreshold >= float.Epsilon) ? (1f / DepthThreshold) : float.MaxValue);
		float_2 = ((float_0 >= float.Epsilon) ? (MotionScale * (1f / float_0)) : 0f);
		float_3 = ((float_1 >= float.Epsilon) ? (MotionScale * (1f / float_1)) : 0f);
		float scale = ((!bool_0) ? float_2 : 0f);
		float fixedScale = ((!bool_0) ? float_3 : 0f);
		Shader.SetGlobalFloat("_AM_MIN_VELOCITY", MinVelocity);
		Shader.SetGlobalFloat("_AM_MAX_VELOCITY", MaxVelocity);
		Shader.SetGlobalFloat("_AM_RCP_TOTAL_VELOCITY", 1f / (MaxVelocity - MinVelocity));
		Shader.SetGlobalVector("_AM_DEPTH_THRESHOLD", new Vector2(DepthThreshold, num));
		renderTexture_0.DiscardContents();
		amplifyMotionCamera_1.PreRenderVectors(renderTexture_0, clearColor, num);
		for (int i = 0; i < amplifyMotionCamera_0.Length; i++)
		{
			AmplifyMotionCamera amplifyMotionCamera = amplifyMotionCamera_0[i];
			if (amplifyMotionCamera != null && amplifyMotionCamera.Overlay && amplifyMotionCamera.isActiveAndEnabled)
			{
				amplifyMotionCamera.PreRenderVectors(renderTexture_0, clearColor, num);
				amplifyMotionCamera.RenderVectors(scale, fixedScale, QualityLevel);
			}
		}
		if (flag)
		{
			float num2 = ((float_0 >= float.Epsilon) ? (MotionScale * CameraMotionMult * (1f / float_0)) : 0f);
			float scale2 = ((!bool_0) ? num2 : 0f);
			renderTexture_0.DiscardContents();
			amplifyMotionCamera_1.RenderReprojectionVectors(renderTexture_0, scale2);
		}
		amplifyMotionCamera_1.RenderVectors(scale, fixedScale, QualityLevel);
		for (int j = 0; j < amplifyMotionCamera_0.Length; j++)
		{
			AmplifyMotionCamera amplifyMotionCamera2 = amplifyMotionCamera_0[j];
			if (amplifyMotionCamera2 != null && amplifyMotionCamera2.Overlay && amplifyMotionCamera2.isActiveAndEnabled)
			{
				amplifyMotionCamera2.RenderVectors(scale, fixedScale, QualityLevel);
			}
		}
		bool_0 = false;
	}

	public void method_20(RenderTexture source, RenderTexture destination, Vector4 blurStep)
	{
		bool flag = QualityLevel == Quality.Mobile;
		int pass = (int)(QualityLevel + (Noise ? 4 : 0));
		RenderTexture renderTexture = null;
		if (flag)
		{
			renderTexture = RenderTexture.GetTemporary(int_0, int_1, 0, RenderTextureFormat.ARGB32);
			renderTexture.name = "AM-DepthTemp";
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Point;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(int_0, int_1, 0, source.format);
		temporary.name = "AM-CombinedTemp";
		temporary.wrapMode = TextureWrapMode.Clamp;
		temporary.filterMode = FilterMode.Point;
		temporary.DiscardContents();
		material_5.SetTexture("_MotionTex", renderTexture_0);
		source.filterMode = FilterMode.Point;
		DebugGraphics.Blit(source, temporary, material_5, 0);
		material_0.SetTexture("_MotionTex", renderTexture_0);
		if (flag)
		{
			DebugGraphics.Blit(null, renderTexture, material_7, 0);
			material_0.SetTexture("_DepthTex", renderTexture);
		}
		if (QualitySteps > 1)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(int_0, int_1, 0, source.format);
			temporary2.name = "AM-CombinedTemp2";
			temporary2.filterMode = FilterMode.Point;
			float num = 1f / (float)QualitySteps;
			float num2 = 1f;
			RenderTexture renderTexture2 = temporary;
			RenderTexture renderTexture3 = temporary2;
			for (int i = 0; i < QualitySteps; i++)
			{
				if (renderTexture3 != destination)
				{
					renderTexture3.DiscardContents();
				}
				material_0.SetVector("_AM_BLUR_STEP", blurStep * num2);
				DebugGraphics.Blit(renderTexture2, renderTexture3, material_0, pass);
				if (i < QualitySteps - 2)
				{
					RenderTexture renderTexture4 = renderTexture3;
					renderTexture3 = renderTexture2;
					renderTexture2 = renderTexture4;
				}
				else
				{
					renderTexture2 = renderTexture3;
					renderTexture3 = destination;
				}
				num2 -= num;
			}
			RenderTexture.ReleaseTemporary(temporary2);
		}
		else
		{
			material_0.SetVector("_AM_BLUR_STEP", blurStep);
			DebugGraphics.Blit(temporary, destination, material_0, pass);
		}
		if (flag)
		{
			material_5.SetTexture("_MotionTex", renderTexture_0);
			DebugGraphics.Blit(source, destination, material_5, 1);
		}
		RenderTexture.ReleaseTemporary(temporary);
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amplifyMotionPostProcess_0 == null)
		{
			PostProcess(source, destination);
		}
		else
		{
			GClass860.BlitOrCopy(source, destination);
		}
	}

	public void PostProcess(RenderTexture source, RenderTexture destination)
	{
		Vector4 zero = Vector4.zero;
		zero.x = MaxVelocity / 1000f;
		zero.y = MaxVelocity / 1000f;
		RenderTexture renderTexture = null;
		if (QualitySettings.antiAliasing > 1)
		{
			renderTexture = RenderTexture.GetTemporary(int_0, int_1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			renderTexture.name = "AM-DilatedTemp";
			renderTexture.filterMode = FilterMode.Point;
			material_6.SetTexture("_MotionTex", renderTexture_0);
			DebugGraphics.Blit(renderTexture_0, renderTexture, material_6, 0);
			material_6.SetTexture("_MotionTex", renderTexture);
			DebugGraphics.Blit(renderTexture, renderTexture_0, material_6, 1);
		}
		if (DebugMode)
		{
			material_8.SetTexture("_MotionTex", renderTexture_0);
			Graphics.Blit(source, destination, material_8);
		}
		else
		{
			method_20(source, destination, zero);
		}
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}
}
