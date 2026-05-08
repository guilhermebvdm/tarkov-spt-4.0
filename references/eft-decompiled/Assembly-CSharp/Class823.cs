using System;
using UnityEngine;

public abstract class Class823
{
	public static Vector3 smethod_0(Vector3 listenerPos, Vector3 preLineStart, Vector3 preLineEnd, float calculationRadius, out double spread)
	{
		if (smethod_1(listenerPos, preLineStart, preLineEnd, calculationRadius, out var newLineStart, out var newLineEnd))
		{
			double num = 1.0 / (double)calculationRadius;
			Vector3 lhs = (float)num * (newLineStart - listenerPos);
			Vector3 rhs = (float)num * (newLineEnd - newLineStart);
			double num2 = rhs.sqrMagnitude;
			double num3 = lhs.sqrMagnitude;
			double num4 = 2f * Vector3.Dot(lhs, rhs);
			double num5 = Math.Sqrt(num2 + num4 + num3);
			double num6 = Math.Sqrt(num3);
			double num7 = Math.Sqrt(num2);
			double num8 = Math.Log(2.0 * num7 * num5 + 2.0 * num2 + num4) / num7;
			double num9 = Math.Log(2.0 * num7 * num6 + num4) / num7;
			double num10 = num5 / num2 - num4 * num8 / (2.0 * num2);
			double num11 = num6 / num2 - num4 * num9 / (2.0 * num2);
			double num12 = 4.0 * num2 * num3 - num4 * num4;
			double num13 = 1.0 - (2.0 * num2 + num4) * num5 / (4.0 * num2) - num12 * num8 / (8.0 * num2);
			double num14 = (0.0 - num4 * num6) / (4.0 * num2) - num12 * num9 / (8.0 * num2);
			spread = num7 * (num13 - num14);
			if (spread <= 0.0)
			{
				spread = 0.0;
				return Vector3.zero;
			}
			double num15 = num8 - num9;
			double num16 = num10 - num11;
			double num17 = (double)lhs.x * num15 + (double)rhs.x * num16 - (double)lhs.x - (double)(rhs.x / 2f);
			double num18 = (double)lhs.y * num15 + (double)rhs.y * num16 - (double)lhs.y - (double)(rhs.y / 2f);
			double num19 = (double)lhs.z * num15 + (double)rhs.z * num16 - (double)lhs.z - (double)(rhs.z / 2f);
			Vector3 vector = new Vector3((float)num17, (float)num18, (float)num19);
			return (float)num7 * vector;
		}
		spread = 0.0;
		return Vector3.zero;
	}

	public static bool smethod_1(Vector3 listenerPos, Vector3 lineStart, Vector3 lineEnd, float calculationRadius, out Vector3 newLineStart, out Vector3 newLineEnd)
	{
		newLineStart = lineStart;
		newLineEnd = lineEnd;
		Vector3 vector = smethod_2(listenerPos, lineStart, lineEnd);
		double num = Vector3.SqrMagnitude(vector - listenerPos);
		double num2 = (double)calculationRadius * (double)calculationRadius;
		if (num + double.Epsilon >= num2)
		{
			return false;
		}
		Vector3 vector2 = lineEnd - lineStart;
		double num3 = vector2.sqrMagnitude;
		Vector3 vector3 = (float)Math.Sqrt((num2 - num) / num3) * vector2;
		if ((double)Vector3.SqrMagnitude(lineStart - listenerPos) > num2)
		{
			newLineStart = vector - vector3;
		}
		if ((double)Vector3.SqrMagnitude(lineEnd - listenerPos) > num2)
		{
			newLineEnd = vector + vector3;
		}
		return true;
	}

	public static Vector3 smethod_2(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 lhs = point - lineStart;
		Vector3 vector = lineEnd - lineStart;
		double num = Vector3.Dot(lhs, vector);
		if (num <= 0.0)
		{
			return lineStart;
		}
		double num2 = vector.sqrMagnitude;
		if (num >= num2)
		{
			return lineEnd;
		}
		return (float)(num / num2) * vector + lineStart;
	}
}
