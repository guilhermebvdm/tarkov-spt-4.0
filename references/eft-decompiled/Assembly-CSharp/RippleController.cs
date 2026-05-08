using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class RippleController : MonoBehaviour
{
	[Serializable]
	public class Cycle
	{
		public float CucleSpeed = 0.4f;

		[GAttribute6(0.0001f, 1f, -1f)]
		public Vector2 CucleLengthRelationMinMax = new Vector2(0.01f, 1f);

		[GAttribute6(0f, 1f, -1f)]
		public Vector2 IntensityMinMax = new Vector2(0f, 0.1f);

		[NonSerialized]
		public float Time;

		[NonSerialized]
		public float LastCucleLengthRelation = 1f;

		public Vector4 Update(float dt, float intensity)
		{
			Time += dt * CucleSpeed;
			float num = 1f / Mathf.Lerp(CucleLengthRelationMinMax.x, CucleLengthRelationMinMax.y, intensity);
			Time = smethod_0(Time, LastCucleLengthRelation, num);
			LastCucleLengthRelation = num;
			return new Vector4(Time, LastCucleLengthRelation, Mathf.InverseLerp(IntensityMinMax.x, IntensityMinMax.y, intensity));
		}

		public static float smethod_0(float currentTime, float lastCucleLengthRelation, float newCucleLengthRelation)
		{
			return currentTime * (newCucleLengthRelation / lastCucleLengthRelation);
		}
	}

	private static readonly int int_0 = Shader.PropertyToID("_WaterRippleCycle");

	private static readonly int int_1 = Shader.PropertyToID("_WaterRippleConfig");

	private static readonly int int_2 = Shader.PropertyToID("_EnvironmentRippleCycle");

	private static readonly int int_3 = Shader.PropertyToID("_RippleTexture");

	private static readonly int int_4 = Shader.PropertyToID("_WetDropSpecular");

	private static readonly int int_5 = Shader.PropertyToID("_WetDropGlossness");

	private static readonly int int_6 = Shader.PropertyToID("_WetDropAlbedo");

	private static readonly int int_7 = Shader.PropertyToID("_WetDropNormal");

	private static readonly int int_8 = Shader.PropertyToID("_RippleFakeLightIntensity");

	private static readonly int int_9 = Shader.PropertyToID("_RainIntensityRippleScale");

	private static readonly int int_10 = Shader.PropertyToID("_SunDirection");

	[CompilerGenerated]
	private float float_0;

	public Texture2D RippleTexture;

	[Header("Water Ripples")]
	public Cycle WaterCucle;

	public int RippleWaveCount = 3;

	public float RippleWaveFreq = 3f;

	[Header("Environment Ripples")]
	public Cycle EnvironmentCucle;

	[Range(0f, 1f)]
	public float WetDropsSpecular = 0.576f;

	[Range(0f, 1f)]
	public float WetDropsGlossness = 0.553f;

	[Range(0f, 1f)]
	public float WetDropsAlbedo = 0.248f;

	[Range(0f, 1f)]
	public float WetDropsNormal = 1f;

	[Range(0f, 3f)]
	public float RippleFakeLightIntensity = 0.4f;

	[Range(0f, 5f)]
	public float RainIntensityRippleScale = 2f;

	[SerializeField]
	private bool _forceTriplanar = true;

	[SerializeField]
	private bool _useFakeRippleLight = true;

	public float Intensity
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

	public void Start()
	{
		if (_forceTriplanar)
		{
			Shader.EnableKeyword("TRIPLANAR");
		}
		else
		{
			Shader.DisableKeyword("TRIPLANAR");
		}
		if (_useFakeRippleLight)
		{
			Shader.EnableKeyword("FAKERIPPLELIGHT");
		}
		else
		{
			Shader.DisableKeyword("FAKERIPPLELIGHT");
		}
	}

	public void OnDestroy()
	{
		Shader.DisableKeyword("TRIPLANAR");
		Shader.DisableKeyword("FAKERIPPLELIGHT");
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		Vector4 waterCycle = WaterCucle.Update(deltaTime, Intensity);
		float dt = (RainController.IsCameraUnderRain ? deltaTime : 0f);
		Vector4 environmentCycle = EnvironmentCucle.Update(dt, Intensity);
		method_0(waterCycle, environmentCycle);
	}

	public void method_0(Vector4 waterCycle, Vector4 environmentCycle)
	{
		Shader.SetGlobalVector(int_0, waterCycle);
		Shader.SetGlobalVector(int_1, new Vector4(RippleWaveCount, RippleWaveFreq));
		Shader.SetGlobalVector(int_2, environmentCycle);
		Shader.SetGlobalTexture(int_3, RippleTexture);
		Shader.SetGlobalFloat(int_4, WetDropsSpecular);
		Shader.SetGlobalFloat(int_5, WetDropsGlossness);
		Shader.SetGlobalFloat(int_6, WetDropsAlbedo);
		Shader.SetGlobalFloat(int_7, WetDropsNormal);
		Shader.SetGlobalFloat(int_8, RippleFakeLightIntensity);
		Shader.SetGlobalFloat(int_9, RainIntensityRippleScale);
		if (GClass4.IsAvailable)
		{
			Shader.SetGlobalVector(int_10, -GClass4.Instance.SunDirection);
		}
	}
}
