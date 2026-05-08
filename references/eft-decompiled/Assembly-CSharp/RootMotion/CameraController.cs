using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion;

public class CameraController : MonoBehaviour
{
	[Serializable]
	public enum UpdateMode
	{
		Update,
		FixedUpdate,
		LateUpdate
	}

	public Transform target;

	public UpdateMode updateMode = UpdateMode.LateUpdate;

	public bool lockCursor = true;

	public bool smoothFollow;

	public float followSpeed = 10f;

	public float distance = 10f;

	public float minDistance = 4f;

	public float maxDistance = 10f;

	public float zoomSpeed = 10f;

	public float zoomSensitivity = 1f;

	public float rotationSensitivity = 3.5f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	public Vector3 offset = new Vector3(0f, 1.5f, 0.5f);

	public bool rotateAlways = true;

	public bool rotateOnLeftButton;

	public bool rotateOnRightButton;

	public bool rotateOnMiddleButton;

	[CompilerGenerated]
	private float float_0;

	[CompilerGenerated]
	private float float_1;

	[CompilerGenerated]
	private float float_2;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Quaternion quaternion_0 = Quaternion.identity;

	private Vector3 vector3_2;

	private Camera camera_0;

	public float x
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public float y
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public float distanceTarget
	{
		[CompilerGenerated]
		get
		{
			return float_2;
		}
		[CompilerGenerated]
		set
		{
			float_2 = value;
		}
	}

	public float Single_0
	{
		get
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis > 0f)
			{
				return 0f - zoomSensitivity;
			}
			if (axis < 0f)
			{
				return zoomSensitivity;
			}
			return 0f;
		}
	}

	public virtual void Awake()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		distanceTarget = distance;
		vector3_2 = base.transform.position;
		camera_0 = GetComponent<Camera>();
	}

	public virtual void Update()
	{
		if (updateMode == UpdateMode.Update)
		{
			UpdateTransform();
		}
	}

	public virtual void FixedUpdate()
	{
		if (updateMode == UpdateMode.FixedUpdate)
		{
			UpdateTransform();
		}
	}

	public virtual void LateUpdate()
	{
		UpdateInput();
		if (updateMode == UpdateMode.LateUpdate)
		{
			UpdateTransform();
		}
	}

	public void UpdateInput()
	{
		if (!(target == null) && camera_0.enabled)
		{
			if (lockCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
			if (rotateAlways || (rotateOnLeftButton && Input.GetMouseButton(0)) || (rotateOnRightButton && Input.GetMouseButton(1)) || (rotateOnMiddleButton && Input.GetMouseButton(2)))
			{
				x += Input.GetAxis("Mouse X") * rotationSensitivity;
				y = method_0(y - Input.GetAxis("Mouse Y") * rotationSensitivity, yMinLimit, yMaxLimit);
			}
			distanceTarget = Mathf.Clamp(distanceTarget + Single_0, minDistance, maxDistance);
		}
	}

	public void UpdateTransform()
	{
		UpdateTransform(Time.deltaTime);
	}

	public void UpdateTransform(float deltaTime)
	{
		if (!(target == null) && camera_0.enabled)
		{
			distance += (distanceTarget - distance) * zoomSpeed * deltaTime;
			quaternion_0 = Quaternion.AngleAxis(x, Vector3.up) * Quaternion.AngleAxis(y, Vector3.right);
			if (!smoothFollow)
			{
				vector3_2 = target.position;
			}
			else
			{
				vector3_2 = Vector3.Lerp(vector3_2, target.position, deltaTime * followSpeed);
			}
			vector3_1 = vector3_2 + quaternion_0 * (offset - Vector3.forward * distance);
			base.transform.position = vector3_1;
			base.transform.rotation = quaternion_0;
		}
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
