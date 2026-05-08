using EFT;

public abstract class GClass1873
{
	public static bool IsRunned(this GameStatus status)
	{
		if (status != GameStatus.Runned)
		{
			return IsStarted(status);
		}
		return true;
	}

	public static bool NotStopped(this GameStatus status)
	{
		if (!IsRunned(status))
		{
			return status == GameStatus.SoftStopping;
		}
		return true;
	}

	public static bool IsStarted(this GameStatus status)
	{
		if (status != GameStatus.Starting)
		{
			return status == GameStatus.Started;
		}
		return true;
	}
}
