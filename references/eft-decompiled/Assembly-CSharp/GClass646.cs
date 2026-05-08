using System.Threading.Tasks;
using Comfort.Common;

public abstract class GClass646
{
	public static void SetFromRequestResult<T>(this TaskCompletionSource<T> completionSource, Result<T> result)
	{
		if (result.Failed)
		{
			completionSource.SetException(new GException1(result.ErrorCode, result.Error));
		}
		else
		{
			completionSource.SetResult(result.Value);
		}
	}
}
