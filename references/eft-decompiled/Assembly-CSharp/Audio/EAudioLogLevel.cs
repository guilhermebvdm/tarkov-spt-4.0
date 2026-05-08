using System;

namespace Audio;

[Flags]
public enum EAudioLogLevel
{
	None = 0,
	Error = 1,
	Warn = 2,
	Info = 4,
	Debug = 8,
	Trace = 0x10,
	Minimum = 3,
	Normal = 7,
	Maximum = 0x1F
}
