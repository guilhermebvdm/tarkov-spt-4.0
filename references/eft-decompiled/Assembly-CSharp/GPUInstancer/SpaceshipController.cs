using UnityEngine;

namespace GPUInstancer;

public class SpaceshipController : MonoBehaviour
{
	public float engineTorque = 1500f;

	public float enginePower = 4500f;

	private Rigidbody rigidbody_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private ParticleSystem.EmissionModule emissionModule_0;

	private ParticleSystem.EmissionModule emissionModule_1;

	private Light light_0;

	private float float_4;

	private float float_5;

	public void Awake()
	{
		rigidbody_0 = GetComponent<Rigidbody>();
		emissionModule_0 = base.transform.GetChild(0).GetComponent<ParticleSystem>().emission;
		float_4 = emissionModule_0.rateOverTime.constant;
		emissionModule_1 = base.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>()
			.emission;
		float_5 = emissionModule_1.rateOverTime.constant;
		Transform transform = base.transform.Find("EngineGlowLight");
		if ((bool)transform)
		{
			light_0 = transform.GetComponent<Light>();
		}
	}

	public void FixedUpdate()
	{
		method_0();
		method_1();
		method_2();
	}

	public void method_0()
	{
		float_3 = Input.GetAxis("Horizontal");
		float_1 = Input.GetAxis("Jump");
		float_2 = Input.GetAxis("Vertical");
		float_0 = (Input.GetKey(KeyCode.Q) ? 1f : (Input.GetKey(KeyCode.E) ? (-1f) : 0f));
	}

	public void method_1()
	{
		rigidbody_0.AddRelativeTorque(Vector3.up * float_3 * engineTorque * Time.deltaTime);
		rigidbody_0.AddRelativeTorque(Vector3.right * float_2 * engineTorque * Time.deltaTime);
		rigidbody_0.AddRelativeTorque(Vector3.forward * float_0 * engineTorque * Time.deltaTime);
		rigidbody_0.AddRelativeForce(Vector3.forward * float_1 * enginePower * Time.deltaTime);
	}

	public void method_2()
	{
		emissionModule_0.rateOverTime = float_4 * float_1;
		emissionModule_1.rateOverTime = Mathf.Lerp(0.5f * float_5, float_5, float_1);
		if ((bool)light_0)
		{
			light_0.intensity = Mathf.Clamp01(0.5f + float_1);
		}
	}
}
