using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class GClass1169
{
	public static NativeArray<Vector3> GenerateFibonacciSphereDirections(int count, Allocator allocator)
	{
		NativeArray<Vector3> result = new NativeArray<Vector3>(count, allocator);
		float num = MathF.PI * (3f - Mathf.Sqrt(5f));
		for (int i = 0; i < count; i++)
		{
			float num2 = 1f - (float)i / (float)(count - 1) * 2f;
			float num3 = Mathf.Sqrt(1f - num2 * num2);
			float f = num * (float)i;
			float x = Mathf.Cos(f) * num3;
			float z = Mathf.Sin(f) * num3;
			Vector3 vector = new Vector3(x, num2, z);
			if (vector.sqrMagnitude > 0.0001f)
			{
				result[i] = vector.normalized;
				continue;
			}
			result[i] = ((num2 > 0f) ? Vector3.up : Vector3.down);
			Debug.LogWarning("[ReflectionCalculator] Near-zero direction generated in FibonacciSphere, using fallback.");
		}
		return result;
	}

	public static float CalculateDistanceAttenuation(float distanceTraveled, float maxDistance, float minEnergy)
	{
		if (maxDistance <= 0f)
		{
			return 1f;
		}
		float num = math.max(0f, distanceTraveled);
		if (num >= maxDistance)
		{
			return minEnergy;
		}
		float t = num / maxDistance;
		return math.lerp(1f, minEnergy, t);
	}
}
