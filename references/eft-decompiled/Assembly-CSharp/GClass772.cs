using System;
using UnityEngine;

public class GClass772
{
	[NonSerialized]
	public GameObject GameObject_0;

	public readonly Transform Transform;

	public readonly int Id;

	public bool IsActive;

	public GClass772(Transform transform, int id)
	{
		GameObject_0 = transform.gameObject;
		Transform = transform;
		Id = id;
		IsActive = GameObject_0.activeSelf;
	}

	public void SetActive(bool isActive)
	{
		if (isActive != IsActive)
		{
			GameObject_0.SetActive(isActive);
			IsActive = isActive;
		}
	}
}
