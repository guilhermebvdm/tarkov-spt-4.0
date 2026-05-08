using System;
using UnityEngine;

public abstract class GClass823
{
	[NonSerialized]
	public static Action<Plane[], Matrix4x4> Action_0;

	[NonSerialized]
	public static Plane[] Plane_0 = new Plane[6];

	public static void CalculateFrustumPlanesNonAlloc(Camera camera, Plane[] planes)
	{
		GeometryUtility.CalculateFrustumPlanes(camera.projectionMatrix * camera.worldToCameraMatrix, planes);
	}

	public static Plane[] CalculateFrustumPlanesNonAlloc(Camera camera)
	{
		CalculateFrustumPlanesNonAlloc(camera, Plane_0);
		return Plane_0;
	}

	public static Plane[] CalculateFrustumPlanesNonAlloc(Matrix4x4 worldToProjectionMatrix)
	{
		GeometryUtility.CalculateFrustumPlanes(worldToProjectionMatrix, Plane_0);
		return Plane_0;
	}

	public static void ConvertBoundsToPlanesNonAlloc(Bounds bounds, Quaternion rotation, bool normalsIn, Plane[] planes)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		min = GClass856.RotateAroundPivot(min, bounds.center, rotation);
		max = GClass856.RotateAroundPivot(max, bounds.center, rotation);
		float num = (normalsIn ? 1 : (-1));
		planes[0] = new Plane(rotation * (num * Vector3.forward), min);
		planes[1] = new Plane(rotation * (num * Vector3.up), min);
		planes[2] = new Plane(rotation * (num * Vector3.right), min);
		num *= -1f;
		planes[3] = new Plane(rotation * (num * Vector3.forward), max);
		planes[4] = new Plane(rotation * (num * Vector3.up), max);
		planes[5] = new Plane(rotation * (num * Vector3.right), max);
	}

	public static bool IsIntersectsNonAlloc(Bounds bounds, Quaternion rotation, Bounds objectBounds, Plane[] planes)
	{
		ConvertBoundsToPlanesNonAlloc(bounds, rotation, normalsIn: true, planes);
		return GeometryUtility.TestPlanesAABB(planes, objectBounds);
	}

	public static bool IsIntersectsNonAlloc(Bounds bounds, Quaternion rotation, Bounds objectBounds)
	{
		return IsIntersectsNonAlloc(bounds, rotation, objectBounds, Plane_0);
	}
}
