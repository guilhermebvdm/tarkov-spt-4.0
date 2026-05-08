using UnityEngine;

public class CameraPixelPerfect : MonoBehaviour
{
	public void Start()
	{
		if (Application.isPlaying)
		{
			method_0();
		}
	}

	public void OnEnable()
	{
		method_0();
	}

	public void method_0()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			component.orthographic = true;
			component.orthographicSize = (float)Screen.height * 0.5f;
		}
	}
}
