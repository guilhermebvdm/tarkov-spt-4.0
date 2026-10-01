using System.Diagnostics;

namespace MagCheckInterrupt.Utils;

public static class LoggerUtil
{
    [Conditional("DEBUG")]
    public static void Debug(string msg)
    {
        MagCheckInterrupt.LogSource.LogDebug(msg);
    }

    public static void Error(string msg)
    {
        MagCheckInterrupt.LogSource.LogError(msg);
    }

    public static void Warning(string msg)
    {
        MagCheckInterrupt.LogSource.LogWarning(msg);
    }

    public static void Info(string msg)
    {
        MagCheckInterrupt.LogSource.LogInfo(msg);
    }
}
