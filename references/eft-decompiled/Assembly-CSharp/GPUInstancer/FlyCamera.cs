using UnityEngine;

namespace GPUInstancer;

public class FlyCamera : MonoBehaviour
{
	public float mainSpeed = 10f;

	public float shiftSpeed = 30f;

	public float rotationSpeed = 5f;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	public void Start()
	{
		vector3_0 = Vector3.zero;
		vector3_1 = base.transform.rotation.eulerAngles;
	}

	public void Update()
	{
		if (Input.GetMouseButton(1))
		{
			vector3_1.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
			vector3_1.y += Input.GetAxis("Mouse X") * rotationSpeed;
			base.transform.eulerAngles = vector3_1;
		}
		method_0();
		base.transform.Translate(vector3_0);
	}

	public void method_0()
	{
		vector3_0.x = 0f;
		vector3_0.y = 0f;
		vector3_0.z = 0f;
		if (Input.GetKey(KeyCode.W))
		{
			vector3_0.z += 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			vector3_0.z -= 1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			vector3_0.x -= 1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			vector3_0.x += 1f;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			vector3_0.y -= 1f;
		}
		if (Input.GetKey(KeyCode.E))
		{
			vector3_0.y += 1f;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			vector3_0 *= Time.deltaTime * shiftSpeed;
		}
		else
		{
			vector3_0 *= Time.deltaTime * mainSpeed;
		}
	}
}
