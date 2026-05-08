using System;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;
using NLog.Config;
using NLog.Targets;
using UnityEngine;

public abstract class LogConfiguratorAbstractClass
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public string LogsDir
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public bool IgnoreErrors
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

	public void method_0(bool ignoreErrors = false)
	{
		IgnoreErrors = ignoreErrors;
		LogsDir = Application.dataPath + "/../Logs";
		if (!Directory.Exists(LogsDir))
		{
			Directory.CreateDirectory(LogsDir);
		}
		LoggingConfiguration loggingConfiguration = Load(ignoreErrors);
		if (loggingConfiguration != null)
		{
			LogManager.Configuration = Reconfig(loggingConfiguration);
			LogManager.ConfigurationChanged += method_2;
			LogManager.ConfigurationReloaded += method_3;
		}
	}

	public void Shutdown()
	{
		Debug.Log("Shutdown()");
		LogManager.ConfigurationChanged -= method_2;
		LogManager.ConfigurationReloaded -= method_3;
		LogManager.Flush();
		LogManager.Configuration = null;
		LogManager.Shutdown();
	}

	public void ReconfigExistingLoggers()
	{
		LogManager.Configuration.Reload();
		method_1();
	}

	public abstract LoggingConfiguration Reconfig(LoggingConfiguration configuration);

	public void method_1()
	{
		Reconfig(LogManager.Configuration);
		LogManager.ReconfigExistingLoggers();
	}

	public void method_2(object sender, LoggingConfigurationChangedEventArgs e)
	{
		method_1();
	}

	public void method_3(object sender, LoggingConfigurationReloadedEventArgs e)
	{
		if (e.Succeeded)
		{
			method_1();
		}
	}

	public static LoggingConfiguration Load(bool ignoreErrors)
	{
		string text = Application.dataPath + "/../NLog/NLog.config";
		if (!File.Exists(text))
		{
			if (ignoreErrors)
			{
				return null;
			}
			throw new FileNotFoundException("Can't find configuration file:" + text);
		}
		return new XmlLoggingConfiguration(text, ignoreErrors)
		{
			AutoReload = false
		};
	}

	public static LoggingConfiguration GetConfiguration()
	{
		return LogManager.Configuration;
	}

	public void AddLogger(string name, FileTarget target, LoggingRule rule)
	{
		LoggingConfiguration configuration = LogManager.Configuration;
		configuration.AddTarget(name, target);
		configuration.LoggingRules.Add(rule);
		ReconfigExistingLoggers();
	}

	public LogConfiguratorAbstractClass()
	{
	}
}
