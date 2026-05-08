using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Frost")]
public class CC_Frost : CC_Base
{
	[Range(0f, 16f)]
	public float scale = 1.2f;

	[Range(-100f, 100f)]
	public float sharpness = 40f;

	[Range(0f, 100f)]
	public float darkness = 35f;

	public bool enableVignette = true;

	private static readonly int int_0 = Shader.PropertyToID("_Scale");

	private static readonly int int_1 = Shader.PropertyToID("_Sharpness");

	private static readonly int int_2 = Shader.PropertyToID("_Darkness");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (scale == 0f)
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		base.material.SetFloat(int_0, scale);
		base.material.SetFloat(int_1, sharpness * 0.01f);
		base.material.SetFloat(int_2, darkness * 0.02f);
		DebugGraphics.Blit(source, destination, base.material, enableVignette ? 1 : 0);
	}
}
