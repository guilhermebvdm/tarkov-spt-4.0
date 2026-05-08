using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLog;

public class GClass396
{
	public class Class381 : LoggerClass
	{
		public Class381()
			: base("aiCoversData", LoggerMode.Add)
		{
		}
	}

	[NonSerialized]
	public Class381 Class381_0;

	[NonSerialized]
	[CompilerGenerated]
	public static GClass396 Gclass396_0 = new GClass396();

	public static GClass396 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass396_0;
		}
	}

	public GClass396()
	{
		Class381_0 = new Class381();
	}

	public bool IsTraceEnable()
	{
		return Class381_0.IsEnabled(LogLevel.Trace);
	}

	public bool IsDebugEnable()
	{
		return Class381_0.IsEnabled(LogLevel.Debug);
	}

	public void LogErrorEditor(string str)
	{
	}

	public void LogException(Exception e)
	{
		Class381_0.LogException(e);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogError(string format, params object[] args)
	{
		Class381_0.LogError(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogWarn(string format, params object[] args)
	{
		Class381_0.LogWarn(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogInfo(string format, params object[] args)
	{
		Class381_0.LogInfo(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogDebug(string format, params object[] args)
	{
		Class381_0.LogDebug(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogTrace(string format, params object[] args)
	{
		Class381_0.LogTrace(format, args);
	}
}
