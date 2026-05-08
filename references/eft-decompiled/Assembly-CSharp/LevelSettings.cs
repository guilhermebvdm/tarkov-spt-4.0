using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Settings.Graphics;
using EFT.SpeedTree;
using EFT.Weather;
using Prism.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[AddComponentMenu("Scripts/Game/Components/LevelSettings")]
public class LevelSettings : MonoBehaviour, CameraClass.GInterface465
{
	public Camera CameraPrefab;

	public PrismPreset PrismPresetPrefab;

	public PostProcessProfile PostProcessProfilePrefab;

	public Material skybox;

	public Bounds RainBounds = new Bounds(Vector3.zero, new Vector3(1000f, 220f, 1000f));

	[Header("Ambient")]
	public AmbientMode AmbientMode = AmbientMode.Flat;

	public Color SkyColor = Color.black;

	public Color EquatorColor = new Color(0.6039216f, 54f / 85f, 0.6509804f);

	public Color GroundColor = new Color(0.6039216f, 54f / 85f, 0.6509804f);

	public float AmbientIntensity;

	[Header("NightVisionAmbient")]
	public Color NightVisionSkyColor = Color.black;

	public Color NightVisionEquatorColor = Color.black;

	public Color NightVisionGroundColor = Color.black;

	public float NightVisionAmbientIntensity;

	[Space]
	[SerializeField]
	private Color _minSmokeAmbientColor = Color.clear;

	public float SSRFactor = 1f;

	[Header("Fog")]
	public bool fog;

	public Color fogColor;

	public float fogDensity;

	public float fogEndDistance;

	public FogMode fogMode;

	public float fogStartDistance;

	[Header("Sun")]
	public Color SunColor = new Color32(byte.MaxValue, 209, 145, byte.MaxValue);

	public float SunScratchesIntensity = 6f;

	public float SunShaftsIntensity = 0.2f;

	public float SunShaftsDensity = 0.15f;

	public bool SunLensFlares = true;

	public bool SunBackGroung = true;

	public float LyingWater;

	[Range(0f, 1f)]
	public float DirectionLightShadowForUnshadowedShaders;

	[Header("Scattering")]
	[Range(0f, 1f)]
	public float HeightFalloff = 0.029f;

	public float ZeroLevel = -78.64f;

	private static readonly int int_0 = Shader.PropertyToID("_DirectionLightShadow");

	private static readonly int int_1 = Shader.PropertyToID("_WaterLevel");

	private static readonly int int_2 = Shader.PropertyToID("_TopHorizontSkyColor");

	private static readonly int int_3 = Shader.PropertyToID("_MinAmbientColor");

	private static readonly int int_4 = Shader.PropertyToID("_SSRFactor");

	[Header("Compass")]
	[SerializeField]
	[Range(0f, 360f)]
	private float _northDirection;

	[SerializeField]
	public Vector4 MaxMapTextureMemory = new Vector4(1024f, 1024f, 1024f, 1024f);

	[CompilerGenerated]
	private AmbientType ambientType_0;

	private Action action_0;

	private IEnumerable<TreeWind> ienumerable_0 = new List<TreeWind>();

	private Color color_0;

	private float float_0;

	private bool bool_0;

	private Color color_1;

	private float float_1;

	private float float_2;

	private FogMode fogMode_0;

	private float float_3;

	private float float_4;

	private Material material_0;

	Camera CameraClass.GInterface465.CameraPrefab => CameraPrefab;

	PrismPreset CameraClass.GInterface465.PrismPresetPrefab => PrismPresetPrefab;

	PostProcessProfile CameraClass.GInterface465.PostProcessProfilePrefab => PostProcessProfilePrefab;

	public AmbientType AmbientType
	{
		[CompilerGenerated]
		get
		{
			return ambientType_0;
		}
		[CompilerGenerated]
		set
		{
			ambientType_0 = value;
		}
	}

	public bool isTaaHighEnabled
	{
		get
		{
			if (!Singleton<SharedGameSettingsClass>.Instantiated)
			{
				return false;
			}
			GraphicsSettingsClass settings = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings;
			if (!settings.IsTaaHigh())
			{
				return settings.IsDLSSEnabled();
			}
			return true;
		}
	}

	public Vector3 NorthVector => Quaternion.Euler(0f, NorthDirection, 0f) * Vector3.forward;

	public float NorthDirection => _northDirection;

	public void Awake()
	{
		Singleton<LevelSettings>.Create(this);
		ApplySettings();
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_0));
	}

	public void OnPostLoadingScene()
	{
		ienumerable_0 = LocationScene.GetAllObjects<TreeWind>();
		ApplySettings();
		ApplyTreeWindSettings();
		action_0 = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.AntiAliasing.Bind(delegate
		{
			method_1();
		});
		AirdropManagerClass airdropManager = new AirdropManagerClass();
		Singleton<GameWorld>.Instance.SynchronizableObjectLogicProcessor.method_0(airdropManager);
		Singleton<GameWorld>.Instance.SynchronizableObjectLogicProcessor.AirdropManager.Init(enableAirdrops: false);
	}

	public void OnDestroy()
	{
		method_3();
		Singleton<LevelSettings>.Release(this);
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_0));
	}

	public void ApplySettings()
	{
		if (Singleton<SharedGameSettingsClass>.Instantiated)
		{
			Singleton<SharedGameSettingsClass>.Instance.Graphics.Controller.RefreshStreamingMemoryBudget(MaxMapTextureMemory);
		}
		RenderSettings.ambientMode = AmbientMode;
		RenderSettings.ambientEquatorColor = EquatorColor;
		RenderSettings.ambientGroundColor = GroundColor;
		RenderSettings.ambientSkyColor = SkyColor;
		RenderSettings.ambientLight = SkyColor;
		RenderSettings.ambientIntensity = AmbientIntensity;
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogMode = fogMode;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.skybox = skybox;
		Shader.SetGlobalFloat(int_0, DirectionLightShadowForUnshadowedShaders);
		Shader.SetGlobalFloat(int_1, LyingWater);
		Shader.SetGlobalColor(int_3, _minSmokeAmbientColor);
		Shader.SetGlobalColor(int_2, Color.clear);
		Shader.SetGlobalFloat(int_4, SSRFactor);
	}

	public void GetSceneParameters()
	{
		AmbientMode = RenderSettings.ambientMode;
		EquatorColor = RenderSettings.ambientEquatorColor;
		GroundColor = RenderSettings.ambientGroundColor;
		SkyColor = RenderSettings.ambientSkyColor;
		if (AmbientMode == AmbientMode.Flat)
		{
			SkyColor = RenderSettings.ambientLight;
		}
		AmbientIntensity = RenderSettings.ambientIntensity;
		fog = RenderSettings.fog;
		fogColor = RenderSettings.fogColor;
		fogDensity = RenderSettings.fogDensity;
		fogEndDistance = RenderSettings.fogEndDistance;
		fogMode = RenderSettings.fogMode;
		fogStartDistance = RenderSettings.fogStartDistance;
		skybox = RenderSettings.skybox;
	}

	public void method_0(Camera cam)
	{
		if (!(Singleton<LevelSettings>.Instance != this))
		{
			switch (AmbientType)
			{
			case AmbientType.NightVision:
				RenderSettings.ambientEquatorColor = NightVisionEquatorColor;
				RenderSettings.ambientGroundColor = NightVisionGroundColor;
				RenderSettings.ambientSkyColor = NightVisionSkyColor;
				RenderSettings.ambientLight = NightVisionSkyColor;
				RenderSettings.ambientIntensity = NightVisionAmbientIntensity;
				break;
			case AmbientType.Default:
				RenderSettings.ambientMode = AmbientMode;
				RenderSettings.ambientEquatorColor = EquatorColor;
				RenderSettings.ambientGroundColor = GroundColor;
				RenderSettings.ambientSkyColor = SkyColor;
				RenderSettings.ambientLight = SkyColor;
				RenderSettings.ambientIntensity = AmbientIntensity;
				break;
			}
		}
	}

	public void method_1()
	{
		method_2();
	}

	public void method_2()
	{
		if (!ienumerable_0.Any())
		{
			method_3();
			return;
		}
		foreach (TreeWind item in ienumerable_0)
		{
			if (!(item == null) && item.hideFlags != HideFlags.NotEditable && item.hideFlags != HideFlags.HideAndDontSave)
			{
				LODGroup component = item.transform.parent.GetComponent<LODGroup>();
				bool flag = false;
				bool flag2 = false;
				if (component != null)
				{
					LOD[] lODs = component.GetLODs();
					Renderer renderer = ((lODs.Length != 0) ? lODs[0].renderers[0] : null);
					Renderer renderer2 = ((lODs.Length > 1) ? lODs[1].renderers[0] : null);
					flag = renderer != null && !(renderer is BillboardRenderer) && renderer.gameObject == item.gameObject;
					flag2 = renderer2 != null && !(renderer2 is BillboardRenderer) && renderer2.gameObject == item.gameObject;
				}
				if (flag || flag2)
				{
					item.SetDrawMotionVectors(isTaaHighEnabled);
				}
			}
		}
	}

	public void method_3()
	{
		action_0?.Invoke();
		action_0 = null;
	}

	public void ApplyTreeWindSettings()
	{
		Dictionary<TreeWind.Settings, MaterialPropertyBlock> dictionary = new Dictionary<TreeWind.Settings, MaterialPropertyBlock>(TreeWind.Settings.SettingsComparer);
		using (GClass4062.BeginSampleWithToken("ApplyTreeWindSettings fill", "ApplyTreeWindSettings"))
		{
			int num = 0;
			TreeWind.Settings[] array = LocationScene.GetAll<TreeWind.Settings>().ToArray();
			TreeWind.Settings[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TreeWind.Settings key = array2[i];
				dictionary[key] = TreeWind.CreateMaterialPropertyBlock(key.BaseMinWindData, key.BaseMaxWindData, key.MinWindData, key.MaxWindData, num);
				num++;
			}
			if (WeatherController.Instance != null && WeatherController.Instance.WindController != null)
			{
				WeatherController.Instance.WindController.SetWind(array);
			}
		}
		using (GClass4062.BeginSampleWithToken("ApplyTreeWindSettings apply", "ApplyTreeWindSettings"))
		{
			foreach (TreeWind item in ienumerable_0)
			{
				dictionary.TryGetValue(item.settings, out var value);
				item.Init(value);
			}
		}
	}

	[CompilerGenerated]
	public void method_4(EAntialiasingMode x)
	{
		method_1();
	}
}
