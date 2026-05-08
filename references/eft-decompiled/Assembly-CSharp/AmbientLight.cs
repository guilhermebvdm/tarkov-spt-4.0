using System;
using System.Collections.Generic;
using GPUInstancer;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[GAttribute8(19000)]
public class AmbientLight : OnRenderObjectConnector
{
	public class Class619 : IDisposable
	{
		public CommandBuffer CbStencil;

		public CommandBuffer CbAmbient;

		public RenderTexture Texture;

		public bool HDR;

		~Class619()
		{
			method_0(disposing: false);
		}

		public void Dispose()
		{
			method_0(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void method_0(bool disposing)
		{
			CbStencil?.Dispose();
			CbStencil = null;
			CbAmbient?.Dispose();
			CbAmbient = null;
			if ((bool)Texture)
			{
				Texture.Release();
				GClass972.SafeDestroy(Texture);
			}
			Texture = null;
		}
	}

	[Serializable]
	public class CullingSettings
	{
		public float Distance = 100f;

		public float FadeLength = 5f;

		[NonSerialized]
		public float MaxSqrtDistance;

		[NonSerialized]
		public float MinSqrtDistance;

		[NonSerialized]
		public float InvSqrtDistRange;

		public void Update()
		{
			MaxSqrtDistance = Distance * Distance;
			MinSqrtDistance = (Distance - FadeLength) * (Distance - FadeLength);
			InvSqrtDistRange = 1f / (MinSqrtDistance - MaxSqrtDistance);
		}

		public bool PassCulling(float sqrtDistance, out float fadeValue)
		{
			if (sqrtDistance > MaxSqrtDistance)
			{
				fadeValue = 0f;
				return false;
			}
			if (sqrtDistance < MinSqrtDistance)
			{
				fadeValue = 1f;
				return true;
			}
			fadeValue = (sqrtDistance - MaxSqrtDistance) * InvSqrtDistRange;
			return true;
		}
	}

	private Dictionary<Camera, Class619> dictionary_0 = new Dictionary<Camera, Class619>();

	private Dictionary<Camera, Class619> dictionary_1 = new Dictionary<Camera, Class619>();

	private static readonly HashSet<AnalyticSource> hashSet_0 = new HashSet<AnalyticSource>();

	private static readonly SortedSet<AnalyticSource> sortedSet_0 = new SortedSet<AnalyticSource>(new GClass973());

	private static readonly HashSet<DynamicAnalyticSource> hashSet_1 = new HashSet<DynamicAnalyticSource>();

	private static readonly SortedSet<DynamicAnalyticSource> sortedSet_1 = new SortedSet<DynamicAnalyticSource>(new GClass973());

	private static readonly HashSet<StencilShadow> hashSet_2 = new HashSet<StencilShadow>();

	private static readonly SortedSet<StencilShadow> sortedSet_2 = new SortedSet<StencilShadow>(new GClass974());

	public static bool UseSortedSources = true;

	[GAttribute11("Use method SetSH to set spherical harmonics")]
	public Shader DepthWriteShader;

	public Shader ClearStencilShader;

	public Shader WriteStencilShader;

	public Shader ScreenAmbientShader;

	[FormerlySerializedAs("AnalSourceShader")]
	public Shader AnalyticSourceShader;

	[Space]
	public float AmbientBlur;

	[Header("Reflections")]
	public float ReflectionIntensity;

	public float ReflectionIntensitySSR = 1f;

	public float RenderDelay = 0.05f;

	public int QubemapResolution = 128;

	public LayerMask CullingMask;

	[Range(0f, 1f)]
	public float ReflectionBottomShade;

	public AnimationCurve _reflectionWettingFunc = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private Material material_0;

	private Material material_1;

	private Material material_2;

	private Material material_3;

	private Material material_4;

	private MaterialPropertyBlock materialPropertyBlock_0;

	private static Mesh mesh_0;

	private static Mesh mesh_1;

	private static readonly Matrix4x4 matrix4x4_0 = Matrix4x4.identity;

	private static int[] int_0;

	private static int[] int_1;

	private static int int_2;

	private bool bool_0;

	private readonly List<UnityEngine.Object> list_0 = new List<UnityEngine.Object>();

	private RenderTexture renderTexture_0;

	private int int_3 = -1;

	private Camera camera_0;

	private float float_0;

	private static readonly int int_4 = Shader.PropertyToID("_Cull");

	private static readonly int int_5 = Shader.PropertyToID("_ZTest");

	private static readonly int int_6 = Shader.PropertyToID("_SrcBlend");

	private static readonly int int_7 = Shader.PropertyToID("_DstBlend");

	private static readonly int int_8 = Shader.PropertyToID("_CullingFade");

	private static readonly int int_9 = Shader.PropertyToID("_AmbientBlur");

	private static readonly int int_10 = Shader.PropertyToID("_EFT_Ambient");

	private static readonly int int_11 = Shader.PropertyToID("_ReflectionIntensity");

	private static readonly int int_12 = Shader.PropertyToID("_ReflectionBottomShade");

	private static readonly int int_13 = Shader.PropertyToID("_MyGlobalReflectionProbe");

	private static readonly string string_0 = "StencilShadows";

	private static readonly string string_1 = "AnalyticSources";

	private static readonly string string_2 = "DynamicAnalyticSources";

	private static List<AnalyticSource> list_1 = new List<AnalyticSource>();

	public bool IsInitialized => bool_0;

	public RenderTexture RenderTexture_0
	{
		get
		{
			if (renderTexture_0 != null)
			{
				return renderTexture_0;
			}
			renderTexture_0 = new RenderTexture(QubemapResolution, QubemapResolution, 0, RenderTextureFormat.ARGBHalf)
			{
				name = "AmbientLight _cubeRT",
				dimension = TextureDimension.Cube,
				useMipMap = true,
				hideFlags = HideFlags.DontSave,
				autoGenerateMips = true
			};
			Shader.SetGlobalTexture(int_13, renderTexture_0);
			return renderTexture_0;
		}
	}

	public bool Boolean_0
	{
		get
		{
			if (CameraClass.Exist)
			{
				return CameraClass.Instance.GetSSREnabled();
			}
			return false;
		}
	}

	public void SetReflectionIntensity(float value)
	{
		ReflectionIntensity = value;
		method_3();
	}

	public static void AddAnalyticSource(AnalyticSource source)
	{
		hashSet_0.Add(source);
		sortedSet_0.Add(source);
	}

	public static void RemoveAnalyticSource(AnalyticSource source)
	{
		hashSet_0.Remove(source);
		sortedSet_0.Remove(source);
	}

	public static void AddDynamicAnalyticSource(DynamicAnalyticSource source)
	{
		hashSet_1.Add(source);
		sortedSet_1.Add(source);
	}

	public static void RemoveDynamicAnalyticSource(DynamicAnalyticSource source)
	{
		hashSet_1.Remove(source);
		sortedSet_1.Remove(source);
	}

	public static void AddStencilShadow(StencilShadow shadow)
	{
		hashSet_2.Add(shadow);
		sortedSet_2.Add(shadow);
	}

	public static void RemoveStencilShadow(StencilShadow shadow)
	{
		hashSet_2.Remove(shadow);
		sortedSet_2.Remove(shadow);
	}

	public void Update()
	{
		foreach (DynamicAnalyticSource item in hashSet_1)
		{
			item.UpdateDynamicValues();
		}
	}

	public void LateUpdate()
	{
		if (!bool_0)
		{
			method_11();
		}
		method_1();
		method_2();
		method_3();
	}

	public override void ManualOnRenderObject(Camera currentCamera)
	{
		base.ManualOnRenderObject(currentCamera);
		if (!mesh_0)
		{
			mesh_0 = GetQuadMesh();
			mesh_1 = smethod_2();
		}
		if (!dictionary_0.TryGetValue(currentCamera, out var value))
		{
			value = new Class619
			{
				CbStencil = GClass1001.FindOrCreate(currentCamera, CameraEvent.BeforeLighting, "Stencil Shadow"),
				CbAmbient = GClass1001.FindOrCreate(currentCamera, CameraEvent.AfterLighting, "Custom Ambient"),
				Texture = smethod_0(currentCamera.pixelWidth, currentCamera.pixelHeight, currentCamera.name)
			};
			method_6(value, currentCamera);
			dictionary_0.Add(currentCamera, value);
			bool_0 = false;
		}
		if (value.Texture.width != currentCamera.pixelWidth || value.Texture.height != currentCamera.pixelHeight)
		{
			if (value.Texture != null)
			{
				value.Texture.Release();
				GClass972.SafeDestroy(value.Texture);
			}
			value.Texture = smethod_0(currentCamera.pixelWidth, currentCamera.pixelHeight, currentCamera.name);
			method_6(value, currentCamera);
		}
		if (value.HDR != currentCamera.allowHDR)
		{
			value.HDR = currentCamera.allowHDR;
			method_6(value, currentCamera);
		}
	}

	public void method_0()
	{
		foreach (var (camera2, class2) in dictionary_0)
		{
			if (camera2 != null)
			{
				camera2.RemoveCommandBuffer(CameraEvent.BeforeLighting, class2.CbStencil);
				camera2.RemoveCommandBuffer(CameraEvent.AfterLighting, class2.CbAmbient);
			}
			class2.Dispose();
		}
		dictionary_0.Clear();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		method_0();
		bool_0 = false;
		list_0.Clear();
		RuntimeOptimizeClear();
	}

	public void method_1()
	{
		bool flag = false;
		foreach (var (camera2, class2) in dictionary_0)
		{
			if (!camera2)
			{
				flag = true;
				class2.Dispose();
			}
			else
			{
				method_6(class2, camera2);
				method_4(class2, camera2);
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<Camera, Class619> item in dictionary_0)
		{
			dictionary_1.Add(item.Key, item.Value);
		}
		Dictionary<Camera, Class619> dictionary = dictionary_1;
		Dictionary<Camera, Class619> dictionary2 = dictionary_0;
		dictionary_0 = dictionary;
		dictionary_1 = dictionary2;
		dictionary_1.Clear();
	}

	public void method_2()
	{
		if (!(float_0 < Time.realtimeSinceStartup) && RenderDelay != 0f)
		{
			return;
		}
		if (camera_0 == null)
		{
			camera_0 = GetComponent<Camera>();
			if (camera_0 == null)
			{
				camera_0 = base.gameObject.AddComponent<Camera>();
			}
			camera_0.clearFlags = CameraClearFlags.Color;
			camera_0.backgroundColor = new Color(0f, 0f, 0f, 1f);
			camera_0.renderingPath = RenderingPath.VertexLit;
			camera_0.nearClipPlane = 0.1f;
			camera_0.farClipPlane = 0.2f;
			camera_0.useOcclusionCulling = false;
			camera_0.allowHDR = true;
			camera_0.enabled = false;
			camera_0.targetTexture = RenderTexture_0;
			camera_0.cullingMask = CullingMask;
		}
		if ((bool)camera_0 && (bool)RenderTexture_0)
		{
			camera_0.targetTexture = RenderTexture_0;
		}
		if (int_3 < 0)
		{
			camera_0.RenderToCubemap(RenderTexture_0, 63);
		}
		else
		{
			if (int_3 == 6)
			{
				int_3 = 0;
			}
			camera_0.RenderToCubemap(RenderTexture_0, 1 << int_3);
		}
		Shader.SetGlobalTexture(int_13, RenderTexture_0);
		float_0 = Time.realtimeSinceStartup + RenderDelay;
		int_3++;
	}

	public void method_3()
	{
		float value = (Boolean_0 ? 1f : (_reflectionWettingFunc.Evaluate(RainController.Wetting) * ReflectionIntensity));
		materialPropertyBlock_0.SetFloat(int_11, value);
	}

	public void method_4(Class619 camSettings, Camera currentCamera)
	{
		CommandBuffer cbStencil = camSettings.CbStencil;
		cbStencil.Clear();
		if (method_12(currentCamera.cullingMask))
		{
			method_5(cbStencil, camSettings.Texture, "_StencilShadow");
			Vector3 position = currentCamera.transform.position;
			Plane[] frustrumPlanes = GClass823.CalculateFrustumPlanesNonAlloc(currentCamera);
			method_7(cbStencil, position, frustrumPlanes);
			method_9(cbStencil, position, frustrumPlanes);
		}
	}

	public void method_5(CommandBuffer commandBuffer, RenderTexture renderTexture, string shaderTextureName)
	{
		commandBuffer.Blit(BuiltinRenderTextureType.GBuffer0, renderTexture, material_0);
		commandBuffer.SetRenderTarget(renderTexture);
		commandBuffer.SetGlobalTexture(shaderTextureName, renderTexture);
		commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
		commandBuffer.SetGlobalTexture(shaderTextureName, renderTexture);
	}

	public void method_6(Class619 camSettings, Camera currentCamera)
	{
		if (material_3 == null)
		{
			Debug.Log("_screenAmbientMaterial");
		}
		CommandBuffer cbAmbient = camSettings.CbAmbient;
		cbAmbient.Clear();
		if (GPUInstancerManager.activeManagerList == null || GPUInstancerManager.activeManagerList.Count == 0)
		{
			Texture2D blackTexture = Texture2D.blackTexture;
			cbAmbient.SetGlobalTexture("_CameraMotionVectorsDepth", blackTexture);
		}
		if (method_12(currentCamera.cullingMask))
		{
			if (currentCamera.allowHDR)
			{
				cbAmbient.SetGlobalFloat(int_6, 1f);
				cbAmbient.SetGlobalFloat(int_7, 1f);
			}
			else
			{
				cbAmbient.SetGlobalFloat(int_6, 2f);
				cbAmbient.SetGlobalFloat(int_7, 0f);
			}
			cbAmbient.SetRenderTarget(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive);
			cbAmbient.DrawMesh(mesh_0, Matrix4x4.identity, material_3, 0, 0, materialPropertyBlock_0);
		}
	}

	public void method_7(CommandBuffer cmdBuf, Vector3 camPos, Plane[] frustrumPlanes)
	{
		cmdBuf.BeginSample(string_0);
		IEnumerable<StencilShadow> enumerable2;
		if (!UseSortedSources)
		{
			IEnumerable<StencilShadow> enumerable = hashSet_2;
			enumerable2 = enumerable;
		}
		else
		{
			IEnumerable<StencilShadow> enumerable = sortedSet_2;
			enumerable2 = enumerable;
		}
		foreach (StencilShadow item in enumerable2)
		{
			if ((bool)item && (bool)item.gameObject && item.isActiveAndEnabled)
			{
				if (!item.Renderer)
				{
					item.Awake();
				}
				Bounds bounds = item.Bounds;
				if (GeometryUtility.TestPlanesAABB(frustrumPlanes, bounds))
				{
					method_8(cmdBuf, item, camPos);
				}
			}
			else
			{
				list_0.Add(item);
			}
		}
		if (list_0.Count > 0)
		{
			for (int i = 0; i < list_0.Count; i++)
			{
				StencilShadow stencilShadow = list_0[i] as StencilShadow;
				if (stencilShadow != null)
				{
					hashSet_2.Remove(stencilShadow);
					sortedSet_2.Remove(stencilShadow);
				}
			}
			list_0.Clear();
		}
		cmdBuf.EndSample(string_0);
	}

	public bool method_8(CommandBuffer cmdBuf, StencilShadow ss, Vector3 camPos, bool disableColorPass = false)
	{
		Bounds bounds = ss.Bounds;
		if (!ss.Culling.PassCulling((bounds.center - camPos).sqrMagnitude, out var fadeValue))
		{
			return false;
		}
		cmdBuf.BeginSample(string_0 + "_unit");
		cmdBuf.DrawMesh(mesh_0, matrix4x4_0, material_1);
		cmdBuf.DrawRenderer(ss.Renderer, material_2, 0, 0);
		cmdBuf.DrawRenderer(ss.Renderer, material_2, 0, 1);
		if (!disableColorPass)
		{
			cmdBuf.SetGlobalColor("_StencilAmbientColor", ss.Ambient * fadeValue);
			cmdBuf.SetGlobalFloat("_StencilFogAttenuation", ss.FogAttenuation);
			cmdBuf.DrawMesh(mesh_0, matrix4x4_0, material_2, 0, 2);
		}
		cmdBuf.DrawRenderer(ss.Renderer, material_2, 0, 3);
		cmdBuf.DrawRenderer(ss.Renderer, material_2, 0, 4);
		cmdBuf.EndSample(string_0 + "_unit");
		return true;
	}

	public void method_9(CommandBuffer cmdBuf, Vector3 camPos, Plane[] frustrumPlanes)
	{
		RuntimeDrawStaticSourcesOptimized(cmdBuf, camPos, frustrumPlanes);
	}

	public void method_10(AnalyticSource source, CommandBuffer cmdBuf, Vector3 camPos, Plane[] frustrumPlanes)
	{
		if ((!AnalyticSource.EnableAutoculling || source.IsAutocullVisible) && GeometryUtility.TestPlanesAABB(frustrumPlanes, source.Bounds) && source.Culling.PassCulling((source.Bounds.center - camPos).sqrMagnitude, out var fadeValue))
		{
			cmdBuf.BeginSample(string_1 + "_unit");
			if (source.Bounds.Contains(camPos))
			{
				cmdBuf.SetGlobalFloat(int_4, 1f);
				cmdBuf.SetGlobalFloat(int_5, 5f);
			}
			else
			{
				cmdBuf.SetGlobalFloat(int_4, 2f);
				cmdBuf.SetGlobalFloat(int_5, 2f);
			}
			if (source.LinkedStencilShadow != null)
			{
				method_8(cmdBuf, source.LinkedStencilShadow, camPos, disableColorPass: true);
			}
			int shaderPass = source.ShaderPath + ((source.LinkedStencilShadow != null) ? 3 : 0);
			source.MaterialPropertyBlock.SetFloat(int_8, fadeValue);
			cmdBuf.DrawMesh(mesh_1, source.LocalToWorldMatrix, material_4, 0, shaderPass, source.MaterialPropertyBlock);
			cmdBuf.EndSample(string_1 + "_unit");
		}
	}

	public static RenderTexture smethod_0(int width, int height, string name = "")
	{
		return new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf)
		{
			name = "AmbientLight CreateIndoorTexture " + name,
			filterMode = FilterMode.Point
		};
	}

	public void method_11()
	{
		float_0 = 0f;
		material_0 = new Material(DepthWriteShader);
		material_1 = new Material(ClearStencilShader);
		material_2 = new Material(WriteStencilShader);
		material_3 = new Material(ScreenAmbientShader);
		material_4 = new Material(AnalyticSourceShader);
		materialPropertyBlock_0 = new MaterialPropertyBlock();
		int_0 = new int[3]
		{
			Shader.PropertyToID("_SHAr"),
			Shader.PropertyToID("_SHAg"),
			Shader.PropertyToID("_SHAb")
		};
		int_1 = new int[3]
		{
			Shader.PropertyToID("_SHBr"),
			Shader.PropertyToID("_SHBg"),
			Shader.PropertyToID("_SHBb")
		};
		int_2 = Shader.PropertyToID("_SHC");
		materialPropertyBlock_0.SetFloat(int_9, 1f / AmbientBlur);
		method_3();
		Shader.SetGlobalFloat(int_12, 2f - ReflectionBottomShade);
		foreach (KeyValuePair<Camera, Class619> item in dictionary_0)
		{
			if ((bool)item.Key)
			{
				method_6(item.Value, item.Key);
			}
		}
		material_1.SetFloat("_StencilValue", 64f);
		bool_0 = true;
	}

	public void OnValidate()
	{
		method_11();
	}

	public override void Start()
	{
		base.Start();
		method_11();
	}

	public void OnEnable()
	{
		method_11();
	}

	public void OnDisable()
	{
		method_0();
		bool_0 = false;
		UnityEngine.Object.DestroyImmediate(renderTexture_0);
	}

	public void SetSH(SphericalHarmonicsL2 sh)
	{
		if (!bool_0)
		{
			method_11();
		}
		Shader.SetGlobalVector(int_2, new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1f));
		for (int i = 0; i < 3; i++)
		{
			Shader.SetGlobalVector(int_0[i], new Vector4(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]));
			Shader.SetGlobalVector(int_1[i], new Vector4(sh[i, 4], sh[i, 5], sh[i, 6] * 3f, sh[i, 7]));
		}
		Color colorTop = GetColorTop(sh);
		Shader.SetGlobalVector(int_10, new Vector3(colorTop.r, colorTop.g, colorTop.b));
	}

	public Color GetColorTop(SphericalHarmonicsL2 sh)
	{
		Color result = default(Color);
		for (int i = 0; i < 3; i++)
		{
			result[i] = sh[i, 1] + (sh[i, 0] - sh[i, 6]);
		}
		result += new Color(0f - sh[0, 8], 0f - sh[1, 8], 0f - sh[2, 8]);
		if (QualitySettings.activeColorSpace != ColorSpace.Gamma)
		{
			result = new Color(Mathf.LinearToGammaSpace(result.r), Mathf.LinearToGammaSpace(result.g), Mathf.LinearToGammaSpace(result.b));
		}
		return result;
	}

	public Color GetColor(SphericalHarmonicsL2 sh, Vector3 normal)
	{
		Vector4 b = new Vector4(normal.x, normal.y, normal.z, 1f);
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			zero[i] = Vector4.Dot(new Vector4(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]), b);
		}
		Vector4 b2 = new Vector4(b.x * b.y, b.y * b.z, b.z * b.z, b.z * b.x);
		Vector3 zero2 = Vector3.zero;
		for (int j = 0; j < 3; j++)
		{
			zero2[j] = Vector4.Dot(new Vector4(sh[j, 4], sh[j, 5], sh[j, 6] * 3f, sh[j, 7]), b2);
		}
		float num = b.x * b.x - b.y * b.y;
		Vector3 vector = new Vector3(sh[0, 8], sh[1, 8], sh[2, 8]) * num;
		Vector3 vector2 = zero + zero2 + vector;
		return new Color(vector2.x, vector2.y, vector2.z);
	}

	public static Mesh GetQuadMesh()
	{
		float z = 0.1f;
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, z),
			new Vector3(1f, -1f, z),
			new Vector3(-1f, 1f, z),
			new Vector3(1f, 1f, z)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		int[] triangles = new int[6] { 0, 1, 3, 3, 2, 0 };
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.name = "AmbientLight GetQuadMesh";
		mesh.RecalculateBounds();
		mesh.name = "QuadMeshForAmbientLight";
		return mesh;
	}

	public static Mesh smethod_1()
	{
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, -0.1f),
			new Vector3(-1f, 1f, -0.1f),
			new Vector3(1f, 1f, -0.1f),
			new Vector3(1f, -1f, -0.1f)
		};
		int[] triangles = new int[6] { 0, 2, 1, 0, 3, 2 };
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(3f, 0f),
			new Vector2(2f, 0f),
			new Vector2(1f, 0f)
		};
		Mesh obj = new Mesh
		{
			name = "AmbientLight CreateQuad",
			vertices = vertices,
			uv = uv,
			triangles = triangles
		};
		Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2.1474836E+09f);
		obj.bounds = bounds;
		return obj;
	}

	public static Mesh smethod_2()
	{
		float num = 1f;
		Vector3[] vertices = new Vector3[8]
		{
			new Vector3(num, num, -1f),
			new Vector3(-1f, num, -1f),
			new Vector3(-1f, num, num),
			new Vector3(num, num, num),
			new Vector3(num, -1f, -1f),
			new Vector3(-1f, -1f, -1f),
			new Vector3(-1f, -1f, num),
			new Vector3(num, -1f, num)
		};
		int[] array = new int[36]
		{
			0, 1, 2, 0, 2, 3, 0, 4, 5, 0,
			5, 1, 1, 5, 6, 1, 6, 2, 2, 6,
			7, 2, 7, 3, 3, 7, 4, 3, 4, 0,
			4, 7, 6, 4, 6, 5
		};
		int[] array2 = new int[0];
		for (int i = 0; i < 0; i++)
		{
			for (int j = 0; j < array.Length; j++)
			{
				int num2 = i * array.Length + j;
				array2[num2] = array[j];
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = array;
		mesh.name = "AmbientLight GetCubeMesh";
		mesh.RecalculateBounds();
		mesh.name = "CubeMeshForAmbientLight";
		return mesh;
	}

	public bool method_12(int cullingMask)
	{
		int layer = base.gameObject.layer;
		if (cullingMask == (cullingMask | (1 << layer)))
		{
			return true;
		}
		return false;
	}

	public static void RuntimeForceAnalyticSourceVisibility(bool flag)
	{
		foreach (AnalyticSource item in list_1)
		{
			item.IsAutocullVisible = flag;
		}
	}

	public static void RuntimeOptimizePrepare()
	{
		list_1 = new List<AnalyticSource>();
		if (UseSortedSources)
		{
			foreach (AnalyticSource item in sortedSet_0)
			{
				list_1.Add(item);
			}
			return;
		}
		foreach (AnalyticSource item2 in hashSet_0)
		{
			list_1.Add(item2);
		}
	}

	public static void RuntimeOptimizeClear()
	{
		if (list_1 != null)
		{
			list_1.Clear();
			list_1 = null;
		}
	}

	public void RuntimeDrawStaticSourcesOptimized(CommandBuffer cmdBuf, Vector3 camPos, Plane[] frustrumPlanes)
	{
		foreach (AnalyticSource item in list_1)
		{
			method_13(item, cmdBuf, camPos, frustrumPlanes);
		}
	}

	public void method_13(AnalyticSource source, CommandBuffer cmdBuf, Vector3 camPos, Plane[] frustrumPlanes)
	{
		if ((!AnalyticSource.EnableAutoculling || source.IsAutocullVisible) && GeometryUtility.TestPlanesAABB(frustrumPlanes, source.Bounds) && source.Culling.PassCulling((source.Bounds.center - camPos).sqrMagnitude, out var fadeValue))
		{
			if (source.Bounds.Contains(camPos))
			{
				cmdBuf.SetGlobalFloat(int_4, 1f);
				cmdBuf.SetGlobalFloat(int_5, 5f);
			}
			else
			{
				cmdBuf.SetGlobalFloat(int_4, 2f);
				cmdBuf.SetGlobalFloat(int_5, 2f);
			}
			if (source.LinkedStencilShadow != null)
			{
				method_8(cmdBuf, source.LinkedStencilShadow, camPos, disableColorPass: true);
			}
			int shaderPass = source.ShaderPath + ((source.LinkedStencilShadow != null) ? 3 : 0);
			source.MaterialPropertyBlock.SetFloat(int_8, fadeValue);
			cmdBuf.DrawMesh(mesh_1, source.LocalToWorldMatrix, material_4, 0, shaderPass, source.MaterialPropertyBlock);
		}
	}
}
