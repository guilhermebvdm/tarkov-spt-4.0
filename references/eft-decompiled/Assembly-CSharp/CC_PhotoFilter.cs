using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Photo Filter")]
public class CC_PhotoFilter : CC_Base
{
	public Color color = new Color(1f, 0.5f, 0.2f, 1f);

	[Range(0f, 1f)]
	public float density = 0.35f;

	private static readonly int int_0 = Shader.PropertyToID("_RGB");

	private static readonly int int_1 = Shader.PropertyToID("_Density");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (density == 0f)
		{
			GClass860.BlitOrCopy(source, destination);
			return;
		}
		base.material.SetColor(int_0, color);
		base.material.SetFloat(int_1, density);
		Graphics.Blit(source, destination, base.material);
	}
}
