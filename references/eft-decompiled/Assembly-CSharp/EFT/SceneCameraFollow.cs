using UnityEngine;

namespace EFT;

public class SceneCameraFollow : MonoBehaviour
{
	private Camera camera_0;

	public Camera Camera_0
	{
		get
		{
			if (camera_0 == null)
			{
				camera_0 = GetComponent<Camera>();
			}
			return camera_0;
		}
	}

	public void Update()
	{
	}

	public static void smethod_0(Camera follower, Camera target)
	{
		Transform transform = target.transform;
		Transform obj = follower.transform;
		obj.position = transform.position;
		obj.rotation = transform.rotation;
	}
}
