using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightShadowCasterDistance : MonoBehaviour
{
	[SerializeField]
	private float _cullDistance = 100f;

	private Light light_0;

	private float[] float_0;

	public void method_0()
	{
		if (light_0 == null)
		{
			light_0 = GetComponent<Light>();
		}
		if (float_0 == null)
		{
			float_0 = new float[32];
		}
		for (int i = 0; i < float_0.Length; i++)
		{
			float_0[i] = _cullDistance;
		}
		light_0.layerShadowCullDistances = float_0;
	}

	public void Update()
	{
		method_0();
	}
}
