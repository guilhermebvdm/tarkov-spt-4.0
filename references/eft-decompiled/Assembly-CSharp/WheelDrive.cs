using System.Collections;
using UnityEngine;

public class WheelDrive : MonoBehaviour
{
	public enum EGear
	{
		Back,
		Fwd
	}

	[SerializeField]
	private RigidbodySpawner _rigidSpawner;

	[SerializeField]
	private Transform Ballistics;

	[SerializeField]
	private RigidbodySpawner _bumperSpawner;

	[SerializeField]
	private Collider _chassisCollider;

	[SerializeField]
	private Collider _bumperCollider;

	public EGear Gear;

	public Vector3 SteerTarget;

	public Transform SteerTargetTransform;

	public float Torque;

	public Vector2 MinSpeedAtDistance;

	public Vector2 MaxpeedAtDistance;

	public float BrakeTorque = 400f;

	public float SteerSpeed = 1000f;

	public Vector2 SpeedByTurnAngel = new Vector2(1f, 1f);

	private float float_0;

	private bool bool_0;

	public EasySuspension ES;

	private Rigidbody rigidbody_0;

	private Rigidbody rigidbody_1;

	[Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
	public float criticalSpeed = 5f;

	[Tooltip("Simulation sub-steps when the speed is above critical.")]
	public int stepsBelow = 5;

	[Tooltip("Simulation sub-steps when the speed is below critical.")]
	public int stepsAbove = 1;

	private WheelCollider[] wheelCollider_0 = new WheelCollider[0];

	private Vector3 vector3_0;

	public WheelCollider[] frontWheels;

	public AudioSource AudioSource;

	public AudioClip OnStartSound;

	public Transform[] wheelGraphics;

	public void Start()
	{
		vector3_0 = Vector3.Lerp(frontWheels[0].transform.position, frontWheels[1].transform.position, 0.5f);
		vector3_0 = base.transform.InverseTransformPoint(vector3_0);
		wheelCollider_0 = GetComponentsInChildren<WheelCollider>();
		wheelGraphics = new Transform[wheelCollider_0.Length];
		for (int i = 0; i < wheelCollider_0.Length; i++)
		{
			WheelCollider wheelCollider = wheelCollider_0[i];
			wheelGraphics[i] = wheelCollider.transform.GetChild(0);
		}
		method_1();
		StartCoroutine(Sleep());
	}

	public IEnumerator Sleep()
	{
		rigidbody_0.drag = 3f;
		yield return new WaitForSeconds(3f);
		base.enabled = false;
	}

	public void OnEnable()
	{
		rigidbody_0 = _rigidSpawner.Create();
		rigidbody_0.drag = 0f;
		EFTPhysicsClass.GClass745.SupportRigidbody(rigidbody_0);
		if (_bumperSpawner != null)
		{
			rigidbody_1 = _bumperSpawner.Create();
			EFTPhysicsClass.GClass745.SupportRigidbody(rigidbody_1);
		}
		if (_chassisCollider != null && _bumperCollider != null)
		{
			Physics.IgnoreCollision(_chassisCollider, _bumperCollider);
		}
		WheelCollider[] array = wheelCollider_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		if (ES != null)
		{
			ES.SetRigidbody(rigidbody_0);
			ES.enabled = true;
		}
	}

	public void OnDisable()
	{
		_rigidSpawner.Remove();
		rigidbody_0 = null;
		if (_bumperSpawner != null)
		{
			_bumperSpawner.Remove();
			rigidbody_1 = null;
		}
		WheelCollider[] array = wheelCollider_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		if (ES != null)
		{
			ES.SetRigidbody(null);
			ES.enabled = false;
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(SteerTarget, 0.5f);
	}

	public void Steer(Vector3 point)
	{
		SteerTarget = point;
		Vector3 lhs = new Vector3(rigidbody_0.velocity.x, 0f, rigidbody_0.velocity.z);
		Vector3 vector = SteerTarget - base.transform.TransformPoint(vector3_0);
		Debug.DrawRay(base.transform.TransformPoint(vector3_0), vector, Color.red, Time.deltaTime);
		Vector3 forward = base.transform.forward;
		float num = Vector3.Dot(forward, vector);
		Gear = ((num > 0f) ? EGear.Fwd : EGear.Back);
		float value = Vector3.Distance(base.transform.TransformPoint(vector3_0), point);
		float t = Mathf.InverseLerp(MinSpeedAtDistance.y, MaxpeedAtDistance.y, value);
		Torque = Mathf.Lerp(MinSpeedAtDistance.x, MaxpeedAtDistance.x, t);
		Torque *= GClass2298.Evaluate(SpeedByTurnAngel, Mathf.Abs(num));
		Vector3 a = ((Gear == EGear.Fwd) ? new Vector3(forward.x, 0f, forward.z) : new Vector3(0f - forward.x, 0f, 0f - forward.z));
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
		bool_0 = lhs.magnitude > 1f && Vector3.Dot(lhs, vector2) < 0f;
		float num2 = Mathf.Clamp(method_0(a, vector2), -30f, 30f);
		if (Gear == EGear.Back)
		{
			num2 = 0f - num2;
			Torque = 0f - Torque;
		}
		float_0 = Mathf.Lerp(float_0, num2, SteerSpeed * Time.deltaTime);
		WheelCollider[] array = frontWheels;
		foreach (WheelCollider obj in array)
		{
			obj.steerAngle = float_0;
			obj.motorTorque = Torque;
		}
		array = wheelCollider_0;
		foreach (WheelCollider wheelCollider in array)
		{
			wheelCollider.brakeTorque = (bool_0 ? BrakeTorque : 0f);
			if (bool_0)
			{
				wheelCollider.motorTorque = 0f;
			}
		}
	}

	public float method_0(Vector3 a, Vector3 b)
	{
		return Vector3.Angle(a, b) * Mathf.Sign(Vector3.Cross(a, b).y);
	}

	public void Update()
	{
		wheelCollider_0[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);
		for (int i = 0; i < wheelCollider_0.Length; i++)
		{
			wheelCollider_0[i].GetWorldPose(out var pos, out var quat);
			Transform obj = wheelGraphics[i];
			obj.position = pos;
			obj.rotation = quat;
		}
		Ballistics.SetPositionAndRotation(base.transform.position, base.transform.rotation);
	}

	public void ManualBrake()
	{
		WheelCollider[] array = wheelCollider_0;
		foreach (WheelCollider obj in array)
		{
			obj.brakeTorque = BrakeTorque;
			obj.motorTorque = 0f;
		}
	}

	public void method_1()
	{
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated && !(AudioSource == null))
		{
			AudioSource.outputAudioMixerGroup = MonoBehaviourSingleton<BetterAudio>.Instance.EnvTechnicalSoundsGroup;
		}
	}
}
