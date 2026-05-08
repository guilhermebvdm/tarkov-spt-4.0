using UnityEngine;

public abstract class GClass1043
{
	public static Vector3 Limit(Vector3 vec, float limit)
	{
		if (vec.x > limit)
		{
			vec.x = limit;
		}
		else if (vec.x < 0f - limit)
		{
			vec.x = 0f - limit;
		}
		if (vec.y > limit)
		{
			vec.y = limit;
		}
		else if (vec.y < 0f - limit)
		{
			vec.y = 0f - limit;
		}
		if (vec.z > limit)
		{
			vec.z = limit;
		}
		else if (vec.z < 0f - limit)
		{
			vec.z = 0f - limit;
		}
		return vec;
	}
}
