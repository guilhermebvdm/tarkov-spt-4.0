using System;
using UnityEngine;

public class MultiplicitySetter : MonoBehaviour
{
	public Camera Camera;

	public float BaseFov;

	public float Multiplicity;

	private float float_0;

	public void Start()
	{
		float_0 = Mathf.Tan(BaseFov * (MathF.PI / 180f));
		if (Camera == null)
		{
			Camera = GetComponent<Camera>();
		}
		if (!(Camera == null))
		{
			SetMultiplicity(Multiplicity);
		}
	}

	public void OnValidate()
	{
		Start();
	}

	public void SetMultiplicity(float multiplicity)
	{
		Camera.fieldOfView = method_0(multiplicity);
	}

	public float method_0(float multiplicity)
	{
		return 57.29578f * Mathf.Atan(float_0 / multiplicity);
	}

	public float method_1(float fov)
	{
		return float_0 / Mathf.Tan(fov * (MathF.PI / 180f));
	}
}
