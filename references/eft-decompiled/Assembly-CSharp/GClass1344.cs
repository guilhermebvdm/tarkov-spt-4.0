using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1344
{
	[NonSerialized]
	[CompilerGenerated]
	public CurrentStateAbstractClass CurrentStateAbstractClass;

	[NonSerialized]
	[CompilerGenerated]
	public TransitionClass TransitionClass;

	[NonSerialized]
	public float Float_0 = -1f;

	[NonSerialized]
	public float Float_1 = -1f;

	public float CurrentStateAbsoluteTime;

	public float TransitionAbsTime;

	public float DestinationStateAbsoluteTime;

	[NonSerialized]
	public float Float_2 = -1f;

	[NonSerialized]
	public float Float_3 = -1f;

	public GClass1312 ForcePlayState;

	public float ForcePlayStateNT;

	public CurrentStateAbstractClass CurrentState
	{
		[CompilerGenerated]
		get
		{
			return CurrentStateAbstractClass;
		}
		[CompilerGenerated]
		set
		{
			CurrentStateAbstractClass = value;
		}
	}

	public TransitionClass Transition
	{
		[CompilerGenerated]
		get
		{
			return TransitionClass;
		}
		[CompilerGenerated]
		set
		{
			TransitionClass = value;
		}
	}

	public CurrentStateAbstractClass DestinationState
	{
		get
		{
			if (Transition != null)
			{
				return Transition.DestinationState;
			}
			return null;
		}
	}

	public float CurrentStateNormalizedTime
	{
		get
		{
			if (CurrentState == null)
			{
				return -1f;
			}
			CurrentStateAbsoluteTime = Mathf.Max(0f, CurrentStateAbsoluteTime);
			if (Float_0 != CurrentStateAbsoluteTime)
			{
				Float_0 = CurrentStateAbsoluteTime;
				Float_1 = smethod_0(CurrentState, CurrentStateAbsoluteTime);
			}
			return Float_1;
		}
	}

	public float TransitionAbsDuration
	{
		get
		{
			TransitionClass transition = Transition;
			if (transition == null)
			{
				return 0f;
			}
			if (!transition.IsFixedDuration)
			{
				return transition.Duration * transition.SourceState.MotionDuration;
			}
			return transition.Duration;
		}
	}

	public float DestinationStateNormalizedTime
	{
		get
		{
			if (DestinationState == null)
			{
				return -1f;
			}
			if (Float_2 != DestinationStateAbsoluteTime)
			{
				Float_2 = DestinationStateAbsoluteTime;
				Float_3 = smethod_0(DestinationState, DestinationStateAbsoluteTime);
			}
			return Float_3;
		}
	}

	public override string ToString()
	{
		return string.Format("{0}:[NormT: {1:F5}, AbsT: {2:F5}] -> {3}:[NormT: {4:F5}, AbsT: {5:F5}]", (CurrentState != null) ? CurrentState.Name : "NULL", CurrentStateNormalizedTime, CurrentStateAbsoluteTime, (DestinationState != null) ? DestinationState.Name : "NULL", DestinationStateNormalizedTime, DestinationStateAbsoluteTime);
	}

	public static float smethod_0(CurrentStateAbstractClass state, float absoluteTime)
	{
		return absoluteTime / state.MotionDuration;
	}
}
