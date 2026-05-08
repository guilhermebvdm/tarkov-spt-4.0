using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
	public float MinIntensity = 0.8f;

	public float MaxIntensity = 1.2f;

	public float FlickerRate = 5f;

	private float float_0;

	private Light light_0;

	public void Start()
	{
		float_0 = Random.Range(0f, 100f);
		light_0 = GetComponent<Light>();
	}

	public void Update()
	{
		float t = Mathf.PerlinNoise(float_0, Time.time * FlickerRate);
		if (light_0 != null)
		{
			light_0.intensity = Mathf.Lerp(MinIntensity, MaxIntensity, t);
		}
	}
}
