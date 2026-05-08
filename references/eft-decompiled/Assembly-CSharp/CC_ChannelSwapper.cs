using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Channel Swapper")]
public class CC_ChannelSwapper : CC_Base
{
	public int red;

	public int green = 1;

	public int blue = 2;

	private static Vector4[] vector4_0 = new Vector4[3]
	{
		new Vector4(1f, 0f, 0f, 0f),
		new Vector4(0f, 1f, 0f, 0f),
		new Vector4(0f, 0f, 1f, 0f)
	};

	private static readonly int int_0 = Shader.PropertyToID("_Red");

	private static readonly int int_1 = Shader.PropertyToID("_Green");

	private static readonly int int_2 = Shader.PropertyToID("_Blue");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetVector(int_0, vector4_0[red]);
		base.material.SetVector(int_1, vector4_0[green]);
		base.material.SetVector(int_2, vector4_0[blue]);
		Graphics.Blit(source, destination, base.material);
	}
}
