using System;

public class GClass1345 : IDisposable
{
	public delegate bool Delegate3(SpeedAnimParamClass animatorParameter, ThresholdClass threshold);

	public enum EConditionModes
	{
		Equal,
		NotEqual,
		GreaterThan,
		LessThan
	}

	public enum EConditionDelegateType
	{
		EqualInt,
		EqualBool,
		EqualTrigger,
		GreaterThanInt,
		GreaterThanFloat,
		LessThanInt,
		LessThanFloat,
		NotEqualInt,
		Length
	}

	[NonSerialized]
	public static Delegate3[] Delegate3_0;

	public readonly SpeedAnimParamClass Parameter;

	public readonly ThresholdClass Threshold;

	public readonly EConditionModes ConditionMode;

	[NonSerialized]
	public int Int_0;

	static GClass1345()
	{
		Delegate3_0 = new Delegate3[8];
		Delegate3_0[0] = smethod_0;
		Delegate3_0[1] = smethod_2;
		Delegate3_0[2] = smethod_3;
		Delegate3_0[3] = smethod_4;
		Delegate3_0[4] = smethod_5;
		Delegate3_0[5] = smethod_6;
		Delegate3_0[6] = smethod_7;
		Delegate3_0[7] = smethod_1;
	}

	public static bool smethod_0(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetIntValue() == threshold.GetIntValue();
	}

	public static bool smethod_1(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetIntValue() != threshold.GetIntValue();
	}

	public static bool smethod_2(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetBoolValue() == threshold.GetBoolValue();
	}

	public static bool smethod_3(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetBoolValue();
	}

	public static bool smethod_4(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetIntValue() > threshold.GetIntValue();
	}

	public static bool smethod_5(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetFloatValue() > threshold.GetFloatValue();
	}

	public static bool smethod_6(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetIntValue() < threshold.GetIntValue();
	}

	public static bool smethod_7(SpeedAnimParamClass animatorParameter, ThresholdClass threshold)
	{
		return animatorParameter.GetFloatValue() < threshold.GetFloatValue();
	}

	public GClass1345(SpeedAnimParamClass parameter, ThresholdClass threshold, EConditionModes conditionMode)
	{
		Parameter = parameter;
		Threshold = threshold;
		ConditionMode = conditionMode;
		Int_0 = (int)smethod_8(ConditionMode, Parameter, Threshold);
	}

	public static EConditionDelegateType smethod_8(EConditionModes conditionMode, SpeedAnimParamClass parameter, ThresholdClass threshold)
	{
		switch (conditionMode)
		{
		default:
			throw new NotImplementedException($"Can't generate check delegate, for {parameter.Name} {conditionMode} {threshold.GetStringValue()}");
		case EConditionModes.Equal:
			return parameter.ValueType switch
			{
				ThresholdClass.EAnimatorValueType.Int => EConditionDelegateType.EqualInt, 
				ThresholdClass.EAnimatorValueType.Boolean => EConditionDelegateType.EqualBool, 
				ThresholdClass.EAnimatorValueType.Trigger => EConditionDelegateType.EqualTrigger, 
				_ => throw new NotImplementedException($"Can't generate check delegate, for {parameter.Name} {conditionMode} {threshold.GetStringValue()}"), 
			};
		case EConditionModes.NotEqual:
			if (parameter.ValueType == ThresholdClass.EAnimatorValueType.Int)
			{
				return EConditionDelegateType.NotEqualInt;
			}
			throw new NotImplementedException($"Can't generate check delegate, for {parameter.Name} {conditionMode} {threshold.GetStringValue()}");
		case EConditionModes.GreaterThan:
			return parameter.ValueType switch
			{
				ThresholdClass.EAnimatorValueType.Float => EConditionDelegateType.GreaterThanFloat, 
				ThresholdClass.EAnimatorValueType.Int => EConditionDelegateType.GreaterThanInt, 
				_ => throw new NotImplementedException($"Can't generate check delegate, for {parameter.Name} {conditionMode} {threshold.GetStringValue()}"), 
			};
		case EConditionModes.LessThan:
			return parameter.ValueType switch
			{
				ThresholdClass.EAnimatorValueType.Float => EConditionDelegateType.LessThanFloat, 
				ThresholdClass.EAnimatorValueType.Int => EConditionDelegateType.LessThanInt, 
				_ => throw new NotImplementedException($"Can't generate check delegate, for {parameter.Name} {conditionMode} {threshold.GetStringValue()}"), 
			};
		}
	}

	public bool CheckThreshold()
	{
		return Delegate3_0[Int_0](Parameter, Threshold);
	}

	public override string ToString()
	{
		return string.Format("[{3}]{0} {1} {2}", Parameter.Name, ConditionMode, Threshold.GetStringValue(), Parameter.ValueType);
	}

	public void Dispose()
	{
		if (Threshold != null)
		{
			Threshold.Dispose();
		}
	}
}
