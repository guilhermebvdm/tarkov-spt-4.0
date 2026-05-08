using UnityEngine;

public class HitCameraShakerTest : MonoBehaviour
{
	public HitCameraShaker Shaker;

	public float Power = 5f;

	public void Start()
	{
		Shaker = GetComponent<HitCameraShaker>();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Keypad7))
		{
			Shaker.Hit(new Vector3(-1f, 0f, 1f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.Keypad8))
		{
			Shaker.Hit(new Vector3(0f, 0f, 1f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.Keypad9))
		{
			Shaker.Hit(new Vector3(1f, 0f, 1f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.Keypad6))
		{
			Shaker.Hit(new Vector3(1f, 0f, 0f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			Shaker.Hit(new Vector3(1f, 0f, -1f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			Shaker.Hit(new Vector3(0f, 0f, -1f).normalized, Power);
		}
		Input.GetKeyDown(KeyCode.Keypad1);
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			Shaker.Hit(new Vector3(-1f, 0f, 0f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			Shaker.Hit(new Vector3(0f, 0f, 0f).normalized, Power);
		}
		if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			Power = Mathf.Min(Power + 1f, 10f);
		}
		if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			Power = Mathf.Max(Power - 1f, 1f);
		}
	}

	public void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 100f, 30f), "Power " + Power);
	}
}
