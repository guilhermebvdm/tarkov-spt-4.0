using System;
using UnityEngine;

public abstract class GClass372
{
	public static Vector2 ScreenCenter => new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);

	public static Vector3[] CreateVectorPartition(this Vector3 fromPos, Vector3 toPos, Vector3 upOffset, float step, bool includeOriginAndEnd, bool evenly, bool forceInsert = false)
	{
		toPos += upOffset;
		fromPos += upOffset;
		Vector3 vector = toPos - fromPos;
		float magnitude = vector.magnitude;
		if (!(magnitude < step) && step >= 0.0001f)
		{
			int num = (int)(magnitude / step);
			if (evenly)
			{
				step = magnitude / (float)(num + 1);
			}
			if (includeOriginAndEnd)
			{
				num += 2;
			}
			Vector3[] array = new Vector3[num];
			if (includeOriginAndEnd)
			{
				array[0] = fromPos;
				array[^1] = toPos;
			}
			int num2 = (includeOriginAndEnd ? 1 : 0);
			int num3 = (includeOriginAndEnd ? (array.Length - 1) : array.Length);
			Vector3 vector2 = fromPos;
			for (int i = num2; i < num3; i++)
			{
				vector2 = (array[i] = vector2 + vector.normalized * step);
			}
			return array;
		}
		GClass666.AILog("Distance between points too small");
		if (includeOriginAndEnd)
		{
			if (forceInsert)
			{
				Vector3 vector3 = fromPos + (toPos - fromPos) / 2f;
				return new Vector3[3] { fromPos, vector3, toPos };
			}
			return new Vector3[2] { fromPos, toPos };
		}
		return new Vector3[0];
	}

	public static float Distance(float xdiff, float ydiff, float zdif)
	{
		return (float)Math.Sqrt(xdiff * xdiff + ydiff * ydiff + zdif * zdif);
	}

	public static float Magnitude(Vector3 a)
	{
		return (float)Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
	}

	public static string ToStringVerbose(this Vector3 vector)
	{
		return vector.ToString("F3");
	}

	public static string ToStringHighResolution(this Vector3 vector)
	{
		return vector.ToString("F8");
	}
}
