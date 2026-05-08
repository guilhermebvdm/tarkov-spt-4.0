using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Time of Day/Camera Main Script")]
public class TOD_Camera : MonoBehaviour
{
	public TOD_Sky sky;

	public bool DomePosToCamera = true;

	public Vector3 DomePosOffset = Vector3.zero;

	public bool DomeScaleToFarClip = true;

	public float DomeScaleFactor = 0.95f;

	private Camera camera_0;

	private Transform transform_0;

	public bool HDR
	{
		get
		{
			if (!camera_0)
			{
				return false;
			}
			return camera_0.allowHDR;
		}
	}

	public void OnValidate()
	{
		DomeScaleFactor = Mathf.Clamp(DomeScaleFactor, 0.01f, 1f);
	}

	public void OnEnable()
	{
		camera_0 = GetComponent<Camera>();
		transform_0 = GetComponent<Transform>();
		if (!sky)
		{
			sky = GClass870.FindUnityObjectOfType(typeof(TOD_Sky)) as TOD_Sky;
		}
	}

	public void Update()
	{
		if ((bool)sky && sky.Initialized)
		{
			sky.Components.Camera = this;
			if (camera_0.clearFlags == CameraClearFlags.Skybox)
			{
				Debug.LogWarning("Skybox camera clear flags are redundant with a sky dome. Changing camera to solid color clear flags.", camera_0);
				camera_0.clearFlags = CameraClearFlags.Color;
			}
		}
	}

	public void OnPreCull()
	{
		if ((bool)sky && sky.Initialized)
		{
			if (DomeScaleToFarClip)
			{
				DoDomeScaleToFarClip();
			}
			if (DomePosToCamera)
			{
				DoDomePosToCamera();
			}
		}
	}

	public void DoDomeScaleToFarClip()
	{
		float num = DomeScaleFactor * camera_0.farClipPlane;
		Vector3 localScale = new Vector3(num, num, num);
		sky.Components.DomeTransform.localScale = localScale;
	}

	public void DoDomePosToCamera()
	{
		Vector3 position = transform_0.position + transform_0.rotation * DomePosOffset;
		sky.Components.DomeTransform.position = position;
	}
}
