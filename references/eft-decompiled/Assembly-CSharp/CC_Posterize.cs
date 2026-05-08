using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Posterize")]
public class CC_Posterize : CC_Base
{
	[Range(2f, 255f)]
	public int levels = 4;

	private static readonly int int_0 = Shader.PropertyToID("_Levels");

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat(int_0, levels);
		Graphics.Blit(source, destination, base.material);
	}
}
