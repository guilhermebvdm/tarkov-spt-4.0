using UnityEngine;

public class CurveRotator : MonoBehaviour
{
	public Transform RotatedTransform;

	public float RotationSpeed;

	public Quaternion OffRotation;

	public Quaternion OnRotation;

	public AnimationCurve AnimationCurve;

	private float float_0;

	private bool bool_0;

	private float float_1;

	public void Set(bool isOn, bool initial)
	{
		if (initial)
		{
			float_0 = (isOn ? 1f : 0f);
			RotatedTransform.localRotation = (isOn ? OnRotation : OffRotation);
			bool_0 = false;
			return;
		}
		float_1 = (isOn ? 1f : 0f);
		if (!bool_0 && method_0())
		{
			bool_0 = true;
		}
	}

	public bool method_0()
	{
		return !Mathf.Approximately(float_0, float_1);
	}

	public void Update()
	{
		if (bool_0)
		{
			if (method_0())
			{
				float_0 = Mathf.MoveTowards(float_0, float_1, Time.deltaTime * RotationSpeed);
				RotatedTransform.localRotation = Quaternion.Lerp(OffRotation, OnRotation, AnimationCurve.Evaluate(float_0));
			}
			else
			{
				bool_0 = false;
			}
		}
	}
}
