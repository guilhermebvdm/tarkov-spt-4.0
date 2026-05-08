using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Lookup Filter (Color Grading)")]
public class CC_LookupFilter : CC_Base
{
	public Texture lookupTexture;

	[Range(0f, 1f)]
	public float amount = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_LookupTex");

	private static readonly int int_1 = Shader.PropertyToID("_Amount");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (lookupTexture == null)
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		base.material.SetTexture(int_0, lookupTexture);
		base.material.SetFloat(int_1, amount);
		DebugGraphics.Blit(source, destination, base.material, CC_Base.IsLinear() ? 1 : 0);
	}
}
