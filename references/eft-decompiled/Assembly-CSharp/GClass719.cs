using System;
using System.Collections.Generic;
using Comfort.Common;
using NLog;
using UnityEngine;

public class GClass719 : LoggerClass
{
	[NonSerialized]
	public const string String_0 = "CI";

	[NonSerialized]
	public string[] String_1 = new string[0];

	[NonSerialized]
	public static Dictionary<LogType, StackTraceLogType> Dictionary_0 = new Dictionary<LogType, StackTraceLogType>
	{
		{
			LogType.Assert,
			StackTraceLogType.Full
		},
		{
			LogType.Error,
			StackTraceLogType.Full
		},
		{
			LogType.Exception,
			StackTraceLogType.Full
		},
		{
			LogType.Warning,
			StackTraceLogType.None
		},
		{
			LogType.Log,
			StackTraceLogType.None
		}
	};

	[NonSerialized]
	public static Dictionary<LogType, StackTraceLogType> Dictionary_1 = new Dictionary<LogType, StackTraceLogType>();

	public static void Setup(LoggerMode mode, bool logApplicationMessages = true)
	{
		if (Singleton<GClass719>.Instantiated)
		{
			Singleton<GClass719>.Release(Singleton<GClass719>.Instance);
		}
		Singleton<GClass719>.Create(new GClass719(mode, logApplicationMessages));
		LogManager.Flush();
		foreach (KeyValuePair<LogType, StackTraceLogType> item in Dictionary_0)
		{
			Dictionary_1[item.Key] = Application.GetStackTraceLogType(item.Key);
			Application.SetStackTraceLogType(item.Key, item.Value);
		}
		Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0} is configured", "CILog");
	}

	public GClass719(LoggerMode mode, bool logApplicationMessages)
		: base("CI", mode, logApplicationMessages)
	{
	}

	public static void Release()
	{
		if (!Singleton<GClass719>.Instantiated)
		{
			return;
		}
		Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0} was reset", "CILog");
		GClass719 instance = Singleton<GClass719>.Instance;
		Singleton<GClass719>.Release(instance);
		LogManager.Flush();
		instance.Dispose();
		foreach (KeyValuePair<LogType, StackTraceLogType> item in Dictionary_1)
		{
			Application.SetStackTraceLogType(item.Key, item.Value);
		}
	}

	public override void LogFormat(LogType logType, string format, params object[] args)
	{
		if (args.Length != 0)
		{
			string text = args[0].ToString();
			for (int i = 0; i < String_1.Length; i++)
			{
				string value = String_1[i];
				if (text.Contains(value))
				{
					return;
				}
			}
		}
		if (logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception)
		{
			format = "[ERROR] " + format;
		}
		base.LogFormat(logType, format, args);
	}
}
