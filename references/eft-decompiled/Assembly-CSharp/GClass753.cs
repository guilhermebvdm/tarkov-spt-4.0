using UnityEngine;

public abstract class GClass753
{
	public static bool Intersects(GStruct32 a, GStruct32 b)
	{
		if (smethod_0(a, b, a.Right))
		{
			return false;
		}
		if (smethod_0(a, b, a.Up))
		{
			return false;
		}
		if (smethod_0(a, b, a.Forward))
		{
			return false;
		}
		if (smethod_0(a, b, b.Right))
		{
			return false;
		}
		if (smethod_0(a, b, b.Up))
		{
			return false;
		}
		if (smethod_0(a, b, b.Forward))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Right, b.Right)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Right, b.Up)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Right, b.Forward)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Up, b.Right)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Up, b.Up)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Up, b.Forward)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Forward, b.Right)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Forward, b.Up)))
		{
			return false;
		}
		if (smethod_0(a, b, Vector3.Cross(a.Forward, b.Forward)))
		{
			return false;
		}
		return true;
	}

	public static bool smethod_0(GStruct32 a, GStruct32 b, Vector3 axis)
	{
		if (axis == Vector3.zero)
		{
			return false;
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		for (int i = 0; i < 8; i++)
		{
			float num5 = Vector3.Dot(a.GetVertex(i), axis);
			num = ((num5 < num) ? num5 : num);
			num2 = ((num5 > num2) ? num5 : num2);
			float num6 = Vector3.Dot(b.GetVertex(i), axis);
			num3 = ((num6 < num3) ? num6 : num3);
			num4 = ((num6 > num4) ? num6 : num4);
		}
		float num7 = Mathf.Max(num2, num4) - Mathf.Min(num, num3);
		float num8 = num2 - num + num4 - num3;
		return num7 >= num8;
	}
}
