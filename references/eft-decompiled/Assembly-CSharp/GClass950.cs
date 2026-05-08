using System;
using System.Collections.Generic;
using EFT.Ballistics;
using UnityEngine;

public abstract class GClass950
{
	[NonSerialized]
	public const float Float_0 = 2.2f;

	[NonSerialized]
	public const float Float_1 = 3f;

	[NonSerialized]
	public const float Float_2 = 4f;

	[NonSerialized]
	public const float Float_3 = 1f;

	[NonSerialized]
	public const float Float_4 = 0.1f;

	[NonSerialized]
	public const float Float_5 = 9f;

	[NonSerialized]
	public const short Short_0 = 2000;

	[NonSerialized]
	public static Dictionary<Vector3, BallisticCollider> Dictionary_0 = new Dictionary<Vector3, BallisticCollider>();

	[NonSerialized]
	public static Dictionary<int, BallisticCollider> Dictionary_1 = new Dictionary<int, BallisticCollider>(2000);

	[NonSerialized]
	public static BallisticCollider BallisticCollider_0;

	[NonSerialized]
	public static Vector3 Vector3_0;

	[NonSerialized]
	public static Vector3 Vector3_1;

	[NonSerialized]
	public static Vector3 Vector3_2;

	[NonSerialized]
	public static float Float_6;

	[NonSerialized]
	public static int Int_0;

	public static float FindObstaclesEffect(RaycastHit[] hits, bool verticalCheck)
	{
		if (hits != null && hits.Length != 0)
		{
			Int_0 = 0;
			Dictionary_0.Clear();
			for (int i = 0; i < hits.Length; i++)
			{
				if ((object)hits[i].collider != null)
				{
					smethod_0(hits[i], verticalCheck);
				}
			}
			return smethod_1();
		}
		return 0f;
	}

	public static void smethod_0(RaycastHit hit, bool verticalCheck)
	{
		Collider collider = hit.collider;
		Vector3_2 = collider.bounds.size;
		Int_0 = collider.GetInstanceID();
		if ((!verticalCheck && (Vector3_2.y < 2.2f || Vector3_2.x < 3f)) || (verticalCheck && Vector3_2.magnitude < 4f))
		{
			return;
		}
		if (Dictionary_1.Count > 2000)
		{
			Dictionary_1.Clear();
		}
		if (Dictionary_1.TryGetValue(Int_0, out var value))
		{
			BallisticCollider_0 = value;
		}
		else
		{
			BallisticCollider_0 = collider.GetComponent<BallisticCollider>();
			if ((object)BallisticCollider_0 == null)
			{
				return;
			}
			Dictionary_1.Add(Int_0, BallisticCollider_0);
		}
		if (!Dictionary_0.ContainsKey(Vector3_0))
		{
			Vector3_0 = hit.point;
			Dictionary_0.Add(Vector3_0, BallisticCollider_0);
			BallisticCollider_0 = null;
		}
	}

	public static float smethod_1()
	{
		if (Dictionary_0.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (var (vector2, ballisticCollider2) in Dictionary_0)
		{
			if (Dictionary_0.Count != 1)
			{
				if (smethod_2(Vector3_1, vector2))
				{
					Float_6 = Mathf.Lerp(1f, 0.1f, ballisticCollider2.PenetrationChance);
					num += Float_6;
				}
				Vector3_1 = vector2;
				continue;
			}
			num = Mathf.Lerp(1f, 0.1f, ballisticCollider2.PenetrationChance);
			return num;
		}
		if (num <= 0f)
		{
			num = Mathf.Lerp(1f, 0.1f, Dictionary_0[Vector3_1].PenetrationChance);
		}
		return num;
	}

	public static bool smethod_2(Vector3 firstHit, Vector3 secondHit)
	{
		return Vector3.SqrMagnitude(firstHit - secondHit) > 9f;
	}

	public static void Clear()
	{
		Dictionary_1.Clear();
		Dictionary_0.Clear();
	}
}
