using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Threshold")]
public class CC_Threshold : CC_Base
{
	[Range(1f, 255f)]
	public float threshold = 128f;

	[Range(0f, 128f)]
	public float noiseRange = 48f;

	public bool useNoise;

	private static readonly int int_0 = Shader.PropertyToID("_Threshold");

	private static readonly int int_1 = Shader.PropertyToID("_Range");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat(int_0, threshold / 255f);
		base.material.SetFloat(int_1, noiseRange / 255f);
		DebugGraphics.Blit(source, destination, base.material, useNoise ? 1 : 0);
	}
}
