using System;
using UnityEngine;

public class GClass916
{
	[NonSerialized]
	public Vector2[] Vector2_0;

	public static GClass916 smethod_0(Vector2[] points)
	{
		if (points == null || points.Length < 3)
		{
			throw new ArgumentException("points");
		}
		return new GClass916(points);
	}

	public GClass916(Vector2[] points)
	{
		Vector2_0 = points;
	}

	public bool Contains(float x, float z)
	{
		bool flag = false;
		int num = Vector2_0.Length - 1;
		for (int i = 0; i < Vector2_0.Length; i++)
		{
			Vector2 vector = Vector2_0[i];
			Vector2 vector2 = Vector2_0[num];
			if (((vector.y < z && vector2.y >= z) || (vector2.y < z && vector.y >= z)) && vector.x + (z - vector.y) / (vector2.y - vector.y) * (vector2.x - vector.x) < x)
			{
				flag = !flag;
			}
			num = i;
		}
		return flag;
	}

	public bool Intersects(Bounds bounds)
	{
		Vector2 vector = new Vector2(bounds.min.x, bounds.min.z);
		Vector2 vector2 = new Vector2(bounds.min.x, bounds.max.z);
		Vector2 vector3 = new Vector2(bounds.max.x, bounds.min.z);
		Vector2 vector4 = new Vector2(bounds.max.x, bounds.max.z);
		if (Contains(vector.x, vector.y))
		{
			return true;
		}
		if (Contains(vector2.x, vector2.y))
		{
			return true;
		}
		if (Contains(vector3.x, vector3.y))
		{
			return true;
		}
		if (Contains(vector4.x, vector4.y))
		{
			return true;
		}
		int num = Vector2_0.Length - 1;
		int num2 = 0;
		while (true)
		{
			if (num2 < Vector2_0.Length)
			{
				Vector2 p = Vector2_0[num];
				Vector2 p2 = Vector2_0[num2];
				if (bounds.Contains(new Vector3(p.x, bounds.center.y, p.y)) || bounds.Contains(new Vector3(p2.x, bounds.center.y, p2.y)) || smethod_1(p, p2, vector, vector2) || smethod_1(p, p2, vector2, vector4) || smethod_1(p, p2, vector4, vector3) || smethod_1(p, p2, vector3, vector))
				{
					break;
				}
				num = num2;
				num2++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static bool smethod_1(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
	{
		Vector2 vector = p2 - p1;
		Vector2 vector2 = p3 - p4;
		Vector2 vector3 = p1 - p3;
		float num = vector2.y * vector3.x - vector2.x * vector3.y;
		float num2 = vector.y * vector2.x - vector.x * vector2.y;
		float num3 = vector.x * vector3.y - vector.y * vector3.x;
		float num4 = vector.y * vector2.x - vector.x * vector2.y;
		bool flag = true;
		if (num2 != 0f && num4 != 0f)
		{
			if (num2 > 0f)
			{
				if (num < 0f || num > num2)
				{
					flag = false;
				}
			}
			else if (num > 0f || num < num2)
			{
				flag = false;
			}
			if (flag && num4 > 0f)
			{
				if (num3 < 0f || num3 > num4)
				{
					flag = false;
				}
			}
			else if (num3 > 0f || num3 < num4)
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		return flag;
	}
}
