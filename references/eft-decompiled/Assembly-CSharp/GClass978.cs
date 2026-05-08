using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass978
{
	[NonSerialized]
	public ThermalVision ThermalVision_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public RenderTexture RenderTexture_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("_MainTex");

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("_AccumOrig");

	public bool IsNeedClear
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public GClass978(ThermalVision thermalVision)
	{
		ThermalVision_0 = thermalVision;
	}

	public void UpdateTexture(CommandBuffer commandBuffer, Camera camera, SSAA ssaa, RenderTargetIdentifier cameraTarget)
	{
		if (!(ThermalVision_0 == null))
		{
			Material material = method_0();
			int num = (ssaa ? ssaa.GetInputWidth() : camera.pixelWidth);
			int num2 = (ssaa ? ssaa.GetInputHeight() : camera.pixelHeight);
			if (RenderTexture_0 == null || RenderTexture_0.width != num || RenderTexture_0.height != num2)
			{
				UnityEngine.Object.DestroyImmediate(RenderTexture_0);
				RenderTexture_0 = new RenderTexture(num, num2, 0);
				RenderTexture_0.name = "MotionBlur RT";
				RenderTexture_0.hideFlags = HideFlags.HideAndDontSave;
				commandBuffer.Blit(cameraTarget, RenderTexture_0);
			}
			if (IsNeedClear)
			{
				commandBuffer.Blit(cameraTarget, RenderTexture_0);
				IsNeedClear = false;
			}
			if (ThermalVision_0.MotionBlurUtilities.ExtraBlur)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(num / 4, num2 / 4, 0);
				RenderTexture_0.MarkRestoreExpected();
				Graphics.Blit(RenderTexture_0, temporary);
				Graphics.Blit(temporary, RenderTexture_0);
				RenderTexture.ReleaseTemporary(temporary);
			}
			ThermalVision_0.MotionBlurUtilities.BlurAmount = Mathf.Clamp(ThermalVision_0.MotionBlurUtilities.BlurAmount, 0f, 0.92f);
			material.SetTexture(Int_0, RenderTexture_0);
			material.SetFloat(Int_1, 1f - ThermalVision_0.MotionBlurUtilities.BlurAmount);
			RenderTexture_0.MarkRestoreExpected();
			commandBuffer.Blit(cameraTarget, RenderTexture_0, material);
			commandBuffer.Blit(RenderTexture_0, cameraTarget);
		}
	}

	public void Dispose()
	{
		UnityEngine.Object.DestroyImmediate(Material_0);
		UnityEngine.Object.DestroyImmediate(RenderTexture_0);
	}

	public Material method_0()
	{
		if (Material_0 != null)
		{
			return Material_0;
		}
		return Material_0 = new Material(ThermalVision_0.MotionBlurUtilities.Shader);
	}
}
