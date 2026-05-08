using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Channel Clamper")]
public class CC_ChannelClamper : CC_Base
{
	public Vector2 red = new Vector2(0f, 1f);

	public Vector2 green = new Vector2(0f, 1f);

	public Vector2 blue = new Vector2(0f, 1f);

	private static readonly int int_0 = Shader.PropertyToID("_RedClamp");

	private static readonly int int_1 = Shader.PropertyToID("_GreenClamp");

	private static readonly int int_2 = Shader.PropertyToID("_BlueClamp");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetVector(int_0, red);
		base.material.SetVector(int_1, green);
		base.material.SetVector(int_2, blue);
		Graphics.Blit(source, destination, base.material);
	}
}
