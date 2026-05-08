using System;
using UnityEngine;

[Serializable]
public struct InitialPosition(Transform t, Rigidbody r)
{
	public Transform Transform = t;

	public Rigidbody Rigidbody = r;

	public Vector3 Positioin = Transform.localPosition;

	public Quaternion Rotation = Transform.localRotation;

	public bool IsKinematic = r != null && r.isKinematic;

	public void Reset()
	{
		Transform.localPosition = Positioin;
		Transform.localRotation = Rotation;
		if (!(Rigidbody == null))
		{
			Rigidbody.isKinematic = IsKinematic;
			Rigidbody.Sleep();
		}
	}
}
