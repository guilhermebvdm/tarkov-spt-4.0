using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Bleach Bypass")]
public class CC_BleachBypass : CC_Base
{
	[Range(0f, 1f)]
	public float amount = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_Amount");

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
		if (amount == 0f)
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		base.material.SetFloat(int_0, amount);
		Graphics.Blit(source, destination, base.material);
	}
}
