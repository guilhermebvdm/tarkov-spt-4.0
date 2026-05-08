using UnityEngine;

public class FastBlurTest : MonoBehaviour
{
	public float Power = 5f;

	private FastBlur fastBlur_0;

	public void Start()
	{
		fastBlur_0 = GetComponent<FastBlur>();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			fastBlur_0.Hit(Power);
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			fastBlur_0.Die();
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			fastBlur_0.Reset();
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
