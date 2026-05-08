using Unity.Mathematics;
using UnityEngine;

public struct GStruct90
{
	public float pathLength;

	public Vector3 diffractionPoint;

	public float energyFactor;

	public bool isDirectPath;

	public static GStruct90 NoPath => new GStruct90
	{
		pathLength = float.MaxValue,
		diffractionPoint = Vector3.zero,
		energyFactor = 0f,
		isDirectPath = false
	};

	public bool FoundPath => pathLength < float.MaxValue;

	public static GStruct90 DirectPath(float directDistance, float3 endPoint)
	{
		return new GStruct90
		{
			pathLength = directDistance,
			diffractionPoint = endPoint,
			energyFactor = 1f,
			isDirectPath = true
		};
	}
}
