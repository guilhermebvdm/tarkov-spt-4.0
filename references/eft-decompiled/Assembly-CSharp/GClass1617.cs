using Comfort.Common;

public abstract class GClass1617
{
	public static GStruct154<TBase> Cast<TSource, TBase>(this GStruct154<TSource> source) where TSource : TBase where TBase : IRaiseEvents
	{
		if (source.Error == null)
		{
			return new GStruct154<TBase>((TBase)(object)source.Value);
		}
		return new GStruct154<TBase>(source.Error);
	}

	public static void Invoke<T>(this Callback callback, GStruct156<T> sourceOption)
	{
		if (sourceOption.Succeeded)
		{
			callback.Succeed();
		}
		else
		{
			callback.Fail(sourceOption.Error?.ToString());
		}
	}

	public static IResult ToResult(this IInventoryEventResult sourceOption)
	{
		if (!sourceOption.Succeeded)
		{
			return new FailedResult(sourceOption.Error?.ToString());
		}
		return SuccessfulResult.New;
	}
}
