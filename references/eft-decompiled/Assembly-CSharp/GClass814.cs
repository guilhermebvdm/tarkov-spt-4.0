using System;
using System.Collections.Generic;
using Comfort.Common;
using NLog;
using NLog.Config;
using NLog.Targets;

public abstract class GClass814
{
	public class Class389 : LoggerClass
	{
		public Class389(string name)
			: base(name, LoggerMode.Add)
		{
		}
	}

	[NonSerialized]
	public static Dictionary<string, Class389> Dictionary_0 = new Dictionary<string, Class389>();

	[NonSerialized]
	public const string String_0 = "default";

	public static void smethod_0(string name)
	{
		if (!Singleton<LogConfiguratorAbstractClass>.Instantiated)
		{
			Singleton<LogConfiguratorAbstractClass>.Create(GClass733.Create());
		}
		FileTarget fileTarget = new FileTarget(name);
		fileTarget.FileName = "${var:filenamePrefix} FDebug_" + name + ".log";
		fileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff zzz}|${var:gameVersion}|${level}|${logger}|${message}${all-event-properties:separator=|} ${onexception:${newline}EXCEPTION\\: ${exception:format=tostring}}";
		fileTarget.ForceManaged = true;
		LoggingRule rule = new LoggingRule(name, LogLevel.Info, fileTarget);
		Singleton<LogConfiguratorAbstractClass>.Instance.AddLogger(name, fileTarget, rule);
		Class389 value = new Class389(name);
		Dictionary_0[name] = value;
	}

	public static Class389 smethod_1(string name)
	{
		if (!Dictionary_0.ContainsKey(name))
		{
			smethod_0(name);
		}
		return Dictionary_0[name];
	}

	public static void NLogFormat(string name, string format, params object[] args)
	{
		smethod_1(name).LogInfo(format, args);
	}

	public static void NLog(string name, object message)
	{
		smethod_1(name).LogInfo(message.ToString());
	}

	public static void LogFormat(string format, params object[] args)
	{
		NLogFormat("default", format, args);
	}

	public static void Log(object message)
	{
		NLog("default", message);
	}
}
