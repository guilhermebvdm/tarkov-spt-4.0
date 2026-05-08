using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Gradient Ramp")]
public class CC_GradientRamp : CC_Base
{
	public Texture rampTexture;

	[Range(0f, 1f)]
	public float amount = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_RampTex");

	private static readonly int int_1 = Shader.PropertyToID("_Amount");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!(rampTexture == null) && amount != 0f)
		{
			base.material.SetTexture(int_0, rampTexture);
			base.material.SetFloat(int_1, amount);
			Graphics.Blit(source, destination, base.material);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}
}
