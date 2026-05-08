using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Kuwahara")]
public class CC_Kuwahara : CC_Base
{
	[Range(1f, 4f)]
	public int radius = 3;

	protected Camera m_Camera;

	private static readonly int int_0 = Shader.PropertyToID("_TexelSize");

	public override void Start()
	{
		base.Start();
		m_Camera = GetComponent<Camera>();
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		radius = Mathf.Clamp(radius, 1, 4);
		base.material.SetVector(int_0, new Vector2(1f / (float)m_Camera.pixelWidth, 1f / (float)m_Camera.pixelHeight));
		DebugGraphics.Blit(source, destination, base.material, radius - 1);
	}
}
