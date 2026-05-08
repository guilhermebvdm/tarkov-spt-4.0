using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1340 : GInterface124, IValueListener, IDisposable
{
	[NonSerialized]
	public const int Int_0 = 0;

	[NonSerialized]
	public static int Int_1 = Animator.StringToHash("IsJumping");

	[NonSerialized]
	public static int Int_2 = Animator.StringToHash("Tilt");

	[NonSerialized]
	public static int Int_3 = Animator.StringToHash("Level");

	[NonSerialized]
	public static int Int_4 = Animator.StringToHash("Prone");

	[NonSerialized]
	public static int Int_5 = Animator.StringToHash("Sprint");

	[NonSerialized]
	public static int Int_6 = Animator.StringToHash("DeltaRotation");

	[NonSerialized]
	public GInterface123 Ginterface123_0;

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public bool IsImportantParameterChanged
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

	public GClass1340(GInterface123 playableAnimator)
	{
		Ginterface123_0 = playableAnimator;
		method_1();
	}

	public bool ProcessAnimator(bool isVisible)
	{
		if (Bool_0 != isVisible && !Ginterface123_0.ManualUpdateMode)
		{
			if (isVisible)
			{
				Ginterface123_0.Play();
			}
			else
			{
				Ginterface123_0.Stop();
			}
		}
		Bool_0 = isVisible;
		if (!isVisible && method_0())
		{
			IsImportantParameterChanged = true;
			return true;
		}
		if (!isVisible)
		{
			return IsImportantParameterChanged;
		}
		return true;
	}

	public void OnParameterValueChanged()
	{
		IsImportantParameterChanged = true;
	}

	void IValueListener.OnParameterValueChanged()
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnParameterValueChanged
		this.OnParameterValueChanged();
	}

	public bool method_0()
	{
		FastAnimatorProcessorClass fastAnimatorProcessorClass = Ginterface123_0.FastAnimator as FastAnimatorProcessorClass;
		GClass1344 stateInfo = fastAnimatorProcessorClass.FastControllerInfo.GetStateInfo(0);
		GClass1331 behavior = stateInfo.CurrentState.GetBehavior<GClass1331>();
		if (behavior.Name != EPlayerState.Prone2Stand && behavior.Name != EPlayerState.Transit2Prone)
		{
			if (!fastAnimatorProcessorClass.IsInTransition(0))
			{
				return false;
			}
			behavior = stateInfo.DestinationState.GetBehavior<GClass1331>();
			if (behavior.Name != EPlayerState.Prone2Stand)
			{
				return behavior.Name == EPlayerState.Transit2Prone;
			}
			return true;
		}
		return true;
	}

	public void method_1()
	{
		FastAnimatorControllerClass fastAnimatorController = (Ginterface123_0.FastAnimator as FastAnimatorProcessorClass).FastAnimatorController;
		fastAnimatorController.FindParameter(Int_1).AddValueListener(this);
		fastAnimatorController.FindParameter(Int_2).AddValueListener(this);
		fastAnimatorController.FindParameter(Int_3).AddValueListener(this);
		fastAnimatorController.FindParameter(Int_4).AddValueListener(this);
		fastAnimatorController.FindParameter(Int_5).AddValueListener(this);
		fastAnimatorController.FindParameter(Int_6).AddValueListener(this);
	}

	public void method_2()
	{
		FastAnimatorControllerClass fastAnimatorController = (Ginterface123_0.FastAnimator as FastAnimatorProcessorClass).FastAnimatorController;
		fastAnimatorController.FindParameter(Int_1).RemoveValueListener(this);
		fastAnimatorController.FindParameter(Int_2).RemoveValueListener(this);
		fastAnimatorController.FindParameter(Int_3).RemoveValueListener(this);
		fastAnimatorController.FindParameter(Int_4).RemoveValueListener(this);
		fastAnimatorController.FindParameter(Int_5).RemoveValueListener(this);
		fastAnimatorController.FindParameter(Int_6).RemoveValueListener(this);
	}

	public void Dispose()
	{
		method_2();
	}
}
