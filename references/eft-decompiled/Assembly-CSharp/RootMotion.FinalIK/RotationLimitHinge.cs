using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Hinge")]
public class RotationLimitHinge : RotationLimit
{
	public bool useLimits = true;

	public float min = -45f;

	public float max = 90f;

	[HideInInspector]
	public float zeroAxisDisplayOffset;

	private Quaternion quaternion_0 = Quaternion.identity;

	private float float_0;

	[ContextMenu("User Manual")]
	public void method_0()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page12.html");
	}

	[ContextMenu("Scrpt Reference")]
	public void method_1()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_hinge.html");
	}

	[ContextMenu("Support Group")]
	public void method_2()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	public void method_3()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public override Quaternion LimitRotation(Quaternion rotation)
	{
		quaternion_0 = method_4(rotation);
		return quaternion_0;
	}

	public Quaternion method_4(Quaternion rotation)
	{
		if (min == 0f && max == 0f && useLimits)
		{
			return Quaternion.AngleAxis(0f, axis);
		}
		Quaternion quaternion = RotationLimit.Limit1DOF(rotation, axis);
		if (!useLimits)
		{
			return quaternion;
		}
		Quaternion quaternion2 = quaternion * Quaternion.Inverse(quaternion_0);
		float num = Quaternion.Angle(Quaternion.identity, quaternion2);
		Vector3 vector = new Vector3(axis.z, axis.x, axis.y);
		Vector3 rhs = Vector3.Cross(vector, axis);
		if (Vector3.Dot(quaternion2 * vector, rhs) > 0f)
		{
			num = 0f - num;
		}
		float_0 = Mathf.Clamp(float_0 + num, min, max);
		return Quaternion.AngleAxis(float_0, axis);
	}
}
