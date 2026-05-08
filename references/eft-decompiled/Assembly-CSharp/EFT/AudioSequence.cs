using UnityEngine;

namespace EFT;

public class AudioSequence : ScriptableObject
{
	public struct GStruct283(double startTime = 0.0, double loopStartTime = 0.0, double initialLength = 0.0, double loopLength = 0.0, double skipTime = -1.0, double jumpTime = -1.0)
	{
		public double StartTime = startTime;

		public double LoopLength = loopLength;

		public double SkipTime = skipTime;

		public double JumpTime = ((jumpTime >= double.Epsilon) ? jumpTime : StartTime);

		public double InitialLength = initialLength;

		public double LoopStartTime = loopStartTime;

		public float Delay = 0f;
	}

	public AudioClip SequenceClip;

	public double OnTime;

	public double WorkingTime;

	public double OffTime;

	public double DisabledTime;

	public double OverlapTime;

	[HideInInspector]
	public AudioClip OnSource;

	[HideInInspector]
	public AudioClip WorkingSource;

	[HideInInspector]
	public AudioClip OffSource;

	[HideInInspector]
	public AudioClip DisabledSource;

	public GStruct283 GetSequenceSettings(EAudioSequenceType sequenceType)
	{
		double initialLength = 0.0;
		double num = 0.0;
		double loopStartTime = 0.0;
		double loopLength = 0.0;
		double skipTime = -1.0;
		double num2 = -1.0;
		switch (sequenceType)
		{
		case EAudioSequenceType.Working:
			num = OnTime;
			loopStartTime = num;
			initialLength = WorkingTime;
			loopLength = WorkingTime;
			break;
		case EAudioSequenceType.Disabled:
			num = OnTime + WorkingTime + OverlapTime + OffTime;
			loopStartTime = num;
			initialLength = DisabledTime;
			loopLength = DisabledTime;
			break;
		case EAudioSequenceType.OnWorking:
			num = 0.0;
			loopStartTime = OnTime;
			initialLength = OnTime + WorkingTime;
			loopLength = WorkingTime;
			break;
		case EAudioSequenceType.OffDisabled:
			num = OnTime + WorkingTime + OverlapTime;
			loopStartTime = num + OffTime;
			initialLength = OffTime + DisabledTime;
			loopLength = DisabledTime;
			break;
		case EAudioSequenceType.OnOffDisabled:
			num = 0.0;
			initialLength = OffTime + DisabledTime;
			loopLength = DisabledTime;
			skipTime = OnTime;
			num2 = OnTime + WorkingTime + OverlapTime;
			loopStartTime = num2 + OffTime;
			break;
		}
		return new GStruct283(num, loopStartTime, initialLength, loopLength, skipTime, num2);
	}
}
