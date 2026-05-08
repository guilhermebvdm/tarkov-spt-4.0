using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/RGB Split")]
public class CC_RGBSplit : CC_Base
{
	public float amount;

	public float angle;

	private static readonly int int_0 = Shader.PropertyToID("_RGBShiftAmount");

	private static readonly int int_1 = Shader.PropertyToID("_RGBShiftAngleCos");

	private static readonly int int_2 = Shader.PropertyToID("_RGBShiftAngleSin");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amount == 0f)
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		base.material.SetFloat(int_0, amount * 0.001f);
		base.material.SetFloat(int_1, Mathf.Cos(angle));
		base.material.SetFloat(int_2, Mathf.Sin(angle));
		Graphics.Blit(source, destination, base.material);
	}
}
