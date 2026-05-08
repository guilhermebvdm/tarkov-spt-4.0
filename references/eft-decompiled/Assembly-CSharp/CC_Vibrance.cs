using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Vibrance")]
public class CC_Vibrance : CC_Base
{
	[Range(-100f, 100f)]
	public float amount;

	[Range(-5f, 5f)]
	public float redChannel = 1f;

	[Range(-5f, 5f)]
	public float greenChannel = 1f;

	[Range(-5f, 5f)]
	public float blueChannel = 1f;

	public bool advanced;

	private static readonly int int_0 = Shader.PropertyToID("_Amount");

	private static readonly int int_1 = Shader.PropertyToID("_Channels");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amount == 0f)
		{
			GClass860.BlitOrCopy(source, destination);
		}
		else if (advanced)
		{
			base.material.SetFloat(int_0, amount * 0.01f);
			base.material.SetVector(int_1, new Vector3(redChannel, greenChannel, blueChannel));
			DebugGraphics.Blit(source, destination, base.material, 1);
		}
		else
		{
			base.material.SetFloat(int_0, amount * 0.02f);
			DebugGraphics.Blit(source, destination, base.material, 0);
		}
	}
}
