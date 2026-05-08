using Comfort.Common;
using EFT.Settings.Graphics;
using UnityEngine;

public class MenuCameraResolutionFixer : MonoBehaviour
{
	private Camera camera_0;

	private CameraClass cameraClass;

	public void Awake()
	{
		camera_0 = GetComponent<Camera>();
	}

	public void OnPreCull()
	{
		if (Singleton<SharedGameSettingsClass>.Instantiated)
		{
			EftResolution resolution = ((GStruct293)Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.DisplaySettings).Resolution;
			camera_0.rect = new Rect(0f, 0f, resolution.Width, resolution.Height);
		}
	}
}
