using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NLog;
using NLog.Config;
using NLog.Targets;

[Serializable]
public class LoggingRuleConfig
{
	[Serializable]
	[CompilerGenerated]
	public class Class414
	{
		public static readonly Class414 class414_0 = new Class414();

		public static Func<NLog.Targets.Target, string> func_0;

		public string method_0(NLog.Targets.Target x)
		{
			return x.Name;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class415<T>
	{
		public static readonly Class415<T> class415_0 = new Class415<T>();

		public static Func<object, string> func_0;

		public string method_0(object x)
		{
			return x.ToString();
		}
	}

	public string LogPattern;

	public LoggingLevelConfig[] LoggingLevelConfigs;

	public string Targets;

	public bool IsFinal;

	public LoggingRuleConfig(LoggingRule loggingRule)
	{
		LogPattern = loggingRule.LoggerNamePattern;
		LoggingLevelConfigs = new LoggingLevelConfig[LogLevel.AllLoggingLevels.Count()];
		int num = 0;
		foreach (LogLevel allLoggingLevel in LogLevel.AllLoggingLevels)
		{
			LoggingLevelConfigs[num++] = new LoggingLevelConfig
			{
				Name = allLoggingLevel.Name,
				IsEnabled = loggingRule.IsLoggingEnabledForLevel(allLoggingLevel)
			};
		}
		Targets = smethod_0(loggingRule.Targets.Select((NLog.Targets.Target x) => x.Name), ", ");
		IsFinal = loggingRule.Final;
	}

	public void Apply(LoggingRule loggingRule)
	{
		LoggingLevelConfig[] loggingLevelConfigs = LoggingLevelConfigs;
		foreach (LoggingLevelConfig loggingLevelConfig in loggingLevelConfigs)
		{
			if (loggingLevelConfig.IsEnabled)
			{
				loggingRule.EnableLoggingForLevel(LogLevel.FromString(loggingLevelConfig.Name));
			}
			else
			{
				loggingRule.DisableLoggingForLevel(LogLevel.FromString(loggingLevelConfig.Name));
			}
		}
	}

	public static string smethod_0<T>(IEnumerable<T> enumerable, string separator = "")
	{
		return string.Join(separator, (from object x in enumerable
			select x.ToString()).ToArray());
	}
}
