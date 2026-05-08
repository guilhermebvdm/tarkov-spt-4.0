using UnityEngine;

public class TimeScaler : MonoBehaviour
{
	public bool IgnoreFixedUpdates = true;

	private float float_0;

	public Vector2 TimeScaleRange = new Vector2(0.01f, 1f);

	[Range(0.01f, 1f)]
	public float TimeScale = 1f;

	public void Awake()
	{
		float_0 = Time.fixedDeltaTime;
	}
}
