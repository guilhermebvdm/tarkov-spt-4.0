public class GClass712 : LoggerClass
{
	public const string LOG_TYPE = "autocull";

	public static GClass712 Instance = new GClass712();

	public GClass712()
		: base("autocull", LoggerMode.Add)
	{
		Instance = this;
	}

	public static void Debug(string msg)
	{
		Instance.LogDebug(msg);
	}

	public static void Error(string msg)
	{
		Instance.LogError(msg);
	}

	public static void Throw(string msg)
	{
		throw new GException8(msg);
	}

	public static void Warning(string msg)
	{
		Instance.LogWarn(msg);
	}
}
