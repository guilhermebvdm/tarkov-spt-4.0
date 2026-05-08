using UnityEngine;

public class ShootPointClass
{
	public float DistCoef;

	public Vector3 Point;

	public ShootPointClass(Vector3 point, float distCoef = 1f)
	{
		Point = point;
		DistCoef = distCoef;
	}
}
