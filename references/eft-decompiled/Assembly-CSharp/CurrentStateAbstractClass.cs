using System;
using System.Collections.Generic;
using FastAnimatorSystem;
using UnityEngine;

public abstract class CurrentStateAbstractClass : GClass1312, IValueListener
{
	[NonSerialized]
	public GClass1313 Gclass1313_0;

	[NonSerialized]
	public List<TransitionClass> List_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public SpeedAnimParamClass SpeedAnimParamClass;

	public List<TransitionClass> _outTransitions__WITH_EXIT_TIME;

	[NonSerialized]
	public bool Bool_0;

	public List<TransitionClass> _outTransitions__NO_EXIT_TIME;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2 = true;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public float Float_1;

	public abstract bool Loop { get; }

	public int OutTransitionsCount => List_0.Count;

	public abstract float RealMotionDuration { get; }

	public float Real => RealMotionDuration;

	public float MotionDuration
	{
		get
		{
			if (!Bool_3)
			{
				return Float_1;
			}
			if (SpeedAnimParamClass == null)
			{
				Float_1 = RealMotionDuration / Mathf.Abs(Float_0);
			}
			else
			{
				float num = Mathf.Abs(Float_0 * SpeedAnimParamClass.GetFloatValue());
				if (num <= Mathf.Epsilon)
				{
					Float_1 = 0f;
				}
				else
				{
					Float_1 = RealMotionDuration / num;
				}
			}
			Bool_3 = false;
			return Float_1;
		}
	}

	public GClass1313 ParentStateMachine => Gclass1313_0;

	public override void ResetCache()
	{
		Bool_3 = true;
		Bool_2 = true;
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].ResetCache();
		}
		base.ResetCache();
	}

	public CurrentStateAbstractClass(string name, int hash, List<TransitionClass> outTransitions, float speedMultiplier, SpeedAnimParamClass speedAnimatorParameterMultiplier)
		: base(name, hash)
	{
		List_0 = outTransitions;
		_outTransitions__NO_EXIT_TIME = new List<TransitionClass>();
		_outTransitions__WITH_EXIT_TIME = new List<TransitionClass>();
		Float_0 = speedMultiplier;
		SpeedAnimParamClass = speedAnimatorParameterMultiplier;
		if (SpeedAnimParamClass != null)
		{
			SpeedAnimParamClass.AddValueListener(this);
		}
		Bool_3 = true;
		Bool_2 = true;
	}

	public CurrentStateAbstractClass(string name, int hash, float speedMultiplier, SpeedAnimParamClass speedAnimatorParameterMultiplier)
		: this(name, hash, new List<TransitionClass>(), speedMultiplier, speedAnimatorParameterMultiplier)
	{
	}

	public TransitionClass GetOutTransition(int outTransitionIndex)
	{
		return List_0[outTransitionIndex];
	}

	public IEnumerable<TransitionClass> GetOutTransitions()
	{
		return List_0;
	}

	public void OnStateEnter(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		for (int i = 0; i < Gclass1328_0.Length; i++)
		{
			Gclass1328_0[i].OnStateEnter(fastAnimatorProcessor, in layerInfo, layerIndex);
		}
	}

	public void OnStateUpdate(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		for (int i = 0; i < Gclass1328_0.Length; i++)
		{
			Gclass1328_0[i].OnStateUpdate(fastAnimatorProcessor, in layerInfo, layerIndex);
		}
	}

	public void OnStateExit(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		for (int i = 0; i < Gclass1328_0.Length; i++)
		{
			Gclass1328_0[i].OnStateExit(fastAnimatorProcessor, in layerInfo, layerIndex);
		}
	}

	public void OnStateMove(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		for (int i = 0; i < Gclass1328_0.Length; i++)
		{
			Gclass1328_0[i].OnStateMove(fastAnimatorProcessor, in layerInfo, layerIndex);
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()}, MotionDuration: {MotionDuration} (ClipDuration: {RealMotionDuration}, Speed: {Float_0}), Loop: {Loop}";
	}

	public void OnParameterValueChanged()
	{
		Bool_3 = true;
	}

	public TransitionClass GetInterruptedTriggeredTransition(CurrentStateAbstractClass current, CurrentStateAbstractClass next, float currentStateAbsTime, float nextStateAbsTime, float dt, TransitionClass currentTransition)
	{
		if (Bool_2 || Bool_1)
		{
			bool bool_ = false;
			List<TransitionClass> outTransitions__NO_EXIT_TIME = _outTransitions__NO_EXIT_TIME;
			if (outTransitions__NO_EXIT_TIME != null)
			{
				int count = outTransitions__NO_EXIT_TIME.Count;
				for (int i = 0; i < count; i++)
				{
					TransitionClass transitionClass = outTransitions__NO_EXIT_TIME[i];
					if (currentTransition != transitionClass && transitionClass.method_0())
					{
						Bool_1 = true;
						if (!method_0(currentTransition, transitionClass))
						{
							return transitionClass;
						}
						bool_ = true;
					}
				}
			}
			Bool_1 = bool_;
		}
		if (Bool_2 || Bool_0)
		{
			Bool_0 = false;
			List<TransitionClass> outTransitions__WITH_EXIT_TIME = _outTransitions__WITH_EXIT_TIME;
			if (outTransitions__WITH_EXIT_TIME != null)
			{
				int count2 = outTransitions__WITH_EXIT_TIME.Count;
				for (int j = 0; j < count2; j++)
				{
					TransitionClass transitionClass2 = outTransitions__WITH_EXIT_TIME[j];
					bool flag = currentTransition != transitionClass2 && transitionClass2.method_0();
					Bool_0 |= flag;
					if (!method_0(currentTransition, transitionClass2))
					{
						float num = 0f;
						if (transitionClass2.SourceState == current)
						{
							num = currentStateAbsTime;
						}
						else if (transitionClass2.SourceState == next)
						{
							num = nextStateAbsTime;
						}
						else
						{
							flag = false;
						}
						float endFrameTime = num + dt;
						if (flag && transitionClass2.method_2(num, endFrameTime))
						{
							return transitionClass2;
						}
					}
				}
			}
		}
		Bool_2 = false;
		return null;
	}

	public TransitionClass GetTriggeredTransition(float absoluteStartFrameTime, float absoluteEndFrameTime)
	{
		if (Bool_2 || Bool_1)
		{
			List<TransitionClass> outTransitions__NO_EXIT_TIME = _outTransitions__NO_EXIT_TIME;
			if (outTransitions__NO_EXIT_TIME != null)
			{
				int count = outTransitions__NO_EXIT_TIME.Count;
				for (int i = 0; i < count; i++)
				{
					TransitionClass transitionClass = outTransitions__NO_EXIT_TIME[i];
					if (transitionClass.method_0())
					{
						Bool_1 = true;
						return transitionClass;
					}
				}
			}
			Bool_1 = false;
		}
		if (Bool_2 || Bool_0)
		{
			Bool_0 = false;
			List<TransitionClass> outTransitions__WITH_EXIT_TIME = _outTransitions__WITH_EXIT_TIME;
			if (outTransitions__WITH_EXIT_TIME != null)
			{
				int count2 = outTransitions__WITH_EXIT_TIME.Count;
				for (int j = 0; j < count2; j++)
				{
					TransitionClass transitionClass2 = outTransitions__WITH_EXIT_TIME[j];
					bool flag = transitionClass2.method_0();
					Bool_0 |= flag;
					if (flag && transitionClass2.method_2(absoluteStartFrameTime, absoluteEndFrameTime))
					{
						return transitionClass2;
					}
				}
			}
		}
		Bool_2 = false;
		return null;
	}

	public bool method_0(TransitionClass currentTransition, TransitionClass transition)
	{
		if (!currentTransition.IsOrderedInterruption)
		{
			return false;
		}
		if (currentTransition.SourceState == transition.SourceState && currentTransition.OrderIndex < transition.OrderIndex)
		{
			return true;
		}
		if (currentTransition.SourceState != transition.SourceState)
		{
			return currentTransition.InterruptionSource == ETransitionInterruptionSource.SourceThenDestination;
		}
		return false;
	}

	public override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_outTransitions__WITH_EXIT_TIME != null)
			{
				for (int i = 0; i < _outTransitions__WITH_EXIT_TIME.Count; i++)
				{
					_outTransitions__WITH_EXIT_TIME[i] = null;
				}
				_outTransitions__WITH_EXIT_TIME.Clear();
				_outTransitions__WITH_EXIT_TIME = null;
			}
			if (_outTransitions__NO_EXIT_TIME != null)
			{
				for (int j = 0; j < _outTransitions__NO_EXIT_TIME.Count; j++)
				{
					_outTransitions__NO_EXIT_TIME[j] = null;
				}
				_outTransitions__NO_EXIT_TIME.Clear();
				_outTransitions__NO_EXIT_TIME = null;
			}
			if (SpeedAnimParamClass != null)
			{
				SpeedAnimParamClass.RemoveValueListener(this);
			}
			List_0?.Clear();
			List_0 = null;
			Gclass1313_0 = null;
		}
		base.Dispose(disposing);
	}

	public void MarkDependentParameterDirty()
	{
		Bool_3 = true;
	}

	public void MarkTransitionDirty()
	{
		Bool_2 = true;
	}
}
