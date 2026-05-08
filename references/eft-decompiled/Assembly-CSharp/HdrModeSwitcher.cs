using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(Camera))]
public class HdrModeSwitcher : MonoBehaviour
{
	[SerializeField]
	private CameraHDRMode HdrMode = CameraHDRMode.FP16;

	private CameraHDRMode cameraHDRMode_0;

	public void OnPreCull()
	{
		cameraHDRMode_0 = ((RuntimeUtilities.defaultHDRRenderTextureFormat == RenderTextureFormat.ARGBHalf) ? CameraHDRMode.FP16 : CameraHDRMode.R11G11B10);
		method_0(HdrMode);
	}

	public void OnPostRender()
	{
		method_0(cameraHDRMode_0);
	}

	public void method_0(CameraHDRMode hdrMode)
	{
		if (hdrMode == CameraHDRMode.FP16)
		{
			RuntimeUtilities.defaultHDRRenderTextureFormat = RenderTextureFormat.ARGBHalf;
			Graphics.activeTier = GraphicsTier.Tier3;
		}
		else
		{
			RuntimeUtilities.defaultHDRRenderTextureFormat = RenderTextureFormat.RGB111110Float;
			Graphics.activeTier = GraphicsTier.Tier2;
		}
	}
}
