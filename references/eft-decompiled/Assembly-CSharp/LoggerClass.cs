using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CustomPlayerLoopSystem;
using JetBrains.Annotations;
using NLog;
using UnityEngine;

public abstract class LoggerClass : IDisposable
{
	public class Class412 : ILogHandler
	{
		[Serializable]
		[CompilerGenerated]
		public class Class413
		{
			public static readonly Class413 class413_0 = new Class413();

			public static Func<string, KeyValuePair<object, object>, string> func_0;

			public string method_0(string current, KeyValuePair<object, object> prop)
			{
				return current + " " + prop.Key?.ToString() + "=" + prop.Value;
			}
		}

		[NonSerialized]
		public object[] Object_0 = new object[0];

		[NonSerialized]
		public LoggerClass LoggerClass;

		[NonSerialized]
		public ILogHandler IlogHandler_0;

		[NonSerialized]
		public int Int_0;

		public static int Int32_0 => FrameCounter.CurrentFrame;

		public Class412(LoggerClass parent, bool replaceUnityLogger)
		{
			LoggerClass = parent;
			IlogHandler_0 = Debug.unityLogger.logHandler;
			if (replaceUnityLogger)
			{
				Debug.unityLogger.logHandler = this;
			}
		}

		public void Release()
		{
			if (LoggerClass != null)
			{
				LoggerClass = null;
				if (Debug.unityLogger.logHandler == this)
				{
					Debug.unityLogger.logHandler = IlogHandler_0;
				}
				IlogHandler_0 = null;
			}
		}

		public void method_0(string loggerName, LogEventInfo logEvent)
		{
			string format = logEvent.Properties.Aggregate(logEvent.Message, (string current, KeyValuePair<object, object> prop) => current + " " + prop.Key?.ToString() + "=" + prop.Value);
			method_1(loggerName, logEvent.Level, format, logEvent.Parameters ?? Object_0);
		}

		[StringFormatMethod("format")]
		public void method_1(string loggerName, LogLevel logLevel, string format, object[] args)
		{
			if (!(logLevel == LogLevel.Off) && LoggerClass.Logger_0.IsEnabled(logLevel))
			{
				LogType logType = ((logLevel < LogLevel.Warn) ? LogType.Log : ((logLevel == LogLevel.Warn) ? LogType.Warning : LogType.Error));
				method_3(loggerName, logType, format, args);
			}
		}

		public void method_2(Exception exception)
		{
			IlogHandler_0.LogException(exception, null);
		}

		[StringFormatMethod("format")]
		public void method_3(string loggerName, LogType logType, string format, object[] args)
		{
			if (format != null && logType == LogType.Error && (format.Contains("out of bound or host has been already removed") || format.Contains("invoked wrong io operation and will be deleted")))
			{
				if (Int_0 > 100)
				{
					return;
				}
				Int_0++;
			}
			if (UnityDebugLogsEnabled && (logType == LogType.Exception || logType == LogType.Error))
			{
				string format2 = "[<b>" + loggerName + "</b>] " + format;
				IlogHandler_0.LogFormat(logType, null, format2, args);
			}
		}

		[StringFormatMethod("format")]
		public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			if (logType == LogType.Error && format != null && logType == LogType.Error && (format.Contains("out of bound or host has been already removed") || format.Contains("invoked wrong io operation and will be deleted")))
			{
				if (Int_0 > 100)
				{
					return;
				}
				Int_0++;
			}
			LoggerClass.LogFormat(logType, format, args);
			if (UnityDebugLogsEnabled && (logType == LogType.Exception || logType == LogType.Error))
			{
				string text = $"[{Int32_0}] {format}";
				if (args.Length == 0)
				{
					IlogHandler_0.LogFormat(logType, context, "{0}", text);
				}
				else
				{
					IlogHandler_0.LogFormat(logType, context, text, args);
				}
			}
		}

		void ILogHandler.LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			//ILSpy generated this explicit interface implementation from .override directive in LogFormat
			this.LogFormat(logType, context, format, args);
		}

		public void LogException(Exception exception, UnityEngine.Object context)
		{
			LoggerClass.method_1(exception);
			IlogHandler_0.LogException(exception, context);
		}

		void ILogHandler.LogException(Exception exception, UnityEngine.Object context)
		{
			//ILSpy generated this explicit interface implementation from .override directive in LogException
			this.LogException(exception, context);
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public static bool Bool_0;

	public static bool IsLogsEnabled = true;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public NLog.Logger Logger_0;

	[NonSerialized]
	public Class412 Class412_0;

	[NonSerialized]
	[CompilerGenerated]
	public LoggerMode LoggerMode_0;

	[NonSerialized]
	public StringBuilder StringBuilder_0 = new StringBuilder();

	public static bool UnityDebugLogsEnabled
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public string NLoggerName => Logger_0.Name;

	public LoggerMode Mode
	{
		[CompilerGenerated]
		get
		{
			return LoggerMode_0;
		}
		[CompilerGenerated]
		set
		{
			LoggerMode_0 = value;
		}
	}

	public LoggerClass(string loggerName, LoggerMode mode, bool logApplicationMessages = false)
		: this(LogManager.GetLogger(loggerName), mode, logApplicationMessages)
	{
	}

	public LoggerClass(NLog.Logger nLogger, LoggerMode mode, bool logApplicationMessages = false)
	{
		Logger_0 = nLogger;
		Mode = mode;
		if (mode != LoggerMode.None)
		{
			bool replaceUnityLogger = mode == LoggerMode.Replace;
			Class412_0 = new Class412(this, replaceUnityLogger);
		}
		Bool_1 = logApplicationMessages;
		if (Bool_1)
		{
			Application.logMessageReceived += method_2;
		}
	}

	public virtual void Dispose()
	{
		if (Logger_0 != null)
		{
			if (Bool_1)
			{
				Application.logMessageReceived -= method_2;
			}
			Logger_0 = null;
			Interlocked.Exchange(ref Class412_0, null)?.Release();
		}
	}

	public bool IsEnabled(LogLevel level)
	{
		return Logger_0.IsEnabled(level);
	}

	public void Log(LogEventInfo logEvent)
	{
		if (IsLogsEnabled)
		{
			Logger_0.Log(logEvent);
			Class412_0?.method_0(Logger_0.Name, logEvent);
		}
	}

	[StringFormatMethod("format")]
	public void LogTrace(string format, params object[] args)
	{
		Log(LogLevel.Trace, format, args);
	}

	[StringFormatMethod("format")]
	public void LogDebug(string format, params object[] args)
	{
		Log(LogLevel.Debug, format, args);
	}

	[StringFormatMethod("format")]
	public void LogInfo(string format, params object[] args)
	{
		Log(LogLevel.Info, format, args);
	}

	[StringFormatMethod("format")]
	public void LogInfoWithStackTrace(string format)
	{
		Log(LogLevel.Info, format, method_0(LogLevel.Info));
	}

	public string method_0(LogLevel logLevel)
	{
		return "<stack trace turned off>";
	}

	[StringFormatMethod("format")]
	public void LogWarn(string format, params object[] args)
	{
		Log(LogLevel.Warn, format, args);
	}

	[StringFormatMethod("format")]
	public void LogError(string format, params object[] args)
	{
		if (format.ToLower().Contains("framerate was set"))
		{
			Log(LogLevel.Warn, format, args);
		}
		else
		{
			Log(LogLevel.Error, format, args);
		}
	}

	public void LogException(Exception e)
	{
		if (IsLogsEnabled)
		{
			Log(LogLevel.Error, e.ToString());
			Class412_0?.method_2(e);
		}
	}

	public virtual void Log(LogLevel logLevel, string format, params object[] args)
	{
		Log(format, format, logLevel, args);
	}

	public void Log(string nlogFormat, string unityFormat, LogLevel logLevel, params object[] args)
	{
		if (!IsLogsEnabled)
		{
			return;
		}
		try
		{
			Logger_0.Log(logLevel, nlogFormat, args);
			Class412_0?.method_1(Logger_0.Name, logLevel, unityFormat, args);
		}
		catch (Exception)
		{
		}
	}

	public virtual void LogFormat(LogType logType, string format, params object[] args)
	{
		if (IsLogsEnabled)
		{
			LogLevel level = ConvertLogTypeToLogLevel(logType);
			if (string.IsNullOrEmpty(format) && args != null && args.Length != 0)
			{
				Logger_0.Log(level, smethod_0(args));
			}
			else
			{
				Logger_0.Log(level, format, args);
			}
		}
	}

	public static LogLevel ConvertLogTypeToLogLevel(LogType logType)
	{
		switch (logType)
		{
		default:
			return LogLevel.Error;
		case LogType.Assert:
		case LogType.Warning:
			return LogLevel.Warn;
		case LogType.Log:
			return LogLevel.Info;
		case LogType.Error:
		case LogType.Exception:
			return LogLevel.Error;
		}
	}

	public static string smethod_0(object[] args)
	{
		while (true)
		{
			if (args.Length != 1)
			{
				if (!(args[1] is object[] array))
				{
					break;
				}
				args = array;
				continue;
			}
			return args[0].ToString();
		}
		return (args[0]?.ToString() + "\n" + args[1]).Replace("\n", "| \n");
	}

	public void method_1(Exception exception)
	{
		Logger_0.Error(exception, "");
	}

	public void method_2(string message, string stackTrace, LogType logType)
	{
		if (IsLogsEnabled && !stackTrace.Contains("\nEFT.UnityLogger") && (logType != LogType.Log || !message.StartsWith("no free events for")) && (logType != LogType.Warning || !message.StartsWith("The referenced script on this Behaviour")))
		{
			object[] args = new object[1] { message + "\n" + stackTrace };
			LogFormat(logType, null, args);
		}
	}

	public void LogInfoWithColor(string color, string format, params object[] args)
	{
		LogInfo(format, args);
	}

	public string LogInfoFieldsWithColor(string color, params object[] args)
	{
		string text = FormatFields(args);
		LogInfo(text);
		return text;
	}

	public string LogWarnFields(params object[] args)
	{
		if (!IsEnabled(LogLevel.Warn))
		{
			return string.Empty;
		}
		string text = FormatFields(args);
		LogWarn(text);
		return text;
	}

	public string LogTraceFields(params object[] args)
	{
		if (!IsEnabled(LogLevel.Trace))
		{
			return string.Empty;
		}
		string text = FormatFields(args);
		LogTrace(text);
		return text;
	}

	public string LogInfoFields(params object[] args)
	{
		if (!IsEnabled(LogLevel.Info))
		{
			return string.Empty;
		}
		string text = FormatFields(args);
		LogInfo(text);
		return text;
	}

	public string LogErrorFields(params object[] args)
	{
		if (!IsEnabled(LogLevel.Error))
		{
			return string.Empty;
		}
		string text = FormatFields(args);
		LogError(text);
		return text;
	}

	public string FormatFields(params object[] args)
	{
		StringBuilder_0.Length = 0;
		for (int i = 0; i < args.Length; i += 2)
		{
			object obj = args[i];
			object obj2 = args[i + 1];
			if (obj != null)
			{
				StringBuilder_0.Append(obj);
				StringBuilder_0.Append(":");
			}
			if (obj2 != null)
			{
				StringBuilder_0.Append(obj2);
				if (i < args.Length - 2)
				{
					StringBuilder_0.Append(", ");
				}
			}
		}
		return StringBuilder_0.ToString();
	}
}
