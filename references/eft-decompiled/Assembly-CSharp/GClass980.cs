using System;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass980
{
	[NonSerialized]
	public ThermalVision ThermalVision_0;

	[NonSerialized]
	public RenderTexture RenderTexture_0;

	[NonSerialized]
	public float Float_0;

	public GClass980(ThermalVision thermalVision)
	{
		ThermalVision_0 = thermalVision;
	}

	public void UpdateTexture(CommandBuffer commandBuffer, Camera camera)
	{
		if (!(ThermalVision_0 == null))
		{
			if (RenderTexture_0 == null)
			{
				RenderTexture_0 = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0)
				{
					name = "Last Frame StuckFPSController"
				};
			}
			int num = UnityEngine.Random.Range(ThermalVision_0.StuckFpsUtilities.MinFramerate, ThermalVision_0.StuckFpsUtilities.MaxFramerate);
			if (Time.time - Float_0 > 1f / (float)num)
			{
				commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, RenderTexture_0);
				Float_0 = Time.time;
			}
			commandBuffer.Blit(RenderTexture_0, BuiltinRenderTextureType.CameraTarget);
		}
	}

	public void Destroy()
	{
		if (RenderTexture_0 != null)
		{
			UnityEngine.Object.DestroyImmediate(RenderTexture_0);
		}
	}
}
