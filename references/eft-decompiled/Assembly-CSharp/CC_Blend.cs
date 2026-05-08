using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Blend")]
public class CC_Blend : CC_Base
{
	public Texture texture;

	[Range(0f, 1f)]
	public float amount = 1f;

	public int mode;

	private static readonly int int_0 = Shader.PropertyToID("_OverlayTex");

	private static readonly int int_1 = Shader.PropertyToID("_Amount");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		if (!(texture == null) && amount != 0f)
		{
			base.material.SetTexture(int_0, texture);
			base.material.SetFloat(int_1, amount);
			DebugGraphics.Blit(source, destination, base.material, mode);
			if (_ssaaPropagator != null)
			{
				_ssaaPropagator.ReleaseSourceDestination(source, destination);
			}
		}
		else
		{
			GClass860.BlitOrCopy(source, destination);
			if (_ssaaPropagator != null)
			{
				_ssaaPropagator.ReleaseSourceDestination(source, destination);
			}
		}
	}
}
