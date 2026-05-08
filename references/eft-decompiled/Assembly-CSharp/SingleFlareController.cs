using MultiFlare;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class SingleFlareController : MonoBehaviour
{
	private Camera camera_0;

	[SerializeField]
	private float _changeSpeed = 2f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _minAngle = 0.6f;

	[SerializeField]
	[FormerlySerializedAs("Light")]
	private FlareLight _light;

	public void OnEnable()
	{
		if ((bool)_light)
		{
			if (!GClass1023.Instance.LightStorage.Contains(_light))
			{
				GClass1023.Instance.AddLight(_light);
			}
			_light.SetAlpha(0f);
		}
	}

	public void OnDisable()
	{
		if ((bool)_light)
		{
			_light.SetAlpha(0f);
		}
	}

	public void Update()
	{
		method_0();
		if ((bool)camera_0 && (bool)_light)
		{
			float target = smethod_0(Vector3.Dot(Vector3.Normalize(camera_0.transform.position - base.transform.position), base.transform.forward), _minAngle, 1f);
			float alpha = Mathf.MoveTowards(_light.Alpha, target, Time.deltaTime * _changeSpeed);
			_light.SetAlpha(alpha);
		}
	}

	public static float smethod_0(float value, float inMin, float inMax)
	{
		return (value - inMin) / (inMax - inMin);
	}

	public void method_0()
	{
		if (!camera_0)
		{
			camera_0 = CameraClass.Instance.Camera;
			if (!camera_0)
			{
				camera_0 = Camera.main;
			}
		}
	}
}
