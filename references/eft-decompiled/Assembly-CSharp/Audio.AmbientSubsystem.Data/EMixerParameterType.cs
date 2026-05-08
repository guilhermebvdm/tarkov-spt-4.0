using System;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public enum EMixerParameterType
{
	None,
	Volume,
	LowPassFreq,
	HighPassFreq,
	ReverbSend,
	DelaySend
}
