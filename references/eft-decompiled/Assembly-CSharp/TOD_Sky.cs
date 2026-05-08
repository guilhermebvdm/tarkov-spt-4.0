using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(TOD_Resources))]
[RequireComponent(typeof(TOD_Components))]
public class TOD_Sky : MonoBehaviourSingleton<TOD_Sky>, GInterface0
{
	private int int_0 = -1;

	public TOD_ColorSpaceType ColorSpace;

	public TOD_ColorRangeType ColorRange;

	public TOD_SkyQualityType SkyQuality;

	public TOD_CloudQualityType CloudQuality = TOD_CloudQualityType.Bumped;

	public TOD_MeshQualityType MeshQuality = TOD_MeshQualityType.High;

	public TOD_WorldParameters World;

	public TOD_DayParameters Day;

	public TOD_NightParameters Night;

	public TOD_SunParameters Sun;

	public TOD_MoonParameters Moon;

	public TOD_StarParameters Stars;

	public TOD_LightParameters Light;

	public TOD_FogParameters Fog;

	public TOD_AmbientParameters Ambient;

	public TOD_ReflectionParameters Reflection;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private TOD_Components tod_Components_0;

	[CompilerGenerated]
	private float float_0;

	[CompilerGenerated]
	private float float_1;

	[CompilerGenerated]
	private TOD_Resources tod_Resources_0;

	[CompilerGenerated]
	private bool bool_1;

	[CompilerGenerated]
	private bool bool_2;

	[CompilerGenerated]
	private float float_2;

	[CompilerGenerated]
	private float float_3;

	[CompilerGenerated]
	private float float_4;

	[CompilerGenerated]
	private Vector3 vector3_0;

	[CompilerGenerated]
	private Vector3 vector3_1;

	[CompilerGenerated]
	private Vector3 vector3_2;

	[CompilerGenerated]
	private Vector3 vector3_3;

	[CompilerGenerated]
	private Vector3 vector3_4;

	[CompilerGenerated]
	private Vector3 vector3_5;

	[CompilerGenerated]
	private Color color_0;

	[CompilerGenerated]
	private Color color_1;

	[CompilerGenerated]
	private Color color_2;

	[CompilerGenerated]
	private Color color_3;

	[CompilerGenerated]
	private Color color_4;

	[CompilerGenerated]
	private Color color_5;

	[CompilerGenerated]
	private Color color_6;

	[CompilerGenerated]
	private Color color_7;

	[CompilerGenerated]
	private Color color_8;

	[CompilerGenerated]
	private Color color_9;

	[CompilerGenerated]
	private Color color_10;

	[CompilerGenerated]
	private Color color_11;

	[CompilerGenerated]
	private ReflectionProbe reflectionProbe_0;

	private float float_5 = float.MaxValue;

	private float float_6 = float.MaxValue;

	private float float_7 = float.MaxValue;

	private float float_8;

	private float float_9;

	private const int int_1 = 2;

	private Vector3 vector3_6;

	private Vector4 vector4_0;

	private Vector4 vector4_1;

	private Vector4 vector4_2;

	private Vector4 vector4_3;

	private const string string_0 = "USE_SIMPLE_REFLECTION_PROBE";

	[Tooltip("sLerp debug rotation speed")]
	public float rotationSpeed = 1f;

	private const float float_10 = MathF.PI;

	private const float float_11 = MathF.PI * 2f;

	private float float_12;

	private float float_13;

	private float float_14;

	private float float_15;

	private Quaternion quaternion_0;

	private Quaternion quaternion_1;

	[field: SerializeField]
	public TOD_CycleParameters Cycle { get; set; }

	[field: SerializeField]
	public TOD_AtmosphereParameters Atmosphere { get; set; }

	public bool Initialized
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public bool Headless => Camera.allCamerasCount == 0;

	public TOD_Components Components
	{
		[CompilerGenerated]
		get
		{
			return tod_Components_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Components_0 = value;
		}
	}

	public Light LightObject => Components.LightSource;

	public TOD_Time CurrentTime => Components.Time;

	public float UpdateInterval
	{
		get
		{
			return Light.UpdateInterval;
		}
		set
		{
			Light.UpdateInterval = value;
		}
	}

	public float ForceIndoor => 0f;

	public float SunVisibility
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public float ClearSky
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public TOD_Resources Resources
	{
		[CompilerGenerated]
		get
		{
			return tod_Resources_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Resources_0 = value;
		}
	}

	public bool IsDay
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public bool IsNight
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public float Radius => Components.DomeTransform.lossyScale.y;

	public float Diameter => Components.DomeTransform.lossyScale.y * 2f;

	public float LerpValue
	{
		[CompilerGenerated]
		get
		{
			return float_2;
		}
		[CompilerGenerated]
		set
		{
			float_2 = value;
		}
	}

	public float SunZenith
	{
		[CompilerGenerated]
		get
		{
			return float_3;
		}
		[CompilerGenerated]
		set
		{
			float_3 = value;
		}
	}

	public float MoonZenith
	{
		[CompilerGenerated]
		get
		{
			return float_4;
		}
		[CompilerGenerated]
		set
		{
			float_4 = value;
		}
	}

	public float LightZenith => Mathf.Min(SunZenith, MoonZenith);

	public float LightIntensity => Components.LightSource.intensity;

	public Vector3 SunDirection
	{
		[CompilerGenerated]
		get
		{
			return vector3_0;
		}
		[CompilerGenerated]
		set
		{
			vector3_0 = value;
		}
	}

	public Vector3 MoonDirection
	{
		[CompilerGenerated]
		get
		{
			return vector3_1;
		}
		[CompilerGenerated]
		set
		{
			vector3_1 = value;
		}
	}

	public Vector3 LightDirection
	{
		[CompilerGenerated]
		get
		{
			return vector3_2;
		}
		[CompilerGenerated]
		set
		{
			vector3_2 = value;
		}
	}

	public Vector3 LocalSunDirection
	{
		[CompilerGenerated]
		get
		{
			return vector3_3;
		}
		[CompilerGenerated]
		set
		{
			vector3_3 = value;
		}
	}

	public Vector3 LocalMoonDirection
	{
		[CompilerGenerated]
		get
		{
			return vector3_4;
		}
		[CompilerGenerated]
		set
		{
			vector3_4 = value;
		}
	}

	public Vector3 LocalLightDirection
	{
		[CompilerGenerated]
		get
		{
			return vector3_5;
		}
		[CompilerGenerated]
		set
		{
			vector3_5 = value;
		}
	}

	public Color SunLightColor
	{
		[CompilerGenerated]
		get
		{
			return color_0;
		}
		[CompilerGenerated]
		set
		{
			color_0 = value;
		}
	}

	public Color MoonLightColor
	{
		[CompilerGenerated]
		get
		{
			return color_1;
		}
		[CompilerGenerated]
		set
		{
			color_1 = value;
		}
	}

	public Color LightColor => Components.LightSource.color;

	public Color SunRayColor
	{
		[CompilerGenerated]
		get
		{
			return color_2;
		}
		[CompilerGenerated]
		set
		{
			color_2 = value;
		}
	}

	public Color MoonRayColor
	{
		[CompilerGenerated]
		get
		{
			return color_3;
		}
		[CompilerGenerated]
		set
		{
			color_3 = value;
		}
	}

	public Color RayColor
	{
		[CompilerGenerated]
		get
		{
			return color_4;
		}
		[CompilerGenerated]
		set
		{
			color_4 = value;
		}
	}

	public Color SunSkyColor
	{
		[CompilerGenerated]
		get
		{
			return color_5;
		}
		[CompilerGenerated]
		set
		{
			color_5 = value;
		}
	}

	public Color MoonSkyColor
	{
		[CompilerGenerated]
		get
		{
			return color_6;
		}
		[CompilerGenerated]
		set
		{
			color_6 = value;
		}
	}

	public Color SunMeshColor
	{
		[CompilerGenerated]
		get
		{
			return color_7;
		}
		[CompilerGenerated]
		set
		{
			color_7 = value;
		}
	}

	public Color MoonMeshColor
	{
		[CompilerGenerated]
		get
		{
			return color_8;
		}
		[CompilerGenerated]
		set
		{
			color_8 = value;
		}
	}

	public Color GroundColor
	{
		[CompilerGenerated]
		get
		{
			return color_9;
		}
		[CompilerGenerated]
		set
		{
			color_9 = value;
		}
	}

	public Color AmbientColor
	{
		[CompilerGenerated]
		get
		{
			return color_10;
		}
		[CompilerGenerated]
		set
		{
			color_10 = value;
		}
	}

	public Color MoonHaloColor
	{
		[CompilerGenerated]
		get
		{
			return color_11;
		}
		[CompilerGenerated]
		set
		{
			color_11 = value;
		}
	}

	public ReflectionProbe Probe
	{
		[CompilerGenerated]
		get
		{
			return reflectionProbe_0;
		}
		[CompilerGenerated]
		set
		{
			reflectionProbe_0 = value;
		}
	}

	public Vector3 OrbitalToUnity(float radius, float theta, float phi)
	{
		float num = Mathf.Sin(theta);
		float num2 = Mathf.Cos(theta);
		float num3 = Mathf.Sin(phi);
		float num4 = Mathf.Cos(phi);
		Vector3 result = default(Vector3);
		result.z = radius * num * num4;
		result.y = radius * num2;
		result.x = radius * num * num3;
		return result;
	}

	public Vector3 OrbitalToLocal(float theta, float phi)
	{
		float num = Mathf.Sin(theta);
		float y = Mathf.Cos(theta);
		float num2 = Mathf.Sin(phi);
		float num3 = Mathf.Cos(phi);
		Vector3 result = default(Vector3);
		result.z = num * num3;
		result.y = y;
		result.x = num * num2;
		return result;
	}

	public Color SampleAtmosphere(Vector3 direction, bool directLight = true)
	{
		Vector3 dir = Components.DomeTransform.InverseTransformDirection(direction);
		Color color = method_16(dir, directLight);
		color = method_12(color);
		return method_15(color);
	}

	public Color SampleAtmosphereRawMT(Quaternion rotation, Vector3 direction, bool directLight = true)
	{
		Vector3 dir = InverseDirection(rotation, direction);
		Color color = method_16(dir, directLight);
		color = method_12(color);
		return method_15(color);
	}

	public Color SampleAtmosphereRaw(Vector3 direction, bool directLight = true)
	{
		Vector3 dir = Components.DomeTransform.InverseTransformDirection(direction);
		Color color = method_16(dir, directLight);
		color = method_12(color);
		return method_15(color);
	}

	public static Vector3 InverseDirection(Quaternion rotation, Vector3 direction)
	{
		return Quaternion.Inverse(rotation) * direction;
	}

	public SphericalHarmonicsL2 RenderToSphericalHarmonics()
	{
		SphericalHarmonicsL2 result = default(SphericalHarmonicsL2);
		Color linear = AmbientColor.linear;
		linear *= 0f;
		Vector3 vector = new Vector3(0.61237246f, 0.5f, 0.61237246f);
		Vector3 up = Vector3.up;
		Color linear2 = SampleAtmosphere(up).linear;
		result.AddDirectionalLight(up, linear2, 0.42857143f);
		Vector3 direction = new Vector3(0f - vector.x, vector.y, 0f - vector.z);
		Color linear3 = SampleAtmosphere(direction).linear;
		result.AddDirectionalLight(direction, linear3, 0.2857143f);
		Vector3 direction2 = new Vector3(vector.x, vector.y, 0f - vector.z);
		Color linear4 = SampleAtmosphere(direction2).linear;
		result.AddDirectionalLight(direction2, linear4, 0.2857143f);
		Vector3 direction3 = new Vector3(0f - vector.x, vector.y, vector.z);
		Color linear5 = SampleAtmosphere(direction3).linear;
		result.AddDirectionalLight(direction3, linear5, 0.2857143f);
		Vector3 direction4 = new Vector3(vector.x, vector.y, vector.z);
		Color linear6 = SampleAtmosphere(direction4).linear;
		result.AddDirectionalLight(direction4, linear6, 0.2857143f);
		Vector3 left = Vector3.left;
		Color linear7 = SampleAtmosphere(left).linear;
		result.AddDirectionalLight(left, linear7, 1f / 7f);
		Vector3 right = Vector3.right;
		Color linear8 = SampleAtmosphere(right).linear;
		result.AddDirectionalLight(right, linear8, 1f / 7f);
		Vector3 back = Vector3.back;
		Color linear9 = SampleAtmosphere(back).linear;
		result.AddDirectionalLight(back, linear9, 1f / 7f);
		Vector3 forward = Vector3.forward;
		Color linear10 = SampleAtmosphere(forward).linear;
		result.AddDirectionalLight(forward, linear10, 1f / 7f);
		Vector3 direction5 = new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z);
		result.AddDirectionalLight(direction5, linear, 0.2857143f);
		Vector3 direction6 = new Vector3(vector.x, 0f - vector.y, 0f - vector.z);
		result.AddDirectionalLight(direction6, linear, 0.2857143f);
		Vector3 direction7 = new Vector3(0f - vector.x, 0f - vector.y, vector.z);
		result.AddDirectionalLight(direction7, linear, 0.2857143f);
		Vector3 direction8 = new Vector3(vector.x, 0f - vector.y, vector.z);
		result.AddDirectionalLight(direction8, linear, 0.2857143f);
		Vector3 down = Vector3.down;
		result.AddDirectionalLight(down, linear, 0.42857143f);
		return result;
	}

	public void RenderToCubemap(RenderTexture targetTexture = null)
	{
		if (!Probe)
		{
			Probe = new GameObject().AddComponent<ReflectionProbe>();
			Probe.name = base.gameObject.name + " Reflection Probe";
			Probe.mode = ReflectionProbeMode.Realtime;
		}
		if (int_0 < 0 || Probe.IsFinishedRendering(int_0))
		{
			float num = float.MaxValue;
			Probe.transform.position = Components.DomeTransform.position;
			Probe.size = new Vector3(num, num, num);
			Probe.intensity = RenderSettings.reflectionIntensity;
			Probe.clearFlags = Reflection.ClearFlags;
			Probe.backgroundColor = Color.black;
			Probe.cullingMask = Reflection.CullingMask;
			Probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			Probe.timeSlicingMode = Reflection.TimeSlicing;
			int_0 = Probe.RenderProbe(targetTexture);
		}
	}

	public Color SampleFogColor(bool directLight = true)
	{
		Vector3 vector = Vector3.forward;
		if (Components.Camera != null)
		{
			vector = Quaternion.Euler(0f, Components.Camera.transform.rotation.eulerAngles.y, 0f) * vector;
		}
		Color color = SampleAtmosphere(Vector3.Lerp(vector, Vector3.up, Fog.HeightBias).normalized, directLight);
		return new Color(color.r, color.g, color.b, 1f);
	}

	public Color SampleSkyColor()
	{
		Vector3 sunDirection = SunDirection;
		sunDirection.y = Mathf.Abs(sunDirection.y);
		Color color = SampleAtmosphere(sunDirection.normalized, directLight: false);
		return new Color(color.r, color.g, color.b, 1f);
	}

	public Color SampleEquatorColor()
	{
		Vector3 sunDirection = SunDirection;
		sunDirection.y = 0f;
		Color color = SampleAtmosphere(sunDirection.normalized, directLight: false);
		return new Color(color.r, color.g, color.b, 1f);
	}

	public void UpdateFog()
	{
		switch (Fog.Mode)
		{
		case TOD_FogType.Color:
			RenderSettings.fogColor = SampleFogColor(directLight: false);
			break;
		case TOD_FogType.Directional:
			RenderSettings.fogColor = SampleFogColor();
			break;
		case TOD_FogType.None:
			break;
		}
	}

	public void UpdateAmbient()
	{
	}

	public void UpdateReflection()
	{
	}

	public void LoadParameters(string xml)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TOD_Parameters));
		XmlTextReader xmlReader = new XmlTextReader(new StringReader(xml));
		(xmlSerializer.Deserialize(xmlReader) as TOD_Parameters).ToSky(this);
	}

	public void method_0()
	{
		if (!Headless)
		{
			Mesh mesh = null;
			Mesh mesh2 = null;
			Mesh mesh3 = null;
			Mesh mesh4 = null;
			Mesh mesh5 = null;
			Mesh mesh6 = null;
			switch (MeshQuality)
			{
			case TOD_MeshQualityType.Low:
				mesh = Resources.IcosphereLow;
				mesh2 = Resources.IcosphereLow;
				mesh3 = Resources.IcosphereLow;
				mesh4 = Resources.HalfIcosphereLow;
				mesh5 = Resources.Quad;
				mesh6 = Resources.SphereLow;
				break;
			case TOD_MeshQualityType.Medium:
				mesh = Resources.IcosphereMedium;
				mesh2 = Resources.IcosphereMedium;
				mesh3 = Resources.IcosphereLow;
				mesh4 = Resources.HalfIcosphereMedium;
				mesh5 = Resources.Quad;
				mesh6 = Resources.SphereMedium;
				break;
			case TOD_MeshQualityType.High:
				mesh = Resources.IcosphereHigh;
				mesh2 = Resources.IcosphereHigh;
				mesh3 = Resources.IcosphereLow;
				mesh4 = Resources.HalfIcosphereHigh;
				mesh5 = Resources.Quad;
				mesh6 = Resources.SphereHigh;
				break;
			}
			mesh5.bounds = new Bounds(Vector3.zero, Vector3.one * 1000000f);
			if ((bool)Components.SpaceRenderer && Components.SpaceMaterial != Resources.SpaceMaterial)
			{
				TOD_Components components = Components;
				Material spaceMaterial = (Components.SpaceRenderer.sharedMaterial = Resources.SpaceMaterial);
				components.SpaceMaterial = spaceMaterial;
			}
			if ((bool)Components.AtmosphereRenderer && Components.AtmosphereMaterial != Resources.AtmosphereMaterial)
			{
				TOD_Components components2 = Components;
				Material spaceMaterial = (Components.AtmosphereRenderer.sharedMaterial = Resources.AtmosphereMaterial);
				components2.AtmosphereMaterial = spaceMaterial;
			}
			if ((bool)Components.ClearRenderer && Components.ClearMaterial != Resources.ClearMaterial)
			{
				TOD_Components components3 = Components;
				Material spaceMaterial = (Components.ClearRenderer.sharedMaterial = Resources.ClearMaterial);
				components3.ClearMaterial = spaceMaterial;
			}
			if ((bool)Components.SunRenderer && Components.SunMaterial != Resources.SunMaterial)
			{
				TOD_Components components4 = Components;
				Material spaceMaterial = (Components.SunRenderer.sharedMaterial = Resources.SunMaterial);
				components4.SunMaterial = spaceMaterial;
			}
			if ((bool)Components.MoonRenderer && Components.MoonMaterial != Resources.MoonMaterial)
			{
				TOD_Components components5 = Components;
				Material spaceMaterial = (Components.MoonRenderer.sharedMaterial = Resources.MoonMaterial);
				components5.MoonMaterial = spaceMaterial;
			}
			if ((bool)Components.SpaceMeshFilter && Components.SpaceMeshFilter.sharedMesh != mesh)
			{
				Components.SpaceMeshFilter.mesh = mesh;
			}
			if ((bool)Components.AtmosphereMeshFilter && Components.AtmosphereMeshFilter.sharedMesh != mesh2)
			{
				Components.AtmosphereMeshFilter.mesh = mesh2;
			}
			if ((bool)Components.ClearMeshFilter && Components.ClearMeshFilter.sharedMesh != mesh3)
			{
				Components.ClearMeshFilter.mesh = mesh3;
			}
			if ((bool)Components.CloudMeshFilter && Components.CloudMeshFilter.sharedMesh != mesh4)
			{
				Components.CloudMeshFilter.mesh = mesh4;
			}
			if ((bool)Components.SunMeshFilter && Components.SunMeshFilter.sharedMesh != mesh5)
			{
				Components.SunMeshFilter.mesh = mesh5;
			}
			if ((bool)Components.MoonMeshFilter && Components.MoonMeshFilter.sharedMesh != mesh6)
			{
				Components.MoonMeshFilter.mesh = mesh6;
			}
		}
	}

	public void method_1()
	{
		if (!Headless)
		{
			UpdateFog();
			if (Application.isPlaying && float_6 < Ambient.UpdateInterval)
			{
				float_6 += Time.deltaTime;
			}
			else
			{
				float_6 = 0f;
				UpdateAmbient();
			}
			if (Application.isPlaying && float_7 < Reflection.UpdateInterval)
			{
				float_7 += Time.deltaTime;
				return;
			}
			float_7 = 0f;
			UpdateReflection();
		}
	}

	public void method_2()
	{
		if (!Headless)
		{
			if ((bool)Resources.AtmosphereMaterial)
			{
				method_6(Resources.AtmosphereMaterial);
				method_4(Resources.AtmosphereMaterial);
				method_5(Resources.AtmosphereMaterial);
			}
			if ((bool)Resources.SkyboxMaterial)
			{
				method_4(Resources.SkyboxMaterial);
				method_5(Resources.SkyboxMaterial);
			}
		}
	}

	public void method_3()
	{
		if (!Headless)
		{
			Shader.SetGlobalColor(Resources.ID_SunSkyColor, SunSkyColor);
			Shader.SetGlobalColor(Resources.ID_MoonSkyColor, MoonSkyColor);
			Shader.SetGlobalColor(Resources.ID_SunLightColor, SunLightColor);
			Shader.SetGlobalColor(Resources.ID_MoonLightColor, MoonLightColor);
			Shader.SetGlobalColor(Resources.ID_SunMeshColor, SunMeshColor);
			Shader.SetGlobalColor(Resources.ID_MoonMeshColor, MoonMeshColor);
			Shader.SetGlobalColor(Resources.ID_GroundColor, GroundColor);
			Shader.SetGlobalColor(Resources.ID_AmbientColor, AmbientColor);
			Shader.SetGlobalColor(Resources.ID_MoonHaloColor, MoonHaloColor);
			Shader.SetGlobalVector(Resources.ID_SunDirection, SunDirection);
			Shader.SetGlobalVector(Resources.ID_MoonDirection, MoonDirection);
			Shader.SetGlobalVector(Resources.ID_LightDirection, LightDirection);
			Shader.SetGlobalVector(Resources.ID_LocalSunDirection, LocalSunDirection);
			Shader.SetGlobalVector(Resources.ID_LocalMoonDirection, LocalMoonDirection);
			Shader.SetGlobalVector(Resources.ID_LocalLightDirection, LocalLightDirection);
			Shader.SetGlobalFloat(Resources.ID_Contrast, Atmosphere.Contrast);
			Shader.SetGlobalFloat(Resources.ID_Brightness, Atmosphere.Brightness);
			Shader.SetGlobalFloat(Resources.ID_ScatteringBrightness, Atmosphere.ScatteringBrightness);
			Shader.SetGlobalFloat(Resources.ID_Fogginess, Atmosphere.Fogginess);
			Shader.SetGlobalFloat(Resources.ID_Directionality, Atmosphere.Directionality);
			Shader.SetGlobalFloat(Resources.ID_MoonHaloPower, 1f / Moon.HaloSize);
			Shader.SetGlobalFloat(Resources.ID_SpaceTiling, Stars.Tiling);
			Shader.SetGlobalFloat(Resources.ID_SpaceBrightness, Stars.Brightness * (1f - Atmosphere.Fogginess) * (1f - LerpValue));
			Shader.SetGlobalFloat(Resources.ID_SunMeshContrast, 1f / Mathf.Max(0.001f, Sun.MeshContrast));
			Shader.SetGlobalFloat(Resources.ID_SunMeshBrightness, Sun.MeshBrightness * (1f - Atmosphere.Fogginess));
			Shader.SetGlobalFloat(Resources.ID_MoonMeshContrast, 1f / Mathf.Max(0.001f, Moon.MeshContrast));
			Shader.SetGlobalFloat(Resources.ID_MoonMeshBrightness, Moon.MeshBrightness * (1f - Atmosphere.Fogginess));
			Shader.SetGlobalVector(Resources.ID_kBetaMie, vector3_6);
			Shader.SetGlobalVector(Resources.ID_kSun, vector4_0);
			Shader.SetGlobalVector(Resources.ID_k4PI, vector4_1);
			Shader.SetGlobalVector(Resources.ID_kRadius, vector4_2);
			Shader.SetGlobalVector(Resources.ID_kScale, vector4_3);
			Shader.SetGlobalMatrix(Resources.ID_World2Sky, Components.DomeTransform.worldToLocalMatrix);
			Shader.SetGlobalMatrix(Resources.ID_Sky2World, Components.DomeTransform.localToWorldMatrix);
		}
	}

	public void method_4(Material material)
	{
		switch (ColorSpace)
		{
		case TOD_ColorSpaceType.Auto:
			if (QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Linear)
			{
				material.EnableKeyword("LINEAR");
				material.DisableKeyword("GAMMA");
			}
			else
			{
				material.DisableKeyword("LINEAR");
				material.EnableKeyword("GAMMA");
			}
			break;
		case TOD_ColorSpaceType.Linear:
			material.EnableKeyword("LINEAR");
			material.DisableKeyword("GAMMA");
			break;
		case TOD_ColorSpaceType.Gamma:
			material.DisableKeyword("LINEAR");
			material.EnableKeyword("GAMMA");
			break;
		}
	}

	public void method_5(Material material)
	{
		switch (ColorRange)
		{
		case TOD_ColorRangeType.Auto:
			if ((bool)Components.Camera && Components.Camera.HDR)
			{
				material.EnableKeyword("HDR");
				material.DisableKeyword("LDR");
			}
			else
			{
				material.DisableKeyword("HDR");
				material.EnableKeyword("LDR");
			}
			break;
		case TOD_ColorRangeType.HDR:
			material.EnableKeyword("HDR");
			material.DisableKeyword("LDR");
			break;
		case TOD_ColorRangeType.LDR:
			material.DisableKeyword("HDR");
			material.EnableKeyword("LDR");
			break;
		}
	}

	public void method_6(Material material)
	{
		switch (SkyQuality)
		{
		case TOD_SkyQualityType.PerPixel:
			material.DisableKeyword("PER_VERTEX");
			material.EnableKeyword("PER_PIXEL");
			break;
		case TOD_SkyQualityType.PerVertex:
			material.EnableKeyword("PER_VERTEX");
			material.DisableKeyword("PER_PIXEL");
			break;
		}
	}

	public float method_7(float inCos)
	{
		float num = 1f - inCos;
		return 0.25f * Mathf.Exp(-0.00287f + num * (0.459f + num * (3.83f + num * (-6.8f + num * 5.25f))));
	}

	public float method_8(float eyeCos, float eyeCos2)
	{
		return vector3_6.x * (1f + eyeCos2) / Mathf.Pow(vector3_6.y + vector3_6.z * eyeCos, 1.5f);
	}

	public float method_9(float eyeCos2)
	{
		return 0.75f + 0.75f * eyeCos2;
	}

	public Color method_10(Vector3 dir)
	{
		return Color.Lerp(MoonSkyColor, Color.black, dir.y);
	}

	public Color method_11(Vector3 dir)
	{
		return MoonHaloColor * Mathf.Pow(Mathf.Max(0f, Vector3.Dot(dir, LocalMoonDirection)), 1f / Moon.HaloSize);
	}

	public Color method_12(Color color)
	{
		return new Color(1f - Mathf.Pow(2f, (0f - Atmosphere.Brightness) * color.r), 1f - Mathf.Pow(2f, (0f - Atmosphere.Brightness) * color.g), 1f - Mathf.Pow(2f, (0f - Atmosphere.Brightness) * color.b), color.a);
	}

	public Color method_13(Color color, float brightness)
	{
		return new Color(1f - Mathf.Pow(2f, (0f - brightness) * color.r), 1f - Mathf.Pow(2f, (0f - brightness) * color.g), 1f - Mathf.Pow(2f, (0f - brightness) * color.b), color.a);
	}

	public Color method_14(Color color)
	{
		return new Color(color.r * color.r, color.g * color.g, color.b * color.b, color.a);
	}

	public Color method_15(Color color)
	{
		return new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), color.a);
	}

	public Color method_16(Vector3 dir, bool directLight = true)
	{
		dir.y = Mathf.Clamp01(dir.y);
		float x = vector4_2.x;
		float y = vector4_2.y;
		float w = vector4_2.w;
		float x2 = vector4_3.x;
		float z = vector4_3.z;
		float w2 = vector4_3.w;
		float x3 = vector4_1.x;
		float y2 = vector4_1.y;
		float z2 = vector4_1.z;
		float w3 = vector4_1.w;
		float x4 = vector4_0.x;
		float y3 = vector4_0.y;
		float z3 = vector4_0.z;
		float w4 = vector4_0.w;
		Vector3 rhs = new Vector3(0f, x + w2, 0f);
		float num = Mathf.Sqrt(w + y * dir.y * dir.y - y) - x * dir.y;
		float num2 = Mathf.Exp(z * (0f - w2));
		float inCos = Vector3.Dot(dir, rhs) / (x + w2);
		float num3 = num2 * method_7(inCos);
		float num4 = num / 2f;
		float num5 = num4 * x2;
		Vector3 vector = dir * num4;
		Vector3 rhs2 = new Vector3(rhs.x + vector.x * 0.5f, rhs.y + vector.y * 0.5f, rhs.z + vector.z * 0.5f);
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		for (int i = 0; i < 2; i++)
		{
			float magnitude = rhs2.magnitude;
			float num9 = 1f / magnitude;
			float num10 = Mathf.Exp(z * (x - magnitude));
			float num11 = num10 * num5;
			float inCos2 = Vector3.Dot(dir, rhs2) * num9;
			float inCos3 = Vector3.Dot(LocalSunDirection, rhs2) * num9;
			float num12 = num3 + num10 * (method_7(inCos3) - method_7(inCos2));
			float num13 = Mathf.Exp((0f - num12) * (x3 + w3));
			float num14 = Mathf.Exp((0f - num12) * (y2 + w3));
			float num15 = Mathf.Exp((0f - num12) * (z2 + w3));
			num6 += num13 * num11;
			num7 += num14 * num11;
			num8 += num15 * num11;
			rhs2.x += vector.x;
			rhs2.y += vector.y;
			rhs2.z += vector.z;
		}
		float num16 = SunSkyColor.r * num6 * x4;
		float num17 = SunSkyColor.g * num7 * y3;
		float num18 = SunSkyColor.b * num8 * z3;
		float num19 = SunSkyColor.r * num6 * w4;
		float num20 = SunSkyColor.g * num7 * w4;
		float num21 = SunSkyColor.b * num8 * w4;
		float num22 = 0f;
		float num23 = 0f;
		float num24 = 0f;
		float num25 = Vector3.Dot(LocalSunDirection, dir);
		float eyeCos = num25 * num25;
		float num26 = method_9(eyeCos);
		num22 += num26 * num16;
		num23 += num26 * num17;
		num24 += num26 * num18;
		if (directLight)
		{
			float num27 = method_8(num25, eyeCos);
			num22 += num27 * num19;
			num23 += num27 * num20;
			num24 += num27 * num21;
		}
		Color color = method_10(dir);
		num22 += color.r;
		num23 += color.g;
		num24 += color.b;
		if (directLight)
		{
			Color color2 = method_11(dir);
			num22 += color2.r;
			num23 += color2.g;
			num24 += color2.b;
		}
		num22 = Mathf.Lerp(num22, AmbientColor.r, 0.5f);
		num23 = Mathf.Lerp(num23, AmbientColor.g, 0.5f);
		num24 = Mathf.Lerp(num24, AmbientColor.b, 0.5f);
		num22 = Mathf.Pow(num22 * Atmosphere.Brightness, Atmosphere.Contrast);
		num23 = Mathf.Pow(num23 * Atmosphere.Brightness, Atmosphere.Contrast);
		num24 = Mathf.Pow(num24 * Atmosphere.Brightness, Atmosphere.Contrast);
		return new Color(num22, num23, num24, 1f);
	}

	public override void Awake()
	{
		base.Awake();
		Initialize();
	}

	public void Initialize()
	{
		Components = GetComponent<TOD_Components>();
		Components.Initialize();
		Resources = GetComponent<TOD_Resources>();
		Resources.Initialize();
		LateUpdate();
		Initialized = true;
		GClass4.RegisterInstance(MonoBehaviourSingleton<TOD_Sky>.Instance);
		Shader.DisableKeyword("USE_SIMPLE_REFLECTION_PROBE");
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)Probe)
		{
			UnityEngine.Object.Destroy(Probe.gameObject);
		}
	}

	public void LateUpdate()
	{
		method_17();
		method_18();
		method_0();
		method_1();
		method_2();
		method_3();
	}

	public void method_17()
	{
		float num = 0f - Atmosphere.Directionality;
		float num2 = num * num;
		vector3_6.x = 1.5f * ((1f - num2) / (2f + num2));
		vector3_6.y = 1f + num2;
		vector3_6.z = 2f * num;
		float num3 = 0.002f * Atmosphere.MieMultiplier;
		float num4 = 0.002f * Atmosphere.RayleighMultiplier;
		float x = num4 * 40f * 5.2701645f;
		float y = num4 * 40f * 9.473284f;
		float z = num4 * 40f * 19.643803f;
		float w = num3 * 40f;
		vector4_0.x = x;
		vector4_0.y = y;
		vector4_0.z = z;
		vector4_0.w = w;
		float x2 = num4 * 4f * MathF.PI * 5.2701645f;
		float y2 = num4 * 4f * MathF.PI * 9.473284f;
		float z2 = num4 * 4f * MathF.PI * 19.643803f;
		float w2 = num3 * 4f * MathF.PI;
		vector4_1.x = x2;
		vector4_1.y = y2;
		vector4_1.z = z2;
		vector4_1.w = w2;
		vector4_2.x = 1f;
		vector4_2.y = 1f;
		vector4_2.z = 1.025f;
		vector4_2.w = 1.050625f;
		vector4_3.x = 40.00004f;
		vector4_3.y = 0.25f;
		vector4_3.z = 160.00015f;
		vector4_3.w = 0.0001f;
	}

	public Vector3 LightDirectionExtrapolated(float addTime)
	{
		Vector3 vector = -Components.LightTransform.forward;
		float num = float_5 * rotationSpeed;
		float t = num + addTime * rotationSpeed;
		if (!Singleton<SharedGameSettingsClass>.Instantiated || (int)Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.ShadowsQuality < 3)
		{
			num = (Time.time - float_9) / (float_8 + 0.0001f);
			t = num + addTime / (float_8 + 0.0001f);
		}
		if ((double)num > 1.3)
		{
			return vector;
		}
		Quaternion rotation = Quaternion.Slerp(quaternion_0, quaternion_1, num);
		return Quaternion.SlerpUnclamped(quaternion_0, quaternion_1, t) * Quaternion.Inverse(rotation) * vector;
	}

	public void method_18()
	{
		float_5 += Time.deltaTime;
		if (!Singleton<SharedGameSettingsClass>.Instantiated || (int)Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.ShadowsQuality > 2)
		{
			Components.LightTransform.rotation = Quaternion.Slerp(quaternion_0, quaternion_1, float_5 * rotationSpeed);
		}
		if (float_5 < Light.UpdateInterval && Application.isPlaying)
		{
			return;
		}
		float f = MathF.PI / 180f * World.Latitude;
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float longitude = World.Longitude;
		float num3 = MathF.PI / 2f;
		int year = Cycle.Year;
		int month = Cycle.Month;
		int day = Cycle.Day;
		float num4 = Cycle.Hour - World.UTC;
		float num5 = (float)(367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730530) + num4 / 24f;
		float num6 = 23.4393f - 3.563E-07f * num5;
		float f2 = MathF.PI / 180f * num6;
		float num7 = Mathf.Sin(f2);
		float num8 = Mathf.Cos(f2);
		float num9 = (Singleton<LevelSettings>.Instantiated ? Singleton<LevelSettings>.Instance.NorthDirection : 180f);
		float num10 = 282.9404f + 4.70935E-05f * num5;
		float num11 = 0.016709f - 1.151E-09f * num5;
		float num12 = 356.047f + 0.98560023f * num5;
		float num13 = MathF.PI / 180f * num12;
		float num14 = Mathf.Sin(num13);
		float num15 = Mathf.Cos(num13);
		float f3 = num13 + num11 * num14 * (1f + num11 * num15);
		float num16 = Mathf.Sin(f3);
		float num17 = Mathf.Cos(f3) - num11;
		float num18 = Mathf.Sqrt(1f - num11 * num11) * num16;
		float num19 = 57.29578f * Mathf.Atan2(num18, num17);
		float num20 = Mathf.Sqrt(num17 * num17 + num18 * num18);
		float num21 = num19 + num10;
		float f4 = MathF.PI / 180f * num21;
		float num22 = Mathf.Sin(f4);
		float num23 = Mathf.Cos(f4);
		float num24 = num20 * num23;
		float num25 = num20 * num22;
		float num26 = num24;
		float num27 = num25 * num8;
		float y = num25 * num7;
		float num28 = Mathf.Atan2(num27, num26);
		float f5 = Mathf.Atan2(y, Mathf.Sqrt(num26 * num26 + num27 * num27));
		float num29 = Mathf.Sin(f5);
		float num30 = Mathf.Cos(f5);
		float num31 = num19 + num10 + 180f + 15f * num4;
		float num32 = MathF.PI / 180f * (num31 + longitude);
		float f6 = num32 - num28;
		float num33 = Mathf.Sin(f6);
		float num34 = Mathf.Cos(f6) * num30;
		float num35 = num33 * num30;
		float num36 = num29;
		float num37 = num34 * num - num36 * num2;
		float num38 = num35;
		float y2 = num34 * num2 + num36 * num;
		float num39 = Mathf.Atan2(num38, num37) + MathF.PI / 180f * num9;
		float num40 = Mathf.Atan2(y2, Mathf.Sqrt(num37 * num37 + num38 * num38));
		float num41 = num3 - num40;
		float num42 = 0f - num39;
		float num82;
		float num83;
		if (Moon.Position == TOD_MoonPositionType.Realistic)
		{
			float num43 = 125.1228f - 0.05295381f * num5;
			float num44 = 318.0634f + 0.16435732f * num5;
			float num45 = 0.0549f;
			float num46 = 115.3654f + 13.064993f * num5;
			float f7 = MathF.PI / 180f * num43;
			float num47 = Mathf.Sin(f7);
			float num48 = Mathf.Cos(f7);
			float num49 = Mathf.Sin(0.08980417f);
			float num50 = Mathf.Cos(0.08980417f);
			float num51 = MathF.PI / 180f * num46;
			float num52 = Mathf.Sin(num51);
			float num53 = Mathf.Cos(num51);
			float f8 = num51 + num45 * num52 * (1f + num45 * num53);
			float num54 = Mathf.Sin(f8);
			float num55 = Mathf.Cos(f8);
			float num56 = 60.2666f * (num55 - num45);
			float num57 = 60.2666f * (Mathf.Sqrt(0.996986f) * num54);
			float num58 = 57.29578f * Mathf.Atan2(num57, num56);
			float num59 = Mathf.Sqrt(num56 * num56 + num57 * num57);
			float num60 = num58 + num44;
			float f9 = MathF.PI / 180f * num60;
			float num61 = Mathf.Sin(f9);
			float num62 = Mathf.Cos(f9);
			float num63 = num59 * (num48 * num62 - num47 * num61 * num50);
			float num64 = num59 * (num47 * num62 + num48 * num61 * num50);
			float num65 = num59 * (num61 * num49);
			float num66 = num63;
			float num67 = num64;
			float num68 = num65;
			float num69 = num66;
			float num70 = num67 * num8 - num68 * num7;
			float y3 = num67 * num7 + num68 * num8;
			float num71 = Mathf.Atan2(num70, num69);
			float f10 = Mathf.Atan2(y3, Mathf.Sqrt(num69 * num69 + num70 * num70));
			float num72 = Mathf.Sin(f10);
			float num73 = Mathf.Cos(f10);
			float f11 = num32 - num71;
			float num74 = Mathf.Sin(f11);
			float num75 = Mathf.Cos(f11) * num73;
			float num76 = num74 * num73;
			float num77 = num72;
			float num78 = num75 * num - num77 * num2;
			float num79 = num76;
			float y4 = num75 * num2 + num77 * num;
			float num80 = Mathf.Atan2(num79, num78) + MathF.PI / 180f * num9;
			float num81 = Mathf.Atan2(y4, Mathf.Sqrt(num78 * num78 + num79 * num79));
			num82 = num3 - num81;
			num83 = 0f - num80;
		}
		else
		{
			num82 = num41 - MathF.PI;
			num83 = num42;
		}
		SunZenith = 57.29578f * num41;
		MoonZenith = 57.29578f * num82;
		Quaternion quaternion = Quaternion.Euler(90f - World.Latitude, 0f, 0f) * Quaternion.Euler(0f, World.Longitude, 0f) * Quaternion.Euler(0f, num32 * 57.29578f, 0f);
		if (Stars.Position == TOD_StarsPositionType.Rotating)
		{
			Components.SpaceTransform.localRotation = quaternion;
		}
		else
		{
			Components.SpaceTransform.localRotation = Quaternion.identity;
		}
		Vector3 localPosition = OrbitalToLocal(num41, num42);
		Components.SunTransform.localPosition = localPosition;
		Components.SunTransform.LookAt(Components.DomeTransform.position, Components.SunTransform.up);
		Vector3 localPosition2 = OrbitalToLocal(num82, num83);
		Vector3 worldUp = quaternion * -Vector3.right;
		Components.MoonTransform.localPosition = localPosition2;
		Components.MoonTransform.LookAt(Components.DomeTransform.position, worldUp);
		float num84 = 2f * Mathf.Tan(MathF.PI / 90f * Sun.MeshSize);
		Vector3 localScale = new Vector3(num84, num84, num84);
		Components.SunTransform.localScale = localScale;
		float num85 = 2f * Mathf.Tan(MathF.PI / 180f * Moon.MeshSize);
		Vector3 localScale2 = new Vector3(num85, num85, num85);
		Components.MoonTransform.localScale = localScale2;
		bool flag = Components.SunTransform.localPosition.y > 0f - num84;
		Components.SunRenderer.enabled = flag;
		bool flag2 = Components.MoonTransform.localPosition.y > 0f - num85;
		Components.MoonRenderer.enabled = flag2;
		Components.SpaceRenderer.enabled = true;
		Components.AtmosphereRenderer.enabled = true;
		bool flag3 = Components.Rays != null;
		Components.ClearRenderer.enabled = flag3;
		LerpValue = Mathf.InverseLerp(110f, 80f, SunZenith);
		float time = 1f - LerpValue;
		float num86 = 1f - Atmosphere.Fogginess;
		float num87 = ((Moon.Position == TOD_MoonPositionType.Realistic) ? Mathf.Clamp01((90f - num82 * 57.29578f) / 5f) : Mathf.Clamp01((90f + num82 * 57.29578f) / 5f));
		ClearSky = num86;
		SunVisibility = Mathf.Clamp01((LerpValue - 0.1f) / 0.9f);
		float num88 = Mathf.Clamp01(num86 * (LerpValue - 0.1f) / 0.9f);
		float num89 = Mathf.Clamp01(num86 * num87 * (0.1f - LerpValue) / 0.1f);
		num89 = Mathf.Clamp01(num86 * num87);
		float multiplier = Day.ColorMultiplier * num88;
		SunLightColor = GClass5.MulRGB(Day.LightColor.Evaluate(time), multiplier);
		float multiplier2 = Night.ColorMultiplier * num89;
		MoonLightColor = GClass5.MulRGB(Night.LightColor.Evaluate(time), multiplier2);
		float multiplier3 = Day.ColorMultiplier * num88;
		SunRayColor = GClass5.MulRGB(Day.RayColor.Evaluate(time), multiplier3);
		float multiplier4 = 0.25f * Night.ColorMultiplier * num89;
		MoonRayColor = GClass5.MulRGB(Night.RayColor.Evaluate(time), multiplier4);
		float colorMultiplier = Day.ColorMultiplier;
		SunSkyColor = GClass5.MulRGB(Day.SkyColor.Evaluate(time), colorMultiplier);
		float multiplier5 = 0.25f * Night.ColorMultiplier;
		MoonSkyColor = GClass5.MulRGB(Night.SkyColor.Evaluate(time), multiplier5);
		float colorMultiplier2 = Day.ColorMultiplier;
		SunMeshColor = GClass5.MulRGB(Sun.MeshColor.Evaluate(time), colorMultiplier2);
		float colorMultiplier3 = Night.ColorMultiplier;
		MoonMeshColor = GClass5.MulRGB(Moon.MeshColor.Evaluate(time), colorMultiplier3);
		float colorMultiplier4 = Day.ColorMultiplier;
		Color b = GClass5.MulRGB(Day.AmbientColor.Evaluate(time), colorMultiplier4);
		float multiplier6 = 0.25f * Night.ColorMultiplier;
		Color a = GClass5.MulRGB(Night.AmbientColor.Evaluate(time), multiplier6);
		GroundColor = Color.Lerp(a, b, LerpValue);
		float colorMultiplier5 = Day.ColorMultiplier;
		Color b2 = GClass5.MulRGB(Day.AmbientColor.Evaluate(time), colorMultiplier5);
		float multiplier7 = 0.5f * Night.ColorMultiplier;
		Color a2 = GClass5.MulRGB(Night.AmbientColor.Evaluate(time), multiplier7);
		AmbientColor = Color.Lerp(a2, b2, LerpValue);
		float multiplier8 = 0.25f * Night.ColorMultiplier * num87;
		MoonHaloColor = GClass5.MulRGB(Moon.HaloColor.Evaluate(time), multiplier8);
		float shadowStrength;
		float intensity;
		if (LerpValue > 0.1f)
		{
			IsDay = true;
			IsNight = false;
			shadowStrength = Day.ShadowStrength;
			intensity = Mathf.Lerp(0f, Day.LightIntensity, num88);
			_ = SunLightColor;
			RayColor = SunRayColor;
		}
		else
		{
			IsDay = false;
			IsNight = true;
			shadowStrength = Night.ShadowStrength;
			intensity = Mathf.Lerp(0f, Night.LightIntensity, num89);
			_ = MoonLightColor;
			RayColor = MoonRayColor;
		}
		Components.LightSource.intensity = intensity;
		Components.LightSource.shadowStrength = shadowStrength;
		if (Singleton<SharedGameSettingsClass>.Instantiated && (int)Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.ShadowsQuality >= 3)
		{
			Vector3 vector = (IsNight ? OrbitalToLocal(Mathf.Min(num82, (1f - Light.MinimumHeight) * MathF.PI / 2f), num83) : OrbitalToLocal(Mathf.Min(num41, (1f - Light.MinimumHeight) * MathF.PI / 2f), num42));
			Vector3 vector2 = (IsNight ? OrbitalToLocal(Mathf.Min(float_14, (1f - Light.MinimumHeight) * MathF.PI / 2f), float_15) : OrbitalToLocal(Mathf.Min(float_12, (1f - Light.MinimumHeight) * MathF.PI / 2f), float_13));
			if (float_12 != num41 || float_13 != num42 || float_14 != num82 || float_15 != num83)
			{
				float_12 = num41;
				float_13 = num42;
				float_14 = num82;
				float_15 = num83;
				quaternion_1 = Quaternion.LookRotation(Components.DomeTransform.position - vector);
				quaternion_0 = Quaternion.LookRotation(Components.DomeTransform.position - vector2);
				rotationSpeed = 1f / Mathf.Max(float_5, 0.001f);
				float_5 = 0f;
			}
		}
		else
		{
			float_5 = 0f;
			Vector3 vector3 = (IsNight ? OrbitalToLocal(Mathf.Min(num82, (1f - Light.MinimumHeight) * MathF.PI / 2f), num83) : OrbitalToLocal(Mathf.Min(num41, (1f - Light.MinimumHeight) * MathF.PI / 2f), num42));
			Components.LightTransform.rotation = Quaternion.LookRotation(Components.DomeTransform.position - vector3);
			if (Quaternion.Angle(quaternion_1, Components.LightTransform.rotation) > 1f)
			{
				quaternion_0 = quaternion_1;
				quaternion_1 = Components.LightTransform.rotation;
				float_8 = Time.time - float_9;
				float_9 = Time.time;
			}
		}
		SunDirection = -Components.SunTransform.forward;
		LocalSunDirection = Components.DomeTransform.InverseTransformDirection(SunDirection);
		MoonDirection = -Components.MoonTransform.forward;
		LocalMoonDirection = Components.DomeTransform.InverseTransformDirection(MoonDirection);
		LightDirection = Vector3.Lerp(MoonDirection, SunDirection, LerpValue * LerpValue);
		LocalLightDirection = Components.DomeTransform.InverseTransformDirection(LightDirection);
	}
}
