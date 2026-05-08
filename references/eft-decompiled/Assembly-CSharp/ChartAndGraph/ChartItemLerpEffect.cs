using UnityEngine;

namespace ChartAndGraph;

public abstract class ChartItemLerpEffect : ChartItemEffect
{
	private const int int_1 = 0;

	private const int int_2 = 1;

	private const int int_3 = 2;

	private const int int_4 = 3;

	public float TimeScale = 1f;

	public AnimationCurve GrowEaseFunction = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve ShrinkEaseFunction = AnimationCurve.EaseInOut(1f, 1f, 0f, 0f);

	private float float_0;

	private float float_1;

	private int int_5;

	private bool bool_0;

	public override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	public void GrowAndShrink()
	{
		float_0 = Time.time;
		float_1 = Mathf.Clamp(GetStartValue(), 0f, 1f);
		int_5 = 3;
		base.enabled = true;
	}

	public bool CheckAnimationEnded(float time, AnimationCurve curve)
	{
		if (curve.length == 0)
		{
			return true;
		}
		bool num = time > curve.keys[curve.length - 1].time;
		if (num && bool_0)
		{
			RaiseDeactivated();
			base.gameObject.SetActive(value: false);
			bool_0 = false;
		}
		return num;
	}

	public void method_2(AnimationCurve curve)
	{
		curve.postWrapMode = WrapMode.Once;
		curve.preWrapMode = WrapMode.Once;
	}

	public abstract void ApplyLerp(float value);

	public abstract float GetStartValue();

	public void Update()
	{
		float num = Time.time - float_0;
		num *= TimeScale;
		switch (int_5)
		{
		case 1:
		{
			method_2(GrowEaseFunction);
			float t = GrowEaseFunction.Evaluate(num);
			t = Mathf.Lerp(float_1, 1f, t);
			ApplyLerp(t);
			if (CheckAnimationEnded(num, GrowEaseFunction))
			{
				int_5 = 0;
				ApplyLerp(1f);
				base.enabled = false;
			}
			break;
		}
		case 2:
		{
			method_2(ShrinkEaseFunction);
			float t = ShrinkEaseFunction.Evaluate(num);
			t = Mathf.Lerp(float_1, 0f, t);
			ApplyLerp(t);
			if (CheckAnimationEnded(num, ShrinkEaseFunction))
			{
				int_5 = 0;
				ApplyLerp(0f);
				base.enabled = false;
			}
			break;
		}
		case 3:
		{
			method_2(GrowEaseFunction);
			float t = GrowEaseFunction.Evaluate(num);
			t = Mathf.Lerp(float_1, 1f, t);
			ApplyLerp(t);
			if (CheckAnimationEnded(num, GrowEaseFunction))
			{
				ApplyLerp(1f);
				Shrink();
			}
			break;
		}
		}
	}

	public override void TriggerOut(bool deactivateOnEnd)
	{
		bool_0 = deactivateOnEnd;
		Shrink();
	}

	public override void TriggerIn(bool deactivateOnEnd)
	{
		bool_0 = deactivateOnEnd;
		Grow();
	}

	public void Grow()
	{
		float_0 = Time.time;
		float_1 = Mathf.Clamp(GetStartValue(), 0f, 1f);
		int_5 = 1;
		base.enabled = true;
	}

	public void Shrink()
	{
		float_0 = Time.time;
		float_1 = Mathf.Clamp(GetStartValue(), 0f, 1f);
		int_5 = 2;
		base.enabled = true;
	}

	public ChartItemLerpEffect()
	{
	}
}
