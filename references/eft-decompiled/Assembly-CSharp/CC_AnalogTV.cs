using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Analog TV")]
public class CC_AnalogTV : CC_Base
{
	public bool autoPhase = true;

	public float phase = 0.5f;

	public bool grayscale;

	[Range(0f, 1f)]
	public float noiseIntensity = 0.5f;

	[Range(0f, 10f)]
	public float scanlinesIntensity = 2f;

	[Range(0f, 4096f)]
	public float scanlinesCount = 768f;

	public float scanlinesOffset;

	[Range(-2f, 2f)]
	public float distortion = 0.2f;

	[Range(-2f, 2f)]
	public float cubicDistortion = 0.6f;

	[Range(0.01f, 2f)]
	public float scale = 0.8f;

	private static readonly int int_0 = Shader.PropertyToID("_Phase");

	private static readonly int int_1 = Shader.PropertyToID("_NoiseIntensity");

	private static readonly int int_2 = Shader.PropertyToID("_ScanlinesIntensity");

	private static readonly int int_3 = Shader.PropertyToID("_ScanlinesCount");

	private static readonly int int_4 = Shader.PropertyToID("_ScanlinesOffset");

	private static readonly int int_5 = Shader.PropertyToID("_Distortion");

	private static readonly int int_6 = Shader.PropertyToID("_CubicDistortion");

	private static readonly int int_7 = Shader.PropertyToID("_Scale");

	public virtual void Update()
	{
		if (autoPhase)
		{
			phase += Time.deltaTime * 0.25f;
		}
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat(int_0, phase);
		base.material.SetFloat(int_1, noiseIntensity);
		base.material.SetFloat(int_2, scanlinesIntensity);
		base.material.SetFloat(int_3, (int)scanlinesCount);
		base.material.SetFloat(int_4, scanlinesOffset);
		base.material.SetFloat(int_5, distortion);
		base.material.SetFloat(int_6, cubicDistortion);
		base.material.SetFloat(int_7, scale);
		DebugGraphics.Blit(source, destination, base.material, grayscale ? 1 : 0);
	}
}
