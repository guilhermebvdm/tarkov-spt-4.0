using UnityEngine;

namespace ChartAndGraph;

public class ChartItemGrowEffect : ChartItemEffect
{
	private const int int_1 = 0;

	private const int int_2 = 1;

	private const int int_3 = 2;

	private const int int_4 = 3;

	public float GrowMultiplier = 1.2f;

	public bool VerticalOnly;

	public float TimeScale = 1f;

	public AnimationCurve GrowEaseFunction = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve ShrinkEaseFunction = AnimationCurve.EaseInOut(1f, 1f, 0f, 0f);

	private float float_0 = 1f;

	private float float_1;

	private float float_2;

	private int int_5;

	private bool bool_0;

	public override Vector3 Vector3_0
	{
		get
		{
			if (VerticalOnly)
			{
				return new Vector3(1f, float_0, 1f);
			}
			return new Vector3(float_0, float_0, float_0);
		}
	}

	public override Quaternion Quaternion_0 => Quaternion.identity;

	public override Vector3 Vector3_1 => Vector3.zero;

	public void GrowAndShrink()
	{
		float_1 = Time.time;
		float_2 = float_0;
		int_5 = 3;
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

	public void Update()
	{
		float num = Time.time - float_1;
		num *= TimeScale;
		switch (int_5)
		{
		case 1:
		{
			method_2(GrowEaseFunction);
			float factor = GrowEaseFunction.Evaluate(num);
			float_0 = GClass1664.smethod_0(float_2, GrowMultiplier, factor);
			if (CheckAnimationEnded(num, GrowEaseFunction))
			{
				int_5 = 0;
				float_0 = GrowMultiplier;
			}
			break;
		}
		case 2:
		{
			method_2(ShrinkEaseFunction);
			float factor = ShrinkEaseFunction.Evaluate(num);
			float_0 = GClass1664.smethod_0(float_2, 1f, factor);
			if (CheckAnimationEnded(num, ShrinkEaseFunction))
			{
				int_5 = 0;
				float_0 = 1f;
			}
			break;
		}
		case 3:
		{
			method_2(GrowEaseFunction);
			float factor = GrowEaseFunction.Evaluate(num);
			float_0 = GClass1664.smethod_0(float_2, GrowMultiplier, factor);
			if (CheckAnimationEnded(num, GrowEaseFunction))
			{
				float_0 = GrowMultiplier;
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
		float_1 = Time.time;
		float_2 = float_0;
		int_5 = 1;
	}

	public void Shrink()
	{
		float_1 = Time.time;
		float_2 = float_0;
		int_5 = 2;
	}
}
