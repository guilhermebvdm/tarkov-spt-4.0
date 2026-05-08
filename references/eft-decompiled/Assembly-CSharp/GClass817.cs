using UnityEngine;

public abstract class GClass817
{
	public static void DebugLogsEnabled(bool enabled)
	{
		if (IsDebugLogEnabled() != enabled)
		{
			Debug.Log(enabled ? "ENABLE all LOGs" : "DISABLE all LOGs");
			Debug.unityLogger.logEnabled = enabled;
			LoggerClass.IsLogsEnabled = enabled;
			StackTraceEnabled(enabled);
		}
	}

	public static bool IsDebugLogEnabled()
	{
		return Debug.unityLogger.logEnabled;
	}

	public static void StackTraceEnabled(bool enabled)
	{
		Application.SetStackTraceLogType(LogType.Log, enabled ? StackTraceLogType.Full : StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Assert, enabled ? StackTraceLogType.Full : StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Error, enabled ? StackTraceLogType.Full : StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Exception, enabled ? StackTraceLogType.Full : StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, enabled ? StackTraceLogType.Full : StackTraceLogType.None);
	}

	public static bool IsStackTraceEnabled()
	{
		return Application.GetStackTraceLogType(LogType.Log) != StackTraceLogType.None;
	}
}
