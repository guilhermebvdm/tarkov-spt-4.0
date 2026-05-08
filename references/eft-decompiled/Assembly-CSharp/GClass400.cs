using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLog;

public class GClass400
{
	public class Class385 : LoggerClass
	{
		public Class385()
			: base("aiMoveData", LoggerMode.Add)
		{
		}
	}

	[NonSerialized]
	public Class385 Class385_0;

	[NonSerialized]
	[CompilerGenerated]
	public static GClass399 Gclass399_0 = new GClass399();

	public static GClass399 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass399_0;
		}
	}

	public GClass400()
	{
		Class385_0 = new Class385();
	}

	public bool IsTraceEnable()
	{
		return Class385_0.IsEnabled(LogLevel.Trace);
	}

	public bool IsDebugEnable()
	{
		return Class385_0.IsEnabled(LogLevel.Debug);
	}

	public void LogErrorEditor(string str)
	{
	}

	public void LogException(Exception e)
	{
		Class385_0.LogException(e);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogError(string format, params object[] args)
	{
		Class385_0.LogError(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogWarn(string format, params object[] args)
	{
		Class385_0.LogWarn(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogInfo(string format, params object[] args)
	{
		Class385_0.LogInfo(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogDebug(string format, params object[] args)
	{
		Class385_0.LogDebug(format, args);
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	[Conditional("SERVER")]
	public void LogTrace(string format, params object[] args)
	{
		Class385_0.LogTrace(format, args);
	}
}
