using CW2;
using UnityEngine;

public class GarbageEmitter : MonoBehaviour
{
	public GameObject[] Garbage;

	public float Speed = 1f;

	public float Delay = 0.1f;

	public bool SaveCollider = true;

	public bool DontDesttroyComponents;

	private float float_0;

	public void Update()
	{
		if (Input.GetMouseButton(0) && float_0 < Time.time)
		{
			float_0 = Time.time + Delay;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			GameObject prefab = Garbage[Random.Range(0, Garbage.Length)];
			method_0(prefab, ray.origin, ray.direction * Speed);
		}
	}

	public void method_0(GameObject prefab, Vector3 position, Vector3 direction)
	{
		GameObject gameObject = Object.Instantiate(prefab, position, Random.rotation);
		gameObject.transform.SetParent(base.transform, worldPositionStays: true);
		if (gameObject.GetComponent<Collider>() == null)
		{
			gameObject.AddComponent<MeshCollider>().convex = true;
		}
		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		rigidbody.velocity = direction;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		AdditiveMeshBaker additiveMeshBaker = gameObject.AddComponent<AdditiveMeshBaker>();
		additiveMeshBaker.SaveCollider = true;
		additiveMeshBaker.DontDesttroyComponents = false;
	}
}
