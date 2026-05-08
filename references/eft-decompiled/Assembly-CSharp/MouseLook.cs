using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public enum InputAxisEnum
	{
		X,
		Y
	}

	public float MaximumY = 90f;

	public float MinimumY = -90f;

	public Vector3 Axis = Vector3.right;

	public Transform Root;

	public float SensitivityY = 5f;

	public InputAxisEnum InputAxis = InputAxisEnum.Y;

	private Quaternion quaternion_0;

	public bool UseLocalUpdate;

	private float float_0;

	public float RotationY => float_0;

	public void Start()
	{
		quaternion_0 = Root.transform.localRotation;
		float_0 = quaternion_0.eulerAngles.y;
	}

	public void Update()
	{
		if (UseLocalUpdate)
		{
			OnUpdate();
		}
	}

	public void OnUpdate()
	{
		float_0 += Input.GetAxis("Mouse " + InputAxis) * SensitivityY;
		float_0 = ClampAngle(float_0, MinimumY, MaximumY);
		Quaternion quaternion = Quaternion.AngleAxis(0f - float_0, Axis);
		Root.localRotation = quaternion_0 * quaternion;
	}

	public static float ClampAngle(float angle, float min, float max)
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
