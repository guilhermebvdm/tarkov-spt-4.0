using UnityEngine;

[ExecuteAlways]
public class FakeInteriorTODController : MonoBehaviour
{
	private static readonly int int_0 = Shader.PropertyToID("_LayerIndexOffset");

	private static readonly int int_1 = Shader.PropertyToID("_TODEmissionMultiplier");

	private const float float_0 = 0.001f;

	private float float_1;

	private bool bool_0 = true;

	private bool bool_1 = true;

	private float float_2;

	[SerializeField]
	private Vector2 _minMaxEmissionMultiplier = new Vector2(0f, 3f);

	[SerializeField]
	[Range(0.01f, 3f)]
	private float _switchDuration = 0.2f;

	[SerializeField]
	private Vector2 _dayTimeRange = new Vector2(7f, 22f);

	public void LateUpdate()
	{
		if ((bool)MonoBehaviourSingleton<TOD_Sky>.Instance)
		{
			float hour = MonoBehaviourSingleton<TOD_Sky>.Instance.Cycle.Hour;
			if (method_1(hour) && bool_0)
			{
				float_2 = 0f;
				method_0(_minMaxEmissionMultiplier.x, isDay: false, isNight: true);
			}
			else if (!method_1(hour) && bool_1)
			{
				float_2 = 1f;
				method_0(_minMaxEmissionMultiplier.y, isDay: true, isNight: false);
			}
		}
	}

	public void method_0(float value, bool isDay, bool isNight)
	{
		if (float_1 < _switchDuration)
		{
			float_1 += Time.deltaTime;
		}
		float num = Mathf.Lerp(1f, value, float_1 / _switchDuration);
		if (Mathf.Abs(num - value) < 0.001f)
		{
			float_1 = 0f;
			num = value;
			bool_0 = isDay;
			bool_1 = isNight;
			Shader.SetGlobalFloat(int_0, float_2);
		}
		Shader.SetGlobalFloat(int_1, num);
	}

	public bool method_1(float value)
	{
		if (value >= Mathf.Min(_dayTimeRange.x, _dayTimeRange.y))
		{
			return value <= Mathf.Max(_dayTimeRange.x, _dayTimeRange.y);
		}
		return false;
	}
}
