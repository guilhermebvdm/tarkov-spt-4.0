using System;
using Audio;

public class GClass1110 : GInterface68
{
	[NonSerialized]
	public static GInterface68 Ginterface68_0;

	public static GInterface68 Instance => Ginterface68_0 ?? (Ginterface68_0 = new GClass1110());

	public void Log(EAudioLogLevel level, string message)
	{
	}

	public void SetLogLevel(EAudioLogLevel levelMask)
	{
	}

	public EAudioLogLevel GetCurrentLogLevelMask()
	{
		return EAudioLogLevel.None;
	}
}
