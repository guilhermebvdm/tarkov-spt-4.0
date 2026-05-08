using System;
using UnityEngine;

public class GClass740
{
	[NonSerialized]
	public const int Int_0 = 20;

	[NonSerialized]
	public Collider[] Collider_0;

	[NonSerialized]
	public BoxCollider BoxCollider_0;

	[NonSerialized]
	public LayerMask LayerMask_0;

	public GClass740(BoxCollider collider, LayerMask mask, int maxCollidersCount = 20)
	{
		Collider_0 = new Collider[maxCollidersCount];
		BoxCollider_0 = collider;
		LayerMask_0 = mask;
	}

	public void OverlapNonAlloc(Action<Collider> colliderCallback)
	{
		int num = method_0(Collider_0);
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				colliderCallback?.Invoke(Collider_0[i]);
			}
		}
	}

	public int method_0(Collider[] results)
	{
		var (center, halfExtents, orientation) = method_1();
		return Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, LayerMask_0);
	}

	public (Vector3 center, Vector3 halfExtents, Quaternion orientation) method_1()
	{
		Transform transform = BoxCollider_0.transform;
		return (center: transform.TransformPoint(BoxCollider_0.center), halfExtents: Vector3.Scale(method_2(transform.lossyScale), BoxCollider_0.size) * 0.5f, orientation: transform.rotation);
	}

	public Vector3 method_2(Vector3 vector)
	{
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}
}
