using UnityEngine;

public class ProfilerControl : MonoBehaviour
{
	private const KeyCode keyCode_0 = KeyCode.F4;

	private int int_0;

	private const int int_1 = 300;

	public static void Add()
	{
		GClass852.Add<ProfilerControl>();
		Debug.Log("ProfilerControl switch key: " + KeyCode.F4);
		Debug.Log("ProfilerControl pause profiler after delayed (300 frames): mouse middle button");
	}

	public static void Remove()
	{
		GClass852.Remove<ProfilerControl>();
	}

	public static void Remove<T>() where T : MonoBehaviour
	{
		T val = GClass870.FindUnityObjectOfType<T>();
		if (val != null)
		{
			Object.Destroy(val.gameObject);
		}
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.F4))
		{
			GClass4062.enabled = !GClass4062.enabled;
		}
		if (Input.GetMouseButtonDown(2))
		{
			int_0 = Time.frameCount;
		}
		if (int_0 + 300 == Time.frameCount)
		{
			GClass4062.enabled = false;
		}
		GClass817.DebugLogsEnabled(!GClass4062.enabled);
	}
}
