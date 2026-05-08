using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Brightness, Contrast, Gamma")]
public class CC_BrightnessContrastGamma : CC_Base
{
	[Range(-100f, 100f)]
	public float brightness;

	[Range(-100f, 100f)]
	public float contrast;

	[Range(0f, 1f)]
	public float redCoeff = 0.5f;

	[Range(0f, 1f)]
	public float greenCoeff = 0.5f;

	[Range(0f, 1f)]
	public float blueCoeff = 0.5f;

	[Range(0.1f, 9.9f)]
	public float gamma = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_BCG");

	private static readonly int int_1 = Shader.PropertyToID("_Coeffs");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		OnRenderImageImpl(source, destination);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}

	public virtual void OnRenderImageImpl(RenderTexture source, RenderTexture destination)
	{
		if (brightness == 0f && contrast == 0f && gamma == 1f)
		{
			Graphics.Blit(source, destination);
			return;
		}
		base.material.SetVector(int_0, new Vector4((brightness + 100f) * 0.01f, (contrast + 100f) * 0.01f, 1f / gamma));
		base.material.SetVector(int_1, new Vector4(redCoeff, greenCoeff, blueCoeff, 1f));
		Graphics.Blit(source, destination, base.material);
	}
}
