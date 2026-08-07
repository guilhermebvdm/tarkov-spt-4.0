using EFT.UI;
using System;
using VisceralCombat;

public static class QuickLogger
{
	public static void Log(ELogType logType, string output)
	{
		try
		{
			if (VisceralEntry.LogSource != null)
			{
				switch (logType)
				{
					case ELogType.Warn:
						VisceralEntry.LogSource.LogWarning(output);
						break;
					case ELogType.Error:
						VisceralEntry.LogSource.LogError(output);
						break;
					default:
						VisceralEntry.LogSource.LogInfo(output);
						break;
				}
			}
		}
		catch { }

		try
		{
			switch (logType)
			{
				case ELogType.Log:
					ConsoleScreen.Log(output);
					break;
				case ELogType.Warn:
					ConsoleScreen.LogWarning(output);
					break;
				case ELogType.Error:
					ConsoleScreen.LogError(output);
					break;
				default:
					ConsoleScreen.Log(output);
					break;
			}
		}
		catch { }
	}
}
