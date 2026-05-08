using System;
using System.Runtime.CompilerServices;
using Audio.AuxiliaryAudioUtils;
using UnityEngine;

public class GClass1126 : IDisposable
{
	[NonSerialized]
	public const float Float_0 = 25f;

	[NonSerialized]
	public GClass1141 Gclass1141_0;

	[NonSerialized]
	public SourceContainerClass SourceContainerClass;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public GClass1180 Gclass1180_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public EAudioSourcePriority EaudioSourcePriority_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	public bool IsValid => SourceContainerClass.IsValid;

	public EAudioSourcePriority Priority
	{
		[CompilerGenerated]
		get
		{
			return EaudioSourcePriority_0;
		}
		[CompilerGenerated]
		set
		{
			EaudioSourcePriority_0 = value;
		}
	}

	public int FramesToSkip
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

	public int SkippedFrames
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public GClass1126(GClass1180 priorityCalculator, SourceContainerClass sourceContainer, Transform listenerTransform, LoggerClass logger)
	{
		Gclass1180_0 = priorityCalculator;
		Gclass1141_0 = GClass1141.Create(listenerTransform, sourceContainer.OcclusionTest, logger);
		Gclass1141_0.SetAudioSourceContainer(sourceContainer);
		SourceContainerClass = sourceContainer;
		Transform_0 = listenerTransform;
		UpdatePriority();
		method_5();
		method_0();
	}

	public void method_0()
	{
		SourceContainerClass.OnSourceAdded += method_1;
		SourceContainerClass.OnSourcePlayed += method_2;
		SourceContainerClass.OnSourceRemoved += UpdatePriority;
		if (!SourceContainerClass.IsLongTermSupported)
		{
			SourceContainerClass.OnContainerEmpty += method_3;
		}
	}

	public void ManualUpdate()
	{
		if (IsValid)
		{
			UpdatePriority();
			if (Priority != EAudioSourcePriority.OutOfRange)
			{
				Gclass1141_0.ManualUpdate();
			}
		}
	}

	public void ManualLateUpdate()
	{
		if (Priority != EAudioSourcePriority.OutOfRange)
		{
			Gclass1141_0.ManualLateUpdate();
		}
	}

	public void ForceUpdate()
	{
		UpdatePriority();
		if (Priority != EAudioSourcePriority.OutOfRange)
		{
			Gclass1141_0.ForceUpdate();
		}
	}

	public void method_1(BetterSource source)
	{
		UpdatePriority();
		UpdateOcclusionEffects(updateImmediately: true);
	}

	public void method_2()
	{
		UpdateOcclusionEffects(updateImmediately: true);
	}

	public void UpdateOcclusionEffects(bool updateImmediately = false)
	{
		Gclass1141_0.UpdateOcclusionEffects(SourceContainerClass, updateImmediately);
	}

	public void UpdatePriority()
	{
		EAudioSourcePriority eAudioSourcePriority;
		if (!SourceContainerClass.IsValid)
		{
			eAudioSourcePriority = EAudioSourcePriority.OutOfRange;
		}
		else
		{
			float sqrMagnitude = (Transform_0.position - SourceContainerClass.CurrentPosition).sqrMagnitude;
			eAudioSourcePriority = Gclass1180_0.GetPriority(sqrMagnitude, SourceContainerClass.SqrMaxDistance + 25f);
		}
		if (eAudioSourcePriority != Priority)
		{
			Priority = eAudioSourcePriority;
			method_5();
		}
	}

	public void Dispose()
	{
		if (!Bool_0)
		{
			Priority = EAudioSourcePriority.OutOfRange;
			method_4();
			Gclass1141_0?.Dispose();
			Bool_0 = true;
		}
	}

	public void method_3(int containerID)
	{
		Gclass1141_0?.Dispose();
		Priority = EAudioSourcePriority.OutOfRange;
		method_5();
	}

	public void method_4()
	{
		if (SourceContainerClass != null)
		{
			SourceContainerClass.OnSourceAdded -= method_1;
			SourceContainerClass.OnSourcePlayed -= method_2;
			SourceContainerClass.OnContainerEmpty -= method_3;
			SourceContainerClass.OnSourceRemoved -= UpdatePriority;
		}
	}

	public void method_5()
	{
		switch (Priority)
		{
		default:
			FramesToSkip = int.MaxValue;
			break;
		case EAudioSourcePriority.Low:
			FramesToSkip = 2;
			break;
		case EAudioSourcePriority.Medium:
			FramesToSkip = 1;
			break;
		case EAudioSourcePriority.High:
			FramesToSkip = 0;
			break;
		}
		SkippedFrames = 0;
	}

	~GClass1126()
	{
		Dispose();
	}
}
