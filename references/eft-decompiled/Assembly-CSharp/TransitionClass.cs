using System;
using System.Runtime.CompilerServices;
using FastAnimatorSystem;

public class TransitionClass : GClass1312, IValueListener
{
	public readonly CurrentStateAbstractClass DestinationState;

	public readonly CurrentStateAbstractClass SourceState;

	public readonly bool IsMuted;

	public readonly bool IsSolo;

	public readonly bool IsFixedDuration;

	public readonly float ExitTimeNormalized;

	public readonly float Duration;

	public readonly bool HasExitTime;

	public readonly float NormalizedDestinationStateOffset;

	public readonly GClass1345[] Conditions;

	public readonly ETransitionInterruptionSource InterruptionSource;

	public readonly bool IsOrderedInterruption;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public int OrderIndex
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public override void ResetCache()
	{
		Bool_0 = true;
		base.ResetCache();
	}

	public TransitionClass(string name, int hash, CurrentStateAbstractClass sourceState, CurrentStateAbstractClass destinationState, bool isMuted, bool isSolo, bool isFixedDuration, float exitTimeNormalized, float duration, bool hasExitTime, float normalizedDestinationStateOffset, GClass1345[] conditions, ETransitionInterruptionSource interruptionSource, bool isOrderedInterruption)
		: base(name, hash)
	{
		SourceState = sourceState;
		DestinationState = destinationState;
		IsMuted = isMuted;
		IsSolo = isSolo;
		IsFixedDuration = isFixedDuration;
		ExitTimeNormalized = exitTimeNormalized;
		Duration = duration;
		HasExitTime = hasExitTime;
		NormalizedDestinationStateOffset = normalizedDestinationStateOffset;
		Conditions = conditions;
		InterruptionSource = interruptionSource;
		IsOrderedInterruption = isOrderedInterruption;
		for (int i = 0; i < conditions.Length; i++)
		{
			conditions[i].Parameter.AddValueListener(this);
		}
		Bool_0 = true;
		if (isMuted && isSolo)
		{
			throw new NotImplementedException("Can't create transition with 'isMuted && isSolo'");
		}
		if (isSolo)
		{
			throw new NotImplementedException("Can't create transition with 'isSolo == true'");
		}
	}

	public bool method_0()
	{
		if (IsMuted)
		{
			return false;
		}
		if (Bool_0)
		{
			Bool_1 = method_1();
			Bool_0 = false;
		}
		return Bool_1;
	}

	public bool method_1()
	{
		bool flag = true;
		GClass1345[] conditions = Conditions;
		int num = conditions.Length;
		int num2 = 0;
		while (flag && num2 < num)
		{
			GClass1345 gClass = conditions[num2];
			flag &= gClass.CheckThreshold();
			num2++;
		}
		return flag;
	}

	public bool method_2(float startFrameTime, float endFrameTime)
	{
		if (!HasExitTime)
		{
			return true;
		}
		float motionDuration = SourceState.MotionDuration;
		float num = -1f;
		if (startFrameTime > motionDuration)
		{
			startFrameTime %= motionDuration;
		}
		if (endFrameTime > motionDuration)
		{
			endFrameTime %= motionDuration;
			if (endFrameTime < startFrameTime)
			{
				num = endFrameTime;
				endFrameTime += motionDuration;
			}
		}
		float num2 = motionDuration * ExitTimeNormalized;
		if (startFrameTime <= num2 && num2 <= endFrameTime)
		{
			return true;
		}
		if (num >= 0f && 0f <= num2 && num2 <= num)
		{
			return true;
		}
		return false;
	}

	public void ResetTriggeredTriggers()
	{
		int num = Conditions.Length;
		for (int i = 0; i < num; i++)
		{
			SpeedAnimParamClass parameter = Conditions[i].Parameter;
			if (parameter.ValueType == ThresholdClass.EAnimatorValueType.Trigger)
			{
				parameter.SetValue(value: false);
			}
		}
	}

	public void MarkDependentParameterDirty()
	{
		Bool_0 = true;
		SourceState.MarkTransitionDirty();
	}

	public override void Dispose(bool disposing)
	{
		if (disposing && Conditions != null)
		{
			for (int i = 0; i < Conditions.Length; i++)
			{
				GClass1345 gClass = Conditions[i];
				if (gClass != null)
				{
					gClass.Dispose();
					Conditions[i] = null;
				}
			}
		}
		base.Dispose(disposing);
	}

	public override string ToString()
	{
		return string.Format(" {0} -> {1}: [mute:{3}, solo:{4}]   {2}", SourceState.Name, DestinationState.Name, method_3(), IsMuted, IsSolo);
	}

	public void OnParameterValueChanged()
	{
		MarkDependentParameterDirty();
	}

	public string method_3()
	{
		return $"NormalizedExitTime: {ExitTimeNormalized}, NormalizedDuration: {Duration}, HasExitTime: {HasExitTime}, NormalizedDestOffset {NormalizedDestinationStateOffset}";
	}
}
