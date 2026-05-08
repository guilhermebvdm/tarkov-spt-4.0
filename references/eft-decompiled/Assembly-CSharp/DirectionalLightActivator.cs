using UnityEngine;

public class DirectionalLightActivator : MonoBehaviour
{
	private Light[] light_0;

	public void Awake()
	{
		light_0 = GetComponentsInChildren<Light>();
		method_1(value: false);
		CameraClass.Instance.OnCameraChanged += method_0;
	}

	public void OnDestroy()
	{
		CameraClass.Instance.OnCameraChanged -= method_0;
	}

	public void method_0()
	{
		method_1(value: true);
	}

	public void method_1(bool value)
	{
		for (int i = 0; i < light_0.Length; i++)
		{
			light_0[i].enabled = value;
		}
	}
}
