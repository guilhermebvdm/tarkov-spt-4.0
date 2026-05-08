using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Fast Vignette")]
public class CC_FastVignette : CC_Base
{
	public Vector2 center = new Vector2(0.5f, 0.5f);

	[Range(-100f, 100f)]
	public float sharpness = 10f;

	[Range(0f, 100f)]
	public float darkness = 30f;

	public bool desaturate;

	private static readonly int int_0 = Shader.PropertyToID("_Data");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		base.material.SetVector(int_0, new Vector4(center.x, center.y, sharpness * 0.01f, darkness * 0.02f));
		DebugGraphics.Blit(source, destination, base.material, desaturate ? 1 : 0);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
