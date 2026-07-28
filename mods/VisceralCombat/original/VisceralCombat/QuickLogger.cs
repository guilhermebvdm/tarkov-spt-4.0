using EFT.UI;

public static class QuickLogger
{
	public static void Log(ELogType logType, string output)
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
}
