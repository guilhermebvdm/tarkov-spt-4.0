using Microsoft.Extensions.Logging;
using NLog;

public abstract class GClass2069
{
	public static NLog.LogLevel ToNLogLevel(this Microsoft.Extensions.Logging.LogLevel logLevel)
	{
		return logLevel switch
		{
			Microsoft.Extensions.Logging.LogLevel.Trace => NLog.LogLevel.Trace, 
			Microsoft.Extensions.Logging.LogLevel.Debug => NLog.LogLevel.Debug, 
			Microsoft.Extensions.Logging.LogLevel.Information => NLog.LogLevel.Info, 
			Microsoft.Extensions.Logging.LogLevel.Warning => NLog.LogLevel.Warn, 
			Microsoft.Extensions.Logging.LogLevel.Error => NLog.LogLevel.Error, 
			Microsoft.Extensions.Logging.LogLevel.Critical => NLog.LogLevel.Fatal, 
			_ => NLog.LogLevel.Off, 
		};
	}
}
