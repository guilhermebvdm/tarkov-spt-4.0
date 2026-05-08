using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Options;

public class GClass873 : ABSTweenPlugin<GStruct56, GStruct56, NoOptions>
{
	public override void Reset(TweenerCore<GStruct56, GStruct56, NoOptions> t)
	{
	}

	public override void SetFrom(TweenerCore<GStruct56, GStruct56, NoOptions> t, bool isRelative)
	{
		GStruct56 endValue = t.endValue;
		t.endValue = t.getter();
		t.startValue = (isRelative ? (t.endValue + endValue) : endValue);
		t.setter(t.startValue);
	}

	public override void SetFrom(TweenerCore<GStruct56, GStruct56, NoOptions> t, GStruct56 fromValue, bool setImmediately)
	{
	}

	public override GStruct56 ConvertToStartValue(TweenerCore<GStruct56, GStruct56, NoOptions> t, GStruct56 value)
	{
		return value;
	}

	public override void SetRelativeEndValue(TweenerCore<GStruct56, GStruct56, NoOptions> t)
	{
		t.endValue = t.startValue + t.changeValue;
	}

	public override void SetChangeValue(TweenerCore<GStruct56, GStruct56, NoOptions> t)
	{
		t.changeValue = t.endValue - t.startValue;
	}

	public override float GetSpeedBasedDuration(NoOptions options, float unitsXSecond, GStruct56 changeValue)
	{
		return unitsXSecond;
	}

	public override void EvaluateAndApply(NoOptions options, Tween t, bool isRelative, DOGetter<GStruct56> getter, DOSetter<GStruct56> setter, float elapsed, GStruct56 startValue, GStruct56 changeValue, float duration, bool usingInversePosition, UpdateNotice updateNotice)
	{
		float num = EaseManager.Evaluate(t, elapsed, duration, t.easeOvershootOrAmplitude, t.easePeriod);
		startValue.min += changeValue.min * num;
		startValue.max += changeValue.max * num;
		setter(startValue);
	}
}
