using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Hue Focus")]
public class CC_HueFocus : CC_Base
{
	[Range(0f, 360f)]
	public float hue;

	[Range(1f, 180f)]
	public float range = 30f;

	[Range(0f, 1f)]
	public float boost = 0.5f;

	[Range(0f, 1f)]
	public float amount = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_Range");

	private static readonly int int_1 = Shader.PropertyToID("_Params");

	[ImageEffectTransformsToLDR]
	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		float num = hue / 360f;
		float num2 = range / 180f;
		base.material.SetVector(int_0, new Vector2(num - num2, num + num2));
		base.material.SetVector(int_1, new Vector3(num, boost + 1f, amount));
		Graphics.Blit(source, destination, base.material);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
