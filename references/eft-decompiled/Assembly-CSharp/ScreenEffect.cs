using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Screen")]
public class ScreenEffect : ImageEffectBase
{
	public float desaturateAmount;

	public Texture textureRamp;

	public float rampOffsetR;

	public float rampOffsetG;

	public float rampOffsetB;

	private static readonly int int_0 = Shader.PropertyToID("_RampTex");

	private static readonly int int_1 = Shader.PropertyToID("_Desat");

	private static readonly int int_2 = Shader.PropertyToID("_RampOffset");

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetTexture(int_0, textureRamp);
		base.material.SetFloat(int_1, desaturateAmount);
		base.material.SetVector(int_2, new Vector4(rampOffsetR, rampOffsetG, rampOffsetB, 0f));
		Graphics.Blit(source, destination, base.material);
	}
}
