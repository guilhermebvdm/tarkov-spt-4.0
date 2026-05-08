using System;
using Unity.Mathematics;

public abstract class Class805
{
	[NonSerialized]
	public const float Float_0 = MathF.PI * 2f;

	public static float3 ComputeMainDirection(float3 listenerPos, float3 sourcePos, float minDistanceThreshold)
	{
		float3 float5 = sourcePos - listenerPos;
		float num = math.length(float5);
		if (!(num > minDistanceThreshold))
		{
			return float3.zero;
		}
		return float5 / num;
	}

	public static void ComputeHorizontalAxes(float3 mainDirection, float minDistanceThreshold, out float3 forwardXZ, out float3 rightXZ)
	{
		float3 float5 = new float3(mainDirection.x, 0f, mainDirection.z);
		float num = math.dot(float5, float5);
		if (num > minDistanceThreshold)
		{
			forwardXZ = float5 / math.sqrt(num);
		}
		else
		{
			forwardXZ = new float3(0f, 0f, 1f);
		}
		rightXZ = new float3(forwardXZ.z, 0f, 0f - forwardXZ.x);
	}

	public static float3 CalculateRayOffset(int rayIndex, int wideningRayCount, float wideningRadius, float3 forwardXZ, float3 rightXZ)
	{
		if (rayIndex > 0 && wideningRayCount > 0)
		{
			float x = (float)(rayIndex - 1) / (float)wideningRayCount * (MathF.PI * 2f);
			float num = math.cos(x);
			float num2 = math.sin(x);
			return (rightXZ * num + forwardXZ * num2) * wideningRadius;
		}
		return float3.zero;
	}

	public static float3 ApplyHeightOffset(float3 basePosition, float height)
	{
		return new float3(basePosition.x, basePosition.y + height, basePosition.z);
	}

	public static float ComputeDistanceSq(float3 a, float3 b)
	{
		float3 obj = b - a;
		return math.dot(obj, obj);
	}

	public static float ComputeCastDistance(float distSq)
	{
		return math.sqrt(distSq);
	}

	public static bool IsValidCastDistance(float distSq, float sphereCastRadius, float maxDistance)
	{
		float num = sphereCastRadius * sphereCastRadius;
		float num2 = maxDistance * maxDistance;
		if (distSq > num)
		{
			return distSq < num2;
		}
		return false;
	}
}
