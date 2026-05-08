using UnityEngine;

public class ExtendedFlycam : MonoBehaviour
{
	public float cameraSensitivity = 90f;

	public float climbSpeed = 4f;

	public float normalMoveSpeed = 10f;

	public float slowMoveFactor = 0.25f;

	public float fastMoveFactor = 3f;

	private float float_0;

	private float float_1;

	public void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void Update()
	{
		if (Cursor.lockState != CursorLockMode.None)
		{
			float_0 += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			float_1 += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
		}
		float_1 = Mathf.Clamp(float_1, -90f, 90f);
		Quaternion b = Quaternion.AngleAxis(float_0, Vector3.up);
		b *= Quaternion.AngleAxis(float_1, Vector3.left);
		base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, b, Time.deltaTime * 5f);
		if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
		{
			if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
			{
				base.transform.position += base.transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
				base.transform.position += base.transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
				if (Input.GetKey(KeyCode.Q))
				{
					base.transform.position += Vector3.up * climbSpeed * Time.deltaTime;
				}
				if (Input.GetKey(KeyCode.E))
				{
					base.transform.position -= Vector3.up * climbSpeed * Time.deltaTime;
				}
			}
			else
			{
				base.transform.position += base.transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
				base.transform.position += base.transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
				if (Input.GetKey(KeyCode.Q))
				{
					base.transform.position += Vector3.up * climbSpeed * slowMoveFactor * Time.deltaTime;
				}
				if (Input.GetKey(KeyCode.E))
				{
					base.transform.position -= Vector3.up * climbSpeed * slowMoveFactor * Time.deltaTime;
				}
			}
		}
		else
		{
			base.transform.position += base.transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			if (Input.GetKey(KeyCode.Q))
			{
				base.transform.position += Vector3.up * climbSpeed * fastMoveFactor * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.E))
			{
				base.transform.position -= Vector3.up * climbSpeed * fastMoveFactor * Time.deltaTime;
			}
		}
		if (Input.GetKeyDown(KeyCode.End) || Input.GetKeyDown(KeyCode.Escape))
		{
			if (Cursor.lockState == CursorLockMode.None)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}
}
