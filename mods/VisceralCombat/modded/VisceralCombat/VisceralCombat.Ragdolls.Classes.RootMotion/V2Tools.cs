using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public static class V2Tools
{
	public static Vector2 XZ(Vector3 v)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(v.x, v.z);
	}

	public static float DeltaAngle(Vector2 dir1, Vector2 dir2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Atan2(dir1.x, dir1.y) * 57.29578f;
		float num2 = Mathf.Atan2(dir2.x, dir2.y) * 57.29578f;
		return Mathf.DeltaAngle(num, num2);
	}

	public static float DeltaAngleXZ(Vector3 dir1, Vector3 dir2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Atan2(dir1.x, dir1.z) * 57.29578f;
		float num2 = Mathf.Atan2(dir2.x, dir2.z) * 57.29578f;
		return Mathf.DeltaAngle(num, num2);
	}

	public static bool LineCircleIntersect(Vector2 p1, Vector2 p2, Vector2 c, float r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = p2 - p1;
		Vector2 val2 = c - p1;
		float num = Vector2.Dot(val, val);
		float num2 = 2f * Vector2.Dot(val2, val);
		float num3 = Vector2.Dot(val2, val2) - r * r;
		float num4 = num2 * num2 - 4f * num * num3;
		if (num4 < 0f)
		{
			return false;
		}
		num4 = Mathf.Sqrt(num4);
		float num5 = 2f * num;
		float num6 = (num2 - num4) / num5;
		float num7 = (num2 + num4) / num5;
		if (num6 >= 0f && num6 <= 1f)
		{
			return true;
		}
		if (num7 >= 0f && num7 <= 1f)
		{
			return true;
		}
		return false;
	}

	public static bool RayCircleIntersect(Vector2 p1, Vector2 dir, Vector2 c, float r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = p1 + dir;
		p1 -= c;
		val -= c;
		float num = val.x - p1.x;
		float num2 = val.y - p1.y;
		float num3 = Mathf.Sqrt(Mathf.Pow(num, 2f) + Mathf.Pow(num2, 2f));
		float num4 = p1.x * val.y - val.x * p1.y;
		float num5 = Mathf.Pow(r, 2f) * Mathf.Pow(num3, 2f) - Mathf.Pow(num4, 2f);
		return num5 >= 0f;
	}
}
