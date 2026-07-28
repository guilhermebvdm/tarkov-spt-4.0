using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public static class V3Tools
{
	public static float GetYaw(Vector3 forward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Atan2(forward.x, forward.z) * 57.29578f;
	}

	public static float GetPitch(Vector3 forward)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		forward = ((Vector3)(ref forward)).normalized;
		return (0f - Mathf.Asin(forward.y)) * 57.29578f;
	}

	public static float GetBank(Vector3 forward, Vector3 up)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Inverse(Quaternion.LookRotation(Vector3.up, forward));
		up = val * up;
		return Mathf.Atan2(up.x, up.z) * 57.29578f;
	}

	public static float GetYaw(Vector3 spaceForward, Vector3 spaceUp, Vector3 forward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Inverse(Quaternion.LookRotation(spaceForward, spaceUp));
		Vector3 val2 = val * forward;
		return Mathf.Atan2(val2.x, val2.z) * 57.29578f;
	}

	public static float GetPitch(Vector3 spaceForward, Vector3 spaceUp, Vector3 forward)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Inverse(Quaternion.LookRotation(spaceForward, spaceUp));
		Vector3 val2 = val * forward;
		return (0f - Mathf.Asin(val2.y)) * 57.29578f;
	}

	public static float GetBank(Vector3 spaceForward, Vector3 spaceUp, Vector3 forward, Vector3 up)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Inverse(Quaternion.LookRotation(spaceForward, spaceUp));
		forward = val * forward;
		up = val * up;
		Quaternion val2 = Quaternion.Inverse(Quaternion.LookRotation(spaceUp, forward));
		up = val2 * up;
		return Mathf.Atan2(up.x, up.z) * 57.29578f;
	}

	public static Vector3 Lerp(Vector3 fromVector, Vector3 toVector, float weight)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f)
		{
			return fromVector;
		}
		if (weight >= 1f)
		{
			return toVector;
		}
		return Vector3.Lerp(fromVector, toVector, weight);
	}

	public static Vector3 Slerp(Vector3 fromVector, Vector3 toVector, float weight)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f)
		{
			return fromVector;
		}
		if (weight >= 1f)
		{
			return toVector;
		}
		return Vector3.Slerp(fromVector, toVector, weight);
	}

	public static Vector3 ExtractVertical(Vector3 v, Vector3 verticalAxis, float weight)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f)
		{
			return Vector3.zero;
		}
		if (verticalAxis == Vector3.up)
		{
			return Vector3.up * v.y * weight;
		}
		return Vector3.Project(v, verticalAxis) * weight;
	}

	public static Vector3 ExtractHorizontal(Vector3 v, Vector3 normal, float weight)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (weight <= 0f)
		{
			return Vector3.zero;
		}
		if (normal == Vector3.up)
		{
			return new Vector3(v.x, 0f, v.z) * weight;
		}
		Vector3 val = v;
		Vector3.OrthoNormalize(ref normal, ref val);
		return Vector3.Project(v, val) * weight;
	}

	public static Vector3 Flatten(Vector3 v, Vector3 normal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (normal == Vector3.up)
		{
			return new Vector3(v.x, 0f, v.z);
		}
		return v - Vector3.Project(v, normal);
	}

	public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (clampWeight <= 0f)
		{
			return direction;
		}
		if (clampWeight >= 1f)
		{
			return normalDirection;
		}
		float num = Vector3.Angle(normalDirection, direction);
		float num2 = 1f - num / 180f;
		if (num2 > clampWeight)
		{
			return direction;
		}
		float num3 = ((clampWeight > 0f) ? Mathf.Clamp(1f - (clampWeight - num2) / (1f - num2), 0f, 1f) : 1f);
		float num4 = ((clampWeight > 0f) ? Mathf.Clamp(num2 / clampWeight, 0f, 1f) : 1f);
		for (int i = 0; i < clampSmoothing; i++)
		{
			float num5 = num4 * MathF.PI * 0.5f;
			num4 = Mathf.Sin(num5);
		}
		return Vector3.Slerp(normalDirection, direction, num4 * num3);
	}

	public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing, out bool changed)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		changed = false;
		if (clampWeight <= 0f)
		{
			return direction;
		}
		if (clampWeight >= 1f)
		{
			changed = true;
			return normalDirection;
		}
		float num = Vector3.Angle(normalDirection, direction);
		float num2 = 1f - num / 180f;
		if (num2 > clampWeight)
		{
			return direction;
		}
		changed = true;
		float num3 = ((clampWeight > 0f) ? Mathf.Clamp(1f - (clampWeight - num2) / (1f - num2), 0f, 1f) : 1f);
		float num4 = ((clampWeight > 0f) ? Mathf.Clamp(num2 / clampWeight, 0f, 1f) : 1f);
		for (int i = 0; i < clampSmoothing; i++)
		{
			float num5 = num4 * MathF.PI * 0.5f;
			num4 = Mathf.Sin(num5);
		}
		return Vector3.Slerp(normalDirection, direction, num4 * num3);
	}

	public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing, out float clampValue)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		clampValue = 1f;
		if (clampWeight <= 0f)
		{
			return direction;
		}
		if (clampWeight >= 1f)
		{
			return normalDirection;
		}
		float num = Vector3.Angle(normalDirection, direction);
		float num2 = 1f - num / 180f;
		if (num2 > clampWeight)
		{
			clampValue = 0f;
			return direction;
		}
		float num3 = ((clampWeight > 0f) ? Mathf.Clamp(1f - (clampWeight - num2) / (1f - num2), 0f, 1f) : 1f);
		float num4 = ((clampWeight > 0f) ? Mathf.Clamp(num2 / clampWeight, 0f, 1f) : 1f);
		for (int i = 0; i < clampSmoothing; i++)
		{
			float num5 = num4 * MathF.PI * 0.5f;
			num4 = Mathf.Sin(num5);
		}
		float num6 = num4 * num3;
		clampValue = 1f - num6;
		return Vector3.Slerp(normalDirection, direction, num6);
	}

	public static Vector3 LineToPlane(Vector3 origin, Vector3 direction, Vector3 planeNormal, Vector3 planePoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(planePoint - origin, planeNormal);
		float num2 = Vector3.Dot(direction, planeNormal);
		if (num2 == 0f)
		{
			return Vector3.zero;
		}
		float num3 = num / num2;
		return origin + ((Vector3)(ref direction)).normalized * num3;
	}

	public static Vector3 PointToPlane(Vector3 point, Vector3 planePosition, Vector3 planeNormal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (planeNormal == Vector3.up)
		{
			return new Vector3(point.x, planePosition.y, point.z);
		}
		Vector3 val = point - planePosition;
		Vector3 val2 = planeNormal;
		Vector3.OrthoNormalize(ref val2, ref val);
		return planePosition + Vector3.Project(point - planePosition, val);
	}

	public static Vector3 TransformPointUnscaled(Transform t, Vector3 point)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return t.position + t.rotation * point;
	}

	public static Vector3 InverseTransformPointUnscaled(Transform t, Vector3 point)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Inverse(t.rotation) * (point - t.position);
	}

	public static Vector3 InverseTransformPoint(Vector3 tPos, Quaternion tRot, Vector3 tScale, Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return Div(Quaternion.Inverse(tRot) * (point - tPos), tScale);
	}

	public static Vector3 TransformPoint(Vector3 tPos, Quaternion tRot, Vector3 tScale, Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return tPos + Vector3.Scale(tRot * point, tScale);
	}

	public static Vector3 Div(Vector3 v1, Vector3 v2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
	}
}
