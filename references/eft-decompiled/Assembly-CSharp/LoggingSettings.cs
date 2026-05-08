using UnityEngine;

public class LoggingSettings : ScriptableObject
{
	private static LoggingSettings loggingSettings_0;

	public bool AILogging;

	public static LoggingSettings Instance => loggingSettings_0 ?? (loggingSettings_0 = GClass861.Load<LoggingSettings>("LoggingSettings"));
}
