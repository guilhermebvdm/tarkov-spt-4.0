using UnityEngine;

namespace RootMotion;

public class CameraControllerFPS : MonoBehaviour
{
	public float rotationSensitivity = 3f;

	public float yMinLimit = -89f;

	public float yMaxLimit = 89f;

	private float float_0;

	private float float_1;

	public void Awake()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		float_0 = eulerAngles.y;
		float_1 = eulerAngles.x;
	}

	public void LateUpdate()
	{
		Cursor.lockState = CursorLockMode.Locked;
		float_0 += Input.GetAxis("Mouse X") * rotationSensitivity;
		float_1 = method_0(float_1 - Input.GetAxis("Mouse Y") * rotationSensitivity, yMinLimit, yMaxLimit);
		base.transform.rotation = Quaternion.AngleAxis(float_0, Vector3.up) * Quaternion.AngleAxis(float_1, Vector3.right);
	}

	public float method_0(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
