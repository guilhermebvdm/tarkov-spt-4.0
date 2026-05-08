using System;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass979
{
	public enum PixelationMode
	{
		Point = 0,
		CRT = 1,
		None = 10
	}

	[NonSerialized]
	public Camera Camera_0;

	[NonSerialized]
	public ThermalVision ThermalVision_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("_PixelBlockCount");

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("_PixelBlockSize");

	[NonSerialized]
	public static int Int_2 = Shader.PropertyToID("_PixelationMask");

	[NonSerialized]
	public static int Int_3 = Shader.PropertyToID("_Alpha");

	[NonSerialized]
	public static int Int_4 = Shader.PropertyToID("_PixelationTempRT");

	public GClass979(ThermalVision thermalVision, Camera camera)
	{
		ThermalVision_0 = thermalVision;
		Camera_0 = camera;
	}

	public void UpdateTexture(CommandBuffer commandBuffer)
	{
		if (ThermalVision_0 == null)
		{
			return;
		}
		PixelationMode mode = ThermalVision_0.PixelationUtilities.Mode;
		if (mode != PixelationMode.None)
		{
			method_1();
			if (mode != PixelationMode.None)
			{
				commandBuffer.GetTemporaryRT(Int_4, -1, -1);
				commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, Int_4, method_0(), (int)mode);
				commandBuffer.Blit(Int_4, BuiltinRenderTextureType.CameraTarget);
				commandBuffer.ReleaseTemporaryRT(Int_4);
			}
		}
	}

	public Material method_0()
	{
		if (Material_0 != null)
		{
			return Material_0;
		}
		return Material_0 = new Material(ThermalVision_0.PixelationUtilities.PixelationShader);
	}

	public void method_1()
	{
		Material material = method_0();
		Vector2 vector = new Vector2(ThermalVision_0.PixelationUtilities.BlockCount, ThermalVision_0.PixelationUtilities.BlockCount / Camera_0.aspect);
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		material.SetVector(Int_0, vector);
		material.SetVector(Int_1, vector2);
		material.SetTexture(Int_2, ThermalVision_0.PixelationUtilities.PixelationMask);
		material.SetFloat(Int_3, ThermalVision_0.PixelationUtilities.Alpha);
	}
}
