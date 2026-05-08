using UnityEngine;

namespace EFT;

public class AudioListenerConsistencyManager : MonoBehaviour
{
	private Transform transform_0;

	private Transform transform_1;

	private readonly Vector3 vector3_0 = new Vector3(0f, -1000f, 0f);

	public void Awake()
	{
		transform_1 = base.transform;
		Reset();
	}

	public void Follow(Transform target)
	{
		transform_0 = target;
		if (transform_0 == null)
		{
			Reset();
		}
	}

	public void Update()
	{
		if (transform_0 != null)
		{
			transform_1.SetPositionAndRotation(transform_0.position, transform_0.rotation);
		}
	}

	public void Reset()
	{
		transform_0 = null;
		if (transform_1 != null)
		{
			transform_1.SetPositionAndRotation(vector3_0, Quaternion.identity);
		}
	}
}
