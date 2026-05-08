using System;
using UnityEngine;

public class SwitchFilter : MonoBehaviour
{
	public float RotationSpeed;

	public AnimationCurve AnimationCurve;

	private Action<float> action_0;

	private float float_0;

	private float float_1;

	public void Init(Action<float> action)
	{
		action_0 = action;
	}

	public void Deinit()
	{
		action_0 = null;
		base.enabled = false;
	}

	public void Set(bool isOn, bool initial)
	{
		if (initial)
		{
			float_0 = (isOn ? 1f : 0f);
			action_0(AnimationCurve.Evaluate(float_0));
			base.enabled = false;
			return;
		}
		float_1 = (isOn ? 1f : 0f);
		if (!base.enabled && method_0())
		{
			base.enabled = true;
		}
	}

	public bool method_0()
	{
		return !Mathf.Approximately(float_0, float_1);
	}

	public void Update()
	{
		if (method_0())
		{
			float_0 = Mathf.MoveTowards(float_0, float_1, Time.deltaTime * RotationSpeed);
			action_0(AnimationCurve.Evaluate(float_0));
		}
		else
		{
			base.enabled = false;
		}
	}
}
