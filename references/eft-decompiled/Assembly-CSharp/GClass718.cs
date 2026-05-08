using NLog;

public class GClass718 : LoggerClass
{
	public GClass718(LoggerMode mode)
		: base(LogManager.GetLogger("application"), mode)
	{
	}
}
