using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Technicolor")]
public class CC_Technicolor : CC_Base
{
	[Range(0f, 8f)]
	public float exposure = 4f;

	public Vector3 balance = new Vector3(0.25f, 0.25f, 0.25f);

	[Range(0f, 1f)]
	public float amount = 0.5f;

	private static readonly int int_0 = Shader.PropertyToID("_Exposure");

	private static readonly int int_1 = Shader.PropertyToID("_Balance");

	private static readonly int int_2 = Shader.PropertyToID("_Amount");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		base.material.SetFloat(int_0, 8f - exposure);
		base.material.SetVector(int_1, Vector3.one - balance);
		base.material.SetFloat(int_2, amount);
		Graphics.Blit(source, destination, base.material);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
