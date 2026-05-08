using System;
using UnityEngine;

namespace RootMotion.FinalIK;

public class AimPoser : MonoBehaviour
{
	[Serializable]
	public class Pose
	{
		public bool visualize = true;

		public string name;

		public Vector3 direction;

		public float yaw = 75f;

		public float pitch = 45f;

		[NonSerialized]
		public float AngleBuffer;

		public bool IsInDirection(Vector3 d)
		{
			if (direction == Vector3.zero)
			{
				return false;
			}
			if (!(yaw <= 0f) && pitch > 0f)
			{
				if (yaw < 180f)
				{
					Vector3 vector = new Vector3(direction.x, 0f, direction.z);
					if (vector == Vector3.zero)
					{
						vector = Vector3.forward;
					}
					if (Vector3.Angle(new Vector3(d.x, 0f, d.z), vector) > yaw + AngleBuffer)
					{
						return false;
					}
				}
				if (pitch >= 180f)
				{
					return true;
				}
				float num = Vector3.Angle(Vector3.up, direction);
				return Mathf.Abs(Vector3.Angle(Vector3.up, d) - num) < pitch + AngleBuffer;
			}
			return false;
		}

		public void SetAngleBuffer(float value)
		{
			AngleBuffer = value;
		}
	}

	public float angleBuffer = 5f;

	public Pose[] poses = new Pose[0];

	public Pose GetPose(Vector3 localDirection)
	{
		if (poses.Length == 0)
		{
			return null;
		}
		int num = 0;
		while (true)
		{
			if (num < poses.Length - 1)
			{
				if (poses[num].IsInDirection(localDirection))
				{
					break;
				}
				num++;
				continue;
			}
			return poses[poses.Length - 1];
		}
		return poses[num];
	}

	public void SetPoseActive(Pose pose)
	{
		for (int i = 0; i < poses.Length; i++)
		{
			poses[i].SetAngleBuffer((poses[i] == pose) ? angleBuffer : 0f);
		}
	}
}
