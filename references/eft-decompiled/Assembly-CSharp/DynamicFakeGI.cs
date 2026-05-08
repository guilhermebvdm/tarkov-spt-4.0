using UnityEngine;

[ExecuteInEditMode]
public class DynamicFakeGI : MonoBehaviour
{
	[SerializeField]
	private float intensityMultiplier = 1f;

	[SerializeField]
	private float lightSubtract = 0.25f;

	[SerializeField]
	private Color hue = new Color(1f, 1f, 1f, 1f);

	[Range(0f, 1f)]
	public float minMaxWeatherInfluenceAmount;

	private Light light_0;

	private AreaLight areaLight_0;

	private Color color_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3;

	public void Start()
	{
		if (!areaLight_0)
		{
			areaLight_0 = GetComponent<AreaLight>();
		}
		if (!light_0)
		{
			light_0 = GetComponent<Light>();
		}
	}

	public void Update()
	{
		if (GClass4.IsAvailable)
		{
			float_3 = Mathf.Clamp(Mathf.Lerp(MonoBehaviourSingleton<TOD_Sky>.Instance.AmbientColor.grayscale - lightSubtract, 1f, 1f - minMaxWeatherInfluenceAmount) * intensityMultiplier, 0f, 500f);
			Color.RGBToHSV(hue, out float_0, out float_1, out float_2);
			color_0 = Color.HSVToRGB(float_0, float_1, float_3);
			if ((bool)areaLight_0)
			{
				areaLight_0.m_Color = color_0;
			}
			else
			{
				light_0.color = color_0;
			}
		}
	}
}
