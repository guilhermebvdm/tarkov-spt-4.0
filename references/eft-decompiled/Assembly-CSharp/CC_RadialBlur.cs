using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Radial Blur")]
public class CC_RadialBlur : CC_Base
{
	[Range(0f, 1f)]
	public float amount = 0.1f;

	[Range(2f, 24f)]
	public int samples = 10;

	public Vector2 center = new Vector2(0.5f, 0.5f);

	public int quality = 1;

	private static readonly int int_0 = Shader.PropertyToID("_Amount");

	private static readonly int int_1 = Shader.PropertyToID("_Center");

	private static readonly int int_2 = Shader.PropertyToID("_Samples");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		if (amount == 0f)
		{
			GClass860.BlitOrCopy(source, destination);
			if (_ssaaPropagator != null)
			{
				_ssaaPropagator.ReleaseSourceDestination(source, destination);
			}
			return;
		}
		base.material.SetFloat(int_0, amount);
		base.material.SetVector(int_1, center);
		base.material.SetFloat(int_2, samples);
		DebugGraphics.Blit(source, destination, base.material, quality);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
