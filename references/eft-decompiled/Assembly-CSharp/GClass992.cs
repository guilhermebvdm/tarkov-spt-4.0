using UnityEngine;

public abstract class GClass992
{
	public static void RaycastHitSort(RaycastHit[] hits)
	{
		int num = hits.Length;
		for (int i = 1; i < num; i++)
		{
			RaycastHit raycastHit = hits[i];
			float distance = raycastHit.distance;
			int num2 = i;
			int num3 = num2 - 1;
			while (num2 > 0 && !(distance >= hits[num3].distance))
			{
				hits[num2] = hits[num3];
				num2--;
				num3--;
			}
			hits[num2] = raycastHit;
		}
	}
}
