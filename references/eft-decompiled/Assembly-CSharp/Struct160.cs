using System;
using UnityEngine;
using UnityEngine.Rendering;

public struct Struct160
{
	[NonSerialized]
	public RenderTexture RenderTexture_0;

	[NonSerialized]
	public RenderTexture RenderTexture_1;

	[NonSerialized]
	public RenderTargetIdentifier[] RenderTargetIdentifier_0;

	public static Struct160 smethod_0(int width, int height, bool allowHDR)
	{
		RenderTexture renderTexture_ = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
		{
			name = "SnowShadowMap"
		};
		RenderTexture renderTexture_2 = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat)
		{
			name = "DisableSnowMask",
			enableRandomWrite = true
		};
		return new Struct160
		{
			RenderTexture_0 = renderTexture_,
			RenderTexture_1 = renderTexture_2,
			RenderTargetIdentifier_0 = new RenderTargetIdentifier[5]
			{
				BuiltinRenderTextureType.GBuffer0,
				BuiltinRenderTextureType.GBuffer1,
				BuiltinRenderTextureType.GBuffer2,
				allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3,
				SnowWetRenderer._wetOffMask
			}
		};
	}

	public static void smethod_1(in Struct160 rendererData)
	{
		rendererData.RenderTexture_0.Release();
		GClass972.SafeDestroy(rendererData.RenderTexture_0);
		rendererData.RenderTexture_1.Release();
		GClass972.SafeDestroy(rendererData.RenderTexture_1);
	}
}
