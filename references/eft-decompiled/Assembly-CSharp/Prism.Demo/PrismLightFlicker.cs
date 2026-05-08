using UnityEngine;

namespace Prism.Demo;

[RequireComponent(typeof(Light))]
public class PrismLightFlicker : MonoBehaviour
{
	[Range(-2f, 2f)]
	public float offset = 0.3f;

	[Range(0f, 1f)]
	public float flickerChance = 0.395f;

	[Range(0f, 5f)]
	public float minAliveTime = 0.04f;

	[Range(0f, 5f)]
	public float maxAliveTime = 0.52f;

	[Range(0f, 5f)]
	public float flickerSpeed = 2f;

	private float float_0;

	private Light light_0;

	private float float_1;

	private float float_2;

	private float float_3;

	public void Start()
	{
		light_0 = GetComponent<Light>();
		float_1 = light_0.intensity;
	}

	public static float smethod_0(float start, float end, float t)
	{
		end -= start;
		return end * t * t * t + start;
	}

	public void Update()
	{
		light_0.intensity = smethod_0(light_0.intensity, float_2, float_3);
		float_3 += Time.deltaTime * flickerSpeed;
		float_3 = Mathf.Min(float_3, 1f);
		if (!(Time.time < float_0))
		{
			float num = Random.Range(0f, 1f);
			if (num > flickerChance)
			{
				num += offset;
				float_2 = float_1 * num;
				float_0 = Time.time + Random.Range(minAliveTime, maxAliveTime);
				float_3 = 0f;
			}
		}
	}
}
