using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Desaturate")]
public class DesaturateEffect : ImageEffectBase
{
	public Texture textureRamp;

	public float rampOffsetR;

	public float rampOffsetG;

	public float rampOffsetB;

	[FormerlySerializedAs("desaturateAmount")]
	public float WeatherDesaturate;

	public float HealthDesaturate;

	public float MaskDesaturate;

	public Vector2 MinMaxRadius = new Vector2(0.325f, 1f);

	[Range(0f, 1f)]
	public float Radius = 1f;

	[Range(0f, 1f)]
	public float RadiusFalloff = 0.425f;

	private static readonly int _nightVisionRatio = Shader.PropertyToID("_NightVisionRatio");

	private static readonly int _rampTex = Shader.PropertyToID("_RampTex");

	private static readonly int _desatWeather = Shader.PropertyToID("_DesatWeather");

	private static readonly int _desatHealth = Shader.PropertyToID("_DesatHealth");

	private static readonly int _desatMask = Shader.PropertyToID("_DesatMask");

	private static readonly int _rampOffset = Shader.PropertyToID("_RampOffset");

	private static readonly int _radius = Shader.PropertyToID("_Radius");

	private static readonly int _radiusFalloff = Shader.PropertyToID("_RadiusFalloff");

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		base.material.SetTexture(_rampTex, textureRamp);
		base.material.SetFloat(_desatWeather, Mathf.Clamp01(WeatherDesaturate));
		base.material.SetFloat(_desatHealth, Mathf.Clamp01(HealthDesaturate));
		base.material.SetVector(_rampOffset, new Vector4(rampOffsetR, rampOffsetG, rampOffsetB, 0f));
		base.material.SetFloat(_desatMask, Mathf.Clamp01(MaskDesaturate));
		base.material.SetFloat(_radius, Radius);
		base.material.SetFloat(_radiusFalloff, RadiusFalloff);
		CameraClass instance = CameraClass.Instance;
		float value = 1f;
		if (instance != null && instance.NightVision != null)
		{
			value = ((!instance.NightVision.On) ? 1 : 0);
		}
		base.material.SetFloat(_nightVisionRatio, value);
		Graphics.Blit(source, destination, base.material);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
