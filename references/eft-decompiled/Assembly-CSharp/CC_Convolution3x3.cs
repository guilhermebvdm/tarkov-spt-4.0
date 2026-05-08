using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Convolution Matrix 3x3")]
public class CC_Convolution3x3 : CC_Base
{
	public Vector3 kernelTop = Vector3.zero;

	public Vector3 kernelMiddle = Vector3.up;

	public Vector3 kernelBottom = Vector3.zero;

	public float divisor = 1f;

	[Range(0f, 1f)]
	public float amount = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_PX");

	private static readonly int int_1 = Shader.PropertyToID("_PY");

	private static readonly int int_2 = Shader.PropertyToID("_Amount");

	private static readonly int int_3 = Shader.PropertyToID("_KernelT");

	private static readonly int int_4 = Shader.PropertyToID("_KernelM");

	private static readonly int int_5 = Shader.PropertyToID("_KernelB");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat(int_0, 1f / (float)Screen.width);
		base.material.SetFloat(int_1, 1f / (float)Screen.height);
		base.material.SetFloat(int_2, amount);
		base.material.SetVector(int_3, kernelTop / divisor);
		base.material.SetVector(int_4, kernelMiddle / divisor);
		base.material.SetVector(int_5, kernelBottom / divisor);
		Graphics.Blit(source, destination, base.material);
	}
}
