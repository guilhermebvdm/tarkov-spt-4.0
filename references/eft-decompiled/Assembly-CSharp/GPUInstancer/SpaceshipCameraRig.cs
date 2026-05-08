using UnityEngine;

namespace GPUInstancer;

public class SpaceshipCameraRig : MonoBehaviour
{
	public Transform m_Target;

	public float m_MoveSpeed = 3f;

	public float m_TurnSpeed = 1f;

	public float m_RollSpeed = 0.2f;

	public bool m_FollowVelocity;

	public bool m_FollowTilt = true;

	public float m_SpinTurnLimit = 90f;

	public float m_TargetVelocityLowerLimit = 4f;

	public float m_SmoothTurnTime = 0.2f;

	private Vector3 vector3_0;

	private Rigidbody rigidbody_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private Vector3 vector3_1 = Vector3.up;

	public void Start()
	{
		if (!(m_Target == null))
		{
			rigidbody_0 = m_Target.GetComponent<Rigidbody>();
		}
	}

	public void FixedUpdate()
	{
		method_0(Time.deltaTime);
	}

	public void method_0(float deltaTime)
	{
		if (!(deltaTime > 0f) || m_Target == null)
		{
			return;
		}
		Vector3 forward = m_Target.forward;
		Vector3 up = m_Target.up;
		if (m_FollowVelocity && Application.isPlaying)
		{
			if (rigidbody_0.velocity.magnitude > m_TargetVelocityLowerLimit)
			{
				forward = rigidbody_0.velocity.normalized;
				up = Vector3.up;
			}
			else
			{
				up = Vector3.up;
			}
			float_1 = Mathf.SmoothDamp(float_1, 1f, ref float_2, m_SmoothTurnTime);
		}
		else
		{
			float target = Mathf.Atan2(forward.x, forward.z) * 57.29578f;
			if (m_SpinTurnLimit > 0f)
			{
				float value = Mathf.Abs(Mathf.DeltaAngle(float_0, target)) / deltaTime;
				float num = Mathf.InverseLerp(m_SpinTurnLimit, m_SpinTurnLimit * 0.75f, value);
				float smoothTime = ((float_1 > num) ? 0.1f : 1f);
				if (Application.isPlaying)
				{
					float_1 = Mathf.SmoothDamp(float_1, num, ref float_2, smoothTime);
				}
				else
				{
					float_1 = num;
				}
			}
			else
			{
				float_1 = 1f;
			}
			float_0 = target;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, m_Target.position, deltaTime * m_MoveSpeed);
		if (!m_FollowTilt)
		{
			forward.y = 0f;
			if (forward.sqrMagnitude < float.Epsilon)
			{
				forward = base.transform.forward;
			}
		}
		Quaternion b = Quaternion.LookRotation(forward, vector3_1);
		vector3_1 = ((m_RollSpeed > 0f) ? Vector3.Slerp(vector3_1, up, m_RollSpeed * deltaTime) : Vector3.up);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, m_TurnSpeed * float_1 * deltaTime);
	}
}
