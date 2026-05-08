using UnityEngine;

public class FreeCamera : MonoBehaviour
{
	public bool enableInputCapture = true;

	public bool holdRightMouseCapture;

	public float lookSpeed = 5f;

	public float moveSpeed = 5f;

	public float sprintSpeed = 50f;

	private bool bool_0;

	private float float_0;

	private float float_1;

	public void Awake()
	{
		base.enabled = enableInputCapture;
	}

	public void OnValidate()
	{
		if (Application.isPlaying)
		{
			base.enabled = enableInputCapture;
		}
	}

	public void method_0()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		bool_0 = true;
		float_0 = base.transform.eulerAngles.y;
		float_1 = base.transform.eulerAngles.x;
	}

	public void method_1()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		bool_0 = false;
	}

	public void OnApplicationFocus(bool focus)
	{
		if (bool_0 && !focus)
		{
			method_1();
		}
	}

	public void Update()
	{
		if (!bool_0)
		{
			if (!holdRightMouseCapture && Input.GetMouseButtonDown(0))
			{
				method_0();
			}
			else if (holdRightMouseCapture && Input.GetMouseButtonDown(1))
			{
				method_0();
			}
		}
		if (!bool_0)
		{
			return;
		}
		if (bool_0)
		{
			if (!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
			{
				method_1();
			}
			else if (holdRightMouseCapture && Input.GetMouseButtonUp(1))
			{
				method_1();
			}
		}
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		float_0 = (float_0 + lookSpeed * axis) % 360f;
		float_1 = (float_1 - lookSpeed * axis2) % 360f;
		base.transform.rotation = Quaternion.AngleAxis(float_0, Vector3.up) * Quaternion.AngleAxis(float_1, Vector3.right);
		float num = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
		float num2 = num * Input.GetAxis("Vertical");
		float num3 = num * Input.GetAxis("Horizontal");
		float num4 = num * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
		base.transform.position += base.transform.forward * num2 + base.transform.right * num3 + Vector3.up * num4;
	}
}
