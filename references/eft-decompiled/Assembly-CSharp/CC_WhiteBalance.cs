using System;
using EFT.BlitDebug;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/White Balance")]
public class CC_WhiteBalance : CC_Base
{
	public Color white = new Color(0.5f, 0.5f, 0.5f);

	public int mode = 1;

	private static readonly int int_0 = Shader.PropertyToID("_White");

	public virtual void Reset()
	{
		white = (CC_Base.IsLinear() ? new Color(MathF.PI * 59f / 254f, MathF.PI * 59f / 254f, MathF.PI * 59f / 254f) : new Color(0.5f, 0.5f, 0.5f));
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetColor(int_0, white);
		DebugGraphics.Blit(source, destination, base.material, mode);
	}
}
