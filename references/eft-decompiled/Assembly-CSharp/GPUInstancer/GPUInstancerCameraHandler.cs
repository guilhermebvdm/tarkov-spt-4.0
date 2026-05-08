using UnityEngine;

namespace GPUInstancer;

[RequireComponent(typeof(Camera))]
public class GPUInstancerCameraHandler : MonoBehaviour
{
	private Camera camera_0;

	private bool bool_0;

	public void OnEnable()
	{
		if (!camera_0)
		{
			camera_0 = GetComponent<Camera>();
		}
		bool_0 = false;
	}

	public void OnDisable()
	{
		bool_0 = false;
	}

	public void Update()
	{
		if (camera_0.isActiveAndEnabled && !bool_0)
		{
			GClass1257.SetCamera(camera_0);
			bool_0 = true;
		}
		else if (!camera_0.isActiveAndEnabled && bool_0)
		{
			bool_0 = false;
		}
	}
}
