using UnityEngine;

namespace GPUInstancer;

public class GrassMowerController : MonoBehaviour
{
	public float engineTorque = 3500f;

	public float enginePower = 4000f;

	private Rigidbody rigidbody_0;

	private float float_0;

	private float float_1;

	public void Awake()
	{
		rigidbody_0 = GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		method_0();
		method_1();
	}

	public void method_0()
	{
		float_1 = Input.GetAxis("Horizontal");
		float_0 = Input.GetAxis("Jump");
	}

	public void method_1()
	{
		rigidbody_0.AddRelativeTorque(Vector3.up * float_1 * engineTorque * Time.deltaTime);
		rigidbody_0.AddRelativeForce(Vector3.forward * float_0 * enginePower * Time.deltaTime);
	}
}
