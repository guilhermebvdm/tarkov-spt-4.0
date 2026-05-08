using UnityEngine;

public struct RaycastHitData
{
	public bool hasHit;

	public Vector3 normal;

	public Vector3 point;

	public static implicit operator RaycastHitData(RaycastHit hit)
	{
		return new RaycastHitData
		{
			normal = hit.normal,
			point = hit.point,
			hasHit = (hit.collider != null)
		};
	}
}
