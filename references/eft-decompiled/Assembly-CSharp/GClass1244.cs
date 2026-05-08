using System;
using UnityEngine;

public abstract class GClass1244
{
	public static float GetWorldSpaceBoundSizeY(this LODGroup group)
	{
		float num = 0f;
		LOD[] lODs = group.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null))
				{
					num = Mathf.Max(num, renderer.bounds.size.y);
				}
			}
		}
		return num;
	}

	public static float GetWorldSpaceSize(this LODGroup group)
	{
		Vector3 lossyScale = group.transform.lossyScale;
		return Mathf.Max(Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y)), Mathf.Abs(lossyScale.z)) * group.size;
	}

	public static bool IsVisibleByCamera(Vector3 camPos, Vector3 objPos, float fieldOfView, float worldSize, float relativeSwitchHeight, float lodBias)
	{
		float magnitude = (objPos - camPos).magnitude;
		float num = DistanceToRelativeHeight(fieldOfView, magnitude, worldSize) * lodBias;
		return relativeSwitchHeight > num;
	}

	public static float CalculateFOVHalfAngle(float fieldOfView)
	{
		return Mathf.Tan(MathF.PI / 180f * fieldOfView * 0.5f);
	}

	public static float DistanceToRelativeHeight(float fieldOfView, float distance, float size)
	{
		return size * 0.5f / (distance * CalculateFOVHalfAngle(fieldOfView));
	}

	public static float CalculateScreenRelativeMetric(float fieldOfView)
	{
		return 2f * CalculateFOVHalfAngle(fieldOfView);
	}

	public static float CalculatePerspectiveDistance(Vector3 objPosition, Vector3 camPosition, float sqrScreenRelativeMetric)
	{
		return Mathf.Sqrt((objPosition - camPosition).sqrMagnitude * sqrScreenRelativeMetric);
	}
}
