using System;
using Comfort.Common;
using EFT.UI.Screens;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SnowWetRenderer : MonoBehaviour
{
	public Shader WetShader;

	[Range(0f, 1f)]
	public float Wetting;

	public Vector3 SpringSnowFactor = SeasonsSettingsClass.SpringSnowFactorDefault;

	public Color DiffuseColor;

	public Color SpecColor;

	public float WetFactor = 1000f;

	public float WetBias = 0.0016f;

	public float Opaqueness = 1f;

	public Material _wetMaterial;

	public Material _snowCopyMaterial;

	public Mesh _wetMesh;

	public Material _snowSparkling;

	public int SparklingsCount = 100;

	private bool bool_0;

	private bool bool_1;

	private const string string_0 = "WINTER_SNOW";

	private static readonly int int_0 = Shader.PropertyToID("_SurfaceSnowBias");

	private static readonly int int_1 = Shader.PropertyToID("_SurfaceSnowIntensity");

	private static readonly int int_2 = Shader.PropertyToID("_SpringSnowFactor");

	private static readonly int int_3 = Shader.PropertyToID("_SurfaceSnowDiffuseColor");

	private static readonly int int_4 = Shader.PropertyToID("_SurfaceSnowSpecColor");

	private static readonly int int_5 = Shader.PropertyToID("_NoiseTiling");

	private static readonly int int_6 = Shader.PropertyToID("_SpecularMap");

	private static readonly int int_7 = Shader.PropertyToID("_BumpMap");

	private static readonly int int_8 = Shader.PropertyToID("_Glossness");

	public static readonly int _wetOffMask = Shader.PropertyToID("wetOffMask");

	private const string string_1 = "SNOW_GLITTERS";

	private static readonly int int_9 = Shader.PropertyToID("_SnowGlitterMask");

	private static readonly int int_10 = Shader.PropertyToID("_SnowNormalMap");

	private static readonly int int_11 = Shader.PropertyToID("_Opaqueness");

	private static readonly int int_12 = Shader.PropertyToID("_SurfaceWetFactor");

	private static readonly int int_13 = Shader.PropertyToID("_WetAngleMin");

	private static readonly int int_14 = Shader.PropertyToID("_WetAngleMax");

	private static readonly int int_15 = Shader.PropertyToID("_SnowTransition");

	private static readonly int int_16 = Shader.PropertyToID("_ScreenSpaceNormalAndAlphaMask");

	private static readonly int int_17 = Shader.PropertyToID("_MapBoundsMin");

	private static readonly int int_18 = Shader.PropertyToID("_MapBoundsMax");

	private static readonly int int_19 = Shader.PropertyToID("_MapDownscale");

	private static readonly int int_20 = Shader.PropertyToID("_TextureTiling");

	private static readonly int int_21 = Shader.PropertyToID("_AlbedoTex");

	private static readonly int int_22 = Shader.PropertyToID("_NoiseMap");

	private static readonly int int_23 = Shader.PropertyToID("_ScreenNoiseTiling");

	private static readonly int int_24 = Shader.PropertyToID("_SnowSpecularFactor");

	private static readonly int int_25 = Shader.PropertyToID("_GlitterThreshold");

	private static readonly int int_26 = Shader.PropertyToID("_GlitterIntensity");

	private static readonly int int_27 = Shader.PropertyToID("_DisableSnowMask");

	private static readonly int int_28 = Shader.PropertyToID("_StormEnabled");

	private static readonly int int_29 = Shader.PropertyToID("_DampingDistance");

	private static readonly int int_30 = Shader.PropertyToID("_swampSnowIgnoreMask");

	private readonly GClass1001 gclass1001_0 = new GClass1001("Swamp", CameraEvent.BeforeGBuffer);

	private readonly GClass1001 gclass1001_1 = new GClass1001("Snow", CameraEvent.BeforeReflections);

	public bool StormEnabled;

	public float WetAngleMin;

	public float WetAngleMax;

	public float SnowTransition = 0.05f;

	[SerializeField]
	[HideInInspector]
	private Vector2 _textureTilingFactor = new Vector2(1f, 1f);

	[SerializeField]
	[HideInInspector]
	private Vector2 _noiseTilingFactor = new Vector2(1f, 1f);

	private Vector2 vector2_0;

	private Vector2 vector2_1;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private float float_0 = 1f;

	private LevelSettings levelSettings_0;

	public float _DampingDistance = 100f;

	private const float float_1 = 4f;

	public Texture2D AlbedoTex;

	public Texture2D SpecularMap;

	public Texture2D BumpMap;

	public Texture2D NoiseMap;

	public Vector2 ScreenNoiseTiling = new Vector2(1f, 1f);

	public float Glossness = 0.001f;

	public Vector4 SnowSpecularFactor = new Vector4(0.7f, 1f, 1.5f, 0f);

	[Range(0f, 1f)]
	public float GlitterThreshold = 1f;

	[Range(0f, 10f)]
	public float GlitterIntensity = 1f;

	public bool WinterShow
	{
		get
		{
			return bool_0;
		}
		set
		{
			if (bool_0 != value)
			{
				bool_0 = value;
				bool_1 = false;
			}
		}
	}

	public void OnValidate()
	{
		method_0();
		if (base.isActiveAndEnabled)
		{
			method_2();
		}
	}

	public void Start()
	{
		method_0();
		method_1();
		method_2();
	}

	public bool method_0()
	{
		LevelSettings instance = Singleton<LevelSettings>.Instance;
		if (instance == null)
		{
			levelSettings_0 = null;
			vector3_0 = default(Vector3);
			vector3_0 = default(Vector3);
			float_0 = 1f;
			vector2_0 = default(Vector2);
			vector2_1 = default(Vector2);
			return false;
		}
		if ((object)levelSettings_0 == instance)
		{
			return true;
		}
		levelSettings_0 = instance;
		Bounds rainBounds = levelSettings_0.RainBounds;
		vector3_0 = rainBounds.center - rainBounds.size;
		vector3_1 = rainBounds.center + rainBounds.size;
		float_0 = (vector3_1.x - vector3_0.x + vector3_1.z - vector3_0.z) / 8f;
		Vector3 vector = rainBounds.size * 2f;
		vector2_0 = new Vector2(vector.x / _textureTilingFactor.x, vector.z / _textureTilingFactor.y);
		vector2_1 = new Vector2(vector.x / _noiseTilingFactor.x, vector.z / _noiseTilingFactor.y);
		return true;
	}

	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_3));
		method_0();
		CurrentScreenSingletonClass instance = CurrentScreenSingletonClass.Instance;
		if (instance != null)
		{
			instance.OnScreenChanged += smethod_0;
		}
		smethod_1();
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_3));
		levelSettings_0 = null;
		CurrentScreenSingletonClass instance = CurrentScreenSingletonClass.Instance;
		if (instance != null)
		{
			instance.OnScreenChanged -= smethod_0;
		}
		smethod_0(EEftScreenType.None);
	}

	public static void smethod_0(EEftScreenType screenType)
	{
		if (screenType == EEftScreenType.BattleUI)
		{
			Shader.EnableKeyword("SNOW_GLITTERS");
		}
		else
		{
			Shader.DisableKeyword("SNOW_GLITTERS");
		}
	}

	public static void smethod_1()
	{
		CurrentScreenSingletonClass instance = CurrentScreenSingletonClass.Instance;
		smethod_0((instance != null && instance.CurrentScreenController != null) ? instance.CurrentScreenController.ScreenType : EEftScreenType.None);
	}

	public void method_1()
	{
		_wetMesh = smethod_2();
		gclass1001_1.DestroyBuffers();
		gclass1001_0.DestroyBuffers();
	}

	public void method_2()
	{
		if (!bool_1)
		{
			bool_1 = true;
			if (bool_0)
			{
				Shader.EnableKeyword("WINTER_SNOW");
			}
			else
			{
				Shader.DisableKeyword("WINTER_SNOW");
			}
		}
		Shader.SetGlobalFloat(int_0, WetBias);
		Shader.SetGlobalFloat(int_1, Wetting);
		Shader.SetGlobalVector(int_2, SpringSnowFactor);
		Shader.SetGlobalColor(int_3, DiffuseColor);
		Shader.SetGlobalColor(int_4, SpecColor);
		Shader.SetGlobalFloat(int_11, Opaqueness);
		Shader.SetGlobalFloat(int_12, WetFactor);
		Shader.SetGlobalFloat(int_13, WetAngleMin);
		Shader.SetGlobalFloat(int_14, WetAngleMax);
		Shader.SetGlobalFloat(int_15, SnowTransition);
		Shader.SetGlobalTexture(int_21, AlbedoTex);
		Shader.SetGlobalTexture(int_22, NoiseMap);
		Shader.SetGlobalVector(int_17, vector3_0);
		Shader.SetGlobalVector(int_18, vector3_1);
		Shader.SetGlobalFloat(int_19, float_0);
		Shader.SetGlobalVector(int_20, vector2_0);
		Shader.SetGlobalVector(int_23, ScreenNoiseTiling);
		Shader.SetGlobalVector(int_24, SnowSpecularFactor);
		Shader.SetGlobalFloat(int_25, GlitterThreshold);
		Shader.SetGlobalFloat(int_26, GlitterIntensity);
		Shader.SetGlobalFloat(int_28, StormEnabled ? 1f : 0f);
		Shader.SetGlobalFloat(int_29, _DampingDistance);
	}

	public void Update()
	{
		Class679.Class679_0?.Update();
		method_0();
		method_2();
	}

	public void method_3(Camera currentCamera)
	{
		CameraClass instance = CameraClass.Instance;
		if (currentCamera == instance.Camera || currentCamera == instance.OpticCameraManager.Camera)
		{
			GInterface29 controller = Class443.Controller;
			if (controller != null && controller.Status != ESeasonStatus.Summer)
			{
				Struct160 data = Class679.Class679_0.method_0(currentCamera, this);
				method_4(in data);
				method_5(in data);
			}
		}
	}

	public void method_4(in Struct160 data)
	{
		if (gclass1001_0.UpdateOnPreCullRender(out var buffer))
		{
			buffer.Clear();
			buffer.SetGlobalTexture(int_30, data.RenderTexture_1);
			buffer.SetRandomWriteTarget(4, data.RenderTexture_1);
		}
	}

	public void method_5(in Struct160 data)
	{
		if (gclass1001_1.UpdateOnPreCullRender(out var buffer))
		{
			buffer.Clear();
			smethod_1();
			buffer.GetTemporaryRT(int_16, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
			buffer.SetGlobalTexture(int_10, BuiltinRenderTextureType.GBuffer2);
			buffer.Blit(BuiltinRenderTextureType.GBuffer0, int_16, _snowCopyMaterial);
			buffer.ClearRandomWriteTargets();
			buffer.SetGlobalTexture(int_6, SpecularMap);
			buffer.SetGlobalTexture(int_7, BumpMap);
			buffer.SetGlobalVector(int_5, vector2_1);
			buffer.SetGlobalFloat(int_8, Glossness);
			buffer.SetGlobalTexture(int_27, data.RenderTexture_1);
			buffer.SetGlobalTexture(int_16, int_16);
			buffer.GetTemporaryRT(_wetOffMask, -1, -1, 0, FilterMode.Point, RenderTextureFormat.R8);
			buffer.SetRenderTarget(_wetOffMask);
			buffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
			buffer.SetRenderTarget(data.RenderTargetIdentifier_0, BuiltinRenderTextureType.CameraTarget);
			buffer.DrawMesh(_wetMesh, Matrix4x4.identity, method_6());
			buffer.ReleaseTemporaryRT(int_16);
			buffer.SetRenderTarget(data.RenderTexture_1);
			buffer.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
			if (!WetRenderer.WetOffMaskUsed)
			{
				buffer.ReleaseTemporaryRT(_wetOffMask);
			}
		}
	}

	public void SetDebugRain(bool debug)
	{
		if (base.isActiveAndEnabled && !debug)
		{
			method_2();
		}
	}

	public Material method_6()
	{
		if (_wetMaterial != null)
		{
			return _wetMaterial;
		}
		_wetMaterial = new Material(WetShader);
		return _wetMaterial;
	}

	public static Mesh smethod_2()
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
			name = "WetRenderer CreateQuad",
			vertices = vertices,
			uv = uv,
			triangles = triangles
		};
		Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2.1474836E+09f);
		obj.bounds = bounds;
		return obj;
	}

	public void OnDestroy()
	{
		Class679.Class679_0.method_2(this);
		UnityEngine.Object.DestroyImmediate(_wetMesh);
	}

	public static Mesh smethod_3(int count)
	{
		Vector3[] array = new Vector3[count * 4];
		Vector2[] array2 = new Vector2[count * 4];
		Vector2[] array3 = new Vector2[count * 4];
		Vector2[] array4 = new Vector2[count * 4];
		int[] array5 = new int[count * 6];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			Vector2 vector = new Vector2
			{
				x = UnityEngine.Random.value * 2f - 1f,
				y = UnityEngine.Random.value * 2f - 1f
			};
			Vector2 vector2 = new Vector2
			{
				x = UnityEngine.Random.value,
				y = UnityEngine.Random.value
			};
			array[num] = new Vector3(-1f, -1f, 0f);
			array2[num] = vector;
			array3[num] = new Vector2(0f, 0f);
			array4[num] = vector2;
			num++;
			array[num] = new Vector3(-1f, 1f, 0f);
			array2[num] = vector;
			array3[num] = new Vector2(0f, 1f);
			array4[num] = vector2;
			num++;
			array[num] = new Vector3(1f, 1f, 0f);
			array2[num] = vector;
			array3[num] = new Vector2(1f, 1f);
			array4[num] = vector2;
			num++;
			array[num] = new Vector3(1f, -1f, 0f);
			array2[num] = vector;
			array3[num] = new Vector2(1f, 0f);
			array4[num] = vector2;
			num++;
			int num3 = (array5[num2] = i * 4);
			array5[num2 + 1] = num3 + 2;
			array5[num2 + 2] = num3 + 1;
			array5[num2 + 3] = num3;
			array5[num2 + 4] = num3 + 3;
			array5[num2 + 5] = num3 + 2;
			num2 += 6;
		}
		Mesh mesh = new Mesh();
		mesh.name = "Sparklings mesh";
		mesh.vertices = array;
		mesh.uv = array3;
		mesh.uv2 = array2;
		mesh.uv3 = array4;
		mesh.SetIndices(array5, MeshTopology.Triangles, 0);
		return mesh;
	}
}
