using System;
using UnityEngine;

public abstract class GClass943
{
	[NonSerialized]
	public const int Int_0 = 3;

	[NonSerialized]
	public static RaycastHit RaycastHit_0;

	[NonSerialized]
	public static RaycastHit[] RaycastHit_1 = new RaycastHit[3];

	[NonSerialized]
	public static RaycastHit[] RaycastHit_2 = new RaycastHit[3];

	public static bool GetNearestHit(Vector3 startPos, Vector3 endPos, out RaycastHit hitInfo, int layerMask)
	{
		float maxDistance = Vector3.Distance(endPos, startPos);
		return GetNearestHit(startPos, endPos, out hitInfo, maxDistance, layerMask);
	}

	public static bool GetNearestHit(Vector3 startPos, Vector3 endPos, out RaycastHit hitInfo, float maxDistance, int layerMask)
	{
		hitInfo = default(RaycastHit);
		int num = Physics.RaycastNonAlloc(startPos, endPos - startPos, RaycastHit_1, maxDistance, layerMask);
		if (num == 0)
		{
			return false;
		}
		float num2 = maxDistance;
		for (int i = 0; i < num; i++)
		{
			if (RaycastHit_1[i].distance < num2)
			{
				hitInfo = RaycastHit_1[i];
				num2 = hitInfo.distance;
			}
		}
		return true;
	}

	public static RaycastHit[] GetAllHits(Vector3 startPos, Vector3 endPos, int layerMask, int maxHits = 5)
	{
		float maxDistance = Vector3.Distance(endPos, startPos);
		if (RaycastHit_2.Length < maxHits)
		{
			Array.Resize(ref RaycastHit_2, maxHits);
		}
		Physics.RaycastNonAlloc(startPos, endPos - startPos, RaycastHit_2, maxDistance, layerMask);
		return RaycastHit_2;
	}

	public static bool TryGetComponentFromRaycast<T>(Vector3 startPos, Vector3 endPos, float maxDistance, int layerMask, out T component) where T : MonoBehaviour
	{
		component = null;
		if (!GetNearestHit(startPos, endPos, out var hitInfo, maxDistance, layerMask))
		{
			return false;
		}
		if (hitInfo.collider == null)
		{
			return false;
		}
		component = hitInfo.collider.GetComponent<T>();
		return component != null;
	}
}
