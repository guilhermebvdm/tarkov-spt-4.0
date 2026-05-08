using UnityEngine;

public class FPSLimit : MonoBehaviour
{
	public bool SetFps;

	public int FPS = -1;

	public void Update()
	{
		if (SetFps)
		{
			SetFps = false;
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = FPS;
		}
	}
}
