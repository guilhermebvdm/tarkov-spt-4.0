using UnityEngine;

namespace RootMotion.FinalIK;

public class BodyTilt : OffsetModifier
{
	[Tooltip("Speed of tilting")]
	public float tiltSpeed = 6f;

	[Tooltip("Sensitivity of tilting")]
	public float tiltSensitivity = 0.07f;

	[Tooltip("The OffsetPose components")]
	public OffsetPose poseLeft;

	[Tooltip("The OffsetPose components")]
	public OffsetPose poseRight;

	private float float_1;

	private Vector3 vector3_0;

	public override void Start()
	{
		base.Start();
		vector3_0 = base.transform.forward;
	}

	public override void OnModifyOffset()
	{
		Quaternion quaternion = Quaternion.FromToRotation(vector3_0, base.transform.forward);
		float angle = 0f;
		Vector3 axis = Vector3.zero;
		quaternion.ToAngleAxis(out angle, out axis);
		if (axis.y > 0f)
		{
			angle = 0f - angle;
		}
		angle *= tiltSensitivity * 0.01f;
		angle /= base.deltaTime;
		angle = Mathf.Clamp(angle, -1f, 1f);
		float_1 = Mathf.Lerp(float_1, angle, base.deltaTime * tiltSpeed);
		float num = Mathf.Abs(float_1) / 1f;
		if (float_1 < 0f)
		{
			poseRight.Apply(ik.solver, num);
		}
		else
		{
			poseLeft.Apply(ik.solver, num);
		}
		vector3_0 = base.transform.forward;
	}
}
