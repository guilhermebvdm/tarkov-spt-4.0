using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Class687
{
	public enum EState
	{
		Off = 0,
		On = 1,
		SwitchingOff = 3,
		SwitchingOn = 2
	}

	[NonSerialized]
	public AnimationCurve AnimationCurve_0;

	[NonSerialized]
	public AnimationCurve AnimationCurve_1;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public SortedList<float, Action> SortedList_0;

	[NonSerialized]
	public SortedList<float, Action> SortedList_1;

	[NonSerialized]
	public IEnumerator<KeyValuePair<float, Action>> Ienumerator_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public EState Estate_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_4;

	public bool On
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool InProcess
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public float Value
	{
		[CompilerGenerated]
		get
		{
			return Float_4;
		}
		[CompilerGenerated]
		set
		{
			Float_4 = value;
		}
	}

	public AnimationCurve SwitchingOn
	{
		get
		{
			return AnimationCurve_0;
		}
		set
		{
			if (!value.Equals(AnimationCurve_0))
			{
				AnimationCurve_0 = value;
				Float_0 = AnimationCurve_0[AnimationCurve_0.length - 1].time;
			}
		}
	}

	public AnimationCurve SwitchingOff
	{
		get
		{
			return AnimationCurve_1;
		}
		set
		{
			if (!value.Equals(AnimationCurve_1))
			{
				AnimationCurve_1 = value;
				Float_1 = AnimationCurve_1[AnimationCurve_1.length - 1].time;
			}
		}
	}

	public void Process(float deltaTime)
	{
		if (!InProcess)
		{
			return;
		}
		Float_2 += deltaTime;
		if (Estate_0 == EState.SwitchingOn)
		{
			Value = SwitchingOn.Evaluate(Float_2);
			method_1();
			if (Float_2 >= Float_0)
			{
				Estate_0 = EState.On;
				On = true;
				InProcess = false;
			}
		}
		else
		{
			Value = SwitchingOff.Evaluate(Float_2);
			method_1();
			if (Float_2 >= Float_1)
			{
				Estate_0 = EState.Off;
				On = false;
				InProcess = false;
			}
		}
	}

	public bool Switch(bool on)
	{
		if (on == (InProcess ? (!On) : On))
		{
			return false;
		}
		Float_2 = 0f;
		Estate_0 = (on ? EState.SwitchingOn : EState.SwitchingOff);
		InProcess = true;
		if (method_0(on) == null)
		{
			return true;
		}
		Ienumerator_0 = method_0(on).GetEnumerator();
		Ienumerator_0.MoveNext();
		Float_3 = Ienumerator_0.Current.Key;
		return true;
	}

	public void ForceSwitch(bool on)
	{
		Float_2 = 0f;
		Estate_0 = (on ? EState.On : EState.Off);
		On = on;
		InProcess = false;
	}

	public void AddCase(float time, Action stateChangedCallback, bool switchingOn)
	{
		if (method_0(switchingOn) == null)
		{
			if (switchingOn)
			{
				SortedList_0 = new SortedList<float, Action>();
			}
			else
			{
				SortedList_1 = new SortedList<float, Action>();
			}
		}
		method_0(switchingOn).Add(time, stateChangedCallback);
	}

	public SortedList<float, Action> method_0(bool on)
	{
		if (!on)
		{
			return SortedList_1;
		}
		return SortedList_0;
	}

	public void method_1()
	{
		if (Ienumerator_0 != null && !(Float_3 >= Float_2))
		{
			Ienumerator_0.Current.Value();
			if (Ienumerator_0.MoveNext())
			{
				Float_3 = Ienumerator_0.Current.Key;
			}
			else
			{
				Ienumerator_0 = null;
			}
		}
	}
}
