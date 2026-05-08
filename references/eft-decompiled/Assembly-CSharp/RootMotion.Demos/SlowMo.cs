using UnityEngine;

namespace RootMotion.Demos;

public class SlowMo : MonoBehaviour
{
	[SerializeField]
	private KeyCode[] keyCodes;

	[SerializeField]
	private bool mouse0;

	[SerializeField]
	private bool mouse1;

	[SerializeField]
	private float slowMoTimeScale = 0.3f;

	public void Update()
	{
		Time.timeScale = (method_0() ? slowMoTimeScale : 1f);
	}

	public bool method_0()
	{
		if (mouse0 && Input.GetMouseButton(0))
		{
			return true;
		}
		if (mouse1 && Input.GetMouseButton(1))
		{
			return true;
		}
		int num = 0;
		while (true)
		{
			if (num < keyCodes.Length)
			{
				if (Input.GetKey(keyCodes[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}
}
