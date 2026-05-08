using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NLog;
using UnityEngine;
using UnityEngine.AI;

public class GClass398
{
	public class Class382 : LoggerClass
	{
		public Class382()
			: base("aiData", LoggerMode.Add)
		{
		}
	}

	public class Class383 : LoggerClass
	{
		public Class383()
			: base("aiVision", LoggerMode.Add)
		{
		}
	}

	[NonSerialized]
	public Class382 Class382_0;

	[NonSerialized]
	public Class383 Class383_0;

	[NonSerialized]
	[CompilerGenerated]
	public static GClass398 Gclass398_0 = new GClass398();

	public static GClass398 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass398_0;
		}
	}

	public GClass398()
	{
		Class382_0 = new Class382();
		Class383_0 = new Class383();
	}

	public bool IsTraceEnable()
	{
		return Class382_0.IsEnabled(LogLevel.Trace);
	}

	public bool IsVisionLogEnabled(LogLevel level)
	{
		return Class383_0.IsEnabled(level);
	}

	public void LogCalculatePathFail(Vector3 from, Vector3 to)
	{
		if (IsTraceEnable())
		{
			string format = $"Bot Calculate Path. from:{from} to:{to} fail";
			Class382_0.Log(LogLevel.Trace, format);
		}
	}

	public void LogErrorEditor(string str)
	{
	}

	public void LogCalculatePath(Vector3 from, Vector3 to, NavMeshPathStatus status, int length)
	{
		if (Class382_0.IsEnabled(LogLevel.Trace) && status != NavMeshPathStatus.PathComplete)
		{
			string format = $"Bot CalculatePath. from:{from} to:{to} complete. status:{status.ToString()} lenght:{length}";
			Class382_0.Log(LogLevel.Trace, format);
		}
	}

	public void LogException(Exception e)
	{
		Class382_0.LogException(e);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogError(string format, params object[] args)
	{
		Class382_0.LogError(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogWarn(string format, params object[] args)
	{
		Class382_0.LogWarn(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogInfo(string format, params object[] args)
	{
		Class382_0.LogInfo(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogDebug(string format, params object[] args)
	{
		Class382_0.LogDebug(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogTrace(string format, params object[] args)
	{
		Class382_0.LogTrace(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogVisionTrace(string format, params object[] args)
	{
		Class383_0.LogTrace(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogVisionDebug(string format, params object[] args)
	{
		Class383_0.LogDebug(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogVisionInfo(string format, params object[] args)
	{
		Class383_0.LogInfo(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogVisionError(string format, params object[] args)
	{
		Class383_0.LogError(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	[StringFormatMethod("format")]
	public void LogVisionWarn(string format, params object[] args)
	{
		Class383_0.LogWarn(format, args);
	}
}
