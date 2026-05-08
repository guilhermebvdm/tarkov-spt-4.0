using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Sharpen")]
public class CC_Sharpen : CC_Base
{
	[Range(0f, 5f)]
	public float strength = 0.6f;

	[Range(0f, 1f)]
	public float clamp = 0.05f;

	public bool DoDesaturate;

	private static readonly int int_0 = Shader.PropertyToID("_PX");

	private static readonly int int_1 = Shader.PropertyToID("_PY");

	private static readonly int int_2 = Shader.PropertyToID("_Strength");

	private static readonly int int_3 = Shader.PropertyToID("_Clamp");

	[CompilerGenerated]
	private DesaturateEffect desaturateEffect_0;

	[CompilerGenerated]
	private bool bool_0;

	public Texture textureRamp;

	public float rampOffsetR;

	public float rampOffsetG;

	public float rampOffsetB;

	public float WeatherDesaturate;

	public float HealthDesaturate;

	public float MaskDesaturate;

	public Vector2 MinMaxRadius = new Vector2(0.325f, 1f);

	[Range(0f, 1f)]
	public float Radius = 1f;

	[Range(0f, 1f)]
	public float RadiusFalloff = 0.425f;

	private static readonly int int_4 = Shader.PropertyToID("_NightVisionRatio");

	private static readonly int int_5 = Shader.PropertyToID("_RampTex");

	private static readonly int int_6 = Shader.PropertyToID("_DesatWeather");

	private static readonly int int_7 = Shader.PropertyToID("_DesatHealth");

	private static readonly int int_8 = Shader.PropertyToID("_DesatMask");

	private static readonly int int_9 = Shader.PropertyToID("_RampOffset");

	private static readonly int int_10 = Shader.PropertyToID("_Radius");

	private static readonly int int_11 = Shader.PropertyToID("_RadiusFalloff");

	private readonly string string_0 = "CC_SHARPEN_DESATURATE";

	public DesaturateEffect DesaturateEffectSettingsProvider
	{
		[CompilerGenerated]
		get
		{
			return desaturateEffect_0;
		}
		[CompilerGenerated]
		set
		{
			desaturateEffect_0 = value;
		}
	}

	public bool FailedToGetDesaturate
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

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		if (strength == 0f && !DoDesaturate)
		{
			GClass860.BlitOrCopy(source, destination);
			if (_ssaaPropagator != null)
			{
				_ssaaPropagator.ReleaseSourceDestination(source, destination);
			}
			return;
		}
		if (!FailedToGetDesaturate && DesaturateEffectSettingsProvider == null)
		{
			DesaturateEffectSettingsProvider = GetComponent<DesaturateEffect>();
			if (DesaturateEffectSettingsProvider == null)
			{
				FailedToGetDesaturate = true;
			}
		}
		if (!FailedToGetDesaturate)
		{
			if (DoDesaturate)
			{
				DesaturateEffectSettingsProvider.enabled = false;
			}
			else
			{
				DesaturateEffectSettingsProvider.enabled = true;
			}
		}
		if (DoDesaturate)
		{
			if (!FailedToGetDesaturate)
			{
				base.material.SetTexture(int_5, DesaturateEffectSettingsProvider.textureRamp);
				base.material.SetFloat(int_6, Mathf.Clamp01(DesaturateEffectSettingsProvider.WeatherDesaturate));
				base.material.SetFloat(int_7, Mathf.Clamp01(DesaturateEffectSettingsProvider.HealthDesaturate));
				base.material.SetVector(int_9, new Vector4(DesaturateEffectSettingsProvider.rampOffsetR, DesaturateEffectSettingsProvider.rampOffsetG, DesaturateEffectSettingsProvider.rampOffsetB, 0f));
				base.material.SetFloat(int_8, Mathf.Clamp01(MaskDesaturate));
				base.material.SetFloat(int_10, DesaturateEffectSettingsProvider.Radius);
				base.material.SetFloat(int_11, DesaturateEffectSettingsProvider.RadiusFalloff);
			}
			else
			{
				base.material.SetTexture(int_5, textureRamp);
				base.material.SetFloat(int_6, Mathf.Clamp01(WeatherDesaturate));
				base.material.SetFloat(int_7, Mathf.Clamp01(HealthDesaturate));
				base.material.SetVector(int_9, new Vector4(rampOffsetR, rampOffsetG, rampOffsetB, 0f));
				base.material.SetFloat(int_8, Mathf.Clamp01(MaskDesaturate));
				base.material.SetFloat(int_10, Radius);
				base.material.SetFloat(int_11, RadiusFalloff);
			}
			CameraClass instance = CameraClass.Instance;
			float value = 1f;
			if (instance != null && instance.NightVision != null)
			{
				value = ((!instance.NightVision.On) ? 1 : 0);
			}
			base.material.SetFloat(int_4, value);
			Shader.EnableKeyword(string_0);
		}
		base.material.SetFloat(int_0, 1f / (float)Screen.width);
		base.material.SetFloat(int_1, 1f / (float)Screen.height);
		base.material.SetFloat(int_2, strength);
		base.material.SetFloat(int_3, clamp);
		Graphics.Blit(source, destination, base.material);
		if (DoDesaturate)
		{
			Shader.DisableKeyword(string_0);
		}
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
