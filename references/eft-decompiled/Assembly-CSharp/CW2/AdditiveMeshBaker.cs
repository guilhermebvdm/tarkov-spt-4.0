using UnityEngine;

namespace CW2;

public class AdditiveMeshBaker : MonoBehaviour
{
	public bool SaveCollider;

	public bool DontDesttroyComponents = true;

	[HideInInspector]
	public Rigidbody Rigidbody;

	public void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		if (Rigidbody != null && !Rigidbody.isKinematic && Rigidbody.IsSleeping())
		{
			GClass1042.Add(base.gameObject, SaveCollider, DontDesttroyComponents);
		}
	}
}
