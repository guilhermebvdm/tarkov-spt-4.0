using System;
using Microsoft.Extensions.Logging;
using NLog;

public class LoggerFactoryClass : ILoggerFactory
{
	public class Class402<T> : Class401, ILogger<T>, Microsoft.Extensions.Logging.ILogger
	{
		public Class402(string categoryName = null)
			: base(categoryName ?? "T")
		{
		}
	}

	public class Class401 : LoggerClass, Microsoft.Extensions.Logging.ILogger
	{
		public class Class1814 : IDisposable
		{
			public static readonly Class1814 Instance = new Class1814();

			public void Dispose()
			{
			}
		}

		public Class401(string categoryName)
			: base(categoryName, LoggerMode.Add)
		{
		}

		public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			NLog.LogLevel level = GClass2069.ToNLogLevel(logLevel);
			if (IsEnabled(level))
			{
				LogEventInfo logEvent = new LogEventInfo
				{
					Level = level,
					Exception = exception,
					Message = ((formatter != null) ? formatter(state, exception) : state.ToString())
				};
				Log(logEvent);
			}
		}

		public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
		{
			NLog.LogLevel level = GClass2069.ToNLogLevel(logLevel);
			return IsEnabled(level);
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return Class1814.Instance;
		}
	}

	public string CategoryName => "FilesChecker";

	public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
	{
		return new Class401(categoryName);
	}

	public ILogger<T> CreateLogger<T>()
	{
		return new Class402<T>(CategoryName);
	}
}
