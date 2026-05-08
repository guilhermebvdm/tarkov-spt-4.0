using System.Diagnostics;
using Audio;

public abstract class GClass1109
{
	[Conditional("UNITY_EDITOR")]
	public static void Log(string moduleName, EAudioLogLevel level, string message)
	{
		GClass1108.Create(moduleName).Log(level, message);
	}

	[Conditional("UNITY_EDITOR")]
	public static void SetLogLevel(string moduleName, EAudioLogLevel level)
	{
		GClass1108.Create(moduleName).SetLogLevel(level);
	}
}
