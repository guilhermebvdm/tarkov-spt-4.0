using System;
using System.Diagnostics;
using UnityEngine;

public abstract class GClass811
{
	[NonSerialized]
	public const string String_0 = "orange";

	[NonSerialized]
	public const string String_1 = "red";

	[NonSerialized]
	public const string String_2 = "green";

	[NonSerialized]
	public const string String_3 = "[{0}]<color={1}><b>!!!!!!!!!!!!!</b></color> {2}";

	[Conditional("UNITY_EDITOR")]
	public static void LogFormat(string format, params object[] args)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void Log(object message)
	{
		smethod_0(LogType.Log, message, "orange");
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogError(object message)
	{
		smethod_0(LogType.Error, message, "orange");
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogRed(object message)
	{
		smethod_0(LogType.Log, message, "red");
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogGreen(object message)
	{
		smethod_0(LogType.Log, message, "green");
	}

	public static void smethod_0(LogType logType, object message, string color)
	{
		UnityEngine.Debug.unityLogger.Log(logType, $"[{Time.frameCount}]<color={color}><b>!!!!!!!!!!!!!</b></color> {message}");
	}
}
