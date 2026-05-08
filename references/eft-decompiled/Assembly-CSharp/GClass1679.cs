using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass1679
{
	public float FoliageIntersectionPercent;

	[NonSerialized]
	[CompilerGenerated]
	public Collider Collider_0;

	[NonSerialized]
	[CompilerGenerated]
	public Transform Transform_0;

	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public static Vector3Int Vector3Int_0 = new Vector3Int(2, 3, 2);

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public CapsuleCollider CapsuleCollider_0;

	[NonSerialized]
	public Transform Transform_1;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public Vector3[] Vector3_2;

	[NonSerialized]
	public HashSet<int> HashSet_0;

	[NonSerialized]
	public bool Bool_0;

	public Collider CurrentFoliageTrigger
	{
		[CompilerGenerated]
		get
		{
			return Collider_0;
		}
		[CompilerGenerated]
		set
		{
			Collider_0 = value;
		}
	}

	public Transform CurrentFoliageRoot
	{
		[CompilerGenerated]
		get
		{
			return Transform_0;
		}
		[CompilerGenerated]
		set
		{
			Transform_0 = value;
		}
	}

	public GClass1679()
	{
		int num = Vector3Int_0.x * Vector3Int_0.y * Vector3Int_0.z;
		HashSet_0 = new HashSet<int>(num);
		Vector3_2 = new Vector3[num];
	}

	public void SetOwnerCollider(CapsuleCollider playerCapsule)
	{
		CapsuleCollider_0 = playerCapsule;
	}

	public void StayInTreeUpdate(Collider enteredCollider, Collider triggerCollider, bool isAI)
	{
		if (Bool_0)
		{
			Bool_0 = false;
			method_1(triggerCollider);
			CurrentFoliageTrigger = triggerCollider;
			CurrentFoliageRoot = ((triggerCollider != null) ? triggerCollider.transform.root : null);
		}
	}

	public void LateUpdate()
	{
		if (Time.time >= Float_1 + 1f)
		{
			Float_1 = Time.time;
			Bool_0 = true;
			HashSet_0.Clear();
		}
		FoliageIntersectionPercent = (float)HashSet_0.Count * Float_2 / Float_5;
	}

	public void RecalculateLocalPoints()
	{
		if (!(CapsuleCollider_0 == null))
		{
			HashSet_0.Clear();
			method_0(CapsuleCollider_0, out Float_2, out Vector3_0);
		}
	}

	public void DrawGizmosFoliageIntersection(bool isAI, Player player)
	{
	}

	public static bool smethod_0(Vector3 pointToCheckWorld, Vector3 capsuleCenterWorld, float capsuleHeight, float capsuleRadius, Quaternion capsuleRotationWorld, Vector3 worldScale, out Vector3 pointToCheckLocal)
	{
		float num = capsuleHeight * worldScale.y;
		float num2 = capsuleRadius * ((worldScale.x + worldScale.z) * 0.5f);
		float num3 = num2 * num2;
		pointToCheckLocal = Quaternion.Inverse(capsuleRotationWorld) * (pointToCheckWorld - capsuleCenterWorld);
		float num4 = num * 0.5f - num2;
		float y = Mathf.Clamp(pointToCheckLocal.y, 0f - num4, num4);
		Vector3 vector = new Vector3(0f, y, 0f);
		return (pointToCheckLocal - vector).sqrMagnitude <= num3;
	}

	public void method_0(CapsuleCollider capsuleCollider, out float stepVolume, out Vector3 stepSize)
	{
		Vector3 min = capsuleCollider.bounds.min;
		Vector3 size = capsuleCollider.bounds.size;
		stepSize = new Vector3(size.x / (float)Vector3Int_0.x, size.y / (float)Vector3Int_0.y, size.z / (float)Vector3Int_0.z);
		stepVolume = stepSize.x * stepSize.y * stepSize.z;
		Transform_1 = capsuleCollider.transform;
		Vector3_1 = capsuleCollider.bounds.center;
		Float_3 = capsuleCollider.radius;
		Float_4 = capsuleCollider.height;
		Float_5 = 0f;
		ushort num = 0;
		float num2 = min.x + stepSize.x * 0.5f;
		for (float num3 = 0f; num3 < (float)Vector3Int_0.x; num3 += 1f)
		{
			float num4 = min.y + stepSize.y * 0.5f;
			for (float num5 = 0f; num5 < (float)Vector3Int_0.y; num5 += 1f)
			{
				float num6 = min.z + stepSize.z * 0.5f;
				for (float num7 = 0f; num7 < (float)Vector3Int_0.z; num7 += 1f)
				{
					if (smethod_0(new Vector3(num2, num4, num6), Vector3_1, Float_4, Float_3, Transform_1.rotation, Transform_1.lossyScale, out var pointToCheckLocal))
					{
						Vector3_2[num] = pointToCheckLocal;
						Float_5 += stepVolume;
						num++;
					}
					num6 += stepSize.z;
				}
				num4 += stepSize.y;
			}
			num2 += stepSize.x;
		}
	}

	public float method_1(Collider colliderToCheck)
	{
		return Mathf.Clamp01(method_2(colliderToCheck) / Float_5);
	}

	public float method_2(Collider colliderToCheck)
	{
		Vector3 lossyScale = colliderToCheck.transform.lossyScale;
		Vector3 center = colliderToCheck.bounds.center;
		float num = 0f;
		if (!(colliderToCheck is CapsuleCollider { radius: var radius } capsuleCollider))
		{
			if (!(colliderToCheck is SphereCollider { radius: var radius2 }))
			{
				return 0f;
			}
			float num2 = radius2 * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
			float num3 = num2 * num2;
			Vector3 a = Quaternion.Inverse(Transform_1.rotation) * (center - CapsuleCollider_0.bounds.center);
			for (int i = 0; i < Vector3_2.Length; i++)
			{
				Vector3 b = Vector3_2[i];
				if (GClass856.SqrDistance(a, b) <= num3)
				{
					num += Float_2;
					HashSet_0.Add(i);
				}
			}
		}
		else
		{
			for (int j = 0; j < Vector3_2.Length; j++)
			{
				if (smethod_0(Transform_1.rotation * Vector3_2[j] + CapsuleCollider_0.bounds.center, center, capsuleCollider.height, radius, capsuleCollider.transform.rotation, lossyScale, out var _))
				{
					num += Float_2;
					HashSet_0.Add(j);
				}
			}
		}
		return num;
	}
}
