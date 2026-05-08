using System;

namespace Audio.SpatialSystem.SpatialAudioCalculator;

[Flags]
public enum EOcclusionDebugMask
{
	None = 0,
	Reflection = 2,
	Diffraction = 4,
	Transmission = 8,
	Propagation = 0x10,
	All = 0x1E
}
