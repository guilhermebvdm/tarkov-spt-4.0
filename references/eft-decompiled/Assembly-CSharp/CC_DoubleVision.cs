using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Double Vision")]
public class CC_DoubleVision : CC_Base
{
	public Vector2 displace = new Vector2(0.7f, 0f);

	[Range(0f, 1f)]
	public float amount = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_Displace");

	private static readonly int int_1 = Shader.PropertyToID("_Amount");

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
		base.material.SetVector(int_0, new Vector2(displace.x / (float)Screen.width, displace.y / (float)Screen.height));
		base.material.SetFloat(int_1, amount);
		Graphics.Blit(source, destination, base.material);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
