using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Lighten")]
public class LightenEffect : ImageEffectBase
{
	public float desaturateAmount;

	public Texture textureRamp;

	public float rampOffsetR;

	public float rampOffsetG;

	public float rampOffsetB;

	private static readonly int _rampTex = Shader.PropertyToID("_RampTex");

	private static readonly int _desat = Shader.PropertyToID("_Desat");

	private static readonly int _rampOffset = Shader.PropertyToID("_RampOffset");

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetTexture(_rampTex, textureRamp);
		base.material.SetFloat(_desat, desaturateAmount);
		base.material.SetVector(_rampOffset, new Vector4(rampOffsetR, rampOffsetG, rampOffsetB, 0f));
		Graphics.Blit(source, destination, base.material);
	}
}
