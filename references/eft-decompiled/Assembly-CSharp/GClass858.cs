using UnityEngine;

public abstract class GClass858
{
	public static bool AllAbsComponentsLessThen(this Vector3 vector3, float value)
	{
		if (Mathf.Abs(vector3.x) < value && Mathf.Abs(vector3.y) < value)
		{
			return Mathf.Abs(vector3.z) < value;
		}
		return false;
	}

	public static bool AllAbsComponentsLessThen(this Vector2 vector2, float value)
	{
		if (Mathf.Abs(vector2.x) < value)
		{
			return Mathf.Abs(vector2.y) < value;
		}
		return false;
	}

	public static bool IsAnyComponentNaN(this Vector3 vector3)
	{
		if (!float.IsNaN(vector3.x) && !float.IsNaN(vector3.y))
		{
			return float.IsNaN(vector3.z);
		}
		return true;
	}

	public static bool IsAnyComponentNaN(this Vector2 vector2)
	{
		if (!float.IsNaN(vector2.x))
		{
			return float.IsNaN(vector2.y);
		}
		return true;
	}

	public static bool IsAnyComponentInfinity(this Vector3 vector3)
	{
		if (!float.IsInfinity(vector3.x) && !float.IsInfinity(vector3.y))
		{
			return float.IsInfinity(vector3.z);
		}
		return true;
	}

	public static bool IsAnyComponentInfinity(this Vector2 vector2)
	{
		if (!float.IsInfinity(vector2.x))
		{
			return float.IsInfinity(vector2.y);
		}
		return true;
	}

	public static bool IsAnyComponentNaN(this Quaternion quaternion)
	{
		if (!float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
		{
			return float.IsNaN(quaternion.w);
		}
		return true;
	}

	public static bool IsAnyComponentInfinity(this Quaternion quaternion)
	{
		if (!float.IsInfinity(quaternion.x) && !float.IsInfinity(quaternion.y) && !float.IsInfinity(quaternion.z))
		{
			return float.IsInfinity(quaternion.w);
		}
		return true;
	}

	public static bool Equals(Vector2 a, Vector2 b, float epsilon)
	{
		if (Mathf.Abs(a.x - b.x) < epsilon)
		{
			return Mathf.Abs(a.y - b.y) < epsilon;
		}
		return false;
	}
}
