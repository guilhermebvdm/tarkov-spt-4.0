using UnityEngine;

public abstract class GClass1242
{
	public static bool Contains(this Bounds bounds, Bounds target)
	{
		if (bounds.Contains(target.min))
		{
			return bounds.Contains(target.max);
		}
		return false;
	}

	public static float GetPerimeter(this Bounds bounds)
	{
		float num = bounds.max.x - bounds.min.x;
		float num2 = bounds.max.y - bounds.min.y;
		return 2f * (num + num2);
	}

	public static float GetSurfaceArea(this Bounds box)
	{
		float num = box.max.x - box.min.x;
		float num2 = box.max.y - box.min.y;
		float num3 = box.max.z - box.min.z;
		return 2f * (num * num2 + num * num3 + num2 * num3);
	}

	public static float Volume(this Bounds bounds)
	{
		return bounds.size.x * bounds.size.y * bounds.size.z;
	}

	public static Bounds ClampInside(this Bounds bounds, Bounds other)
	{
		Bounds result = bounds;
		result.min = Vector3.Max(bounds.min, other.min);
		result.max = Vector3.Min(bounds.max, other.max);
		return result;
	}

	public static (Bounds a, Bounds b, Bounds c, Bounds d, Bounds e, Bounds f, Bounds g, Bounds h) SplitBy8(this Bounds original)
	{
		Vector3 vector = original.size * 0.5f;
		Vector3 vector2 = vector * 0.5f;
		return (a: new Bounds(original.center + new Vector3(vector2.x, vector2.y, vector2.z), vector), b: new Bounds(original.center + new Vector3(vector2.x, vector2.y, 0f - vector2.z), vector), c: new Bounds(original.center + new Vector3(vector2.x, 0f - vector2.y, vector2.z), vector), d: new Bounds(original.center + new Vector3(vector2.x, 0f - vector2.y, 0f - vector2.z), vector), e: new Bounds(original.center + new Vector3(0f - vector2.x, vector2.y, vector2.z), vector), f: new Bounds(original.center + new Vector3(0f - vector2.x, vector2.y, 0f - vector2.z), vector), g: new Bounds(original.center + new Vector3(0f - vector2.x, 0f - vector2.y, vector2.z), vector), h: new Bounds(original.center + new Vector3(0f - vector2.x, 0f - vector2.y, 0f - vector2.z), vector));
	}
}
