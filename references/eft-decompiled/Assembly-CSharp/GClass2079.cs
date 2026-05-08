using EFT;

public abstract class GClass2079
{
	public static bool Finished(this EOperationStatus status)
	{
		if (status != EOperationStatus.Succeeded)
		{
			return status == EOperationStatus.Failed;
		}
		return true;
	}

	public static bool InProgress(this EOperationStatus status)
	{
		return status == EOperationStatus.Started;
	}
}
