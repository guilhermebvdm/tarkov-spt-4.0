using UnityEngine;

public class GClass1875
{
	public Vector3 Position;

	public float Radius;

	public float RadiusSqr;

	public float StartTime;

	public float Duration;

	public GClass1875(Vector3 position, float radius, float radiusSqr, float startTime = -1f, float duration = -1f)
	{
		Position = position;
		Radius = radius;
		RadiusSqr = radiusSqr;
		StartTime = startTime;
		Duration = duration;
	}
}
